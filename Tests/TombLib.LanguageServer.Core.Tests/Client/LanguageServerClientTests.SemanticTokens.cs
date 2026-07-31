namespace Nickelony.LanguageServer.Core.Tests;

public partial class LanguageServerClientTests
{
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

		void handler()
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
	public async Task HandleSemanticTokensRefreshRequestAsync_WhenTransportIsAttachedButNotReady_DeliversRefreshCallback()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		var refreshRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		SetActiveSession(client, session);
		client.SemanticTokensRefreshRequested += () => refreshRequested.TrySetResult(true);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));
		object? result = await InvokePrivateTaskAsync<object?>(rpcTarget, "RefreshSemanticTokensAsync").ConfigureAwait(false);

		Assert.IsNull(result);
		await refreshRequested.Task.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
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
}
