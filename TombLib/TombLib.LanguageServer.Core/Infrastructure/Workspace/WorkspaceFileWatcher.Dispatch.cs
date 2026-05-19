namespace TombLib.LanguageServer.Core;

public sealed partial class WorkspaceFileWatcher
{
	/// <summary>
	/// Queues a single workspace change when the watcher is active.
	/// </summary>
	/// <param name="filePath">The changed file path.</param>
	/// <param name="kind">The change kind.</param>
	private void QueueChange(string filePath, FileChangeKind kind)
	{
		if (_isDisposed)
			return;

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		_pendingChanges.Queue(normalizedFilePath, kind);
	}

	/// <summary>
	/// Dispatches any currently pending changes and tracks concurrent disposal state.
	/// </summary>
	private async Task DispatchPendingChangesAsync()
	{
		if (!TryEnterDispatchOperation())
			return;

		try
		{
			await DispatchPendingChangesCoreAsync().ConfigureAwait(false);
		}
		finally
		{
			ExitDispatchOperation();
		}
	}

	/// <summary>
	/// Drains and forwards the currently pending file changes.
	/// </summary>
	private async Task DispatchPendingChangesCoreAsync()
	{
		bool dispatchGateHeld = false;
		FileChangeBatch? batch = null;

		try
		{
			await _dispatchGate.WaitAsync(_lifetimeCts.Token).ConfigureAwait(false);
			dispatchGateHeld = true;

			if (_pendingChanges.IsEmpty)
				return;

			batch = _pendingChanges.DrainBatch();

			if (batch.Count == 0)
				return;

			await _dispatchAsync(batch, _lifetimeCts.Token).ConfigureAwait(false);
			_consecutiveDispatchFailures = 0;
		}
		catch (OperationCanceledException)
		{ }
		catch (ObjectDisposedException)
		{ }
		catch (Exception exception)
		{
			int consecutiveDispatchFailures = 0;

			bool willRetry = false;
			TimeSpan retryDelay = DispatchDebounce;

			if (batch is not null)
			{
				if (_isDisposed)
				{
					if (ShouldFlushDisposeRetryChanges())
						BufferDisposeRetryChanges(batch);
				}
				else if (CanRequeuePendingChanges())
				{
					consecutiveDispatchFailures = ++_consecutiveDispatchFailures;

					if (consecutiveDispatchFailures < DispatchFailureEscalationThreshold)
					{
						willRetry = true;
						retryDelay = GetDispatchRetryDelay(consecutiveDispatchFailures);

						_pendingChanges.Requeue(batch, retryDelay);
					}
					else
					{
						// Preserve the terminal failed batch so the watcher-failure recovery path can
						// attempt one last owner-driven dispatch instead of silently dropping it.
						_pendingChanges.Restore(batch);
					}
				}
			}

			if (consecutiveDispatchFailures >= DispatchFailureEscalationThreshold)
			{
				Log.Warn(exception,
					"Workspace file watcher dispatch failed for '{Workspace}' {FailureCount} times in a row; no further retries will be attempted and the owner will be notified.",
					_workspaceRootDirectoryPath,
					consecutiveDispatchFailures);
			}
			else if (consecutiveDispatchFailures >= DispatchFailureWarningThreshold)
			{
				Log.Warn(exception,
					"Workspace file watcher dispatch failed for '{Workspace}' with {Count} queued change(s) {FailureCount} times in a row; retrying in {RetryDelayMs} ms with backoff.",
					_workspaceRootDirectoryPath,
					batch?.Count ?? 0,
					consecutiveDispatchFailures,
					(int)retryDelay.TotalMilliseconds);
			}
			else if (willRetry)
			{
				Log.Debug(exception,
					"Workspace file watcher dispatch failed for '{Workspace}' with {Count} queued change(s); retrying in {RetryDelayMs} ms.",
					_workspaceRootDirectoryPath,
					batch?.Count ?? 0,
					(int)retryDelay.TotalMilliseconds);
			}

			if (consecutiveDispatchFailures >= DispatchFailureEscalationThreshold)
				HandleWatcherError(exception);
		}
		finally
		{
			if (dispatchGateHeld)
			{
				try
				{
					_dispatchGate.Release();
				}
				catch (ObjectDisposedException)
				{ }
			}
		}
	}

	private static TimeSpan GetDispatchRetryDelay(int consecutiveDispatchFailures)
	{
		int exponentialShift = Math.Clamp(consecutiveDispatchFailures - 1, 0, 4);
		double retryDelayMilliseconds = DispatchDebounce.TotalMilliseconds * (1 << exponentialShift);

		return TimeSpan.FromMilliseconds(Math.Min(MaxDispatchRetryDelay.TotalMilliseconds, retryDelayMilliseconds));
	}

	private bool TryEnterDispatchOperation()
	{
		lock (_dispatchLifecycleSyncRoot)
		{
			if (_disposeFinalizationStarted)
				return false;

			_activeDispatchCount++;
			return true;
		}
	}

	private void ExitDispatchOperation()
	{
		Task? disposeFinalizationTask = null;

		lock (_dispatchLifecycleSyncRoot)
		{
			_activeDispatchCount--;

			if (_isDisposed && _activeDispatchCount == 0 && !_disposeFinalizationStarted)
			{
				_disposeFinalizationStarted = true;
				disposeFinalizationTask = Task.Run(FinalizeDisposeAsync);
			}
		}

		if (disposeFinalizationTask is not null)
			EnsureDisposeCompletionTask();
	}

	private bool CanRequeuePendingChanges()
	{
		lock (_dispatchLifecycleSyncRoot)
			return !_disposeFinalizationStarted;
	}

	private void BufferDisposeRetryChanges(FileChangeBatch batch)
	{
		for (int i = 0; i < batch.Count; i++)
			_disposeRetryChanges.Add(batch.Entries[i].Path, batch.Entries[i].Kind);
	}

	[Obsolete("For testing purposes only.")]
	internal void QueueChangeForTest(string filePath, FileChangeKind kind) => QueueChange(filePath, kind);

	[Obsolete("For testing purposes only.")]
	internal Task DispatchPendingChangesForTestAsync() => DispatchPendingChangesAsync();
}
