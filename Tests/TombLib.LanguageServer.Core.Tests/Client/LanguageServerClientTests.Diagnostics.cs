using Microsoft.Extensions.Logging;
using Nickelony.LanguageServer.Testing;

namespace Nickelony.LanguageServer.Core.Tests;

public partial class LanguageServerClientTests
{
	[TestMethod]
	public async Task HandleDiagnosticsPublished_WhenTransportIsAttachedButNotReady_QueuesDiagnostics()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 6, process: null, Stream.Null, Stream.Null);
		var publishedMessage = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		InvokePrivateMethod(client, "EnsureTransportBackgroundLoopsRunning", true);
		client.DiagnosticsPublished += parameters => publishedMessage.TrySetResult(parameters.Diagnostics?[0].Message);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		InvokePrivateMethod(rpcTarget,
			"PublishDiagnostics",
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Initial warning."));

		Assert.AreEqual("Initial warning.", await publishedMessage.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));
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
		using var logScope = new TestLoggerScope(LogLevel.Debug);
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions, logScope.CreateLogger<LanguageServerClient>());
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
	public async Task PumpDiagnosticsAsync_SubscribersReceiveIndependentDiagnosticsSnapshots()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		var firstSubscriberMutated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondSubscriberObserved = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

		client.DiagnosticsPublished += parameters =>
		{
			if (parameters.Diagnostics is { Length: > 0 } diagnostics)
			{
				diagnostics[0] = diagnostics[0] with { Message = "Mutated warning." };
			}

			firstSubscriberMutated.TrySetResult(true);
		};

		client.DiagnosticsPublished += parameters =>
		{
			firstSubscriberMutated.Task.GetAwaiter().GetResult();
			secondSubscriberObserved.TrySetResult(parameters.Diagnostics?[0].Message);
			CancelLifetime(client);
		};

		Task diagnosticsPumpTask = InvokePrivateTaskAsync(client, "PumpDiagnosticsAsync");

		InvokePrivateMethod(client, "RaiseDiagnosticsPublished", 0L,
			CreateDiagnosticsParameters("file:///C:/Workspace/test.lua", "Original warning."));

		Assert.AreEqual("Original warning.",
			await secondSubscriberObserved.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));

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

		void handler(PublishDiagnosticsParams _)
		{
			int currentInvocation = Interlocked.Increment(ref invocationCount);

			if (currentInvocation == 1)
			{
				firstInvocationEntered.TrySetResult(true);
				allowFirstInvocationToFinish.Task.GetAwaiter().GetResult();
				return;
			}

			unexpectedSecondInvocation.TrySetResult(true);
		}

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
}
