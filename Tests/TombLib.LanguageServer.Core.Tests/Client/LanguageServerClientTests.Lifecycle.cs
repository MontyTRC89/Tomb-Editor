using Microsoft.Extensions.Logging;
using Nickelony.LanguageServer.Testing;
using StreamJsonRpc;
using System.Diagnostics;
using System.Reflection;

namespace Nickelony.LanguageServer.Core.Tests;

public partial class LanguageServerClientTests
{
	[TestMethod]
	public void Dispose_WritesGracefulShutdownMessagesAndLogsGracefulAttempt()
	{
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		Process? startedProcess = null;
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			logScope.CreateLogger<LanguageServerClient>(),
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using var startupCancellation = new CancellationTokenSource();
		var processStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowStartupToContinue = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			logScope.CreateLogger<LanguageServerClient>(),
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		var processStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowStartupToContinue = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		int? startedProcessId = null;

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			DefaultClientOptions,
			logScope.CreateLogger<LanguageServerClient>(),
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();
		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
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
		}, logScope.CreateLogger<LanguageServerClient>());

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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using Process process = StartDisposableProcess();
		using var serverOutputStream = new PendingReadStream();
		using var serverInputStream = new RecordingStream();

		using var client = new LanguageServerClient(@"C:\Workspace", process.StartInfo.FileName, new LanguageServerClientOptions(static () => new { })
		{
			ShutdownRequestTimeout = TimeSpan.FromMilliseconds(50)
		}, logScope.CreateLogger<LanguageServerClient>());

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
		using var logScope = new TestLoggerScope(LogLevel.Warning);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
			null,
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		var initializeStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		using var client = new LanguageServerClient(
			@"C:\Workspace",
			Path.Combine(Environment.SystemDirectory, "cmd.exe"),
			new LanguageServerClientOptions(static () => new { })
			{
				InitializeTimeout = TimeSpan.FromMilliseconds(50)
			},
			logScope.CreateLogger<LanguageServerClient>(),
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
				null,
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
}
