namespace TombLib.LanguageServer.Core;

/// <summary>
/// Forwards workspace file changes when the owner allows it and buffers failed deliveries for replay.
/// </summary>
public sealed class WorkspaceFileChangeForwarder : IDisposable
{
	private readonly Func<bool> _canForwardAccessor;
	private readonly Func<bool> _isDisposedAccessor;
	private readonly Func<CancellationToken, Task<bool>> _ensureStartedAsync;
	private readonly Action _markTransportUnavailable;
	private readonly Action<Exception>? _logForwardingFailure;
	private readonly WorkspaceChangeAccumulator _deferredChanges = new();
	private readonly SemaphoreSlim _forwardingGate = new(1, 1);
	private readonly object _disposeSyncRoot = new();
	private int _activeOperationCount;
	private bool _disposeRequested;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceFileChangeForwarder"/> class.
	/// </summary>
	/// <param name="canForwardAccessor">Reports whether forwarding attempts are currently allowed.</param>
	/// <param name="isDisposedAccessor">Reports whether the owner has been disposed.</param>
	/// <param name="ensureStartedAsync">Starts or validates the underlying transport before forwarding.</param>
	/// <param name="markTransportUnavailable">Marks the current transport as unavailable after forwarding failures.</param>
	/// <param name="logForwardingFailure">Logs unexpected forwarding failures.</param>
	public WorkspaceFileChangeForwarder(
		Func<bool> canForwardAccessor,
		Func<bool> isDisposedAccessor,
		Func<CancellationToken, Task<bool>> ensureStartedAsync,
		Action markTransportUnavailable,
		Action<Exception>? logForwardingFailure = null)
	{
		_canForwardAccessor = canForwardAccessor;
		_isDisposedAccessor = isDisposedAccessor;
		_ensureStartedAsync = ensureStartedAsync;
		_markTransportUnavailable = markTransportUnavailable;
		_logForwardingFailure = logForwardingFailure;
	}

	/// <summary>
	/// Attempts to forward a new change set immediately.
	/// The change set is buffered only after forwarding was allowed and startup or transport forwarding failed.
	/// When forwarding is not currently allowed, the change set is ignored.
	/// </summary>
	/// <param name="changes">The file changes to forward.</param>
	/// <param name="forwardAsync">The transport forwarding callback.</param>
	/// <param name="cancellationToken">Cancels the forwarding operation.</param>
	public async Task DispatchAsync(
		IReadOnlyList<WorkspaceFileChange> changes,
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		if (!TryEnterOperation())
			return;

		bool forwardingGateHeld = false;

		try
		{
		if (changes.Count == 0 || !_canForwardAccessor())
			return;

		await _forwardingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
		forwardingGateHeld = true;

			if (!_canForwardAccessor())
				return;

			if (!await _ensureStartedAsync(cancellationToken).ConfigureAwait(false))
			{
				_deferredChanges.AddRange(changes);
				return;
			}

			await TryForwardAsync(changes, forwardAsync, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			if (forwardingGateHeld)
				_forwardingGate.Release();

			ExitOperation();
		}
	}

	/// <summary>
	/// Replays any previously buffered changes now that forwarding is allowed again.
	/// If forwarding is still not allowed, the buffered set is preserved for a later replay attempt.
	/// </summary>
	/// <param name="forwardAsync">The transport forwarding callback.</param>
	/// <param name="cancellationToken">Cancels the replay operation.</param>
	public async Task ReplayDeferredAsync(
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		if (!TryEnterOperation())
			return;

		if (!_canForwardAccessor() || _deferredChanges.IsEmpty)
		{
			ExitOperation();
			return;
		}

		bool forwardingGateHeld = false;

		try
		{
			await _forwardingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
			forwardingGateHeld = true;

			if (!_canForwardAccessor() || _deferredChanges.IsEmpty)
				return;

			List<WorkspaceFileChange> deferredChanges = _deferredChanges.DrainChanges();

			if (deferredChanges.Count == 0)
				return;

			await TryForwardAsync(deferredChanges, forwardAsync, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			return;
		}
		finally
		{
			if (forwardingGateHeld)
				_forwardingGate.Release();

			ExitOperation();
		}
	}

	/// <summary>
	/// Releases the owned synchronization gate once no forwarding operations remain active.
	/// </summary>
	public void Dispose()
	{
		bool shouldDisposeForwardingGate = false;

		lock (_disposeSyncRoot)
		{
			if (_disposeRequested)
				return;

			_disposeRequested = true;
			shouldDisposeForwardingGate = TryMarkDisposedUnderLock();
		}

		if (shouldDisposeForwardingGate)
			_forwardingGate.Dispose();
	}

	/// <summary>
	/// Forwards a change set and converts transport failures into buffered replay state.
	/// </summary>
	/// <param name="changes">The file changes to forward.</param>
	/// <param name="forwardAsync">The transport forwarding callback.</param>
	/// <param name="cancellationToken">Cancels the forwarding operation.</param>
	private async Task TryForwardAsync(
		IReadOnlyList<WorkspaceFileChange> changes,
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		try
		{
			await forwardAsync(changes, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_deferredChanges.AddRange(changes);
			_markTransportUnavailable();
		}
		catch (IOException exception)
		{
			_deferredChanges.AddRange(changes);
			_markTransportUnavailable();

			_logForwardingFailure?.Invoke(exception);
		}
		catch (ObjectDisposedException)
		{
			if (!_isDisposedAccessor())
			{
				_deferredChanges.AddRange(changes);
				_markTransportUnavailable();
			}
		}
		catch (OperationCanceledException)
		{
			if (!_isDisposedAccessor())
				_deferredChanges.AddRange(changes);
		}
		catch (Exception exception)
		{
			_deferredChanges.AddRange(changes);
			_markTransportUnavailable();

			_logForwardingFailure?.Invoke(exception);
		}
	}

	private bool TryEnterOperation()
	{
		lock (_disposeSyncRoot)
		{
			if (_disposeRequested)
				return false;

			_activeOperationCount++;
			return true;
		}
	}

	private void ExitOperation()
	{
		bool shouldDisposeForwardingGate = false;

		lock (_disposeSyncRoot)
		{
			_activeOperationCount--;
			shouldDisposeForwardingGate = TryMarkDisposedUnderLock();
		}

		if (shouldDisposeForwardingGate)
			_forwardingGate.Dispose();
	}

	private bool TryMarkDisposedUnderLock()
	{
		if (!_disposeRequested || _disposed || _activeOperationCount != 0)
			return false;

		_disposed = true;
		return true;
	}
}
