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
	/// <returns><see langword="true"/> when the batch was forwarded immediately; otherwise, <see langword="false"/>.</returns>
	public async Task<bool> DispatchAsync(
		IReadOnlyList<WorkspaceFileChange> changes,
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		if (!TryEnterOperation())
			return false;

		bool forwardingGateHeld = false;

		try
		{
			if (changes.Count == 0)
				return false;

			if (!_canForwardAccessor())
			{
				BufferChangesWhenForwardingDisabled(changes);
				return false;
			}

			bool started = await _ensureStartedAsync(cancellationToken).ConfigureAwait(false);

			if (IsDisposeRequested())
				return false;

			if (!started)
			{
				_deferredChanges.AddRange(changes);
				return false;
			}

			await _forwardingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
			forwardingGateHeld = true;

			if (IsDisposeRequested())
				return false;

			if (!_canForwardAccessor())
			{
				BufferChangesWhenForwardingDisabled(changes);
				return false;
			}

			return await TryForwardAsync(changes, forwardAsync, cancellationToken).ConfigureAwait(false);
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
	/// <returns>The buffered changes that were replayed successfully, or an empty list when none were forwarded.</returns>
	public async Task<IReadOnlyList<WorkspaceFileChange>> ReplayDeferredAsync(
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		if (!TryEnterOperation())
			return [];

		if (!_canForwardAccessor() || _deferredChanges.IsEmpty)
		{
			ExitOperation();
			return [];
		}

		bool forwardingGateHeld = false;
		List<WorkspaceFileChange>? deferredChanges = null;

		try
		{
			await _forwardingGate.WaitAsync(cancellationToken).ConfigureAwait(false);
			forwardingGateHeld = true;

			if (IsDisposeRequested())
				return [];

			if (!_canForwardAccessor() || _deferredChanges.IsEmpty)
				return [];

			deferredChanges = _deferredChanges.DrainChanges();

			if (deferredChanges.Count == 0)
				return [];

			if (IsDisposeRequested())
				return [];

			bool forwarded = await TryForwardAsync(deferredChanges, forwardAsync, cancellationToken).ConfigureAwait(false);

			return forwarded ? deferredChanges : [];
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			if (deferredChanges is not null && !_isDisposedAccessor())
				_deferredChanges.AddRange(deferredChanges);

			return [];
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
	/// <returns><see langword="true"/> when the batch was forwarded successfully; otherwise, <see langword="false"/>.</returns>
	private async Task<bool> TryForwardAsync(
		IReadOnlyList<WorkspaceFileChange> changes,
		Func<IReadOnlyList<WorkspaceFileChange>, CancellationToken, Task> forwardAsync,
		CancellationToken cancellationToken)
	{
		try
		{
			await forwardAsync(changes, cancellationToken).ConfigureAwait(false);
			return true;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_deferredChanges.AddRange(changes);
			_markTransportUnavailable();
			return false;
		}
		catch (IOException exception)
		{
			_deferredChanges.AddRange(changes);
			_markTransportUnavailable();

			_logForwardingFailure?.Invoke(exception);
			return false;
		}
		catch (ObjectDisposedException)
		{
			if (!_isDisposedAccessor())
			{
				_deferredChanges.AddRange(changes);
				_markTransportUnavailable();
			}

			return false;
		}
		catch (OperationCanceledException)
		{
			if (!_isDisposedAccessor())
				_deferredChanges.AddRange(changes);

			return false;
		}
		catch (Exception exception)
		{
			_logForwardingFailure?.Invoke(exception);
			return false;
		}
	}
}
