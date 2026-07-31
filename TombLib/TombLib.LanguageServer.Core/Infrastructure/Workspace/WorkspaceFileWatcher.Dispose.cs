namespace TombLib.LanguageServer.Core;

public sealed partial class WorkspaceFileWatcher
{
	private static readonly TimeSpan DisposeFinalFlushTimeout = TimeSpan.FromSeconds(2.0);

	/// <summary>
	/// Stops active file-system watchers, waits for dispatch finalization, and releases watcher resources.
	/// </summary>
	public void Dispose()
	{
		if (!BeginDispose(flushDisposeRetryChanges: true))
			return;

		TryStartDisposeFinalization();
		GetDisposeCompletionTask().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Stops active file-system watchers, waits for dispatch finalization asynchronously, and releases watcher resources.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (BeginDispose(flushDisposeRetryChanges: true))
			TryStartDisposeFinalization();

		await GetDisposeCompletionTask().ConfigureAwait(false);
	}

	/// <summary>
	/// Releases all watcher resources while intentionally dropping any dispose-time retry batch.
	/// </summary>
	public void DisposeWithoutFinalFlush()
	{
		if (!BeginDispose(flushDisposeRetryChanges: false))
			return;

		TryStartDisposeFinalization();
		GetDisposeCompletionTask().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Attempts one final flush of buffered retry changes during disposal after all in-flight dispatches have quiesced.
	/// </summary>
	private async Task FinalizeDisposeAsync()
	{
		bool dispatchGateHeld = false;

		try
		{
			await _dispatchGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
			dispatchGateHeld = true;

			if (!ShouldFlushDisposeRetryChanges())
				return;

			if (_disposeRetryChanges.IsEmpty)
				return;

			FileChangeBatch batch = _disposeRetryChanges.DrainBatch();

			if (batch.Count == 0)
				return;

			bool finalFlushTimedOut = false;

			using var finalFlushTimeout = new CancellationTokenSource(DisposeFinalFlushTimeout);

			try
			{
				await _dispatchAsync(batch, finalFlushTimeout.Token).ConfigureAwait(false);
				finalFlushTimedOut = finalFlushTimeout.IsCancellationRequested;
			}
			catch (OperationCanceledException) when (finalFlushTimeout.IsCancellationRequested)
			{
				finalFlushTimedOut = true;
			}

			if (finalFlushTimedOut)
			{
				_logger.LogWarning("Workspace file watcher final dispose flush exceeded {Timeout} for '{Workspace}'; cancellation was requested and disposal waited for the callback to unwind.",
					DisposeFinalFlushTimeout,
					_workspaceRootDirectoryPath);
			}
		}
		catch (Exception exception)
		{
			_logger.LogDebug(exception,
				"Workspace file watcher final dispose flush failed for '{Workspace}'.",
				_workspaceRootDirectoryPath);
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

			_pendingChanges.Dispose();

			try
			{
				_lifetimeCts.Cancel();
			}
			catch (ObjectDisposedException)
			{ }

			DisposeDispatchResources();
			SignalDisposeCompleted();
		}
	}

	/// <summary>
	/// Starts watcher disposal and records whether dispose-time retry changes should still be flushed.
	/// </summary>
	/// <param name="flushDisposeRetryChanges">Whether disposal should attempt one final flush of buffered retry changes.</param>
	/// <returns><see langword="true"/> when this call started disposal; otherwise, <see langword="false"/>.</returns>
	private bool BeginDispose(bool flushDisposeRetryChanges)
	{
		if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
			return false;

		_isDisposed = true;

		lock (_dispatchLifecycleSyncRoot)
		{
			_flushDisposeRetryChanges = flushDisposeRetryChanges;
			_disposeCompletionSource ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		}

		StopWatching();
		_pendingChanges.Stop();

		return true;
	}

	/// <summary>
	/// Reports whether disposal should still flush buffered retry changes.
	/// </summary>
	/// <returns><see langword="true"/> when disposal should attempt a final retry flush.</returns>
	private bool ShouldFlushDisposeRetryChanges()
	{
		lock (_dispatchLifecycleSyncRoot)
			return _flushDisposeRetryChanges;
	}

	/// <summary>
	/// Attempts to start asynchronous disposal finalization once disposal is active and all in-flight dispatch operations have quiesced.
	/// </summary>
	private void TryStartDisposeFinalization()
	{
		lock (_dispatchLifecycleSyncRoot)
		{
			if (!_isDisposed || _disposeFinalizationStarted || _activeDispatchCount != 0)
				return;

			_disposeFinalizationStarted = true;
			Task.Run(FinalizeDisposeAsync);
		}

		EnsureDisposeCompletionTask();
	}

	/// <summary>
	/// Ensures a dispose-completion task exists for sync and async disposal waiters.
	/// </summary>
	private void EnsureDisposeCompletionTask()
	{
		lock (_dispatchLifecycleSyncRoot)
			_disposeCompletionSource ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
	}

	/// <summary>
	/// Gets the task that completes when watcher disposal finalization has finished.
	/// </summary>
	/// <returns>The disposal completion task, or <see cref="Task.CompletedTask"/> when disposal already finished.</returns>
	private Task GetDisposeCompletionTask()
	{
		lock (_dispatchLifecycleSyncRoot)
			return _disposeCompletionSource?.Task ?? Task.CompletedTask;
	}

	/// <summary>
	/// Completes any sync or async waiters blocked on watcher disposal finalization.
	/// </summary>
	private void SignalDisposeCompleted()
	{
		TaskCompletionSource<bool>? disposeCompletionSource;

		lock (_dispatchLifecycleSyncRoot)
			disposeCompletionSource = _disposeCompletionSource;

		disposeCompletionSource?.TrySetResult(true);
	}

	/// <summary>
	/// Disposes the dispatch cancellation token source and gate once no dispatch is active.
	/// </summary>
	private void DisposeDispatchResources()
	{
		if (Interlocked.Exchange(ref _dispatchResourcesDisposed, 1) != 0)
			return;

		_lifetimeCts.Dispose();
		_dispatchGate.Dispose();
	}

	/// <summary>
	/// Disposes a watcher-owned resource while logging failures.
	/// </summary>
	/// <param name="disposable">The resource to dispose.</param>
	/// <param name="resourceName">The resource name used for diagnostics.</param>
	private void TryDispose(IDisposable disposable, string resourceName)
	{
		try
		{
			disposable.Dispose();
		}
		catch (Exception exception)
		{
			_logger.LogDebug(exception, "Failed to dispose workspace watcher resource '{ResourceName}'.", resourceName);
		}
	}

	[Obsolete("For testing purposes only.")]
	internal Task WaitForDisposeCompletionForTestAsync() => GetDisposeCompletionTask();

	[Obsolete("For testing purposes only.")]
	internal void ReportErrorForTest(Exception? exception) => HandleWatcherError(exception);
}
