using NLog;
using System.Diagnostics;
using System.Text.Json;

namespace TombLib.LanguageServer.Core.Tests;

public partial class LanguageServerClientTests
{
	[TestMethod]
	public async Task WaitForBackgroundLoopsAsync_WhenLoopFaults_LogsSpecificWarningWithoutFallback()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		await InvokePrivateTaskAsync(client, "WaitForBackgroundLoopsAsync",
			Task.FromException(new IOException("Simulated loop failure.")),
			Task.CompletedTask).ConfigureAwait(false);

		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("background loop", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated loop failure.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsFalse(logScope.Logs.Any(log => log.Contains("Language server background loop failed during disposal.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task ObserveBackgroundLoop_WhenLoopFaultsBeforeDisposal_LogsImmediateWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client,
			"ObserveBackgroundLoop",
			Task.FromException(new IOException("Simulated callback pump failure.")),
			"callback dispatcher",
			true);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("background loop", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("callback dispatcher", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("terminated unexpectedly", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated callback pump failure.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task ObserveBackgroundLoop_WhenTrackedPumpFaults_MarksReadyClientUnhealthy()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		InvokePrivateMethod(client,
			"ObserveBackgroundLoop",
			Task.FromException(new IOException("Simulated callback pump failure.")),
			"callback dispatcher",
			true);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public void EnsureTransportBackgroundLoopsRunning_WhenCallbackPumpFaulted_ReplacesTrackedCallbackPumpTask()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		Task faultedCallbackPump = Task.FromException(new IOException("Simulated callback pump failure."));

		SetPrivateField(client, "_callbackPumpTask", faultedCallbackPump);

		InvokePrivateMethod(client, "EnsureTransportBackgroundLoopsRunning", false);

		Task replacementTask = (Task)GetPrivateField(client, "_callbackPumpTask");

		Assert.AreNotSame(faultedCallbackPump, replacementTask);
		Assert.IsFalse(replacementTask.IsCompleted, "Restart recovery should recreate the callback pump instead of keeping the faulted task tracked.");
	}

	[TestMethod]
	public async Task EnsureTransportBackgroundLoopsRunning_WhenFaultedPumpIsReplaced_ForgetsObservedTermination()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		Task faultedCallbackPump = Task.FromException(new IOException("Simulated callback pump failure."));

		SetPrivateField(client, "_callbackPumpTask", faultedCallbackPump);
		InvokePrivateMethod(client,
			"ObserveBackgroundLoop",
			faultedCallbackPump,
			"callback dispatcher",
			true);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsTrue((bool)InvokePrivateMethodWithReturn(client, "WasObservedBackgroundLoopTermination", faultedCallbackPump));

		InvokePrivateMethod(client, "EnsureTransportBackgroundLoopsRunning", false);

		Assert.IsFalse((bool)InvokePrivateMethodWithReturn(client, "WasObservedBackgroundLoopTermination", faultedCallbackPump));
	}

	[TestMethod]
	public async Task EnsureTransportBackgroundLoopsRunning_WhenDiagnosticsPumpFaulted_RestartRecoveryStillPublishesDiagnostics()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object originalSession = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);
		var publishedMessage = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
		Task faultedDiagnosticsPump = Task.FromException(new IOException("Simulated diagnostics pump failure."));

		SetActiveSession(client, originalSession);
		SetReadyState(client, true);

		client.DiagnosticsPublished += parameters => publishedMessage.TrySetResult(parameters.Diagnostics?[0].Message);

		SetPrivateField(client, "_diagnosticsPumpTask", faultedDiagnosticsPump);

		InvokePrivateMethod(client,
			"ObserveBackgroundLoop",
			faultedDiagnosticsPump,
			"diagnostics pump",
			true);

		await Task.Delay(50).ConfigureAwait(false);

		Assert.IsFalse(client.IsReady);

		object restartedSession = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, restartedSession);
		InvokePrivateMethod(client, "EnsureTransportBackgroundLoopsRunning", true);
		SetReadyState(client, true);

		InvokePrivateMethod(client,
			"RaiseDiagnosticsPublished",
			GetTransportGeneration(restartedSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Recovered warning."));

		Assert.AreEqual("Recovered warning.",
			await publishedMessage.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task WaitWithDisposeBudgetAsync_WhenLoopAlreadyLogged_DoesNotLogDuplicateDisposalWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		Task faultedLoopTask = Task.FromException(new IOException("Simulated callback pump failure."));

		InvokePrivateMethod(client, "ObserveBackgroundLoop", faultedLoopTask, "callback dispatcher", true);

		await Task.Delay(50).ConfigureAwait(false);

		await InvokePrivateTaskAsync(client,
			"WaitWithDisposeBudgetAsync",
			faultedLoopTask,
			Stopwatch.StartNew(),
			"Language server callback dispatcher did not complete within {TimeoutMs} ms during disposal.",
			"Disposing the language server callback dispatcher raised exceptions.").ConfigureAwait(false);

		Assert.IsFalse(logScope.Logs.Any(log => log.Contains("Disposing the language server callback dispatcher raised exceptions.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.AreEqual(1, logScope.Logs.Count(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("callback dispatcher", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated callback pump failure.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task WaitForBackgroundLoopsAsync_WhenLoopIsCanceled_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		await InvokePrivateTaskAsync(client, "WaitForBackgroundLoopsAsync",
			Task.FromCanceled(new CancellationToken(canceled: true)),
			Task.CompletedTask).ConfigureAwait(false);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("was canceled during disposal", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("JSON-RPC completion", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("background loop", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}
}
