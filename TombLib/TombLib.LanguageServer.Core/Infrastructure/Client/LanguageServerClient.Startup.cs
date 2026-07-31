using System.Diagnostics;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Starts the language-server process and completes the initialize/initialized handshake.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel startup.</param>
	/// <returns><see langword="true"/> when startup succeeded; otherwise, <see langword="false"/>.</returns>
	public async Task<bool> StartAsync(CancellationToken cancellationToken)
	{
		ThrowIfDisposed(allowDisposed: false);

		if (IsReady)
			return true;

		using var disposeAwareStartupCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeCts.Token);
		CancellationToken effectiveCancellationToken = disposeAwareStartupCts.Token;

		bool startLockHeld = false;
		Process? startedProcess = null;
		LanguageServerTransportSession? startedSession = null;
		bool sessionActivated = false;

		try
		{
			await _startLock.WaitAsync(effectiveCancellationToken).ConfigureAwait(false);
			startLockHeld = true;

			ThrowIfDisposed(allowDisposed: false);

			if (IsReady)
				return true;

			LanguageServerTransportSession? previousSession = DetachActiveSession();

			if (previousSession is not null)
			{
				_logger.LogInformation("Restarting language server transport by replacing generation {Generation}.", previousSession.Generation);
				await DisposeSessionAsync(previousSession).ConfigureAwait(false);
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = _serverExecutablePath,
				WorkingDirectory = Path.GetDirectoryName(_serverExecutablePath) ?? Environment.CurrentDirectory,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			startedProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

			if (!startedProcess.Start())
			{
				startedProcess.Dispose();
				startedProcess = null;
				return false;
			}

			if (OperatingSystem.IsWindows())
				ProcessJobObject.TryAssignProcess(startedProcess);

			if (_processStartedTestHook is not null)
				await _processStartedTestHook(startedProcess, effectiveCancellationToken).ConfigureAwait(false);

			LanguageServerTransportSession session = CreateTransportSession(startedProcess);
			startedSession = session;
			ConfigureTransportSession(session);
			SetActiveSession(session);
			sessionActivated = true;
			StartTransportSession(session);

			_logger.LogInformation("Activated language server transport generation {Generation} for workspace '{Workspace}'; completing initialization handshake.",
				session.Generation,
				_workspaceRootDirectoryPath);

			if (_sessionActivatedTestHook is not null)
				await _sessionActivatedTestHook(effectiveCancellationToken).ConfigureAwait(false);

			EnsureTransportBackgroundLoopsRunning(includeDiagnosticsPump: true);

			using var initializeTimeout = CancellationTokenSource.CreateLinkedTokenSource(effectiveCancellationToken);
			initializeTimeout.CancelAfter(_initializeTimeout);

			if (_beforeInitializeRequestTestHook is not null)
				await _beforeInitializeRequestTestHook(initializeTimeout.Token).ConfigureAwait(false);

			InitializeResponse initializeResponse = await SendRequestCoreAsync<InitializeResponse>(session,
				"initialize", BuildInitializeParams(), initializeTimeout.Token, allowDisposed: false).ConfigureAwait(false);

			CaptureServerCapabilitiesForGeneration(session.Generation, initializeResponse);

			await SendNotificationCoreAsync(session, "initialized", new EmptyParams(), cancellationToken, allowDisposed: false).ConfigureAwait(false);

			CachedSettingsSnapshot settingsSnapshot = RefreshCachedSettingsSnapshotFromProvider();

			await SendNotificationCoreAsync(session,
				"workspace/didChangeConfiguration",
				new DidChangeConfigurationParams(settingsSnapshot.SettingsPayload),
				cancellationToken,
				allowDisposed: false).ConfigureAwait(false);

			SetCapabilityReadinessForGeneration(session.Generation, true);

			_logger.LogInformation("Language server transport generation {Generation} completed initialization and is ready.", session.Generation);
			return true;
		}
		catch (OperationCanceledException) when (sessionActivated && !effectiveCancellationToken.IsCancellationRequested)
		{
			_logger.LogWarning("Language server transport generation {Generation} did not complete initialization within {TimeoutMs} ms for workspace '{Workspace}'; tearing down the session and leaving the client not ready.",
				startedSession?.Generation ?? 0,
				(int)_initializeTimeout.TotalMilliseconds,
				_workspaceRootDirectoryPath);

			LogRecentStandardErrorContext(startedSession, "Initialization timeout");

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: true).ConfigureAwait(false);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			throw;
		}
		catch (OperationCanceledException) when (_isDisposed)
		{
			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: true).ConfigureAwait(false);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception,
				"Failed to start the language server (executable='{Executable}', workspace='{Workspace}', generation={Generation}, stage='{Stage}').",
				_serverExecutablePath,
				_workspaceRootDirectoryPath,
				startedSession?.Generation ?? 0,
				sessionActivated ? "initialization" : "startup");

			LogRecentStandardErrorContext(startedSession, sessionActivated ? "Initialization failure" : "Startup failure");

			if (!sessionActivated)
				await DisposeStartupSessionResourcesAsync(startedSession, startedProcess, isExpectedCancellation: false).ConfigureAwait(false);

			await DisposeActiveSessionAsync().ConfigureAwait(false);
			return false;
		}
		finally
		{
			if (startLockHeld)
				_startLock.Release();
		}
	}

	/// <summary>
	/// Builds the initialize request payload for the active workspace.
	/// </summary>
	/// <returns>The initialize request payload.</returns>
	private object BuildInitializeParams() => new
	{
		processId = Environment.ProcessId,
		initializationOptions = _initializationOptionsProvider(_workspaceRootDirectoryPath),
		rootUri = LanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
		workspaceFolders = new[]
		{
			new
			{
				uri = LanguageServerPathHelper.CreateFileUri(_workspaceRootDirectoryPath),
				name = _workspaceFolderName
			}
		},
		capabilities = BuildClientCapabilitiesPayload()
	};

	/// <summary>
	/// Derives the workspace-folder display name from the normalized workspace root path.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root path.</param>
	/// <returns>The folder name to advertise to the language server.</returns>
	private static string GetWorkspaceFolderName(string workspaceRootDirectoryPath)
	{
		string trimmedWorkspaceRootDirectoryPath = workspaceRootDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (trimmedWorkspaceRootDirectoryPath.Length == 0)
			trimmedWorkspaceRootDirectoryPath = workspaceRootDirectoryPath;

		string folderName = Path.GetFileName(trimmedWorkspaceRootDirectoryPath);

		if (!string.IsNullOrEmpty(folderName))
			return folderName;

		string rootPath = Path.GetPathRoot(workspaceRootDirectoryPath) ?? string.Empty;
		string trimmedRootPath = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (!string.IsNullOrEmpty(trimmedRootPath))
			return trimmedRootPath;

		if (!string.IsNullOrEmpty(rootPath))
			return rootPath;

		return workspaceRootDirectoryPath;
	}
}
