using NLog;

namespace TombLib.LanguageServer.Lua;

internal sealed class LuaWorkspaceChangeCoordinator : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private enum WorkspaceWatcherRecoveryResult
	{
		Recovered,
		Unavailable,
		Failed
	}

	private readonly string _workspaceApiDirectoryPath;
	private readonly string _workspaceRootDirectoryPath;
	private readonly IReadOnlyList<WorkspaceWatchSpecification> _watchSpecifications;
	private readonly Func<ILanguageServerClient?> _clientAccessor;
	private readonly Func<bool> _isDisposedAccessor;
	private readonly Func<CancellationToken, Task<bool>> _ensureStartedAsync;
	private readonly Action _markTransportUnavailable;
	private readonly Action<WorkspaceWatcherFailure> _raiseWorkspaceWatcherFailed;
	private readonly Func<string, Func<FileChangeBatch, CancellationToken, Task>, Action<WorkspaceFileWatcher, Exception?>, WorkspaceFileWatcher> _workspaceFileWatcherFactory;
	private readonly WorkspaceFileChangeForwarder _workspaceFileChangeForwarder;
	private readonly object _watcherSyncRoot = new();

	private WorkspaceFileWatcher? _workspaceFileWatcher;
	private Dictionary<string, WorkspaceSnapshotEntry> _workspaceSnapshot = new(StringComparer.OrdinalIgnoreCase);
	private int _workspaceWatcherFailureReported;

	private readonly record struct WorkspaceSnapshotEntry(bool IsDirectory, long LastWriteUtcTicks, long Length);

	public LuaWorkspaceChangeCoordinator(
		string workspaceRootDirectoryPath,
		IReadOnlyList<WorkspaceWatchSpecification> watchSpecifications,
		Func<string, Func<FileChangeBatch, CancellationToken, Task>, Action<WorkspaceFileWatcher, Exception?>, WorkspaceFileWatcher> workspaceFileWatcherFactory,
		Func<ILanguageServerClient?> clientAccessor,
		Func<bool> isDisposedAccessor,
		Func<CancellationToken, Task<bool>> ensureStartedAsync,
		Action markTransportUnavailable,
		Action<WorkspaceWatcherFailure> raiseWorkspaceWatcherFailed)
	{
		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_workspaceApiDirectoryPath = Path.Combine(workspaceRootDirectoryPath, ".API");
		_watchSpecifications = watchSpecifications;
		_workspaceFileWatcherFactory = workspaceFileWatcherFactory;
		_clientAccessor = clientAccessor;
		_isDisposedAccessor = isDisposedAccessor;
		_ensureStartedAsync = ensureStartedAsync;
		_markTransportUnavailable = markTransportUnavailable;
		_raiseWorkspaceWatcherFailed = raiseWorkspaceWatcherFailed;
		_workspaceFileChangeForwarder = new WorkspaceFileChangeForwarder(
			// Buffering is reserved for temporary startup or transport failures after a client exists.
			// When there is no client or the provider is disposed, workspace changes are intentionally ignored.
			() => _clientAccessor() is not null && !_isDisposedAccessor(),
			_isDisposedAccessor,
			_ensureStartedAsync,
			_markTransportUnavailable,
			exception => Log.Debug(exception,
				"Failed to forward workspace file changes to the Lua language server; the changes were buffered for replay."));
	}

	internal WorkspaceFileWatcher? CurrentWatcher
	{
		get
		{
			lock (_watcherSyncRoot)
				return _workspaceFileWatcher;
		}
	}

	public void EnsureWorkspaceFileWatcherStarted()
	{
		WorkspaceFileWatcher? watcherToDispose = null;
		Exception? startupException = null;
		bool shouldReportFailure = false;

		lock (_watcherSyncRoot)
		{
			if (_workspaceFileWatcher is not null || _clientAccessor() is null || string.IsNullOrEmpty(_workspaceRootDirectoryPath) || _isDisposedAccessor())
				return;

			WorkspaceFileWatcher watcher = _workspaceFileWatcherFactory(
				_workspaceRootDirectoryPath,
				DispatchWorkspaceFileChangesAsync,
				HandleWorkspaceWatcherFailed);

			WorkspaceWatcherStartStatus startStatus = watcher.Start(out startupException);

			if (startStatus is not (WorkspaceWatcherStartStatus.Started or WorkspaceWatcherStartStatus.AlreadyRunning))
			{
				watcherToDispose = watcher;
				shouldReportFailure = startStatus == WorkspaceWatcherStartStatus.StartupFailed;
			}
			else
			{
				_workspaceFileWatcher = watcher;
				_workspaceSnapshot = CaptureWorkspaceSnapshot();
				Interlocked.Exchange(ref _workspaceWatcherFailureReported, 0);
			}
		}

		DisposeWatcher(watcherToDispose, "Failed to dispose an unstarted Lua workspace file watcher.");

		if (shouldReportFailure)
			ReportWorkspaceWatcherStartupFailure(startupException);
	}

	public async Task DispatchWorkspaceFileChangesAsync(FileChangeBatch batch, CancellationToken cancellationToken)
	{
		if (_clientAccessor() is null || _isDisposedAccessor() || batch.Count == 0)
			return;

		var changes = new List<WorkspaceFileChange>(batch.Count);

		foreach ((string path, FileChangeKind kind) in batch.Entries)
		{
			if (!LanguageServerPathHelper.TryNormalizeLocalPath(path, out string normalizedPath))
				continue;

			changes.Add(new WorkspaceFileChange(normalizedPath, kind));
		}

		if (changes.Count == 0)
			return;

		ApplyWorkspaceSnapshotChanges(changes);

		await _workspaceFileChangeForwarder.DispatchAsync(changes, SendWorkspaceFileChangesAsync, cancellationToken).ConfigureAwait(false);
	}

	public async Task ReplayDeferredWorkspaceFileChangesAsync(CancellationToken cancellationToken)
	{
		if (_clientAccessor() is null || _isDisposedAccessor())
			return;

		await _workspaceFileChangeForwarder.ReplayDeferredAsync(SendWorkspaceFileChangesAsync, cancellationToken).ConfigureAwait(false);
	}

	public void Dispose()
	{
		WorkspaceFileWatcher? watcher;

		lock (_watcherSyncRoot)
		{
			watcher = _workspaceFileWatcher;
			_workspaceFileWatcher = null;
		}

		DisposeWatcher(watcher, "Failed to dispose the Lua workspace file watcher.");
		_workspaceFileChangeForwarder.Dispose();
	}

	private async Task SendWorkspaceFileChangesAsync(IReadOnlyList<WorkspaceFileChange> changes, CancellationToken cancellationToken)
	{
		ILanguageServerClient? client = _clientAccessor();

		if (client is null || _isDisposedAccessor() || changes.Count == 0)
			return;

		bool shouldRefreshConfiguration = false;
		var payloads = new List<FileEventPayload>(changes.Count);

		for (int i = 0; i < changes.Count; i++)
		{
			WorkspaceFileChange change = changes[i];
			shouldRefreshConfiguration |= IsWorkspaceConfigurationPath(change.Path);
			payloads.Add(new FileEventPayload(LanguageServerPathHelper.CreateFileUri(change.Path), (int)change.Kind));
		}

		if (shouldRefreshConfiguration)
		{
			await client.SendNotificationAsync("workspace/didChangeConfiguration",
				new DidChangeConfigurationParams(LuaLanguageServerSettingsFactory.Create(_workspaceRootDirectoryPath)),
				cancellationToken).ConfigureAwait(false);
		}

		await client.SendNotificationAsync("workspace/didChangeWatchedFiles",
			new DidChangeWatchedFilesParams([.. payloads]), cancellationToken).ConfigureAwait(false);
	}

	private bool IsWorkspaceConfigurationPath(string normalizedPath)
	{
		if (string.Equals(normalizedPath, _workspaceApiDirectoryPath, StringComparison.OrdinalIgnoreCase))
			return true;

		string apiDirectoryPrefix = _workspaceApiDirectoryPath + Path.DirectorySeparatorChar;

		if (normalizedPath.StartsWith(apiDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
			return true;

		string fileName = Path.GetFileName(normalizedPath);

		return string.Equals(fileName, ".luarc.json", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(fileName, ".luarc.jsonc", StringComparison.OrdinalIgnoreCase);
	}

	private void HandleWorkspaceWatcherFailed(WorkspaceFileWatcher watcher, Exception? exception)
	{
		if (_isDisposedAccessor())
			return;

		Log.Warn(exception,
			"Lua workspace watching failed for '{Workspace}'. Attempting to restart the watcher automatically.",
			_workspaceRootDirectoryPath);

		WorkspaceWatcherRecoveryResult recoveryResult = RecoverWorkspaceFileWatcher(watcher);

		if (recoveryResult is WorkspaceWatcherRecoveryResult.Recovered or WorkspaceWatcherRecoveryResult.Unavailable)
			return;

		if (Interlocked.Exchange(ref _workspaceWatcherFailureReported, 1) != 0)
			return;

		_raiseWorkspaceWatcherFailed(new WorkspaceWatcherFailure(
			"The Lua workspace file watcher encountered an internal error and automatic recovery failed.\n\n" +
			"Lua IntelliSense will continue to work for files edited in the editor, but external workspace changes - such as Git pull updates, generated .API files, or .luarc changes - may not be forwarded until the watcher can be restarted."));
	}

	private bool TryRestartWorkspaceFileWatcher(WorkspaceFileWatcher failedWatcher)
		=> RecoverWorkspaceFileWatcher(failedWatcher) == WorkspaceWatcherRecoveryResult.Recovered;

	private WorkspaceWatcherRecoveryResult RecoverWorkspaceFileWatcher(WorkspaceFileWatcher failedWatcher)
	{
		bool watcherRecovered = false;
		bool replacementWatcherStarted = false;
		bool startupFailed = false;
		bool workspaceUnavailable = false;
		Exception? startupException = null;
		WorkspaceFileWatcher? failedWatcherToDispose = failedWatcher;
		WorkspaceFileWatcher? replacementWatcherToDispose = null;
		Dictionary<string, WorkspaceSnapshotEntry>? previousSnapshot = null;
		Dictionary<string, WorkspaceSnapshotEntry>? currentSnapshot = null;

		lock (_watcherSyncRoot)
		{
			if (!ReferenceEquals(_workspaceFileWatcher, failedWatcher))
			{
				watcherRecovered = _workspaceFileWatcher is not null;
			}
			else
			{
				previousSnapshot = CloneWorkspaceSnapshot(_workspaceSnapshot);
				_workspaceFileWatcher = null;
			}

			if (!watcherRecovered && !_isDisposedAccessor() && _clientAccessor() is not null && !string.IsNullOrEmpty(_workspaceRootDirectoryPath))
			{
				WorkspaceFileWatcher replacementWatcher = _workspaceFileWatcherFactory(
					_workspaceRootDirectoryPath,
					DispatchWorkspaceFileChangesAsync,
					HandleWorkspaceWatcherFailed);

				WorkspaceWatcherStartStatus startStatus = replacementWatcher.Start(out startupException);

				if (startStatus is WorkspaceWatcherStartStatus.Started or WorkspaceWatcherStartStatus.AlreadyRunning)
				{
					_workspaceFileWatcher = replacementWatcher;
					currentSnapshot = CaptureWorkspaceSnapshot();
					_workspaceSnapshot = currentSnapshot;
					watcherRecovered = true;
					replacementWatcherStarted = true;
					Interlocked.Exchange(ref _workspaceWatcherFailureReported, 0);
				}
				else
				{
					startupFailed = startStatus == WorkspaceWatcherStartStatus.StartupFailed;
					workspaceUnavailable = startStatus == WorkspaceWatcherStartStatus.WorkspaceRootMissing;
					replacementWatcherToDispose = replacementWatcher;
				}
			}
		}

		DisposeWatcher(failedWatcherToDispose, "Failed to dispose a Lua workspace file watcher while recovering from a watcher error.", flushPendingChanges: false);
		DisposeWatcher(replacementWatcherToDispose, "Failed to dispose a Lua workspace file watcher while recovering from a watcher error.", flushPendingChanges: false);

		if (watcherRecovered)
		{
			Log.Info("Lua workspace watching recovered successfully for '{Workspace}'.", _workspaceRootDirectoryPath);

			if (replacementWatcherStarted && previousSnapshot is not null && currentSnapshot is not null)
				ObserveBackgroundTask(ReconcileWorkspaceSnapshotAsync(previousSnapshot, currentSnapshot), "Lua workspace watcher recovery reconciliation");

			return WorkspaceWatcherRecoveryResult.Recovered;
		}

		if (workspaceUnavailable)
		{
			Log.Info(
				"Lua workspace watching remains unavailable for '{Workspace}' because the workspace path does not exist.",
				_workspaceRootDirectoryPath);

			return WorkspaceWatcherRecoveryResult.Unavailable;
		}

		if (startupFailed)
		{
			Log.Warn(startupException,
				"Lua workspace watching could not be restarted for '{Workspace}' because watcher startup failed.",
				_workspaceRootDirectoryPath);
		}

		return WorkspaceWatcherRecoveryResult.Failed;
	}

	private void ReportWorkspaceWatcherStartupFailure(Exception? exception)
	{
		if (Interlocked.Exchange(ref _workspaceWatcherFailureReported, 1) != 0)
			return;

		Log.Warn(exception,
			"Lua workspace watching could not start for '{Workspace}'. External workspace changes will not be forwarded until the watcher can be started successfully.",
			_workspaceRootDirectoryPath);

		_raiseWorkspaceWatcherFailed(new WorkspaceWatcherFailure(
			"The Lua workspace file watcher could not be started for the current workspace.\n\n" +
			"Lua IntelliSense will continue to work for files edited in the editor, but external workspace changes - such as Git pull updates, generated .API files, or .luarc changes - will not be forwarded until the watcher can be started successfully."));
	}

	private async Task ReconcileWorkspaceSnapshotAsync(
		Dictionary<string, WorkspaceSnapshotEntry> previousSnapshot,
		Dictionary<string, WorkspaceSnapshotEntry> currentSnapshot)
	{
		if (_isDisposedAccessor())
			return;

		FileChangeBatch batch = BuildWorkspaceSnapshotDeltaBatch(previousSnapshot, currentSnapshot);

		if (batch.Count == 0)
			return;

		Log.Info(
			"Replaying {Count} reconciled workspace file change(s) after Lua workspace watcher recovery for '{Workspace}'.",
			batch.Count,
			_workspaceRootDirectoryPath);

		await DispatchWorkspaceFileChangesAsync(batch, CancellationToken.None).ConfigureAwait(false);
	}

	private void ApplyWorkspaceSnapshotChanges(IReadOnlyList<WorkspaceFileChange> changes)
	{
		lock (_watcherSyncRoot)
		{
			for (int i = 0; i < changes.Count; i++)
			{
				WorkspaceFileChange change = changes[i];

				if (change.Kind == FileChangeKind.Deleted)
				{
					_workspaceSnapshot.Remove(change.Path);
					continue;
				}

				if (TryCreateWorkspaceSnapshotEntry(change.Path, out WorkspaceSnapshotEntry entry))
					_workspaceSnapshot[change.Path] = entry;
				else
					_workspaceSnapshot.Remove(change.Path);
			}
		}
	}

	private Dictionary<string, WorkspaceSnapshotEntry> CaptureWorkspaceSnapshot()
	{
		var snapshot = new Dictionary<string, WorkspaceSnapshotEntry>(StringComparer.OrdinalIgnoreCase);

		if (!Directory.Exists(_workspaceRootDirectoryPath))
			return snapshot;

		for (int i = 0; i < _watchSpecifications.Count; i++)
			CaptureWorkspaceSnapshotForSpecification(snapshot, _watchSpecifications[i]);

		return snapshot;
	}

	private void CaptureWorkspaceSnapshotForSpecification(
		Dictionary<string, WorkspaceSnapshotEntry> snapshot,
		WorkspaceWatchSpecification watchSpecification)
	{
		if (watchSpecification.Filter.IndexOfAny(['*', '?']) < 0)
		{
			TryAddWorkspaceSnapshotPath(snapshot, Path.Combine(_workspaceRootDirectoryPath, watchSpecification.Filter));
			return;
		}

		var enumerationOptions = new EnumerationOptions
		{
			IgnoreInaccessible = true,
			RecurseSubdirectories = watchSpecification.IncludeSubdirectories,
			ReturnSpecialDirectories = false
		};

		foreach (string filePath in Directory.EnumerateFiles(_workspaceRootDirectoryPath, watchSpecification.Filter, enumerationOptions))
			TryAddWorkspaceSnapshotPath(snapshot, filePath);
	}

	private void TryAddWorkspaceSnapshotPath(Dictionary<string, WorkspaceSnapshotEntry> snapshot, string path)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(path, out string normalizedPath))
			return;

		if (TryCreateWorkspaceSnapshotEntry(normalizedPath, out WorkspaceSnapshotEntry entry))
			snapshot[normalizedPath] = entry;
	}

	private static Dictionary<string, WorkspaceSnapshotEntry> CloneWorkspaceSnapshot(Dictionary<string, WorkspaceSnapshotEntry> snapshot)
		=> new(snapshot, StringComparer.OrdinalIgnoreCase);

	private static bool TryCreateWorkspaceSnapshotEntry(string normalizedPath, out WorkspaceSnapshotEntry entry)
	{
		entry = default;

		try
		{
			if (File.Exists(normalizedPath))
			{
				var fileInfo = new FileInfo(normalizedPath);
				entry = new WorkspaceSnapshotEntry(IsDirectory: false, fileInfo.LastWriteTimeUtc.Ticks, fileInfo.Length);
				return true;
			}

			if (Directory.Exists(normalizedPath))
			{
				var directoryInfo = new DirectoryInfo(normalizedPath);
				entry = new WorkspaceSnapshotEntry(IsDirectory: true, directoryInfo.LastWriteTimeUtc.Ticks, 0);
				return true;
			}
		}
		catch (Exception)
		{ }

		return false;
	}

	private static FileChangeBatch BuildWorkspaceSnapshotDeltaBatch(
		Dictionary<string, WorkspaceSnapshotEntry> previousSnapshot,
		Dictionary<string, WorkspaceSnapshotEntry> currentSnapshot)
	{
		var changes = new List<WorkspaceFileChange>();

		foreach ((string path, WorkspaceSnapshotEntry previousEntry) in previousSnapshot)
		{
			if (!currentSnapshot.TryGetValue(path, out WorkspaceSnapshotEntry currentEntry))
			{
				changes.Add(new WorkspaceFileChange(path, FileChangeKind.Deleted));
				continue;
			}

			if (!currentEntry.Equals(previousEntry))
				changes.Add(new WorkspaceFileChange(path, FileChangeKind.Changed));
		}

		foreach ((string path, WorkspaceSnapshotEntry _) in currentSnapshot)
		{
			if (!previousSnapshot.ContainsKey(path))
				changes.Add(new WorkspaceFileChange(path, FileChangeKind.Created));
		}

		changes.Sort(static (left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Path, right.Path));
		return new FileChangeBatch(changes);
	}

	private static void ObserveBackgroundTask(Task task, string operationName)
	{
		_ = task.ContinueWith(static (completedTask, state) =>
			{
				if (completedTask.Exception is not { } exception)
					return;

				Log.Warn(exception.Flatten(), "{OperationName} failed.", state);
			},
			operationName,
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
			TaskScheduler.Default);
	}

	private static void DisposeWatcher(WorkspaceFileWatcher? watcher, string message, bool flushPendingChanges = true)
	{
		if (watcher is null)
			return;

		try
		{
			if (flushPendingChanges)
				watcher.Dispose();
			else
				watcher.DisposeWithoutFinalFlush();
		}
		catch (Exception exception)
		{
			Log.Debug(exception, message);
		}
	}
}
