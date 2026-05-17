using NLog;

namespace TombLib.LanguageServer.Core;

/// <summary>
/// Watches the workspace root for configured file patterns and forwards coalesced changes to the owner.
/// A startup failure disposes the instance, so recovery requires creating a replacement watcher.
/// </summary>
public sealed class WorkspaceFileWatcher : IDisposable, IAsyncDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private static readonly TimeSpan DispatchDebounce = TimeSpan.FromMilliseconds(250);
	private static readonly TimeSpan MaxDispatchRetryDelay = TimeSpan.FromSeconds(5.0f);
	private const int DispatchFailureWarningThreshold = 3;
	private readonly string _workspaceRootDirectoryPath;
	private readonly IReadOnlyList<WorkspaceWatchSpecification> _watchSpecifications;
	private readonly Func<string, WorkspaceWatchSpecification, FileSystemWatcher> _fileSystemWatcherFactory;
	private readonly Func<FileChangeBatch, CancellationToken, Task> _dispatchAsync;
	private readonly Action<WorkspaceFileWatcher, Exception?>? _watcherFailed;
	private readonly WorkspaceChangeDebouncer _pendingChanges;
	private readonly WorkspaceChangeAccumulator _disposeRetryChanges = new();
	private readonly SemaphoreSlim _dispatchGate = new(1, 1);
	private readonly CancellationTokenSource _lifetimeCts = new();
	private readonly List<FileSystemWatcher> _watchers = [];
	private readonly object _watchersSyncRoot = new();
	private readonly object _dispatchLifecycleSyncRoot = new();
	private volatile bool _isDisposed;
	private int _disposeStarted;
	private TaskCompletionSource<bool>? _disposeCompletionSource;
	private bool _flushDisposeRetryChanges = true;
	private int _activeDispatchCount;
	private bool _disposeFinalizationStarted;
	private int _dispatchResourcesDisposed;
	private int _watcherFailureReported;
	private int _consecutiveDispatchFailures;

	/// <summary>
	/// Gets a value indicating whether any file-system watchers are currently active.
	/// </summary>
	internal bool HasActiveWatchers
	{
		get
		{
			lock (_watchersSyncRoot)
				return _watchers.Count > 0;
		}
	}

	/// <summary>
	/// Gets the number of active file-system watchers.
	/// </summary>
	internal int ActiveWatcherCount
	{
		get
		{
			lock (_watchersSyncRoot)
				return _watchers.Count;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the watcher has been disposed.
	/// </summary>
	internal bool IsDisposed => _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceFileWatcher"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The workspace root directory to watch.</param>
	/// <param name="dispatchAsync">The callback that forwards coalesced changes to the owner.</param>
	/// <param name="watchSpecifications">The explicit file patterns that should be watched under the workspace root.</param>
	/// <param name="watcherFailed">The callback that reports an unrecoverable watcher failure to the owner.</param>
	/// <param name="fileSystemWatcherFactory">Creates one file-system watcher for a watch specification.</param>
	public WorkspaceFileWatcher(
		string workspaceRootDirectoryPath,
		Func<FileChangeBatch, CancellationToken, Task> dispatchAsync,
		IReadOnlyList<WorkspaceWatchSpecification> watchSpecifications,
		Action<WorkspaceFileWatcher, Exception?>? watcherFailed = null,
		Func<string, WorkspaceWatchSpecification, FileSystemWatcher>? fileSystemWatcherFactory = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRootDirectoryPath);
		ArgumentNullException.ThrowIfNull(dispatchAsync);
		ArgumentNullException.ThrowIfNull(watchSpecifications);

		if (watchSpecifications.Count == 0)
			throw new ArgumentException("At least one watch specification is required.", nameof(watchSpecifications));

		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_dispatchAsync = dispatchAsync;
		_watchSpecifications = watchSpecifications;
		_watcherFailed = watcherFailed;
		_fileSystemWatcherFactory = fileSystemWatcherFactory ?? CreateFileSystemWatcher;

		_pendingChanges = new WorkspaceChangeDebouncer(DispatchDebounce, () => _ = DispatchPendingChangesAsync());
	}

	/// <summary>
	/// Starts watching the configured workspace for external file changes.
	/// </summary>
	/// <returns><see langword="true"/> when the watcher is running; otherwise, <see langword="false"/>.</returns>
	public bool Start()
		=> Start(out _) is WorkspaceWatcherStartStatus.Started or WorkspaceWatcherStartStatus.AlreadyRunning;

	/// <summary>
	/// Starts watching the configured workspace for external file changes and reports why startup failed.
	/// A startup failure disposes this watcher instance, so later retries should use a replacement watcher.
	/// </summary>
	/// <param name="startupException">Receives the startup exception when watcher creation failed.</param>
	/// <returns>The watcher startup status.</returns>
	public WorkspaceWatcherStartStatus Start(out Exception? startupException)
	{
		startupException = null;

		if (_isDisposed)
			return WorkspaceWatcherStartStatus.Disposed;

		try
		{
			if (!Directory.Exists(_workspaceRootDirectoryPath))
			{
				Log.Debug("Workspace file watcher start skipped because '{Workspace}' does not exist.", _workspaceRootDirectoryPath);
				return WorkspaceWatcherStartStatus.WorkspaceRootMissing;
			}
		}
		catch (Exception exception)
		{
			startupException = exception;
			Log.Debug(exception, "Failed to validate the workspace root for '{Workspace}' before starting the workspace watcher.", _workspaceRootDirectoryPath);
			Dispose();

			return WorkspaceWatcherStartStatus.StartupFailed;
		}

		try
		{
			lock (_watchersSyncRoot)
			{
				if (_isDisposed)
					return WorkspaceWatcherStartStatus.Disposed;

				if (_watchers.Count > 0)
					return WorkspaceWatcherStartStatus.AlreadyRunning;

				for (int i = 0; i < _watchSpecifications.Count; i++)
					_watchers.Add(CreateWatcher(_watchSpecifications[i]));

				Interlocked.Exchange(ref _watcherFailureReported, 0);

				Log.Debug("Started workspace file watcher for '{Workspace}' with {Count} watcher(s).",
					_workspaceRootDirectoryPath,
					_watchers.Count);

				return WorkspaceWatcherStartStatus.Started;
			}
		}
		catch (Exception exception)
		{
			startupException = exception;
			Log.Debug(exception, "Failed to start the workspace file watcher for '{Workspace}'.", _workspaceRootDirectoryPath);
			Dispose();

			return WorkspaceWatcherStartStatus.StartupFailed;
		}
	}

	/// <summary>
	/// Creates and configures a file-system watcher for one specification.
	/// </summary>
	/// <param name="specification">The watch specification to apply.</param>
	/// <returns>The configured file-system watcher.</returns>
	private FileSystemWatcher CreateWatcher(WorkspaceWatchSpecification specification)
	{
		FileSystemWatcher watcher = _fileSystemWatcherFactory(_workspaceRootDirectoryPath, specification);
		watcher.IncludeSubdirectories = specification.IncludeSubdirectories;
		watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName;
		watcher.InternalBufferSize = 64 * 1024;

		watcher.Created += (_, e) => QueueChange(e.FullPath, FileChangeKind.Created);
		watcher.Changed += (_, e) => QueueChange(e.FullPath, FileChangeKind.Changed);
		watcher.Deleted += (_, e) => QueueChange(e.FullPath, FileChangeKind.Deleted);
		watcher.Renamed += (_, e) =>
		{
			QueueChange(e.OldFullPath, FileChangeKind.Deleted);
			QueueChange(e.FullPath, FileChangeKind.Created);
		};

		watcher.Error += (_, e) => HandleWatcherError(e.GetException());
		watcher.EnableRaisingEvents = true;

		return watcher;
	}

	private static FileSystemWatcher CreateFileSystemWatcher(string workspaceRootDirectoryPath, WorkspaceWatchSpecification specification)
		=> new(workspaceRootDirectoryPath, specification.Filter);

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
					retryDelay = GetDispatchRetryDelay(consecutiveDispatchFailures);
					_pendingChanges.Requeue(batch, retryDelay);
				}
			}

			if (consecutiveDispatchFailures >= DispatchFailureWarningThreshold)
			{
				Log.Warn(exception,
					"Workspace file watcher dispatch failed for '{Workspace}' with {Count} queued change(s) {FailureCount} times in a row; retrying in {RetryDelayMs} ms with backoff.",
					_workspaceRootDirectoryPath,
					batch?.Count ?? 0,
					consecutiveDispatchFailures,
					(int)retryDelay.TotalMilliseconds);
			}
			else
			{
				Log.Debug(exception,
					"Workspace file watcher dispatch failed for '{Workspace}' with {Count} queued change(s); retrying in {RetryDelayMs} ms.",
					_workspaceRootDirectoryPath,
					batch?.Count ?? 0,
					(int)retryDelay.TotalMilliseconds);
			}
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

	/// <summary>
	/// Stops all active watchers and reports the failure to the owner once.
	/// </summary>
	/// <param name="exception">The watcher error, if one was provided.</param>
	private void HandleWatcherError(Exception? exception)
	{
		if (_isDisposed)
			return;

		StopWatching();

		if (_isDisposed)
			return;

		if (!_pendingChanges.IsEmpty)
			_ = DispatchPendingChangesAsync();

		if (Interlocked.Exchange(ref _watcherFailureReported, 1) != 0)
			return;

		Log.Warn(exception,
			"Workspace file watcher encountered an internal error for '{Workspace}' and stopped watching until the owner handles recovery.",
			_workspaceRootDirectoryPath);

		try
		{
			_watcherFailed?.Invoke(this, exception);
		}
		catch (Exception callbackException)
		{
			Log.Warn(callbackException, "Workspace watcher failure handler threw.");
		}
	}

	/// <summary>
	/// Stops and disposes all active file-system watchers.
	/// </summary>
	private void StopWatching()
	{
		List<FileSystemWatcher> watchersToDispose;

		lock (_watchersSyncRoot)
		{
			if (_watchers.Count == 0)
				return;

			watchersToDispose = [.. _watchers];
			_watchers.Clear();
		}

		for (int i = watchersToDispose.Count - 1; i >= 0; i--)
		{
			FileSystemWatcher watcher = watchersToDispose[i];
			TryDispose(watcher, nameof(FileSystemWatcher));
		}
	}

	/// <summary>
	/// Releases all native file-system watchers and pending dispatch resources.
	/// </summary>
	public void Dispose()
	{
		if (!BeginDispose(flushDisposeRetryChanges: true))
			return;

		_ = TryStartDisposeFinalization();
		GetDisposeCompletionTask().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Releases all native file-system watchers and pending dispatch resources asynchronously.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (BeginDispose(flushDisposeRetryChanges: true))
			_ = TryStartDisposeFinalization();

		await GetDisposeCompletionTask().ConfigureAwait(false);
	}

	/// <summary>
	/// Releases all watcher resources while intentionally dropping any dispose-time retry batch.
	/// </summary>
	public void DisposeWithoutFinalFlush()
	{
		if (!BeginDispose(flushDisposeRetryChanges: false))
			return;

		_ = TryStartDisposeFinalization();
		GetDisposeCompletionTask().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Attempts one final asynchronous dispatch for buffered changes during disposal after all in-flight dispatches have quiesced.
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

			await _dispatchAsync(batch, CancellationToken.None).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			Log.Debug(exception,
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

			try { _lifetimeCts.Cancel(); } catch (ObjectDisposedException) { }

			DisposeDispatchResources();
			SignalDisposeCompleted();
		}
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
			StoreDisposeFinalizationTask(disposeFinalizationTask);
	}

	private bool CanRequeuePendingChanges()
	{
		lock (_dispatchLifecycleSyncRoot)
		{
			return !_disposeFinalizationStarted;
		}
	}

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

	private bool ShouldFlushDisposeRetryChanges()
	{
		lock (_dispatchLifecycleSyncRoot)
		{
			return _flushDisposeRetryChanges;
		}
	}

	private void BufferDisposeRetryChanges(FileChangeBatch batch)
	{
		for (int i = 0; i < batch.Count; i++)
			_disposeRetryChanges.Add(batch.Entries[i].Path, batch.Entries[i].Kind);
	}

	private Task? TryStartDisposeFinalization()
	{
		Task? disposeFinalizationTask = null;

		lock (_dispatchLifecycleSyncRoot)
		{
			if (!_isDisposed || _disposeFinalizationStarted || _activeDispatchCount != 0)
				return null;

			_disposeFinalizationStarted = true;
			disposeFinalizationTask = Task.Run(FinalizeDisposeAsync);
		}

		StoreDisposeFinalizationTask(disposeFinalizationTask);
		return disposeFinalizationTask;
	}

	private void StoreDisposeFinalizationTask(Task disposeFinalizationTask)
	{
		_ = disposeFinalizationTask;

		lock (_dispatchLifecycleSyncRoot)
		{
			_disposeCompletionSource ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		}
	}

	private Task GetDisposeCompletionTask()
	{
		lock (_dispatchLifecycleSyncRoot)
		{
			return _disposeCompletionSource?.Task ?? Task.CompletedTask;
		}
	}

	private void SignalDisposeCompleted()
	{
		TaskCompletionSource<bool>? disposeCompletionSource;

		lock (_dispatchLifecycleSyncRoot)
		{
			disposeCompletionSource = _disposeCompletionSource;
		}

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
	private static void TryDispose(IDisposable disposable, string resourceName)
	{
		try
		{
			disposable.Dispose();
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to dispose workspace watcher resource '{ResourceName}'.", resourceName);
		}
	}

	#region Test Hooks

	[Obsolete("For testing purposes only.")]
	internal void QueueChangeForTest(string filePath, FileChangeKind kind) => QueueChange(filePath, kind);

	[Obsolete("For testing purposes only.")]
	internal Task DispatchPendingChangesForTestAsync() => DispatchPendingChangesAsync();

	[Obsolete("For testing purposes only.")]
	internal Task WaitForDisposeCompletionForTestAsync() => GetDisposeCompletionTask();

	[Obsolete("For testing purposes only.")]
	internal void ReportErrorForTest(Exception? exception) => HandleWatcherError(exception);

	#endregion Test Hooks
}
