namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Owns the Lua workspace watcher lifecycle and forwards external workspace changes to the language server.
/// Workspace snapshot capture and reconciliation are delegated to a dedicated tracker helper.
/// </summary>
internal sealed class LuaWorkspaceChangeCoordinator : IDisposable
{
	private readonly ILogger _logger;

	private enum WorkspaceWatcherRecoveryResult
	{
		Recovered,
		Unavailable,
		Failed
	}

	private readonly string _workspaceApiDirectoryPath;
	private readonly string _workspaceRootDirectoryPath;

	private readonly Func<ILanguageServerClient?> _clientAccessor;
	private readonly Func<bool> _isDisposedAccessor;
	private readonly Func<CancellationToken, Task<bool>> _ensureStartedAsync;
	private readonly Action<long> _markTransportUnavailable;
	private readonly Action<WorkspaceWatcherFailure> _raiseWorkspaceWatcherFailed;

	private readonly Func<string, Func<FileChangeBatch, CancellationToken, Task>, Action<WorkspaceFileWatcher, Exception?>, WorkspaceFileWatcher> _workspaceFileWatcherFactory;
	private readonly WorkspaceFileChangeForwarder _workspaceFileChangeForwarder;
	private readonly LuaWorkspaceSnapshotTracker _workspaceSnapshotTracker;

	private readonly object _watcherSyncRoot = new();

	private WorkspaceFileWatcher? _workspaceFileWatcher;
	private int _workspaceWatcherFailureReported;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaWorkspaceChangeCoordinator"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root directory.</param>
	/// <param name="watchSpecifications">The file patterns that should be mirrored to the language server.</param>
	/// <param name="workspaceFileWatcherFactory">Builds the low-level workspace watcher.</param>
	/// <param name="clientAccessor">Returns the active language-server client when available.</param>
	/// <param name="isDisposedAccessor">Returns whether the owner has been disposed.</param>
	/// <param name="ensureStartedAsync">Starts the language server on demand before forwarding file changes.</param>
	/// <param name="markTransportUnavailable">Marks one observed transport generation unhealthy after forwarding failures.</param>
	/// <param name="raiseWorkspaceWatcherFailed">Reports unrecoverable watcher failures to the owner.</param>
	/// <param name="logger">The logger instance, or <see langword="null"/> for a no-op logger.</param>
	internal LuaWorkspaceChangeCoordinator(
		string workspaceRootDirectoryPath,
		IReadOnlyList<WorkspaceWatchSpecification> watchSpecifications,
		Func<string, Func<FileChangeBatch, CancellationToken, Task>, Action<WorkspaceFileWatcher, Exception?>, WorkspaceFileWatcher> workspaceFileWatcherFactory,
		Func<ILanguageServerClient?> clientAccessor,
		Func<bool> isDisposedAccessor,
		Func<CancellationToken, Task<bool>> ensureStartedAsync,
		Action<long> markTransportUnavailable,
		Action<WorkspaceWatcherFailure> raiseWorkspaceWatcherFailed,
		ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;

		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_workspaceApiDirectoryPath = Path.Combine(workspaceRootDirectoryPath, ".API");
		_workspaceFileWatcherFactory = workspaceFileWatcherFactory;
		_clientAccessor = clientAccessor;
		_isDisposedAccessor = isDisposedAccessor;
		_ensureStartedAsync = ensureStartedAsync;
		_markTransportUnavailable = markTransportUnavailable;
		_raiseWorkspaceWatcherFailed = raiseWorkspaceWatcherFailed;
		_workspaceSnapshotTracker = new LuaWorkspaceSnapshotTracker(workspaceRootDirectoryPath, watchSpecifications, _logger);
		_workspaceFileChangeForwarder = new WorkspaceFileChangeForwarder(
			// Buffering is reserved for temporary startup or transport failures after a client exists.
			// When there is no client or the provider is disposed, workspace changes are intentionally ignored.
			// This keeps external-change replay scoped to recoverable transport/startup gaps instead of pre-start noise.
			// Unexpected forwarding failures are intentionally dropped by the forwarder to avoid ambiguous duplicate
			// delivery after partial observation; watcher recovery can still reconcile missed filesystem state later.
			() => _clientAccessor() is not null && !_isDisposedAccessor(),
			_isDisposedAccessor,
			_ensureStartedAsync,
			static () => { },
			failure =>
			{
				string firstPath = string.IsNullOrWhiteSpace(failure.FirstPath) ? "<unknown>" : failure.FirstPath;

				if (failure.WasDropped)
				{
					_logger.LogWarning(failure.Exception,
						"Dropped {BatchCount} Lua workspace file change(s) for '{Workspace}' after an unexpected forwarding failure. First path: '{FirstPath}'.",
						failure.BatchCount,
						_workspaceRootDirectoryPath,
						firstPath);
				}
				else
				{
					_logger.LogDebug(failure.Exception,
						"Failed to forward {BatchCount} Lua workspace file change(s) for '{Workspace}' starting at '{FirstPath}'; the batch was buffered for replay.",
						failure.BatchCount,
						_workspaceRootDirectoryPath,
						firstPath);
				}
			},
			bufferChangesWhileForwardingDisabled: false);
	}

	/// <summary>
	/// Gets the currently active workspace watcher, when one is running.
	/// </summary>
	internal WorkspaceFileWatcher? CurrentWatcher
	{
		get
		{
			lock (_watcherSyncRoot)
				return _workspaceFileWatcher;
		}
	}

	/// <summary>
	/// Starts the external workspace watcher when the language-server client is available.
	/// </summary>
	internal void EnsureWorkspaceFileWatcherStarted()
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
				_workspaceSnapshotTracker.CaptureTrackedSnapshot();
				Interlocked.Exchange(ref _workspaceWatcherFailureReported, 0);
			}
		}

		DisposeWatcher(watcherToDispose, "Failed to dispose an unstarted Lua workspace file watcher.");

		if (shouldReportFailure)
			ReportWorkspaceWatcherStartupFailure(startupException);
	}

	/// <summary>
	/// Normalizes and forwards a coalesced batch of external workspace changes to the language server.
	/// </summary>
	/// <param name="batch">The coalesced file change batch.</param>
	/// <param name="cancellationToken">Cancels the forwarding operation.</param>
	internal async Task DispatchWorkspaceFileChangesAsync(FileChangeBatch batch, CancellationToken cancellationToken)
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

		bool forwarded = await _workspaceFileChangeForwarder.DispatchAsync(changes, SendWorkspaceFileChangesAsync, cancellationToken).ConfigureAwait(false);

		if (forwarded)
			_workspaceSnapshotTracker.ApplyChanges(changes);
	}

	/// <summary>
	/// Replays any buffered workspace changes once the language server is ready again.
	/// </summary>
	/// <param name="cancellationToken">Cancels the replay operation.</param>
	internal async Task ReplayDeferredWorkspaceFileChangesAsync(CancellationToken cancellationToken)
	{
		if (_clientAccessor() is null || _isDisposedAccessor())
			return;

		IReadOnlyList<WorkspaceFileChange> replayedChanges = await _workspaceFileChangeForwarder.ReplayDeferredAsync(SendWorkspaceFileChangesAsync, cancellationToken).ConfigureAwait(false);

		if (replayedChanges.Count > 0)
			_workspaceSnapshotTracker.ApplyChanges(replayedChanges);
	}

	/// <summary>
	/// Disposes the active watcher and any buffered forwarding state.
	/// </summary>
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

		long transportGeneration = client.TransportGeneration;

		bool shouldRefreshConfiguration = false;
		var payloads = new List<FileEventPayload>(changes.Count);

		for (int i = 0; i < changes.Count; i++)
		{
			WorkspaceFileChange change = changes[i];
			shouldRefreshConfiguration |= IsWorkspaceConfigurationPath(change.Path);
			payloads.Add(new FileEventPayload(LanguageServerPathHelper.CreateFileUri(change.Path), (int)change.Kind));
		}

		try
		{
			if (shouldRefreshConfiguration)
			{
				await client.SendNotificationAsync("workspace/didChangeConfiguration",
					new DidChangeConfigurationParams(LuaLanguageServerSettingsFactory.Create(_workspaceRootDirectoryPath)),
					cancellationToken).ConfigureAwait(false);
			}

			await client.SendNotificationAsync("workspace/didChangeWatchedFiles",
				new DidChangeWatchedFilesParams([.. payloads]), cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_markTransportUnavailable(transportGeneration);
			throw;
		}
		catch (IOException)
		{
			_markTransportUnavailable(transportGeneration);
			throw;
		}
		catch (ObjectDisposedException) when (!_isDisposedAccessor())
		{
			_markTransportUnavailable(transportGeneration);
			throw;
		}
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

		_logger.LogWarning(exception,
			"Lua workspace watching failed for '{Workspace}'. Attempting to restart the watcher automatically and replay any missed tracked changes.",
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

	/// <summary>
	/// Restarts a failed workspace file watcher and reports whether recovery succeeded.
	/// </summary>
	/// <remarks>This method is used by <c>LuaLanguageServerIntellisenseProviderTests</c> via reflection. Do not remove without updating the tests.</remarks>
	private bool TryRestartWorkspaceFileWatcher(WorkspaceFileWatcher failedWatcher)
		=> RecoverWorkspaceFileWatcher(failedWatcher) == WorkspaceWatcherRecoveryResult.Recovered;

	/// <summary>
	/// Attempts to replace a failed watcher, reconcile any missed tracked changes, and classify the recovery outcome.
	/// </summary>
	private WorkspaceWatcherRecoveryResult RecoverWorkspaceFileWatcher(WorkspaceFileWatcher failedWatcher)
	{
		bool watcherRecovered = false;
		bool replacementWatcherStarted = false;
		bool startupFailed = false;
		bool workspaceUnavailable = false;
		Exception? startupException = null;
		WorkspaceFileWatcher? failedWatcherToDispose = failedWatcher;
		WorkspaceFileWatcher? replacementWatcherToDispose = null;
		Dictionary<string, LuaWorkspaceSnapshotEntry>? previousSnapshot = null;
		Dictionary<string, LuaWorkspaceSnapshotEntry>? currentSnapshot = null;

		// Decide under the lock whether recovery is needed and, if so, capture the snapshots required for reconciliation.
		lock (_watcherSyncRoot)
		{
			if (!ReferenceEquals(_workspaceFileWatcher, failedWatcher))
			{
				watcherRecovered = _workspaceFileWatcher is not null;
			}
			else
			{
				previousSnapshot = _workspaceSnapshotTracker.CloneTrackedSnapshot();
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
					currentSnapshot = _workspaceSnapshotTracker.ReplaceTrackedSnapshotWithCurrent();
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

		// Dispose watcher instances outside the lock so recovery bookkeeping stays responsive.
		DisposeWatcher(failedWatcherToDispose, "Failed to dispose a Lua workspace file watcher while recovering from a watcher error.", flushPendingChanges: false);
		DisposeWatcher(replacementWatcherToDispose, "Failed to dispose a Lua workspace file watcher while recovering from a watcher error.", flushPendingChanges: false);

		if (watcherRecovered)
		{
			_logger.LogInformation("Lua workspace watching recovered successfully for '{Workspace}'.", _workspaceRootDirectoryPath);

			// Reconcile any tracked changes that may have happened while the watcher was unavailable.
			if (replacementWatcherStarted && previousSnapshot is not null && currentSnapshot is not null)
				ObserveBackgroundTask(ReconcileWorkspaceSnapshotAsync(previousSnapshot, currentSnapshot), "Lua workspace watcher recovery reconciliation");

			return WorkspaceWatcherRecoveryResult.Recovered;
		}

		if (workspaceUnavailable)
		{
			_logger.LogInformation(
				"Lua workspace watching remains unavailable for '{Workspace}' because the workspace path does not exist.",
				_workspaceRootDirectoryPath);

			return WorkspaceWatcherRecoveryResult.Unavailable;
		}

		// Log startup failures separately so the caller can surface the right watcher-failure message.
		if (startupFailed)
		{
			_logger.LogWarning(startupException,
				"Lua workspace watching could not be restarted for '{Workspace}' because watcher startup failed.",
				_workspaceRootDirectoryPath);
		}

		return WorkspaceWatcherRecoveryResult.Failed;
	}

	private void ReportWorkspaceWatcherStartupFailure(Exception? exception)
	{
		if (Interlocked.Exchange(ref _workspaceWatcherFailureReported, 1) != 0)
			return;

		_logger.LogWarning(exception,
			"Lua workspace watching could not start for '{Workspace}'. External workspace changes will not be forwarded until the watcher can be started successfully.",
			_workspaceRootDirectoryPath);

		_raiseWorkspaceWatcherFailed(new WorkspaceWatcherFailure(
			"The Lua workspace file watcher could not be started for the current workspace.\n\n" +
			"Lua IntelliSense will continue to work for files edited in the editor, but external workspace changes - such as Git pull updates, generated .API files, or .luarc changes - will not be forwarded until the watcher can be started successfully."));
	}

	/// <summary>
	/// Replays the tracked workspace delta detected while the watcher was unavailable.
	/// </summary>
	private async Task ReconcileWorkspaceSnapshotAsync(
		Dictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot,
		Dictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot)
	{
		if (_isDisposedAccessor())
			return;

		FileChangeBatch batch = LuaWorkspaceSnapshotTracker.BuildDeltaBatch(previousSnapshot, currentSnapshot);

		if (batch.Count == 0)
			return;

		_logger.LogInformation(
			"Replaying {Count} reconciled workspace file change(s) after Lua workspace watcher recovery for '{Workspace}'.",
			batch.Count,
			_workspaceRootDirectoryPath);

		await DispatchWorkspaceFileChangesAsync(batch, CancellationToken.None).ConfigureAwait(false);
	}

	private void ObserveBackgroundTask(Task task, string operationName)
	{
		task.ContinueWith((completedTask, state) =>
			{
				if (completedTask.Exception is not { } exception)
					return;

				(string OperationName, string Workspace) = ((string OperationName, string Workspace))state!;
				_logger.LogWarning(exception.Flatten(), "{OperationName} failed for '{Workspace}'.", OperationName, Workspace);
			},
			(operationName, _workspaceRootDirectoryPath),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
			TaskScheduler.Default);
	}

	private void DisposeWatcher(WorkspaceFileWatcher? watcher, string message, bool flushPendingChanges = true)
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
			_logger.LogDebug(exception, message);
		}
	}
}
