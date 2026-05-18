namespace TombLib.LanguageServer.Core;

public sealed partial class WorkspaceFileChangeForwarder
{
	/// <summary>
	/// Attempts to forward a new change set immediately.
	/// The change set is buffered only after forwarding was allowed and startup or transport forwarding failed.
	/// When forwarding is not currently allowed, the change set is either buffered or ignored based on construction options.
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
			if (changes.Count == 0)
				return;

			if (!_canForwardAccessor())
			{
				BufferChangesWhenForwardingDisabled(changes);
				return;
			}

			await _forwardingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
			forwardingGateHeld = true;

			if (IsDisposeRequested())
				return;

			if (!_canForwardAccessor())
			{
				BufferChangesWhenForwardingDisabled(changes);
				return;
			}

			bool started = await _ensureStartedAsync(cancellationToken).ConfigureAwait(false);

			if (IsDisposeRequested())
				return;

			if (!started)
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

	private void BufferChangesWhenForwardingDisabled(IReadOnlyList<WorkspaceFileChange> changes)
	{
		if (_bufferChangesWhileForwardingDisabled)
			_deferredChanges.AddRange(changes);
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

			if (IsDisposeRequested())
				return;

			if (!_canForwardAccessor() || _deferredChanges.IsEmpty)
				return;

			List<WorkspaceFileChange> deferredChanges = _deferredChanges.DrainChanges();

			if (deferredChanges.Count == 0)
				return;

			if (IsDisposeRequested())
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
}
