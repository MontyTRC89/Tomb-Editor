namespace TombLib.LanguageServer.Lua;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
	{
		if (_isDisposed || _client is null || _consecutiveStartupFailures >= HardStartupFailureThreshold)
			return false;

		bool shieldCancellationForRestart = _startupSucceeded && !_client.IsReady;
		CancellationToken startupCancellationToken = shieldCancellationForRestart
			? CancellationToken.None
			: cancellationToken;
		using var disposeAwareStartupCts = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, _disposeCts.Token);
		CancellationToken effectiveStartupCancellationToken = disposeAwareStartupCts.Token;
		bool startLockHeld = false;

		// Fast path: once the client is healthy, keep the workspace watcher alive and avoid taking the startup lock.
		if (_startupSucceeded && _client.IsReady)
		{
			_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
			return true;
		}

		try
		{
			await _startLock.WaitAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
			startLockHeld = true;

			IReadOnlyList<DocumentSnapshot> documentsToReopen = [];

			// Re-check state after taking the lock so concurrent callers share the same restart/startup work.
			if (_consecutiveStartupFailures >= HardStartupFailureThreshold)
				return false;

			if (_startupSucceeded && _client.IsReady)
			{
				_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
				return true;
			}

			if (!_client.IsReady)
			{
				if (_startupSucceeded)
					Log.Info("Lua language server connection dropped, restarting and reopening tracked documents.");

				documentsToReopen = _documents.PrepareForRestart();
			}

			// Start the transport, then replay tracked documents when this is a restart rather than a cold start.
			_startupSucceeded = await _client.StartAsync(startupCancellationToken).ConfigureAwait(false);

			if (_startupSucceeded && documentsToReopen.Count > 0)
			{
				_startupSucceeded = await ReopenTrackedDocumentsAsync(documentsToReopen, effectiveStartupCancellationToken).ConfigureAwait(false);

				if (!_startupSucceeded)
					Log.Warn("Failed to replay tracked documents after Lua language server restart.");
			}

			if (_startupSucceeded)
			{
				_consecutiveStartupFailures = 0;

				ResetRequestTimeoutTracking(_client.TransportGeneration);

				_transientStartupFailureReported = false;
				_permanentStartupFailureReported = false;

				_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
				await _workspaceChanges.ReplayDeferredWorkspaceFileChangesAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
			}
			else
			{
				// Record repeated failures so IntelliSense eventually stops advertising availability until restart.
				_consecutiveStartupFailures++;

				bool isPermanentFailure = _consecutiveStartupFailures >= HardStartupFailureThreshold;

				if (isPermanentFailure)
				{
					Log.Error("Lua language server failed to start {Count} times consecutively for workspace '{Workspace}'; IntelliSense is now disabled until the editor is restarted.",
						_consecutiveStartupFailures, _workspaceRootDirectoryPath);
				}
				else
				{
					Log.Warn("Failed to start the Lua language server for workspace '{Workspace}' (attempt {Attempt}/{Threshold}).",
						_workspaceRootDirectoryPath, _consecutiveStartupFailures, HardStartupFailureThreshold);
				}

				ReportStartupFailure(isPermanentFailure);
			}

			return _startupSucceeded;
		}
		catch (OperationCanceledException) when (_isDisposed)
		{
			return false;
		}
		finally
		{
			if (startLockHeld)
				_startLock.Release();
		}
	}

	private void ReportStartupFailure(bool isPermanentFailure)
	{
		if (isPermanentFailure)
		{
			if (_permanentStartupFailureReported)
				return;

			_permanentStartupFailureReported = true;
		}
		else
		{
			if (_transientStartupFailureReported)
				return;

			_transientStartupFailureReported = true;
		}

		LanguageServerStartupFailure failure = isPermanentFailure
			? new LanguageServerStartupFailure(
				"The bundled Lua language server failed to start repeatedly and Lua IntelliSense is now disabled until the application is restarted. See the log for technical details.",
				true)
			: new LanguageServerStartupFailure(
				"The bundled Lua language server failed to start. Lua IntelliSense will remain unavailable until the application can start the server successfully. The application will retry automatically when Lua IntelliSense is requested again.",
				false);

		RaiseStartupFailed(failure);
	}

	/// <summary>
	/// Releases the language-server client, workspace watcher, and any in-flight semantic-token requests.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		if (_client is not null)
		{
			_client.DiagnosticsPublished -= HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested -= HandleSemanticTokensRefreshRequested;
		}

		try
		{
			_disposeCts.Cancel();
		}
		catch (ObjectDisposedException)
		{ }

		_workspaceChanges.Dispose();

		CancelAllQueuedDocumentUpdates();
		CancelAllSemanticTokenRequests();

		_client?.Dispose();
	}
}
