using StreamJsonRpc;
using System.Diagnostics;
using System.Text.Json;

namespace TombLib.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Creates a transport session from a started language-server process.
	/// </summary>
	/// <param name="process">The started language-server process.</param>
	/// <returns>The configured transport session.</returns>
	private LanguageServerTransportSession CreateTransportSession(Process process)
	{
		long generation = Interlocked.Increment(ref _transportGeneration);

		return new LanguageServerTransportSession(generation,
			process,
			process.StandardOutput.BaseStream,
			process.StandardInput.BaseStream);
	}

	/// <summary>
	/// Configures the transport objects and process callbacks for one session before it becomes active.
	/// </summary>
	/// <param name="session">The session to configure.</param>
	private void ConfigureTransportSession(LanguageServerTransportSession session)
	{
		Process? process = session.Process;

		session.MessageHandler = CreateMessageHandler(session.ServerInputStream, session.ServerOutputStream);
		session.RpcTarget = new LanguageServerClientRpcTarget(this, session.Generation);
		session.JsonRpc = CreateJsonRpc(session);
		session.RpcCompletionTask = session.JsonRpc.Completion;

		session.ProcessExitedHandler = (_, _) => Process_Exited(session);

		if (process is not null)
			process.Exited += session.ProcessExitedHandler;
	}

	/// <summary>
	/// Starts the JSON-RPC listener and background stderr loop for one configured session.
	/// </summary>
	/// <param name="session">The configured session to start.</param>
	private void StartTransportSession(LanguageServerTransportSession session)
	{
		JsonRpc jsonRpc = session.JsonRpc
			?? throw new InvalidOperationException("The language server transport session is missing a JSON-RPC transport.");

		jsonRpc.StartListening();
		session.StderrLoopTask = Task.Run(() => ReadStandardErrorLoopAsync(session), CancellationToken.None);
	}

	/// <summary>
	/// Creates the JSON-RPC message handler for the transport streams.
	/// </summary>
	/// <param name="serverInputStream">The writable stream carrying host requests to the server.</param>
	/// <param name="serverOutputStream">The readable stream carrying server responses back to the host.</param>
	/// <returns>The configured message handler.</returns>
	private static HeaderDelimitedMessageHandler CreateMessageHandler(Stream serverInputStream, Stream serverOutputStream)
	{
		var formatter = new SystemTextJsonFormatter
		{
			JsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}
		};

		return new HeaderDelimitedMessageHandler(serverInputStream, serverOutputStream, formatter);
	}

	/// <summary>
	/// Creates the JSON-RPC transport bound to the supplied session.
	/// </summary>
	/// <param name="session">The transport session.</param>
	/// <returns>The configured JSON-RPC instance.</returns>
	private JsonRpc CreateJsonRpc(LanguageServerTransportSession session)
	{
		HeaderDelimitedMessageHandler messageHandler = session.MessageHandler
			?? throw new InvalidOperationException("The language server transport session is missing a JSON-RPC message handler.");

		LanguageServerClientRpcTarget rpcTarget = session.RpcTarget
			?? throw new InvalidOperationException("The language server transport session is missing a JSON-RPC callback target.");

		var jsonRpc = new JsonRpc(messageHandler, rpcTarget)
		{
			CancelLocallyInvokedMethodsWhenConnectionIsClosed = true
		};

		jsonRpc.Disconnected += (_, eventArgs) => JsonRpc_Disconnected(session, eventArgs);
		return jsonRpc;
	}

	/// <summary>
	/// Marks the supplied session as the currently active transport session.
	/// </summary>
	/// <param name="session">The active transport session.</param>
	private void SetActiveSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			_activeSession = session;
			PublishCapabilitySnapshot(CreateActiveSessionCapabilitySnapshot(session.Generation));
		}
	}

	/// <summary>
	/// Gets the active transport session once initialization completed.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The active transport session.</returns>
	private LanguageServerTransportSession GetRequiredReadySession(bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);
			LanguageServerTransportSession? session = _activeSession;

			if (!snapshot.IsReady)
				throw new IOException("The language server transport is not ready.");

			if (session is null || session.Generation != snapshot.TransportGeneration)
				throw new IOException("The language server transport is not available.");

			return session;
		}
	}

	/// <summary>
	/// Gets the active transport session or throws when none is available.
	/// </summary>
	/// <param name="allowDisposed">Whether disposed-state checks should be skipped.</param>
	/// <returns>The active transport session.</returns>
	private LanguageServerTransportSession GetRequiredActiveSession(bool allowDisposed)
	{
		ThrowIfDisposed(allowDisposed);

		LanguageServerTransportSession? session = Volatile.Read(ref _activeSession);

		if (session is null)
			throw new IOException("The language server transport is not available.");

		return session;
	}

	/// <summary>
	/// Detaches the current active transport session and clears ready-state tracking.
	/// </summary>
	/// <returns>The detached session, if one existed.</returns>
	private LanguageServerTransportSession? DetachActiveSession()
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			LanguageServerTransportSession? session = _activeSession;

			if (session is null)
				return null;

			_activeSession = null;
			PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot());

			return session;
		}
	}

	/// <summary>
	/// Detaches the supplied session only when it is still the current active transport session.
	/// </summary>
	/// <param name="session">The session to detach.</param>
	/// <returns><see langword="true"/> when the session was detached; otherwise, <see langword="false"/>.</returns>
	private bool TryDetachSpecificActiveSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			if (!ReferenceEquals(_activeSession, session))
				return false;

			_activeSession = null;
			PublishCapabilitySnapshot(CreateDefaultCapabilitySnapshot());

			return true;
		}
	}

	/// <summary>
	/// Disposes the currently active transport session, if any.
	/// </summary>
	private async Task DisposeActiveSessionAsync()
	{
		LanguageServerTransportSession? session = DetachActiveSession();

		if (session is not null)
			await DisposeSessionAsync(session).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles unexpected server process exit for one transport session.
	/// </summary>
	/// <param name="session">The session whose process exited.</param>
	private void Process_Exited(LanguageServerTransportSession session)
	{
		if (!TryDetachSpecificActiveSession(session))
			return;

		DisposeFailedSessionInBackground(session, "process exit");

		int? exitCode = TryReadProcessExitCode(session.Process);

		if (!_isDisposed)
		{
			Log.Warn("Language server process for transport generation {Generation} in workspace '{Workspace}' exited unexpectedly{ExitCodeSuffix}; the host will recreate the session on the next startup attempt.",
				session.Generation,
				_workspaceRootDirectoryPath,
				exitCode is not null ? $" with code {exitCode.Value}" : string.Empty);

			LogRecentStandardErrorContext(session, "Unexpected process exit");
		}
	}

	/// <summary>
	/// Reports whether one completed request result still belongs to the active ready transport session.
	/// </summary>
	/// <param name="session">The session that produced the request result.</param>
	/// <returns><see langword="true"/> when the request result still belongs to the active ready transport; otherwise, <see langword="false"/>.</returns>
	private bool CanAcceptRequestResultForSession(LanguageServerTransportSession session)
	{
		lock (_publishedCapabilitySnapshotSyncRoot)
		{
			PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

			return snapshot.IsReady
				&& snapshot.TransportGeneration == session.Generation
				&& ReferenceEquals(_activeSession, session);
		}
	}

	/// <summary>
	/// Reports whether the supplied transport generation still belongs to the active session.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to inspect.</param>
	/// <returns><see langword="true"/> when the generation is still active.</returns>
	private bool IsActiveTransportGeneration(long transportGeneration)
		=> transportGeneration != 0 && transportGeneration == TransportGeneration;

	/// <summary>
	/// Reports whether the supplied transport generation may currently publish server callbacks.
	/// </summary>
	/// <param name="transportGeneration">The transport generation to inspect.</param>
	/// <returns><see langword="true"/> when the generation still owns the published callback-enabled snapshot.</returns>
	private bool CanAcceptServerCallbacksForGeneration(long transportGeneration)
	{
		PublishedCapabilitySnapshot snapshot = Volatile.Read(ref _publishedCapabilitySnapshot);

		return snapshot.AcceptsServerCallbacks
			&& transportGeneration != 0
			&& snapshot.TransportGeneration == transportGeneration;
	}

	/// <summary>
	/// Handles JSON-RPC transport disconnection for the active session.
	/// </summary>
	/// <param name="session">The disconnected session.</param>
	/// <param name="eventArgs">The disconnect event arguments.</param>
	private void JsonRpc_Disconnected(LanguageServerTransportSession session, JsonRpcDisconnectedEventArgs eventArgs)
	{
		if (!TryDetachSpecificActiveSession(session))
		{
			Log.Debug("Ignoring disconnect from stale language server transport generation {Generation}: {Description}",
				session.Generation,
				eventArgs.Description);

			return;
		}

		DisposeFailedSessionInBackground(session, "transport disconnect");

		if (_isDisposed)
		{
			Log.Debug("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected during client disposal: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Description);

			return;
		}

		if (eventArgs.Reason == DisconnectedReason.LocallyDisposed)
		{
			Log.Info("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected during expected local shutdown: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Description);

			return;
		}

		Exception? exception = eventArgs.Exception;

		if (exception is not null)
		{
			Log.Warn(exception,
				"Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected unexpectedly (reason={Reason}); the host will recreate the session on the next startup attempt: {Description}",
				session.Generation,
				_workspaceRootDirectoryPath,
				eventArgs.Reason,
				eventArgs.Description);

			LogRecentStandardErrorContext(session, "Unexpected transport disconnect");

			return;
		}

		Log.Warn("Language server JSON-RPC transport generation {Generation} for workspace '{Workspace}' disconnected unexpectedly (reason={Reason}); the host will recreate the session on the next startup attempt: {Description}",
			session.Generation,
			_workspaceRootDirectoryPath,
			eventArgs.Reason,
			eventArgs.Description);

		LogRecentStandardErrorContext(session, "Unexpected transport disconnect");
	}

	/// <summary>
	/// Disposes a detached failed session without blocking the disconnect or process-exit callback thread.
	/// </summary>
	/// <param name="session">The detached failed session.</param>
	/// <param name="reason">The failure reason used for diagnostics when cleanup itself fails.</param>
	private void DisposeFailedSessionInBackground(LanguageServerTransportSession session, string reason)
	{
		lock (_failedSessionDisposalSyncRoot)
		{
			Task previousDisposal = _queuedFailedSessionDisposal;

			_queuedFailedSessionDisposal = Task.Run(
				() => DisposeFailedSessionAfterAsync(previousDisposal, session, reason));
		}
	}

	private async Task DisposeFailedSessionAfterAsync(Task previousDisposal, LanguageServerTransportSession session, string reason)
	{
		try
		{
			await previousDisposal.ConfigureAwait(false);
		}
		catch
		{
			// Later cleanup should still run even if earlier detached cleanup failed.
		}

		await DisposeFailedSessionAsync(session, reason).ConfigureAwait(false);
	}

	/// <summary>
	/// Logs one public request or notification failure with transport-generation context.
	/// </summary>
	/// <param name="operationKind">The operation kind, such as request or notification.</param>
	/// <param name="method">The JSON-RPC method name.</param>
	/// <param name="generation">The session generation that owned the operation.</param>
	/// <param name="exception">The transport failure.</param>
	private void LogTransportOperationFailure(string operationKind, string method, long generation, Exception exception)
	{
		if (_isDisposed)
			return;

		if (generation != 0 && generation != TransportGeneration)
		{
			Log.Debug(exception,
				"Language server {OperationKind} '{Method}' failed on stale transport generation {Generation} for workspace '{Workspace}'.",
				operationKind,
				method,
				generation,
				_workspaceRootDirectoryPath);

			return;
		}

		Log.Warn(exception,
			"Language server {OperationKind} '{Method}' failed on transport generation {Generation} for workspace '{Workspace}'; the host will recover by recreating the session when needed.",
			operationKind,
			method,
			generation,
			_workspaceRootDirectoryPath);
	}

	private void LogNonTransportRequestFailure(string method, long generation, Exception exception)
	{
		if (_isDisposed)
			return;

		if (generation != 0 && generation != TransportGeneration)
		{
			Log.Debug(exception,
				"Language server request '{Method}' failed on stale transport generation {Generation} for workspace '{Workspace}' without invalidating the active session.",
				method,
				generation,
				_workspaceRootDirectoryPath);

			return;
		}

		Log.Warn(exception,
			"Language server request '{Method}' failed on transport generation {Generation} for workspace '{Workspace}' without invalidating the active session.",
			method,
			generation,
			_workspaceRootDirectoryPath);
	}

	private void LogNonTransportNotificationFailure(string method, long generation, Exception exception)
	{
		if (_isDisposed)
			return;

		if (generation != 0 && generation != TransportGeneration)
		{
			Log.Debug(exception,
				"Language server notification '{Method}' failed on stale transport generation {Generation} for workspace '{Workspace}' without invalidating the active session.",
				method,
				generation,
				_workspaceRootDirectoryPath);

			return;
		}

		Log.Warn(exception,
			"Language server notification '{Method}' failed on transport generation {Generation} for workspace '{Workspace}' without invalidating the active session.",
			method,
			generation,
			_workspaceRootDirectoryPath);
	}

	/// <summary>
	/// Logs recent stderr context for one session when a transport failure path needs more diagnostics.
	/// </summary>
	/// <param name="session">The session whose recent stderr should be reported.</param>
	/// <param name="context">The failure context label.</param>
	private static void LogRecentStandardErrorContext(LanguageServerTransportSession? session, string context)
	{
		string? recentStandardError = session?.GetRecentStandardErrorSummary();

		if (string.IsNullOrWhiteSpace(recentStandardError))
			return;

		Log.Warn("{Context} recent language server stderr: {RecentStandardError}", context, recentStandardError);
	}

	/// <summary>
	/// Starts one background loop task and attaches immediate fault observation.
	/// </summary>
	/// <param name="backgroundLoop">The background loop delegate.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	/// <returns>The started background loop task.</returns>
	private Task StartObservedBackgroundLoop(
		Func<Task> backgroundLoop,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		Task task = Task.Run(backgroundLoop, CancellationToken.None);
		ObserveBackgroundLoop(task, loopName, markTransportUnhealthyOnUnexpectedTermination);
		return task;
	}

	/// <summary>
	/// Ensures the callback pump and, optionally, the diagnostics pump are running for the current client instance.
	/// Completed or faulted pumps are recreated so a transport restart can recover callback delivery.
	/// </summary>
	/// <param name="includeDiagnosticsPump">Whether the diagnostics pump should also be ensured.</param>
	private void EnsureTransportBackgroundLoopsRunning(bool includeDiagnosticsPump)
	{
		lock (_backgroundLoopSyncRoot)
		{
			if (_callbackPumpTask.IsCompleted)
			{
				ForgetObservedBackgroundLoopTermination(_callbackPumpTask);

				_callbackPumpTask = StartObservedBackgroundLoop(
					PumpCallbacksAsync,
					"callback dispatcher",
					markTransportUnhealthyOnUnexpectedTermination: true);
			}

			if (includeDiagnosticsPump && _diagnosticsPumpTask.IsCompleted)
			{
				ForgetObservedBackgroundLoopTermination(_diagnosticsPumpTask);

				_diagnosticsPumpTask = StartObservedBackgroundLoop(
					PumpDiagnosticsAsync,
					"diagnostics pump",
					markTransportUnhealthyOnUnexpectedTermination: true);
			}
		}
	}

	/// <summary>
	/// Observes one background loop task so unexpected termination is logged immediately while the client is still active.
	/// </summary>
	/// <param name="task">The background loop task to observe.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	private void ObserveBackgroundLoop(
		Task task,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		task.ContinueWith(
			static (completedTask, state) =>
			{
				BackgroundLoopObservation observation = (BackgroundLoopObservation)state!;

				observation.Owner.LogUnexpectedBackgroundLoopTermination(
					completedTask,
					observation.LoopName,
					observation.MarkTransportUnhealthyOnUnexpectedTermination);
			},
			new BackgroundLoopObservation(this, loopName, markTransportUnhealthyOnUnexpectedTermination),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);
	}

	/// <summary>
	/// Logs unexpected background loop termination while the client is still active.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <param name="loopName">The logical loop name used for diagnostics.</param>
	private void LogUnexpectedBackgroundLoopTermination(
		Task task,
		string loopName,
		bool markTransportUnhealthyOnUnexpectedTermination)
	{
		if (_isDisposed || _lifetimeCts.IsCancellationRequested)
			return;

		if (!TryMarkObservedBackgroundLoopTermination(task))
			return;

		if (markTransportUnhealthyOnUnexpectedTermination)
			MarkTransportUnhealthy();

		if (task.IsFaulted && task.Exception is { } aggregateException)
		{
			Exception loggedException = aggregateException.Flatten().InnerExceptions.Count == 1
				? aggregateException.Flatten().InnerExceptions[0]
				: aggregateException.Flatten();

			Log.Warn(loggedException, "Language server background loop '{LoopName}' terminated unexpectedly while the client was still active.", loopName);
			return;
		}

		if (task.IsCanceled)
		{
			Log.Warn("Language server background loop '{LoopName}' was canceled unexpectedly while the client was still active.", loopName);
			return;
		}

		Log.Warn("Language server background loop '{LoopName}' completed unexpectedly while the client was still active.", loopName);
	}

	/// <summary>
	/// Records that one background loop termination has already been logged before disposal.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <returns><see langword="true"/> when the task was newly marked; otherwise, <see langword="false"/>.</returns>
	private bool TryMarkObservedBackgroundLoopTermination(Task task)
	{
		lock (_observedBackgroundLoopSyncRoot)
			return _observedBackgroundLoopTerminations.Add(task);
	}

	/// <summary>
	/// Reports whether the supplied background loop task already logged its unexpected termination before disposal.
	/// </summary>
	/// <param name="task">The completed background loop task.</param>
	/// <returns><see langword="true"/> when the task already logged unexpected termination; otherwise, <see langword="false"/>.</returns>
	private bool WasObservedBackgroundLoopTermination(Task task)
	{
		lock (_observedBackgroundLoopSyncRoot)
			return _observedBackgroundLoopTerminations.Contains(task);
	}

	private void ForgetObservedBackgroundLoopTermination(Task task)
	{
		lock (_observedBackgroundLoopSyncRoot)
			_observedBackgroundLoopTerminations.Remove(task);
	}

	/// <summary>
	/// Stores the state needed to log one observed background loop termination.
	/// </summary>
	/// <param name="Owner">The owning client.</param>
	/// <param name="LoopName">The logical loop name.</param>
	private readonly record struct BackgroundLoopObservation(
		LanguageServerClient Owner,
		string LoopName,
		bool MarkTransportUnhealthyOnUnexpectedTermination);

	/// <summary>
	/// Reads standard-error output from the language-server process and logs non-empty lines.
	/// </summary>
	/// <param name="session">The transport session whose process stderr should be read.</param>
	private async Task ReadStandardErrorLoopAsync(LanguageServerTransportSession session)
	{
		try
		{
			while (!_isDisposed)
			{
				Process? process = session.Process;

				if (process is null || process.HasExited)
					break;

				string? line = await process.StandardError.ReadLineAsync(_lifetimeCts.Token).ConfigureAwait(false);

				if (line is null)
					break;

				if (!string.IsNullOrWhiteSpace(line))
				{
					session.RecordStandardErrorLine(line);
					Log.Debug("[LS stderr] {Line}", line);
				}
			}
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			Log.Debug(exception, "Language server stderr read loop failed.");
		}
	}

	/// <summary>
	/// Logs a window message or log message notification from the language server.
	/// </summary>
	/// <param name="method">The originating method name.</param>
	/// <param name="parameters">The message payload.</param>
	private static void LogServerMessage(string method, WindowMessageParams parameters)
	{
		string? messageText = parameters.Message;

		if (string.IsNullOrWhiteSpace(messageText))
			return;

		int messageType = parameters.Type ?? 4;

		switch (messageType)
		{
			case 1: Log.Error("[LS {Method}] {Message}", method, messageText); break;
			case 2: Log.Warn("[LS {Method}] {Message}", method, messageText); break;
			case 3: Log.Info("[LS {Method}] {Message}", method, messageText); break;
			default: Log.Debug("[LS {Method}] {Message}", method, messageText); break;
		}
	}

	/// <summary>
	/// Attempts to read the exit code for a process that may already be disposed.
	/// </summary>
	/// <param name="process">The process to inspect.</param>
	/// <returns>The exit code, or <see langword="null"/> when unavailable.</returns>
	private static int? TryReadProcessExitCode(Process? process)
	{
		if (process is null)
			return null;

		try
		{
			return process.HasExited ? process.ExitCode : null;
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}
}
