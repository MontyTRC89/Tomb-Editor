namespace Nickelony.LanguageServer.Lua;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	/// <summary>
	/// Ensures the language-server transport is running and that tracked documents and workspace watching are restored after reconnects.
	/// </summary>
	private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
	{
		if (_isDisposed || _client is null || GetConsecutiveStartupFailures() >= HardStartupFailureThreshold)
			return false;

		bool shieldCancellationForRestart = GetStartupSucceeded() && !_client.IsReady;

		CancellationToken startupCancellationToken = shieldCancellationForRestart
			? CancellationToken.None
			: cancellationToken;

		using var disposeAwareStartupCts = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, _disposeCts.Token);
		CancellationToken effectiveStartupCancellationToken = disposeAwareStartupCts.Token;
		bool startLockHeld = false;

		// Fast path: once the client is healthy, keep the workspace watcher alive and avoid taking the startup lock.
		if (GetStartupSucceeded() && _client.IsReady)
		{
			_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
			await _workspaceChanges.ReplayDeferredWorkspaceFileChangesAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
			return true;
		}

		try
		{
			// Serialize startup and restart work so concurrent callers share one recovery flow.
			await _startLock.WaitAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
			startLockHeld = true;

			IReadOnlyList<DocumentSnapshot> documentsToReopen = [];

			// Re-check state after taking the lock so concurrent callers share the same restart/startup work.
			if (GetConsecutiveStartupFailures() >= HardStartupFailureThreshold)
				return false;

			if (GetStartupSucceeded() && _client.IsReady)
			{
				_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
				await _workspaceChanges.ReplayDeferredWorkspaceFileChangesAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
				return true;
			}

			if (!_client.IsReady)
			{
				documentsToReopen = _documents.PrepareForRestart();

				if (GetStartupSucceeded())
				{
					_logger.LogInformation("Lua language server connection dropped for workspace '{Workspace}'; restarting and reopening {DocumentCount} tracked document(s).",
						_workspaceRootDirectoryPath,
						documentsToReopen.Count);
				}
			}

			// Start the transport, then replay tracked documents when this is a restart rather than a cold start.
			bool startupSucceeded = await _client.StartAsync(effectiveStartupCancellationToken).ConfigureAwait(false);

			if (startupSucceeded && documentsToReopen.Count > 0)
			{
				startupSucceeded = await ReopenTrackedDocumentsAsync(documentsToReopen, effectiveStartupCancellationToken).ConfigureAwait(false);

				if (!startupSucceeded)
				{
					_logger.LogWarning("Failed to replay {DocumentCount} tracked document(s) after Lua language server restart for workspace '{Workspace}'.",
						documentsToReopen.Count,
						_workspaceRootDirectoryPath);
				}
			}

			// Update startup bookkeeping before resuming watcher and timeout management.
			SetStartupSucceeded(startupSucceeded);

			if (startupSucceeded)
			{
				ResetStartupStateAfterSuccessfulStart();
				ResetRequestTimeoutTracking(_client.TransportGeneration);

				_workspaceChanges.EnsureWorkspaceFileWatcherStarted();
				await _workspaceChanges.ReplayDeferredWorkspaceFileChangesAsync(effectiveStartupCancellationToken).ConfigureAwait(false);
			}
			else
			{
				// Record repeated failures so IntelliSense eventually stops advertising availability until restart.
				int consecutiveStartupFailures = RegisterStartupFailure();
				bool isPermanentFailure = consecutiveStartupFailures >= HardStartupFailureThreshold;

				if (isPermanentFailure)
				{
					_logger.LogError("Lua language server failed to start {Count} times consecutively for workspace '{Workspace}'; IntelliSense is now disabled until the editor is restarted.",
						consecutiveStartupFailures, _workspaceRootDirectoryPath);
				}
				else
				{
					_logger.LogWarning("Failed to start the Lua language server for workspace '{Workspace}' (attempt {Attempt}/{Threshold}).",
						_workspaceRootDirectoryPath, consecutiveStartupFailures, HardStartupFailureThreshold);
				}

				ReportStartupFailure(isPermanentFailure);
			}

			return startupSucceeded;
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
		if (!TryMarkStartupFailureReported(isPermanentFailure))
			return;

		LanguageServerStartupFailure failure = isPermanentFailure
			? new LanguageServerStartupFailure(
				"The bundled Lua language server failed to start repeatedly and Lua IntelliSense is now disabled until the application is restarted. See the log for technical details.",
				true)
			: new LanguageServerStartupFailure(
				"The bundled Lua language server failed to start. Lua IntelliSense will remain unavailable until the application can start the server successfully. The application will retry automatically when Lua IntelliSense is requested again.",
				false);

		RaiseStartupFailed(failure);
	}

	/// <inheritdoc />
	/// <remarks>
	/// Disposal stops the workspace watcher, cancels queued document and semantic token work, and releases the active
	/// Lua language-server client.
	/// </remarks>
	public void Dispose()
	{
		if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
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

		try
		{
			_client?.Dispose();
		}
		finally
		{
			_disposeCts.Dispose();
		}
	}
}
