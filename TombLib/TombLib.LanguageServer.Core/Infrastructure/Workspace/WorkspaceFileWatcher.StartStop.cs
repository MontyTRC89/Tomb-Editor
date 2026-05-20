namespace TombLib.LanguageServer.Core;

public sealed partial class WorkspaceFileWatcher
{
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

		if (_isDisposed)
			return;

		if (!_pendingChanges.IsEmpty)
			_ = DispatchPendingChangesAsync();
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
}
