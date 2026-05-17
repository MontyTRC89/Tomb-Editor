using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using StreamJsonRpc;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LanguageServerClientTests
{
	private static readonly LanguageServerClientOptions DefaultClientOptions = new(static () => new { });

	[TestMethod]
	public void LanguageServerClientOptions_DefaultLifecycleTimeouts_AreRelaxedForNormalMachineSlowness()
	{
		var options = new LanguageServerClientOptions(static () => new { });

		Assert.AreEqual(TimeSpan.FromSeconds(20), options.InitializeTimeout);
		Assert.AreEqual(TimeSpan.FromSeconds(3), options.ShutdownRequestTimeout);
		Assert.AreEqual(TimeSpan.FromSeconds(5), options.DisposeWaitTimeout);
	}

	[TestMethod]
	public void FreshClient_ExposesConservativeCapabilitiesBeforeStartup()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(0L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensFull);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
	}

	[TestMethod]
	public void ActiveSessionBeforeHandshake_ExposesConservativeCapabilities()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(3L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensFull);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
	}

	[TestMethod]
	public void CaptureServerCapabilities_UsesFullTextSyncWhenServerAdvertisesFullSync()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    }
			  }
			}
			"""));

		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
	}

	[TestMethod]
	public void CaptureServerCapabilities_RejectsMissingDocumentChangeSupport()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
				"""
				{
				  "capabilities": {
				    "textDocumentSync": {}
				  }
				}
				""")));

		Assert.IsInstanceOfType(exception.InnerException, typeof(NotSupportedException));
	}

	[TestMethod]
	public void CaptureServerCapabilities_RecognizesReferenceRenameAndFormattingProviders()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "referencesProvider": {},
			    "renameProvider": { "prepareProvider": true },
			    "documentFormattingProvider": true
			  }
			}
			"""));

		Assert.IsTrue(client.SupportsReferences);
		Assert.IsTrue(client.SupportsRename);
		Assert.IsTrue(client.SupportsFormatting);
	}

	[TestMethod]
	public void CaptureServerCapabilities_RecognizesSemanticTokensLegendAndDeltaSupport()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function", "variable"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		Assert.IsTrue(client.SupportsSemanticTokensDelta);
		Assert.IsTrue(client.SupportsSemanticTokensFull);
		CollectionAssert.AreEqual(new[] { "function", "variable" }, client.SemanticTokenTypes.ToArray());
		CollectionAssert.AreEqual(new[] { "declaration" }, client.SemanticTokenModifiers.ToArray());
	}

	[TestMethod]
	public void CaptureServerCapabilities_TracksWhenFullSemanticTokensAreUnsupported()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "semanticTokensProvider": {
			      "full": false,
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": []
			      }
			    }
			  }
			}
			"""));

		Assert.IsFalse(client.SupportsSemanticTokensFull);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
		CollectionAssert.AreEqual(new[] { "function" }, client.SemanticTokenTypes.ToArray());
	}

	[TestMethod]
	public void CaptureServerCapabilities_RejectsMissingCapabilitiesPayload()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse("""{}""")));

		Assert.IsInstanceOfType(exception.InnerException, typeof(NotSupportedException));
	}

	[TestMethod]
	public void CaptureServerCapabilities_RejectsMissingTextDocumentSyncCapability()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
				"""
				{
				  "capabilities": {
				    "referencesProvider": true,
				    "renameProvider": true
				  }
				}
				""")));

		Assert.IsInstanceOfType(exception.InnerException, typeof(NotSupportedException));
	}

	[TestMethod]
	public void DeserializeInitializeResponse_UnsupportedCapabilityShapesDegradePredictably()
	{
		InitializeResponse response = DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": "invalid"
			    },
			    "referencesProvider": "unexpected",
			    "renameProvider": [true],
			    "documentFormattingProvider": 123,
			    "semanticTokensProvider": {
			      "full": [],
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			""");

		Assert.IsNotNull(response.Capabilities);
		Assert.AreEqual(TextDocumentSyncKind.None, response.Capabilities.TextDocumentSync?.Kind);
		Assert.IsFalse(response.Capabilities.ReferencesProvider?.IsSupported ?? true);
		Assert.IsFalse(response.Capabilities.RenameProvider?.IsSupported ?? true);
		Assert.IsFalse(response.Capabilities.DocumentFormattingProvider?.IsSupported ?? true);
		Assert.IsFalse(response.Capabilities.SemanticTokensProvider?.Full?.SupportsDelta ?? true);
		CollectionAssert.AreEqual(new[] { "function" }, response.Capabilities.SemanticTokensProvider?.Legend?.TokenTypes);
		CollectionAssert.AreEqual(new[] { "declaration" }, response.Capabilities.SemanticTokensProvider?.Legend?.TokenModifiers);
	}

	[TestMethod]
	public void DeserializeInitializeResponse_BooleanTextDocumentSyncDegradesToNone()
	{
		InitializeResponse response = DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": true
			  }
			}
			""");

		Assert.IsNotNull(response.Capabilities);
		Assert.AreEqual(TextDocumentSyncKind.None, response.Capabilities.TextDocumentSync?.Kind);
	}

	[TestMethod]
	public void SemanticTokenCapabilityLists_AreNotExposedAsMutableArrays()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);

		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "semanticTokensProvider": {
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		Assert.IsFalse(client.SemanticTokenTypes is string[]);
		Assert.IsFalse(client.SemanticTokenModifiers is string[]);
	}

	[TestMethod]
	public void Dispose_WritesGracefulShutdownMessagesAndLogsGracefulAttempt()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process, serverOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);

		client.Dispose();

		string writtenPayload = serverInputStream.GetWrittenText();

		Assert.IsTrue(writtenPayload.Contains("\"method\":\"shutdown\"", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"method\":\"exit\"", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Info|", StringComparison.Ordinal)
			&& log.Contains("Attempting graceful shutdown", StringComparison.Ordinal)
			&& log.Contains("generation 1", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task DisposeAsync_WritesGracefulShutdownMessages()
	{
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process, serverOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);

		await client.DisposeAsync().ConfigureAwait(false);

		string writtenPayload = serverInputStream.GetWrittenText();

		Assert.IsTrue(writtenPayload.Contains("\"method\":\"shutdown\"", StringComparison.Ordinal), writtenPayload);
		Assert.IsTrue(writtenPayload.Contains("\"method\":\"exit\"", StringComparison.Ordinal), writtenPayload);
	}

	[TestMethod]
	public async Task Dispose_AndDisposeAsync_DisposeStartLockAfterCleanup()
	{
		for (int i = 0; i < 2; i++)
		{
			var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
			SemaphoreSlim startLock = GetStartLock(client);

			if (i == 0)
				client.Dispose();
			else
				await client.DisposeAsync().ConfigureAwait(false);

			Assert.ThrowsException<ObjectDisposedException>(() => startLock.Wait(0));
		}
	}

	[TestMethod]
	public async Task StartAsync_DisposeDuringStartupWait_ReturnsFalseWithoutObjectDisposedException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		SemaphoreSlim startLock = GetStartLock(client);

		startLock.Wait();

		try
		{
			Task<bool> startTask = client.StartAsync(CancellationToken.None);

			await Task.Delay(50).ConfigureAwait(false);
			client.Dispose();

			Assert.IsFalse(await startTask.ConfigureAwait(false));
		}
		finally
		{
			startLock.Release();
		}
	}

	[TestMethod]
	public async Task StartAsync_WhenDisposalAlreadyBegan_DoesNotReportReadySuccess()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		bool disposeStarted = (bool)InvokePrivateMethodWithReturn(client, "TryBeginDispose");

		Assert.IsTrue(disposeStarted);
		Assert.IsTrue(client.IsReady, "The regression test expects readiness to still be visible immediately after disposal begins.");

		await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () =>
			await client.StartAsync(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task StartAsync_WhenStartupFailsBeforeSessionActivation_KillsSpawnedProcess()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		Process? startedProcess = null;
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: async (process, _) =>
			{
				startedProcess = process;
				startedProcessId = process.Id;
				throw new InvalidOperationException("Simulated startup failure before session activation.");
			});

		bool started = await client.StartAsync(CancellationToken.None).ConfigureAwait(false);

		Assert.IsFalse(started);
		Assert.IsNotNull(startedProcess);
		Assert.IsNotNull(startedProcessId);
		Assert.IsTrue(await WaitForProcessExitAsync(startedProcessId.Value).ConfigureAwait(false));
		Assert.IsFalse(client.IsReady);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("Startup cleanup is forcing language server process termination", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task StartAsync_WhenCallerCancelsBeforeSessionActivation_CleansStartupProcessAndRethrows()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var startupCancellation = new CancellationTokenSource();
		var processStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowStartupToContinue = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: async (process, cancellationToken) =>
			{
				startedProcessId = process.Id;
				processStarted.TrySetResult(true);
				Task completedTask = await Task.WhenAny(
					allowStartupToContinue.Task,
					Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)).ConfigureAwait(false);

				if (!ReferenceEquals(completedTask, allowStartupToContinue.Task))
					cancellationToken.ThrowIfCancellationRequested();

				await allowStartupToContinue.Task.ConfigureAwait(false);
			});

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await processStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		startupCancellation.Cancel();
		allowStartupToContinue.TrySetResult(true);

		await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			await startTask.ConfigureAwait(false)).ConfigureAwait(false);

		Assert.IsNotNull(startedProcessId);
		Assert.IsTrue(await WaitForProcessExitAsync(startedProcessId.Value).ConfigureAwait(false));
		Assert.IsFalse(client.IsReady);
		Assert.IsTrue(logScope.Logs.Any(log =>
			(log.StartsWith("Debug|", StringComparison.Ordinal)
				&& log.Contains("after cancellation before session activation completed", StringComparison.OrdinalIgnoreCase))
			|| (log.StartsWith("Info|", StringComparison.Ordinal)
				&& log.Contains("Attempting graceful shutdown", StringComparison.OrdinalIgnoreCase))),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("Startup cleanup", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task StartAsync_WhenClientIsDisposedBeforeSessionActivation_CleansStartupProcessAndReturnsFalse()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		var processStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowStartupToContinue = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: async (process, cancellationToken) =>
			{
				startedProcessId = process.Id;
				processStarted.TrySetResult(true);

				Task completedTask = await Task.WhenAny(
					allowStartupToContinue.Task,
					Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)).ConfigureAwait(false);

				if (!ReferenceEquals(completedTask, allowStartupToContinue.Task))
					cancellationToken.ThrowIfCancellationRequested();

				await allowStartupToContinue.Task.ConfigureAwait(false);
			});

		Task<bool> startTask = client.StartAsync(CancellationToken.None);

		await processStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		client.Dispose();
		allowStartupToContinue.TrySetResult(true);

		Assert.IsFalse(await startTask.ConfigureAwait(false));
		Assert.IsNotNull(startedProcessId);
		Assert.IsTrue(await WaitForProcessExitAsync(startedProcessId.Value).ConfigureAwait(false));
		Assert.IsFalse(client.IsReady);
		Assert.IsTrue(logScope.Logs.Any(log =>
			(log.StartsWith("Debug|", StringComparison.Ordinal)
				&& log.Contains("after cancellation before session activation completed", StringComparison.OrdinalIgnoreCase))
			|| (log.StartsWith("Info|", StringComparison.Ordinal)
				&& log.Contains("Attempting graceful shutdown", StringComparison.OrdinalIgnoreCase))),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("Startup cleanup", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task DisposeStartupSessionResourcesAsync_WhenCancellationIsExpected_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions);
		int processId = process.Id;

		await InvokePrivateTaskAsync(client, "DisposeStartupSessionResourcesAsync", null, process, true).ConfigureAwait(false);

		Assert.IsTrue(await WaitForProcessExitAsync(processId).ConfigureAwait(false));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("after cancellation before session activation completed", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("Startup cleanup", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Dispose_WhenShutdownRequestTimesOut_LogsForcedTermination()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions);
		object session = CreateTransportSession(client, 7, process, serverOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);
		RecordStandardErrorLine(session, "LuaLS did not respond to shutdown.");

		client.Dispose();

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("did not acknowledge shutdown", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 7", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("forcing process termination", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 7", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("recent language server stderr", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("LuaLS did not respond to shutdown.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task Dispose_WhenShutdownAcknowledgesWithinConfiguredBudget_DoesNotLogTimeoutOrForcedTermination()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using Process process = StartShortLivedProcess();
		int processId = process.Id;
		using var serverOutputStream = new DelayedJsonRpcResponseStream(
			CreateJsonRpcResultMessage(id: 1, resultJson: "null"),
			TimeSpan.FromMilliseconds(150));
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, new LanguageServerClientOptions(static () => new { })
		{
			ShutdownRequestTimeout = TimeSpan.FromMilliseconds(1500),
			DisposeWaitTimeout = TimeSpan.FromMilliseconds(1500)
		});
		object session = CreateTransportSession(client, 8, process, serverOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);

		client.Dispose();

		Assert.IsTrue(await WaitForProcessExitAsync(processId).ConfigureAwait(false));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("did not acknowledge shutdown", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("forcing process termination", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Dispose_WhenShutdownRequestTimesOut_UsesConfiguredTimeoutInLog()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, new LanguageServerClientOptions(static () => new { })
		{
			ShutdownRequestTimeout = TimeSpan.FromMilliseconds(50)
		});
		object session = CreateTransportSession(client, 9, process, serverOutputStream, serverInputStream, startListening: true);

		SetActiveSession(client, session);

		client.Dispose();

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("did not acknowledge shutdown", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("50", StringComparison.Ordinal)
			&& log.Contains("generation 9", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task WaitWithDisposeBudgetAsync_UsesConfiguredBudgetWithoutImmediateTimeout()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			DisposeWaitTimeout = TimeSpan.FromMilliseconds(250)
		});
		var disposeStopwatch = Stopwatch.StartNew();
		var releaseTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		Task delayedTask = Task.Run(async () =>
		{
			await Task.Delay(125).ConfigureAwait(false);
			releaseTask.TrySetResult(true);
		});

		await InvokePrivateTaskAsync(
			client,
			"WaitWithDisposeBudgetAsync",
			releaseTask.Task,
			disposeStopwatch,
			"timeout message {0}",
			"exception message")
			.ConfigureAwait(false);

		await delayedTask.ConfigureAwait(false);
		Assert.IsTrue(releaseTask.Task.IsCompletedSuccessfully);
	}

	[TestMethod]
	public async Task DisposeStartLockAsync_WhenStartupGateStaysBusy_LogsTimeoutWithoutDisposingGate()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		SemaphoreSlim startLock = GetStartLock(client);
		bool reacquiredStartLock = false;

		startLock.Wait();

		try
		{
			await InvokePrivateTaskAsync(client, "DisposeStartLockAsync", TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
		}
		finally
		{
			startLock.Release();
		}

		try
		{
			reacquiredStartLock = startLock.Wait(0);
			Assert.IsTrue(reacquiredStartLock);
		}
		finally
		{
			if (reacquiredStartLock)
				startLock.Release();
		}

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("startup gate did not become available", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("50", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task StartAsync_WhenCancelledDuringHandshake_DetachesPublishedSessionAndLeavesClientNotReady()
	{
		var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var startupCancellation = new CancellationTokenSource();
		await using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: null,
			sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		startupCancellation.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await startTask.ConfigureAwait(false)).ConfigureAwait(false);

		Assert.IsFalse(client.IsReady);
		Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethodWithReturn(client, "GetRequiredActiveSession", false));
	}

	[TestMethod]
	public async Task StartAsync_WhenInitializationTimesOut_UsesConfiguredTimeoutAndLeavesClientNotReady()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		var initializeStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			new LanguageServerClientOptions(static () => new { })
			{
				InitializeTimeout = TimeSpan.FromMilliseconds(50)
			},
			processStartedTestHook: null,
			sessionActivatedTestHook: null,
			beforeInitializeRequestTestHook: async cancellationToken =>
			{
				initializeStarted.TrySetResult(true);
				await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			});

		Task<bool> startTask = client.StartAsync(CancellationToken.None);

		await initializeStarted.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		bool started = await startTask.ConfigureAwait(false);

		Assert.IsFalse(started);
		Assert.IsFalse(client.IsReady);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("did not complete initialization within 50 ms", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void JsonRpc_Disconnected_UnexpectedDisconnect_LogsRecentStderrContext()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 11, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);
		RecordStandardErrorLine(session, "LuaLS handshake failed near initialize.");

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			session,
			new JsonRpcDisconnectedEventArgs("stream closed", DisconnectedReason.StreamError));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("disconnected unexpectedly", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("workspace", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("recreate the session", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 11", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("recent language server stderr", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("LuaLS handshake failed near initialize.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void LanguageServerClientOptions_RejectsNonPositiveTimeouts()
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LanguageServerClientOptions(static () => new { })
		{
			InitializeTimeout = TimeSpan.Zero
		});

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LanguageServerClientOptions(static () => new { })
		{
			ShutdownRequestTimeout = Timeout.InfiniteTimeSpan
		});

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new LanguageServerClientOptions(static () => new { })
		{
			DisposeWaitTimeout = TimeSpan.FromMilliseconds(-1)
		});
	}

	[TestMethod]
	public async Task Dispose_ConcurrentSyncAndAsyncCalls_DoNotFault()
	{
		for (int i = 0; i < 25; i++)
		{
			await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
			object session = CreateTransportSession(client, i + 1, process: null, Stream.Null, Stream.Null, startListening: true);

			SetActiveSession(client, session);

			Task[] disposeTasks =
			[
				Task.Run(client.Dispose),
				Task.Run(async () => await client.DisposeAsync().ConfigureAwait(false)),
				Task.Run(client.Dispose)
			];

			await Task.WhenAll(disposeTasks).ConfigureAwait(false);

			await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() =>
				client.SendNotificationAsync("workspace/didChangeConfiguration", new { settings = new { } }, CancellationToken.None))
				.ConfigureAwait(false);
		}
	}

	[TestMethod]
	public void Dispose_UsesOneOverallDisposeBudgetAcrossTeardownStages()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			DisposeWaitTimeout = TimeSpan.FromMilliseconds(100)
		});

		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var pendingRpcCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var pendingStderrLoop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		SemaphoreSlim startLock = GetStartLock(client);

		SetSessionProperty(session, "RpcCompletionTask", pendingRpcCompletion.Task);
		SetSessionProperty(session, "StderrLoopTask", pendingStderrLoop.Task);
		SetActiveSession(client, session);

		startLock.Wait();
		var stopwatch = Stopwatch.StartNew();

		try
		{
			client.Dispose();
		}
		finally
		{
			stopwatch.Stop();

			try
			{
				startLock.Release();
			}
			catch (ObjectDisposedException)
			{ }
			catch (SemaphoreFullException)
			{ }
		}

		Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromMilliseconds(175),
			$"Dispose should honor a single overall budget, but took {stopwatch.Elapsed.TotalMilliseconds:F0} ms.");
	}

	[TestMethod]
	public async Task StartAsync_InterleavedWithDisposeAsync_DoesNotLeaveReadyClientReachable()
	{
		for (int i = 0; i < 10; i++)
		{
			var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			await using var client = new LanguageServerClient(
				@"C:\Workspace",
				Path.Combine(Environment.SystemDirectory, "cmd.exe"),
				DefaultClientOptions,
				processStartedTestHook: null,
				sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

			Task<bool> startTask = client.StartAsync(CancellationToken.None);

			await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

			await client.DisposeAsync().ConfigureAwait(false);

			Assert.IsFalse(await startTask.ConfigureAwait(false));
			Assert.IsFalse(client.IsReady);
			Assert.ThrowsException<ObjectDisposedException>(() => GetStartLock(client).Wait(0));

			await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() =>
				client.SendNotificationAsync("workspace/didChangeConfiguration", new { settings = new { } }, CancellationToken.None))
				.ConfigureAwait(false);
		}
	}

	[TestMethod]
	public async Task SendNotificationAsync_CompletesAfterLocalDispatchEvenWhenTransportWriteRemainsBlocked()
	{
		using var blockingWriteStream = new BlockingWriteStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, blockingWriteStream, Stream.Null);
		using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task notificationTask = client.SendNotificationAsync(
			"workspace/didChangeConfiguration",
			new { settings = new { } },
			cancellationSource.Token);

		try
		{
			Task completedTask = await Task.WhenAny(notificationTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
			Assert.AreSame(notificationTask, completedTask);
			await notificationTask.ConfigureAwait(false);
		}
		finally
		{
			blockingWriteStream.Release();
		}
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_RaisesEventAndReturnsNull()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var refreshRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		client.SemanticTokensRefreshRequested += () => refreshRequested.TrySetResult(true);

		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.IsNull(result);
		await refreshRequested.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_ReturnsBeforeSlowHandlerCompletes()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var handlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowHandlerToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		client.SemanticTokensRefreshRequested += () =>
		{
			handlerEntered.TrySetResult(true);
			allowHandlerToFinish.Task.GetAwaiter().GetResult();
		};

		Task<object?> refreshTask = InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync");

		Task completedTask = await Task.WhenAny(refreshTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(refreshTask, completedTask);
		Assert.IsNull(await refreshTask.ConfigureAwait(false));

		await handlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
		allowHandlerToFinish.TrySetResult(true);
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_SlowSubscriberDoesNotBlockLaterSubscriber()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var firstSubscriberEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondSubscriberObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstSubscriberToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		client.SemanticTokensRefreshRequested += () =>
		{
			firstSubscriberEntered.TrySetResult(true);
			allowFirstSubscriberToFinish.Task.GetAwaiter().GetResult();
		};

		client.SemanticTokensRefreshRequested += () => secondSubscriberObserved.TrySetResult(true);

		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.IsNull(result);
		await Task.WhenAll(
			firstSubscriberEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)),
			secondSubscriberObserved.Task.WaitAsync(TimeSpan.FromSeconds(1))).ConfigureAwait(false);

		allowFirstSubscriberToFinish.TrySetResult(true);
	}

	[TestMethod]
	public async Task InvokeSemanticTokensRefreshRequested_WhenSubscriberIsBusy_CoalescesPendingSignalsPerSubscriber()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var firstInvocationEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstInvocationToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondInvocationEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedThirdInvocation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int invocationCount = 0;

		client.SemanticTokensRefreshRequested += () =>
		{
			int currentInvocation = Interlocked.Increment(ref invocationCount);

			if (currentInvocation == 1)
			{
				firstInvocationEntered.TrySetResult(true);
				allowFirstInvocationToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			if (currentInvocation == 2)
			{
				secondInvocationEntered.TrySetResult(true);
				return;
			}

			unexpectedThirdInvocation.TrySetResult(true);
		};

		InvokePrivateMethod(client, "InvokeSemanticTokensRefreshRequested");
		await firstInvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		for (int i = 0; i < 10; i++)
			InvokePrivateMethod(client, "InvokeSemanticTokensRefreshRequested");

		allowFirstInvocationToFinish.TrySetResult(true);
		await secondInvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		Task completedTask = await Task.WhenAny(unexpectedThirdInvocation.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedThirdInvocation.Task, completedTask);
		Assert.AreEqual(2, Volatile.Read(ref invocationCount));
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_CoalescesRepeatedRequestsWhileHandlerIsBusy()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var firstHandlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstHandlerToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondHandlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedThirdHandler = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int refreshRequestedCount = 0;

		SetActiveSession(client, session);
		SetReadyState(client, true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		client.SemanticTokensRefreshRequested += () =>
		{
			int invocationCount = Interlocked.Increment(ref refreshRequestedCount);

			if (invocationCount == 1)
			{
				firstHandlerEntered.TrySetResult(true);
				allowFirstHandlerToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			if (invocationCount == 2)
			{
				secondHandlerEntered.TrySetResult(true);
				return;
			}

			unexpectedThirdHandler.TrySetResult(true);
		};

		Assert.IsNull(await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false));
		await firstHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		Task<object?>[] repeatedRefreshRequests =
		[
			InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync"),
			InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync"),
			InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync"),
			InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync"),
			InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync")
		];

		object?[] repeatedResults = await Task.WhenAll(repeatedRefreshRequests).WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		Assert.IsTrue(repeatedResults.All(result => result is null));

		allowFirstHandlerToFinish.TrySetResult(true);
		await secondHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		Task completedTask = await Task.WhenAny(unexpectedThirdHandler.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedThirdHandler.Task, completedTask);
		Assert.AreEqual(2, Volatile.Read(ref refreshRequestedCount));
	}

	[TestMethod]
	public async Task InvokeSemanticTokensRefreshRequested_AfterUnsubscribe_DoesNotDeliverQueuedCallbacks()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var firstInvocationEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstInvocationToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedSecondInvocation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int invocationCount = 0;

		Action handler = () =>
		{
			int currentInvocation = Interlocked.Increment(ref invocationCount);

			if (currentInvocation == 1)
			{
				firstInvocationEntered.TrySetResult(true);
				allowFirstInvocationToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			unexpectedSecondInvocation.TrySetResult(true);
		};

		client.SemanticTokensRefreshRequested += handler;

		InvokePrivateMethod(client, "InvokeSemanticTokensRefreshRequested");
		await firstInvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		client.SemanticTokensRefreshRequested -= handler;

		for (int i = 0; i < 5; i++)
			InvokePrivateMethod(client, "InvokeSemanticTokensRefreshRequested");

		allowFirstInvocationToFinish.TrySetResult(true);

		Task completedTask = await Task.WhenAny(unexpectedSecondInvocation.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedSecondInvocation.Task, completedTask);
		Assert.AreEqual(1, Volatile.Read(ref invocationCount));
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_IgnoresStaleTransportGeneration()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);
		int refreshRequestedCount = 0;

		SetActiveSession(client, newSession);
		SetReadyState(client, true);

		client.SemanticTokensRefreshRequested += () => refreshRequestedCount++;

		object oldRpcTarget = CreateRpcTarget(client, GetTransportGeneration(oldSession));
		object? result = await InvokePrivateTaskAsync<object?>(oldRpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.AreEqual(0, refreshRequestedCount);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_WhenTransportIsAttachedButNotReady_IgnoresRequest()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		int refreshRequestedCount = 0;

		SetActiveSession(client, session);
		client.SemanticTokensRefreshRequested += () => refreshRequestedCount++;

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.AreEqual(0, refreshRequestedCount);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task HandleSemanticTokensRefreshRequestAsync_IgnoresUnhealthyTransportGeneration()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		int refreshRequestedCount = 0;

		SetActiveSession(client, session);
		SetReadyState(client, true);
		client.SemanticTokensRefreshRequested += () => refreshRequestedCount++;

		client.MarkTransportUnhealthy();

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.AreEqual(0, refreshRequestedCount);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void CapabilityRegistrationParams_Deserialize_BindsRegistrationsWireProperty()
	{
		CapabilityRegistrationParams parameters = JsonSerializer.Deserialize<CapabilityRegistrationParams>(
			"""
			{
			  "registrations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Registrations);
		Assert.AreEqual(1, parameters.Registrations.Length);
		Assert.AreEqual("1", parameters.Registrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Registrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Deserialize_BindsHistoricalWireProperty()
	{
		CapabilityUnregistrationParams parameters = JsonSerializer.Deserialize<CapabilityUnregistrationParams>(
			"""
			{
			  "unregisterations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Unregistrations);
		Assert.AreEqual(1, parameters.Unregistrations.Length);
		Assert.AreEqual("1", parameters.Unregistrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Unregistrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Deserialize_BindsCorrectedPropertyName()
	{
		CapabilityUnregistrationParams parameters = JsonSerializer.Deserialize<CapabilityUnregistrationParams>(
			"""
			{
			  "unregistrations": [
			    { "id": "1", "method": "textDocument/rename" }
			  ]
			}
			""");

		Assert.IsNotNull(parameters.Unregistrations);
		Assert.AreEqual(1, parameters.Unregistrations.Length);
		Assert.AreEqual("1", parameters.Unregistrations[0].Id);
		Assert.AreEqual("textDocument/rename", parameters.Unregistrations[0].Method);
	}

	[TestMethod]
	public void CapabilityUnregistrationParams_Serialize_WritesCurrentSpecPropertyName()
	{
		string json = JsonSerializer.Serialize(
			new CapabilityUnregistrationParams(
			[
				new CapabilityUnregistrationPayload("1", "textDocument/rename")
			]));

		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement root = document.RootElement;

		Assert.IsTrue(root.TryGetProperty("unregisterations", out JsonElement unregistrations));
		Assert.IsFalse(root.TryGetProperty("unregistrations", out _));
		Assert.AreEqual(JsonValueKind.Array, unregistrations.ValueKind);
		Assert.AreEqual(1, unregistrations.GetArrayLength());
	}

	[TestMethod]
	public void GetRequiredReadySession_WhenClientIsNotReady_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethodWithReturn(client, "GetRequiredReadySession", false));

		Assert.IsInstanceOfType(exception.InnerException, typeof(IOException));
	}

	[TestMethod]
	public void GetRequiredReadySession_WhenActiveSessionGenerationDoesNotMatchPublishedReadyGeneration_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object readySession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object replacementSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, readySession);
		SetReadyState(client, true);
		SetPrivateField(client, "_activeSession", replacementSession);

		TargetInvocationException exception = Assert.ThrowsException<TargetInvocationException>(() =>
			InvokePrivateMethodWithReturn(client, "GetRequiredReadySession", false));

		Assert.IsInstanceOfType(exception.InnerException, typeof(IOException));
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenClientIsNotReady_ThrowsIOException()
	{
		using var blockingWriteStream = new BlockingWriteStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, blockingWriteStream, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendNotificationAsync(
				"workspace/didChangeConfiguration",
				new { settings = new { } },
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenClientIsNotReady_ThrowsIOException()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task MarkTransportUnhealthy_DuringInFlightRequest_DoesNotCancelOwnedSessionButBlocksFutureRequests()
	{
		using var serverOutputStream = new PendingReadStream();
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		client.MarkTransportUnhealthy();

		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task ActiveSessionDisconnect_WhileRequestIsInFlight_CompletesPendingRequestAndMarksClientNotReady()
	{
		using var serverOutputStream = new PendingReadStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(requestTask, completedTask);
		Assert.IsFalse(client.IsReady);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenActiveTransportFails_LogsRequestFailureWithGeneration()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var serverOutputStream = new PendingReadStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 7, process: null, serverOutputStream, Stream.Null, startListening: true);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		Task completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromMilliseconds(100))).ConfigureAwait(false);
		Assert.AreNotSame(requestTask, completedTask);

		((JsonRpc)GetPropertyValue(session, "JsonRpc")).Dispose();

		await AssertFaultedOrCanceledAsync(requestTask).ConfigureAwait(false);
		Assert.IsFalse(client.IsReady);

		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("request", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("workspace/configuration", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("failed", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 7", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task HandleDiagnosticsPublished_IgnoresUnhealthyTransportGeneration()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 6, process: null, Stream.Null, Stream.Null);
		var publishedMessage = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		SetReadyState(client, true);
		client.DiagnosticsPublished += parameters => publishedMessage.TrySetResult(parameters.Diagnostics?[0].Message);

		client.MarkTransportUnhealthy();

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		InvokePrivateMethod(rpcTarget,
			"PublishDiagnostics",
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Ignored warning."));

		Task completedTask = await Task.WhenAny(publishedMessage.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(publishedMessage.Task, completedTask);
	}

	[TestMethod]
	public void JsonRpc_Disconnected_LocallyDisposedActiveTransport_LogsExpectedShutdownAtInfo()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			session,
			new JsonRpcDisconnectedEventArgs("active transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Info|", StringComparison.Ordinal)
			&& log.Contains("expected local shutdown", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 3", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task JsonRpc_Disconnected_DuringClientDisposal_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		bool disposeStarted = (bool)InvokePrivateMethodWithReturn(client, "TryBeginDispose");

		Assert.IsTrue(disposeStarted);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			session,
			new JsonRpcDisconnectedEventArgs("disposing transport closed", DisconnectedReason.StreamError));

		Assert.IsFalse(client.IsReady);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("during client disposal", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("generation 4", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("disconnected unexpectedly", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));

		await InvokePrivateTaskAsync(client, "DisposeCoreAsync").ConfigureAwait(false);
	}

	[TestMethod]
	public void MarkTransportUnhealthy_WhenReady_LogsRestartBoundary()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "renameProvider": true,
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));
		SetReadyState(client, true);

		client.MarkTransportUnhealthy();

		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(5L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("generation 5", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("restart", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void MarkTransportUnhealthyForGeneration_StaleGenerationDoesNotOverwriteActiveSnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 5, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 6, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "renameProvider": true
			  }
			}
			"""));
		SetReadyState(client, true);

		InvokePrivateMethod(client, "MarkTransportUnhealthyForGeneration", GetTransportGeneration(oldSession));

		Assert.AreEqual(6L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsRename);
	}

	[TestMethod]
	public void DetachActiveSession_ResetsPublishedCapabilitySnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 9, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "referencesProvider": true,
			    "renameProvider": true,
			    "documentFormattingProvider": true,
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));
		SetReadyState(client, true);

		object detachedSession = InvokePrivateMethodWithReturn(client, "DetachActiveSession");

		Assert.AreSame(session, detachedSession);
		Assert.AreEqual(0L, client.TransportGeneration);
		Assert.IsFalse(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.None, client.TextDocumentSyncKind);
		Assert.AreEqual(0, client.SemanticTokenTypes.Count);
		Assert.AreEqual(0, client.SemanticTokenModifiers.Count);
		Assert.IsFalse(client.SupportsCompletionResolve);
		Assert.IsFalse(client.SupportsReferences);
		Assert.IsFalse(client.SupportsRename);
		Assert.IsFalse(client.SupportsFormatting);
		Assert.IsFalse(client.SupportsSemanticTokensDelta);
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenStartupHandshakeIsInProgress_ThrowsIOException()
	{
		var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var startupCancellation = new CancellationTokenSource();
		await using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: null,
			sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendNotificationAsync(
				"workspace/didChangeConfiguration",
				new { settings = new { } },
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		startupCancellation.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await startTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenStartupHandshakeIsInProgress_ThrowsIOException()
	{
		var sessionActivated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var startupCancellation = new CancellationTokenSource();
		await using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			processStartedTestHook: null,
			sessionActivatedTestHook: cancellationToken => WaitForStartupCancellationAsync(sessionActivated, cancellationToken));

		Task<bool> startTask = client.StartAsync(startupCancellation.Token);

		await sessionActivated.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		await Assert.ThrowsExceptionAsync<IOException>(async () =>
			await client.SendRequestAsync<JsonElement>(
				"workspace/configuration",
				new WorkspaceConfigurationParams([]),
				CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);

		startupCancellation.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await startTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public void RegisterCapability_IgnoresDynamicRegistrationAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "RegisterCapability",
			new CapabilityRegistrationParams(
			[
				new CapabilityRegistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)
			&& log.Contains("generation 1", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("dynamicRegistration = false", StringComparison.Ordinal)
			&& log.Contains("textDocument/rename", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void UnregisterCapability_IgnoresDynamicUnregistrationAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "UnregisterCapability",
			new CapabilityUnregistrationParams(
			[
				new CapabilityUnregistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/unregisterCapability", StringComparison.Ordinal)
			&& log.Contains("generation 2", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("dynamicRegistration = false", StringComparison.Ordinal)
			&& log.Contains("textDocument/rename", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void RegisterCapability_StaleTransportGeneration_LogsDebugWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(oldSession));
		object? result = InvokePrivateMethodAllowingNull(rpcTarget, "RegisterCapability",
			new CapabilityRegistrationParams(
			[
				new CapabilityRegistrationPayload("1", "textDocument/rename")
			]));

		Assert.IsNull(result);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)
			&& log.Contains("generation 3", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("stale", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("client/registerCapability", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void IgnoredUnsupportedCallbacks_LogDebugBoundaryWithoutWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 12, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		Assert.IsNull(InvokePrivateMethodAllowingNull(rpcTarget, "CreateWorkDoneProgress", new EmptyParams()));
		InvokePrivateMethod(rpcTarget, "TelemetryEvent", new EmptyParams());
		InvokePrivateMethod(rpcTarget, "Progress", new EmptyParams());

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("window/workDoneProgress/create", StringComparison.Ordinal)
			&& log.Contains("unsupported server callback", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("telemetry/event", StringComparison.Ordinal)
			&& log.Contains("unsupported server callback", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Debug|", StringComparison.Ordinal)
			&& log.Contains("$/progress", StringComparison.Ordinal)
			&& log.Contains("unsupported server callback", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& (log.Contains("window/workDoneProgress/create", StringComparison.Ordinal)
				|| log.Contains("telemetry/event", StringComparison.Ordinal)
				|| log.Contains("$/progress", StringComparison.Ordinal))),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_CoalescesQueuedDiagnosticsByFile()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		int publishedCount = 0;
		string? lastMessage = null;

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Stale warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Current warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_CoalescesWindowsFileUrisThatDifferOnlyByCase()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		int publishedCount = 0;
		string? lastMessage = null;

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/Test.lua", "First warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///c:/workspace/test.lua", "Latest warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Latest warning.", lastMessage);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_DropsQueuedDiagnosticsFromInactiveTransportGeneration()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);
		int publishedCount = 0;
		string? lastMessage = null;

		SetActiveSession(client, newSession);

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(oldSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/stale.lua", "Stale warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(newSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/current.lua", "Current warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_SameUriStaleGenerationDoesNotOverwriteCurrentGenerationPayload()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);
		int publishedCount = 0;
		string? lastMessage = null;

		SetActiveSession(client, newSession);

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;
			lastMessage = parameters.Diagnostics?[0].Message;
			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(newSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Current warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", GetTransportGeneration(oldSession),
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Stale warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(1, publishedCount);
		Assert.AreEqual("Current warning.", lastMessage);
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_WhenHandlerThrows_LogsWarningAndContinuesProcessing()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		int publishedCount = 0;
		var publishedMessages = new List<string>();

		client.DiagnosticsPublished += parameters =>
		{
			publishedCount++;

			if (parameters.Diagnostics?[0].Message is { } message)
				publishedMessages.Add(message);

			if (publishedCount == 1)
				throw new InvalidOperationException("Simulated diagnostics subscriber failure.");

			CancelLifetime(client);
		};

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/first.lua", "First warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/second.lua", "Second warning."));

		await InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync").ConfigureAwait(false);

		Assert.AreEqual(2, publishedCount);
		CollectionAssert.AreEquivalent(new[] { "First warning.", "Second warning." }, publishedMessages);
		Assert.IsTrue(logScope.Logs.Any(log => log.Contains("Diagnostics handler threw", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("Simulated diagnostics subscriber failure.", StringComparison.Ordinal)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_SlowSubscriberDoesNotBlockLaterSubscriber()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var firstSubscriberEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var firstSubscriberCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondSubscriberObserved = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstSubscriberToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		client.DiagnosticsPublished += parameters =>
		{
			firstSubscriberEntered.TrySetResult(true);
			allowFirstSubscriberToFinish.Task.GetAwaiter().GetResult();
			firstSubscriberCompleted.TrySetResult(true);
		};

		client.DiagnosticsPublished += parameters =>
		{
			secondSubscriberObserved.TrySetResult(parameters.Diagnostics?[0].Message);
			CancelLifetime(client);
		};

		Task diagnosticsPumpTask = InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync");

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Current warning."));

		await Task.WhenAll(
			firstSubscriberEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)),
			secondSubscriberObserved.Task.WaitAsync(TimeSpan.FromSeconds(1))).ConfigureAwait(false);

		Assert.AreEqual("Current warning.", await secondSubscriberObserved.Task.ConfigureAwait(false));

		allowFirstSubscriberToFinish.TrySetResult(true);

		await firstSubscriberCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
		await diagnosticsPumpTask.ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokeDiagnosticsPublished_WhenSubscriberIsBusy_CoalescesPendingPayloadsPerDocument()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		const string uri = "file:///C:/Workspace/test.lua";
		var firstInvocationEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstInvocationToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondInvocationMessage = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedThirdInvocation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int invocationCount = 0;

		client.DiagnosticsPublished += parameters =>
		{
			int currentInvocation = Interlocked.Increment(ref invocationCount);
			string? message = parameters.Diagnostics?[0].Message;

			if (currentInvocation == 1)
			{
				firstInvocationEntered.TrySetResult(true);
				allowFirstInvocationToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			if (currentInvocation == 2)
			{
				secondInvocationMessage.TrySetResult(message);
				return;
			}

			unexpectedThirdInvocation.TrySetResult(true);
		};

		InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "First warning."));
		await firstInvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "Second warning."));
		InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "Third warning."));
		InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "Latest warning."));

		allowFirstInvocationToFinish.TrySetResult(true);
		Assert.AreEqual("Latest warning.", await secondInvocationMessage.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		Task completedTask = await Task.WhenAny(unexpectedThirdInvocation.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedThirdInvocation.Task, completedTask);
		Assert.AreEqual(2, Volatile.Read(ref invocationCount));
	}

	[TestMethod]
	public async Task PumpDiagnosticsAsync_CoalescesRepeatedUpdatesWhileCallbackPumpIsBusy()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		const string uri = "file:///C:/Workspace/test.lua";
		var firstHandlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstHandlerToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondHandlerMessage = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedThirdHandler = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int publishedCount = 0;

		client.DiagnosticsPublished += parameters =>
		{
			int invocationCount = Interlocked.Increment(ref publishedCount);
			string? message = parameters.Diagnostics?[0].Message;

			if (invocationCount == 1)
			{
				firstHandlerEntered.TrySetResult(true);
				allowFirstHandlerToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			if (invocationCount == 2)
			{
				secondHandlerMessage.TrySetResult(message);
				CancelLifetime(client);
				return;
			}

			unexpectedThirdHandler.TrySetResult(true);
		};

		Task diagnosticsPumpTask = InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync");

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters(uri, "First warning."));
		await firstHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters(uri, "Second warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters(uri, "Third warning."));
		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L, CreateDiagnosticsParameters(uri, "Latest warning."));

		allowFirstHandlerToFinish.TrySetResult(true);

		Assert.AreEqual("Latest warning.", await secondHandlerMessage.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));
		await diagnosticsPumpTask.ConfigureAwait(false);

		Task completedTask = await Task.WhenAny(unexpectedThirdHandler.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedThirdHandler.Task, completedTask);
		Assert.AreEqual(2, Volatile.Read(ref publishedCount));
	}

	[TestMethod]
	public async Task InvokeDiagnosticsPublished_AfterUnsubscribe_DoesNotDeliverQueuedCallbacks()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		const string uri = "file:///C:/Workspace/test.lua";
		var firstInvocationEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowFirstInvocationToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var unexpectedSecondInvocation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int invocationCount = 0;

		Action<PublishDiagnosticsParams> handler = _ =>
		{
			int currentInvocation = Interlocked.Increment(ref invocationCount);

			if (currentInvocation == 1)
			{
				firstInvocationEntered.TrySetResult(true);
				allowFirstInvocationToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			unexpectedSecondInvocation.TrySetResult(true);
		};

		client.DiagnosticsPublished += handler;

		InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "First warning."));
		await firstInvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

		client.DiagnosticsPublished -= handler;

		for (int i = 0; i < 5; i++)
			InvokePrivateMethod(client, "InvokeDiagnosticsPublished", uri, CreateDiagnosticsParameters(uri, "Later warning."));

		allowFirstInvocationToFinish.TrySetResult(true);

		Task completedTask = await Task.WhenAny(unexpectedSecondInvocation.Task, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(unexpectedSecondInvocation.Task, completedTask);
		Assert.AreEqual(1, Volatile.Read(ref invocationCount));
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsRequestedLuaSections()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Lua = new
			{
				runtime = new
				{
					version = "Lua 5.4"
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(response[1]).GetProperty("version").GetString());
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsRequestedNestedNonLuaSections()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Editor = new
			{
				theme = "Dark",
				fonts = new
				{
					size = 14
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Editor"),
				new WorkspaceConfigurationItem("Editor.fonts")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.AreEqual("Dark", JsonSerializer.SerializeToElement(response[0]).GetProperty("theme").GetString());
		Assert.AreEqual(14, JsonSerializer.SerializeToElement(response[1]).GetProperty("size").GetInt32());
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsNullWhenLuaSectionIsMissing()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Editor = new
			{
				theme = "Dark"
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);
	}

	[TestMethod]
	public void BuildConfigurationResponse_TypedSettingsObject_MatchesNestedSectionCaseInsensitively()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new TestConfigurationRoot
		{
			Lua = new TestLuaConfiguration
			{
				Runtime = new TestLuaRuntimeConfiguration
				{
					Version = "Lua 5.4"
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(1, response.Length);
		Assert.IsNotNull(response[0]);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(response[0]).GetProperty("version").GetString());
	}

	[TestMethod]
	public void WorkspaceConfiguration_WhenSettingsSerializationFails_ReturnsNullValuesAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);
		var cyclicSettings = new Dictionary<string, object?>();
		cyclicSettings["self"] = cyclicSettings;
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(() => cyclicSettings));
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		object[] response = (object[])InvokePrivateMethodWithReturn(rpcTarget,
			"WorkspaceConfiguration",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);
		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("workspace/configuration", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("returning null values", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void BuildInitializeParams_UsesInjectedInitializationOptions()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new { },
			InitializationOptionsProvider = static workspaceRoot => new
			{
				workspace = workspaceRoot,
				customFlag = true
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement initializationOptions = initializeParams.GetProperty("initializationOptions");

		Assert.AreEqual(@"C:\Workspace", initializationOptions.GetProperty("workspace").GetString());
		Assert.IsTrue(initializationOptions.GetProperty("customFlag").GetBoolean());
	}

	[TestMethod]
	public void BuildInitializeParams_UsesInjectedClientCapabilities()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static workspaceRoot => new
			{
				workspace = new
				{
					workspaceFolders = true,
					configuration = true,
					root = workspaceRoot
				}
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement capabilities = initializeParams.GetProperty("capabilities");

		Assert.IsTrue(capabilities.GetProperty("workspace").GetProperty("workspaceFolders").GetBoolean());
		Assert.IsTrue(capabilities.GetProperty("workspace").GetProperty("configuration").GetBoolean());
		Assert.AreEqual(@"C:\Workspace", capabilities.GetProperty("workspace").GetProperty("root").GetString());
	}

	[TestMethod]
	public void BuildInitializeParams_ForcesUnsupportedDynamicRegistrationFlagsToFalse()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new
			{
				workspace = new
				{
					didChangeWatchedFiles = new { dynamicRegistration = true }
				},
				textDocument = new
				{
					rename = new { dynamicRegistration = true, prepareSupport = true },
					references = new { dynamicRegistration = true },
					formatting = new { dynamicRegistration = true },
					completion = new { dynamicRegistration = true }
				}
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement capabilities = initializeParams.GetProperty("capabilities");

		Assert.IsFalse(capabilities.GetProperty("workspace").GetProperty("didChangeWatchedFiles").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("rename").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("references").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("formatting").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("completion").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsTrue(capabilities.GetProperty("textDocument").GetProperty("rename").GetProperty("prepareSupport").GetBoolean());
	}

	[TestMethod]
	public void BuildInitializeParams_NormalizesWorkspaceRootAndFolderName()
	{
		using var client = new LanguageServerClient(@"C:/Workspace/", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new { },
			InitializationOptionsProvider = static workspaceRoot => new
			{
				workspace = workspaceRoot
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement workspaceFolder = initializeParams.GetProperty("workspaceFolders")[0];
		string expectedWorkspaceRoot = LanguageServerPathHelper.NormalizeLocalPath(@"C:/Workspace/");

		Assert.AreEqual(expectedWorkspaceRoot, initializeParams.GetProperty("initializationOptions").GetProperty("workspace").GetString());
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(expectedWorkspaceRoot), initializeParams.GetProperty("rootUri").GetString());
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(expectedWorkspaceRoot), workspaceFolder.GetProperty("uri").GetString());
		Assert.AreEqual("Workspace", workspaceFolder.GetProperty("name").GetString());
	}

	[TestMethod]
	public void WorkspaceFolders_ReturnsDriveRootNameWhenWorkspaceRootIsDriveRoot()
	{
		using var client = new LanguageServerClient(@"C:\", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		WorkspaceFolder[] workspaceFolders = (WorkspaceFolder[])InvokePrivateMethodWithReturn(rpcTarget, "WorkspaceFolders");

		Assert.AreEqual(1, workspaceFolders.Length);
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(@"C:\"), workspaceFolders[0].Uri);
		Assert.AreEqual("C:", workspaceFolders[0].Name);
	}

	[TestMethod]
	public void WorkspaceConfiguration_StaleTransportGeneration_ReturnsNullValuesWithoutReadingActiveSettings()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Lua = new
			{
				Runtime = new
				{
					Version = "5.4"
				}
			}
		}));
		object staleSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object activeSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, activeSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(staleSession));
		object[] response = (object[])InvokePrivateMethodWithReturn(rpcTarget,
			"WorkspaceConfiguration",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);
	}

	[TestMethod]
	public void WorkspaceFolders_StaleTransportGeneration_ReturnsEmptyArray()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object staleSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object activeSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, activeSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(staleSession));
		WorkspaceFolder[] workspaceFolders = (WorkspaceFolder[])InvokePrivateMethodWithReturn(rpcTarget, "WorkspaceFolders");

		Assert.AreEqual(0, workspaceFolders.Length);
	}

	[TestMethod]
	public async Task SendRequestAsync_WhenTransportGenerationIsReplacedBeforeSuccessfulResponse_CompletesWithIOException()
	{
		using var deferredServerOutputStream = new DeferredJsonRpcResponseStream();
		using var serverInputStream = new RecordingStream();
		await using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object originalSession = CreateTransportSession(client, 1, process: null, deferredServerOutputStream, serverInputStream, startListening: true);
		object replacementSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, originalSession);
		SetReadyState(client, true);

		Task<JsonElement> requestTask = client.SendRequestAsync<JsonElement>(
			"workspace/configuration",
			new WorkspaceConfigurationParams([]),
			CancellationToken.None);

		int requestId = await WaitForRequestIdAsync(serverInputStream).ConfigureAwait(false);

		SetActiveSession(client, replacementSession);
		SetReadyState(client, true);
		deferredServerOutputStream.SetPayload(CreateJsonRpcResultMessage(requestId, "{\"value\":1}"));

		await Assert.ThrowsExceptionAsync<IOException>(async () => await requestTask.ConfigureAwait(false)).ConfigureAwait(false);
	}

	[TestMethod]
	public void JsonRpc_Disconnected_OldTransportGenerationDoesNotAffectActiveSession()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			oldSession,
			new JsonRpcDisconnectedEventArgs("old transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsTrue(client.IsReady);

		InvokePrivateMethod(client, "JsonRpc_Disconnected",
			newSession,
			new JsonRpcDisconnectedEventArgs("active transport closed", DisconnectedReason.LocallyDisposed));

		Assert.IsFalse(client.IsReady);
		Assert.IsNull(GetPrivateFieldAllowingNull(client, "_activeSession"));
	}

	[TestMethod]
	public void Process_Exited_SupersededTransportGenerationDoesNotAffectActiveSessionOrWarn()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "Process_Exited", oldSession);

		Assert.AreEqual(2L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);
		Assert.IsFalse(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("generation 1", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("exited unexpectedly", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void Process_Exited_ActiveTransport_DetachesActiveSessionImmediately()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);

		InvokePrivateMethod(client, "Process_Exited", session);

		Assert.IsFalse(client.IsReady);
		Assert.IsNull(GetPrivateFieldAllowingNull(client, "_activeSession"));
	}

	[TestMethod]
	public async Task DisposeAsync_WaitsForDetachedFailedSessionCleanup()
	{
		var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var queuedCleanup = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		SetPrivateField(client, "_queuedFailedSessionDisposal", queuedCleanup.Task);

		Task disposeTask = client.DisposeAsync().AsTask();
		Task completedTask = await Task.WhenAny(disposeTask, Task.Delay(TimeSpan.FromMilliseconds(200))).ConfigureAwait(false);

		Assert.AreNotSame(disposeTask, completedTask);

		queuedCleanup.TrySetResult(true);
		await disposeTask.ConfigureAwait(false);
	}

	[TestMethod]
	public void SetCapabilityReadinessForGeneration_StaleGenerationDoesNotClearActiveSnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		InvokePrivateMethod(client, "CaptureServerCapabilities", DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "documentFormattingProvider": true
			  }
			}
			"""));
		SetReadyState(client, true);

		InvokePrivateMethod(client, "SetCapabilityReadinessForGeneration", GetTransportGeneration(oldSession), false);

		Assert.AreEqual(2L, client.TransportGeneration);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsFormatting);
	}

	[TestMethod]
	public void CaptureServerCapabilitiesForGeneration_StaleGenerationDoesNotOverwriteActiveCapabilities()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object oldSession = CreateTransportSession(client, 3, process: null, Stream.Null, Stream.Null);
		object newSession = CreateTransportSession(client, 4, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, newSession);
		InvokePrivateMethod(client, "CaptureServerCapabilitiesForGeneration",
			GetTransportGeneration(newSession),
			DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 1
			    },
			    "semanticTokensProvider": {
			      "full": {
			        "delta": true
			      },
			      "legend": {
			        "tokenTypes": ["function"],
			        "tokenModifiers": ["declaration"]
			      }
			    }
			  }
			}
			"""));

		InvokePrivateMethod(client, "CaptureServerCapabilitiesForGeneration",
			GetTransportGeneration(oldSession),
			DeserializeInitializeResponse(
			"""
			{
			  "capabilities": {
			    "textDocumentSync": {
			      "change": 2
			    },
			    "renameProvider": true
			  }
			}
			"""));

		Assert.AreEqual(4L, client.TransportGeneration);
		Assert.AreEqual(TextDocumentSyncKind.Full, client.TextDocumentSyncKind);
		Assert.IsTrue(client.SupportsSemanticTokensDelta);
		CollectionAssert.AreEqual(new[] { "function" }, client.SemanticTokenTypes.ToArray());
		CollectionAssert.AreEqual(new[] { "declaration" }, client.SemanticTokenModifiers.ToArray());
		Assert.IsFalse(client.SupportsRename);
	}

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

	private static Process StartDisposableProcess()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			Arguments = "/c ping 127.0.0.1 -n 10 > nul",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		return Process.Start(startInfo)
			?? throw new InvalidOperationException("Unable to start the disposable test process.");
	}

	private static Process StartShortLivedProcess()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			Arguments = "/c ping 127.0.0.1 -n 1 -w 200 > nul",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		return Process.Start(startInfo)
			?? throw new InvalidOperationException("Unable to start the short-lived disposable test process.");
	}

	private static string CreateJsonRpcResultMessage(int id, string resultJson)
	{
		string payload = "{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":" + resultJson + "}";
		int payloadLength = Encoding.UTF8.GetByteCount(payload);
		return "Content-Length: " + payloadLength + "\r\n\r\n" + payload;
	}

	private static SemaphoreSlim GetStartLock(LanguageServerClient client)
	{
		FieldInfo field = typeof(LanguageServerClient).GetField("_startLock", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_startLock' was not found.");

		return (SemaphoreSlim)(field.GetValue(client)
			?? throw new InvalidOperationException("Client start lock was null."));
	}

	private static void SetPrivateField(object instance, string fieldName, object? value)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		field.SetValue(instance, value);
	}

	private static object CreateTransportSession(LanguageServerClient client, long generation, Process? process, Stream serverOutputStream, Stream serverInputStream, bool startListening = false)
	{
		Type sessionType = typeof(LanguageServerClient).GetNestedType("LanguageServerTransportSession", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LanguageServerTransportSession' was not found.");

		ConstructorInfo constructor = sessionType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(long), typeof(Process), typeof(Stream), typeof(Stream)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua transport session constructor was not found.");

		object session = constructor.Invoke([generation, process, serverOutputStream, serverInputStream]);
		object messageHandler = InvokePrivateStaticMethodWithReturn(typeof(LanguageServerClient), "CreateMessageHandler", serverInputStream, serverOutputStream);
		object rpcTarget = CreateRpcTarget(client, generation);

		SetSessionProperty(session, "MessageHandler", messageHandler);
		SetSessionProperty(session, "RpcTarget", rpcTarget);
		object jsonRpc = InvokePrivateMethodWithReturn(client, "CreateJsonRpc", session);
		SetSessionProperty(session, "JsonRpc", jsonRpc);
		SetSessionProperty(session, "RpcCompletionTask", GetPropertyValue(jsonRpc, "Completion"));

		if (startListening)
			((JsonRpc)jsonRpc).StartListening();

		return session;
	}

	private static object CreateRpcTarget(LanguageServerClient client, long generation = 0)
	{
		Type targetType = typeof(LanguageServerClient).GetNestedType("LanguageServerClientRpcTarget", BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Nested type 'LanguageServerClientRpcTarget' was not found.");

		ConstructorInfo constructor = targetType.GetConstructor(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			binder: null,
			[typeof(LanguageServerClient), typeof(long)],
			modifiers: null)
			?? throw new InvalidOperationException("Lua RPC target constructor was not found.");

		return constructor.Invoke([client, generation]);
	}

	private static object GetPropertyValue(object instance, string propertyName)
	{
		PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");

		return property.GetValue(instance)
			?? throw new InvalidOperationException($"Property '{propertyName}' returned null.");
	}

	private static object GetPrivateField(object instance, string fieldName)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return field.GetValue(instance)
			?? throw new InvalidOperationException($"Private field '{fieldName}' returned null.");
	}

	private static object? GetPrivateFieldAllowingNull(object instance, string fieldName)
	{
		FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private field '{fieldName}' was not found.");

		return field.GetValue(instance);
	}

	private static void SetSessionProperty(object session, string propertyName, object? value)
	{
		PropertyInfo property = session.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Transport session property '{propertyName}' was not found.");

		property.SetValue(session, value);
	}

	private static void SetActiveSession(LanguageServerClient client, object session)
	{
		InvokePrivateMethod(client, "SetActiveSession", session);
	}

	private static void SetReadyState(LanguageServerClient client, bool isReady)
	{
		InvokePrivateMethod(client, "SetCapabilityReadiness", isReady);
	}

	private static long GetTransportGeneration(object session)
	{
		PropertyInfo property = session.GetType().GetProperty("Generation", BindingFlags.Instance | BindingFlags.Public)
			?? throw new InvalidOperationException("Transport session property 'Generation' was not found.");

		return (long)(property.GetValue(session)
			?? throw new InvalidOperationException("Transport session generation value was null."));
	}

	private static void RecordStandardErrorLine(object session, string line)
	{
		MethodInfo method = session.GetType().GetMethod("RecordStandardErrorLine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Transport session method 'RecordStandardErrorLine' was not found.");

		method.Invoke(session, [line]);
	}

	private static void InvokePrivateMethod(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		method.Invoke(instance, parameters);
	}

	private static object InvokePrivateMethodWithReturn(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		return method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null.");
	}

	private static object? InvokePrivateMethodAllowingNull(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		return method.Invoke(instance, parameters);
	}

	private static object InvokePrivateStaticMethodWithReturn(Type type, string methodName, params object?[] parameters)
	{
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private static method '{methodName}' was not found.");

		return method.Invoke(obj: null, parameters)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null.");
	}

	private static async Task InvokePrivateStaticTaskAsync(Type type, string methodName, params object?[] parameters)
	{
		MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private static method '{methodName}' was not found.");

		Task task = (Task)(method.Invoke(obj: null, parameters)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null instead of a Task."));

		await task.ConfigureAwait(false);
	}

	private static async Task InvokePrivateTaskAsync(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Method '{methodName}' was not found.");

		Task task = (Task)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Method '{methodName}' returned null instead of a Task."));

		await task.ConfigureAwait(false);
	}

	private static async Task<T> InvokePrivateTaskAsync<T>(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Method '{methodName}' was not found.");

		Task<T> task = (Task<T>)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Method '{methodName}' returned null instead of a Task."));

		return await task.ConfigureAwait(false);
	}

	private static InitializeResponse DeserializeInitializeResponse(string json)
		=> JsonSerializer.Deserialize<InitializeResponse>(json)
			?? throw new InvalidOperationException("Failed to deserialize the Lua initialize response test payload.");

	private static PublishDiagnosticsParams CreateDiagnosticsParameters(string uri, string message)
		=> new(
			uri,
			Version: null,
			Diagnostics:
			[
				new DiagnosticPayload(
					new ProtocolRangePayload(
						new ProtocolNullablePosition(0, 0),
						new ProtocolNullablePosition(0, 1)),
					Severity: null,
					Message: message,
					Source: null,
					Code: null)
			]);

	private static void CancelLifetime(LanguageServerClient client)
	{
		FieldInfo field = typeof(LanguageServerClient).GetField("_lifetimeCts", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_lifetimeCts' was not found.");

		((CancellationTokenSource)field.GetValue(client)!).Cancel();
	}

	private static async Task WaitForStartupCancellationAsync(TaskCompletionSource<bool> sessionActivated, CancellationToken cancellationToken)
	{
		sessionActivated.TrySetResult(true);
		await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
	}

	private static async Task AssertCanceledAsync(Task task)
	{
		try
		{
			await task.ConfigureAwait(false);
			Assert.Fail("Expected the task to be canceled.");
		}
		catch (OperationCanceledException)
		{ }
	}

	private static async Task AssertFaultedOrCanceledAsync(Task task)
	{
		try
		{
			await task.ConfigureAwait(false);
			Assert.Fail("Expected the task to fault or be canceled.");
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception)
		{ }
	}

	private static async Task<bool> WaitForProcessExitAsync(int processId)
	{
		for (int attempt = 0; attempt < 20; attempt++)
		{
			try
			{
				using Process process = Process.GetProcessById(processId);

				if (process.HasExited)
					return true;
			}
			catch (ArgumentException)
			{
				return true;
			}

			await Task.Delay(50).ConfigureAwait(false);
		}

		return false;
	}

	private static async Task<int> WaitForRequestIdAsync(RecordingStream stream)
	{
		for (int attempt = 0; attempt < 20; attempt++)
		{
			byte[] writtenPayload = stream.GetWrittenBytes();

			if (TryExtractJsonRpcRequestId(writtenPayload, out int requestId))
				return requestId;

			await Task.Delay(25).ConfigureAwait(false);
		}

		throw new AssertFailedException("Timed out waiting for the JSON-RPC request payload to be written.");
	}

	private static bool TryExtractJsonRpcRequestId(byte[] writtenPayload, out int requestId)
	{
		requestId = 0;

		if (writtenPayload.Length == 0)
			return false;

		string payloadText = Encoding.UTF8.GetString(writtenPayload);
		int bodySeparatorIndex = payloadText.IndexOf("\r\n\r\n", StringComparison.Ordinal);

		if (bodySeparatorIndex < 0)
			return false;

		string headerText = payloadText[..bodySeparatorIndex];
		const string contentLengthPrefix = "Content-Length:";
		int contentLengthLineIndex = headerText.IndexOf(contentLengthPrefix, StringComparison.OrdinalIgnoreCase);

		if (contentLengthLineIndex < 0)
			throw new AssertFailedException("The JSON-RPC request payload did not contain a Content-Length header.");

		int contentLengthValueStart = contentLengthLineIndex + contentLengthPrefix.Length;
		int contentLengthValueEnd = headerText.IndexOf("\r\n", contentLengthValueStart, StringComparison.Ordinal);
		string contentLengthText = (contentLengthValueEnd >= 0
			? headerText[contentLengthValueStart..contentLengthValueEnd]
			: headerText[contentLengthValueStart..]).Trim();

		if (!int.TryParse(contentLengthText, out int contentLength) || contentLength < 0)
			throw new AssertFailedException("The JSON-RPC request payload contained an invalid Content-Length header.");

		int bodyStartIndex = bodySeparatorIndex + 4;

		if (writtenPayload.Length < bodyStartIndex + contentLength)
			return false;

		string jsonPayload = Encoding.UTF8.GetString(writtenPayload, bodyStartIndex, contentLength);
		using JsonDocument document = JsonDocument.Parse(jsonPayload);

		if (!document.RootElement.TryGetProperty("id", out JsonElement idElement)
			|| !idElement.TryGetInt32(out requestId))
		{
			throw new AssertFailedException("The JSON-RPC request payload did not contain an integer request id.");
		}

		return true;
	}

	private sealed class RecordingStream : Stream
	{
		private readonly object _syncRoot = new();
		private readonly MemoryStream _innerStream = new();

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => _innerStream.Length;

		public override long Position
		{
			get
			{
				lock (_syncRoot)
					return _innerStream.Position;
			}
			set
			{
				lock (_syncRoot)
					_innerStream.Position = value;
			}
		}

		public byte[] GetWrittenBytes()
		{
			lock (_syncRoot)
				return _innerStream.ToArray();
		}

		public string GetWrittenText()
			=> Encoding.UTF8.GetString(GetWrittenBytes());

		public override void Flush()
		{
			lock (_syncRoot)
				_innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
		{
			lock (_syncRoot)
				_innerStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (_syncRoot)
				_innerStream.Write(buffer, offset, count);
		}

		public override void Write(ReadOnlySpan<byte> buffer)
		{
			lock (_syncRoot)
				_innerStream.Write(buffer);
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			lock (_syncRoot)
			{
				_innerStream.Write(buffer.Span);
				return ValueTask.CompletedTask;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_syncRoot)
					_innerStream.Flush();
			}

			base.Dispose(disposing);
		}
	}

	private sealed class BlockingWriteStream : Stream
	{
		private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public void Release()
			=> _release.TrySetResult(true);

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override void Write(ReadOnlySpan<byte> buffer)
			=> throw new NotSupportedException();

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
			=> new(WaitForReleaseAsync(cancellationToken));

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			=> WaitForReleaseAsync(cancellationToken);

		public override void Flush()
		{ }

		public override Task FlushAsync(CancellationToken cancellationToken)
			=> Task.CompletedTask;

		private async Task WaitForReleaseAsync(CancellationToken cancellationToken)
		{
			if (_release.Task.IsCompleted)
				return;

			if (!cancellationToken.CanBeCanceled)
			{
				await _release.Task.ConfigureAwait(false);
				return;
			}

			await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	private sealed class PendingReadStream : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			try
			{
				await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{ }

			return 0;
		}

		public override void Flush()
		{ }

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();
	}

	private sealed class DelayedJsonRpcResponseStream : Stream
	{
		private readonly byte[] _payloadBytes;
		private readonly TimeSpan _delay;
		private int _position;
		private int _delayApplied;

		public DelayedJsonRpcResponseStream(string payload, TimeSpan delay)
		{
			_payloadBytes = Encoding.UTF8.GetBytes(payload);
			_delay = delay;
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _payloadBytes.Length;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (Interlocked.Exchange(ref _delayApplied, 1) == 0 && _delay > TimeSpan.Zero)
				await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);

			if (_position >= _payloadBytes.Length)
				return 0;

			int bytesToCopy = Math.Min(buffer.Length, _payloadBytes.Length - _position);
			_payloadBytes.AsMemory(_position, bytesToCopy).CopyTo(buffer);
			_position += bytesToCopy;
			return bytesToCopy;
		}

		public override void Flush()
		{ }

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();
	}

	private sealed class DeferredJsonRpcResponseStream : Stream
	{
		private readonly TaskCompletionSource<byte[]> _payloadSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private byte[]? _payloadBytes;
		private int _position;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _payloadBytes?.Length ?? 0;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public void SetPayload(string payload)
			=> _payloadSource.TrySetResult(Encoding.UTF8.GetBytes(payload));

		public override int Read(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			_payloadBytes ??= await _payloadSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

			if (_position >= _payloadBytes.Length)
				return 0;

			int bytesToCopy = Math.Min(buffer.Length, _payloadBytes.Length - _position);
			_payloadBytes.AsMemory(_position, bytesToCopy).CopyTo(buffer);
			_position += bytesToCopy;
			return bytesToCopy;
		}

		public override void Flush()
		{ }

		public override long Seek(long offset, SeekOrigin origin)
			=> throw new NotSupportedException();

		public override void SetLength(long value)
			=> throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
			=> throw new NotSupportedException();
	}

	private sealed class TestConfigurationRoot
	{
		public TestLuaConfiguration? Lua { get; init; }
	}

	private sealed class TestLuaConfiguration
	{
		public TestLuaRuntimeConfiguration? Runtime { get; init; }
	}

	private sealed class TestLuaRuntimeConfiguration
	{
		public string? Version { get; init; }
	}

	private sealed class NLogMemoryScope : IDisposable
	{
		private readonly LoggingConfiguration? _previousConfiguration;

		public NLogMemoryScope(LogLevel minLevel)
		{
			_previousConfiguration = LogManager.Configuration;

			var target = new MemoryTarget("LanguageServerClientTests")
			{
				Layout = "${level}|${message}|${exception:format=Message}"
			};

			var configuration = new LoggingConfiguration();
			configuration.AddTarget(target);
			configuration.AddRule(minLevel, LogLevel.Fatal, target);

			LogManager.Configuration = configuration;
			LogManager.ReconfigExistingLoggers();

			Target = target;
		}

		public MemoryTarget Target { get; }

		public IList<string> Logs => Target.Logs;

		public void Dispose()
		{
			LogManager.Configuration = _previousConfiguration;
			LogManager.ReconfigExistingLoggers();
		}
	}

}
