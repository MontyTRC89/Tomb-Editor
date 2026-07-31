using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerIntellisenseProviderTests
{
	[TestMethod]
	public async Task GetHoverAsync_RetriesWorkspaceWatcherStartAfterWorkspaceDirectoryAppears()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRetry_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
			var failures = new List<WorkspaceWatcherFailure>();

			provider.WorkspaceWatcherFailed += failure => failures.Add(failure);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			Assert.IsNull(GetWorkspaceWatcher(provider));
			Assert.AreEqual(0, failures.Count);

			Directory.CreateDirectory(workspaceRoot);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			Assert.IsNotNull(GetWorkspaceWatcher(provider));
			Assert.AreEqual(0, failures.Count);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task GetHoverAsync_WatcherStartupFailure_RaisesWorkspaceWatcherFailureOnce()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherStartupFailure_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var client = new FakeLanguageServerClient();

			using var provider = new LuaLanguageServerIntellisenseProvider(
				workspaceRoot,
				client,
				workspaceFileWatcherFactory: (rootPath, dispatchAsync, watcherFailed) => new WorkspaceFileWatcher(
					rootPath,
					dispatchAsync,
					LuaLanguageServerIntellisenseProvider.WorkspaceWatchSpecifications,
					watcherFailed,
					static (_, _) => throw new InvalidOperationException("Simulated watcher creation failure.")));

			var failures = new List<WorkspaceWatcherFailure>();

			provider.WorkspaceWatcherFailed += failures.Add;

			await provider.GetHoverAsync(filePath, content, 0, 0);
			await provider.GetHoverAsync(filePath, content, 0, 0);

			Assert.IsNull(GetWorkspaceWatcher(provider));
			Assert.AreEqual(1, failures.Count);
			Assert.IsTrue(failures[0].Message.Contains("could not be started", StringComparison.OrdinalIgnoreCase));
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherFailure_AutomaticallyRestartsWithoutRaisingFailure()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherFailure_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
			var failures = new List<WorkspaceWatcherFailure>();

			provider.WorkspaceWatcherFailed += failures.Add;

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			Assert.IsTrue(watcher.HasActiveWatchers);

#pragma warning disable CS0618
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure."));
#pragma warning restore CS0618

			WorkspaceFileWatcher replacementWatcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to restart.");

			Assert.AreNotSame(watcher, replacementWatcher);
			Assert.IsFalse(watcher.HasActiveWatchers);
			Assert.IsTrue(replacementWatcher.HasActiveWatchers);
			Assert.AreEqual(0, failures.Count);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherFailure_WhenReplacementRestartFails_DisposesBothWatchersAndRaisesFailure()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherFailure_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var client = new FakeLanguageServerClient();
			var createdWatchers = new List<WorkspaceFileWatcher>();
			int watcherCreationCount = 0;

			using var provider = new LuaLanguageServerIntellisenseProvider(
				workspaceRoot,
				client,
				workspaceFileWatcherFactory: (rootPath, dispatchAsync, watcherFailed) =>
				{
					var watcher = new WorkspaceFileWatcher(
						rootPath,
						dispatchAsync,
						LuaLanguageServerIntellisenseProvider.WorkspaceWatchSpecifications,
						watcherFailed,
						watcherCreationCount++ == 0
							? null
							: static (_, _) => throw new InvalidOperationException("Simulated watcher creation failure."));
					createdWatchers.Add(watcher);
					return watcher;
				});

			var failures = new List<WorkspaceWatcherFailure>();
			provider.WorkspaceWatcherFailed += failures.Add;

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

#pragma warning disable CS0618
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure."));
#pragma warning restore CS0618

			Assert.AreEqual(2, createdWatchers.Count);
			Assert.IsTrue(createdWatchers[0].IsDisposed);
			Assert.IsTrue(createdWatchers[1].IsDisposed);
			Assert.IsFalse(createdWatchers[0].HasActiveWatchers);
			Assert.IsFalse(createdWatchers[1].HasActiveWatchers);
			Assert.IsNull(GetWorkspaceWatcher(provider));
			Assert.AreEqual(1, failures.Count);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherFailure_RepeatedAutomaticRestartsContinueWithoutRaisingFailure()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRepeatedFailure_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
			var failures = new List<WorkspaceWatcherFailure>();

			provider.WorkspaceWatcherFailed += failures.Add;

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher firstWatcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the initial workspace watcher to start.");

#pragma warning disable CS0618
			firstWatcher.ReportErrorForTest(new IOException("Simulated watcher failure 1."));
#pragma warning restore CS0618

			WorkspaceFileWatcher secondWatcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the first replacement watcher to start.");

#pragma warning disable CS0618
			secondWatcher.ReportErrorForTest(new IOException("Simulated watcher failure 2."));
#pragma warning restore CS0618

			WorkspaceFileWatcher thirdWatcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the second replacement watcher to start.");

			Assert.AreNotSame(firstWatcher, secondWatcher);
			Assert.AreNotSame(secondWatcher, thirdWatcher);
			Assert.IsFalse(firstWatcher.HasActiveWatchers);
			Assert.IsFalse(secondWatcher.HasActiveWatchers);
			Assert.IsTrue(thirdWatcher.HasActiveWatchers);
			Assert.AreEqual(0, failures.Count);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherRecovery_ReconcilesLuaFilesCreatedWhileWatcherWasDown()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRecoveryCreate_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		string missedFilePath = Path.Combine(workspaceRoot, "Scripts", "missed.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			watcher.Dispose();
			File.WriteAllText(missedFilePath, "return 1");

			bool recovered = InvokePrivateMethodWithReturn<bool>(LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider), "TryRestartWorkspaceFileWatcher", watcher);

			Assert.IsTrue(recovered);
			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 1, TimeSpan.FromSeconds(1)));

			JsonElement changes = client.GetLastNotificationParameters("workspace/didChangeWatchedFiles").GetProperty("changes");
			Assert.AreEqual(1, changes.GetArrayLength());
			Assert.AreEqual(new Uri(missedFilePath).AbsoluteUri, changes[0].GetProperty("uri").GetString());
			Assert.AreEqual((int)FileChangeKind.Created, changes[0].GetProperty("type").GetInt32());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherRecovery_ReplaysConfigurationRefreshForMissedWorkspaceConfigChanges()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRecoveryConfig_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		string configFilePath = Path.Combine(workspaceRoot, ".luarc.json");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			watcher.Dispose();
			File.WriteAllText(configFilePath, "{\"Lua.workspace.maxPreload\": 1000}");

			bool recovered = InvokePrivateMethodWithReturn<bool>(LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider), "TryRestartWorkspaceFileWatcher", watcher);

			Assert.IsTrue(recovered);
			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeConfiguration", 1, TimeSpan.FromSeconds(1)));
			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 1, TimeSpan.FromSeconds(1)));

			string[] sentMethods = client.GetSentMethodNames();

			CollectionAssert.AreEqual(
				new[] { "textDocument/didOpen", "textDocument/hover", "workspace/didChangeConfiguration", "workspace/didChangeWatchedFiles" },
				sentMethods);

			JsonElement changes = client.GetLastNotificationParameters("workspace/didChangeWatchedFiles").GetProperty("changes");
			Assert.AreEqual(1, changes.GetArrayLength());
			Assert.AreEqual(new Uri(configFilePath).AbsoluteUri, changes[0].GetProperty("uri").GetString());
			Assert.AreEqual((int)FileChangeKind.Created, changes[0].GetProperty("type").GetInt32());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherRecovery_DoesNotReplayChangesAlreadyForwardedBeforeTheOutage()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRecoveryDuplicate_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);
			File.WriteAllText(filePath, content);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			File.WriteAllText(filePath, content + Environment.NewLine + "return value");

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(filePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			Assert.AreEqual(1, CountSentMethods(client, "workspace/didChangeWatchedFiles"));

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			watcher.Dispose();
			bool recovered = InvokePrivateMethodWithReturn<bool>(LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider), "TryRestartWorkspaceFileWatcher", watcher);

			Assert.IsTrue(recovered);
			await Task.Delay(150).ConfigureAwait(false);
			Assert.AreEqual(1, CountSentMethods(client, "workspace/didChangeWatchedFiles"));
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherRecovery_ReconcilesDroppedChangesFromUnexpectedForwardingFailure()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRecoveryDropped_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";
		const string updatedContent = "local value = 2";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);
			File.WriteAllText(filePath, content);

			using var client = new FakeLanguageServerClient
			{
				ThrowInvalidOperationOnNextWatchedFilesNotification = true,
				HoverResponse = JsonSerializer.SerializeToElement(new
				{
					contents = new
					{
						kind = "markdown",
						value = "Hover docs."
					}
				})
			};

			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			File.WriteAllText(filePath, updatedContent);

			await DispatchWorkspaceFileChangesAsync(
				provider,
				new FileChangeBatch(
				[
					new WorkspaceFileChange(filePath, FileChangeKind.Changed)
				]),
				CancellationToken.None);

			Assert.AreEqual(1, CountSentMethods(client, "workspace/didChangeWatchedFiles"));

			watcher.Dispose();

			bool recovered = InvokePrivateMethodWithReturn<bool>(
				LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider),
				"TryRestartWorkspaceFileWatcher",
				watcher);

			Assert.IsTrue(recovered);
			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 2, TimeSpan.FromSeconds(1)));

			JsonElement changes = client.GetLastNotificationParameters("workspace/didChangeWatchedFiles").GetProperty("changes");
			Assert.AreEqual(1, changes.GetArrayLength());
			Assert.AreEqual(new Uri(filePath).AbsoluteUri, changes[0].GetProperty("uri").GetString());
			Assert.AreEqual((int)FileChangeKind.Changed, changes[0].GetProperty("type").GetInt32());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherRecovery_ConcurrentDispatchDuringRecovery_ConvergesWithoutExtraReplayOnNextRecovery()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRecoveryConcurrent_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		string reconciledFilePath = Path.Combine(workspaceRoot, "Scripts", "reconciled.lua");
		string liveFilePath = Path.Combine(workspaceRoot, "Scripts", "live.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			WorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			watcher.Dispose();
			File.WriteAllText(reconciledFilePath, "return 1");
			File.WriteAllText(liveFilePath, "return 2");

			client.BlockNextWatchedFilesNotification();

			Task liveDispatchTask = DispatchWorkspaceFileChangesAsync(
				provider,
				new FileChangeBatch(
				[
					new WorkspaceFileChange(liveFilePath, FileChangeKind.Created)
				]),
				CancellationToken.None);

			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 1, TimeSpan.FromSeconds(1)));

			bool recovered = InvokePrivateMethodWithReturn<bool>(
				LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider),
				"TryRestartWorkspaceFileWatcher",
				watcher);

			Assert.IsTrue(recovered);

			client.ReleaseWatchedFilesNotification();
			await liveDispatchTask.ConfigureAwait(false);

			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 2, TimeSpan.FromSeconds(1)));

			WorkspaceFileWatcher replacementWatcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the replacement workspace watcher to start.");

			replacementWatcher.Dispose();

			bool recoveredAgain = InvokePrivateMethodWithReturn<bool>(
				LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceChangeCoordinator(provider),
				"TryRestartWorkspaceFileWatcher",
				replacementWatcher);

			Assert.IsTrue(recoveredAgain);
			await Task.Delay(150).ConfigureAwait(false);
			Assert.AreEqual(2, CountSentMethods(client, "workspace/didChangeWatchedFiles"));
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task SemanticTokensRefreshRequested_RefreshesTrackedDocuments()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"]
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var semanticTokensUpdated = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (updatedFilePath, tokens) =>
		{
			if (string.Equals(updatedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				semanticTokensUpdated.TrySetResult(tokens);
		};

		await provider.GetHoverAsync(filePath, content, 0, 0);

		client.PublishSemanticTokensRefreshRequested();

		Task completedTask = await Task.WhenAny(semanticTokensUpdated.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(semanticTokensUpdated.Task, completedTask);

		IReadOnlyList<LuaSemanticToken> semanticTokens = await semanticTokensUpdated.Task.ConfigureAwait(false);

		Assert.AreEqual(1, semanticTokens.Count);
		Assert.AreEqual("variable", semanticTokens[0].Type);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/hover", "textDocument/semanticTokens/full" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task OpenDocument_DisposeDuringInFlightSemanticTokensRequest_DoesNotRaiseSemanticTokensUpdated()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"]
		};

		client.BlockNextSemanticTokensFullRequest();

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var semanticTokensUpdated = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (_, tokens) => semanticTokensUpdated.TrySetResult(tokens);

		provider.OpenDocument(filePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/semanticTokens/full", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		provider.Dispose();
		client.ReleaseSemanticTokensFullRequest();

		Task completedTask = await Task.WhenAny(semanticTokensUpdated.Task, Task.Delay(TimeSpan.FromMilliseconds(250))).ConfigureAwait(false);

		Assert.AreNotSame(semanticTokensUpdated.Task, completedTask);
	}

	[TestMethod]
	public async Task SemanticTokensRefreshRequested_RequestFailure_ClearsCachedSemanticTokens()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"]
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var initialTokensUpdated = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);
		var clearedTokensUpdated = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (updatedFilePath, tokens) =>
		{
			if (!string.Equals(updatedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				return;

			if (tokens.Count > 0)
				initialTokensUpdated.TrySetResult(tokens);
			else
				clearedTokensUpdated.TrySetResult(tokens);
		};

		provider.OpenDocument(filePath, content);

		Task initialCompletedTask = await Task.WhenAny(initialTokensUpdated.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(initialTokensUpdated.Task, initialCompletedTask);
		Assert.AreEqual(1, provider.GetSemanticTokens(filePath).Count);

		client.ThrowInvalidOperationOnNextRequestMethod = "textDocument/semanticTokens/full";
		client.PublishSemanticTokensRefreshRequested();

		Task clearedCompletedTask = await Task.WhenAny(clearedTokensUpdated.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(clearedTokensUpdated.Task, clearedCompletedTask);
		Assert.AreEqual(0, clearedTokensUpdated.Task.Result.Count);
		Assert.AreEqual(0, provider.GetSemanticTokens(filePath).Count);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/semanticTokens/full",
				"textDocument/semanticTokens/full"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task OpenDocument_SemanticTokensFullRequest_OmitsPreviousResultId()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"]
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/semanticTokens/full", 1, TimeSpan.FromSeconds(1)));

		JsonElement parameters = client.GetLastRequestParameters("textDocument/semanticTokens/full");

		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
		Assert.IsFalse(parameters.TryGetProperty("previousResultId", out _));
	}

	[TestMethod]
	public async Task SemanticTokensRefreshRequested_FallsBackToFullRefreshAfterInvalidDelta()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"],
			SupportsSemanticTokensDelta = true
		};

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-1"
		}));

		client.EnqueueSemanticTokensDeltaResponse(JsonSerializer.SerializeToElement(new
		{
			resultId = "tokens-2"
		}));

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-3"
		}));

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var firstRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (updatedFilePath, tokens) =>
		{
			if (!string.Equals(updatedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				return;

			if (!firstRefresh.Task.IsCompleted)
				firstRefresh.TrySetResult(tokens);
			else
				secondRefresh.TrySetResult(tokens);
		};

		await provider.GetHoverAsync(filePath, content, 0, 0);

		client.PublishSemanticTokensRefreshRequested();

		Task firstCompletedTask = await Task.WhenAny(firstRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(firstRefresh.Task, firstCompletedTask);

		client.PublishSemanticTokensRefreshRequested();

		Task secondCompletedTask = await Task.WhenAny(secondRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(secondRefresh.Task, secondCompletedTask);

		IReadOnlyList<LuaSemanticToken> semanticTokens = await secondRefresh.Task.ConfigureAwait(false);

		Assert.AreEqual(1, semanticTokens.Count);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/semanticTokens/full",
				"textDocument/semanticTokens/full/delta",
				"textDocument/semanticTokens/full"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task SemanticTokensRefreshRequested_FallsBackToFullRefreshAfterMalformedDeltaEdit()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"],
			SupportsSemanticTokensDelta = true
		};

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-1"
		}));

		client.EnqueueSemanticTokensDeltaResponse(JsonSerializer.SerializeToElement(new
		{
			resultId = "tokens-2",
			edits = new object[]
			{
				new { deleteCount = 1, data = new[] { 0, 6 } }
			}
		}));

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-3"
		}));

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var firstRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (updatedFilePath, tokens) =>
		{
			if (!string.Equals(updatedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				return;

			if (!firstRefresh.Task.IsCompleted)
				firstRefresh.TrySetResult(tokens);
			else
				secondRefresh.TrySetResult(tokens);
		};

		await provider.GetHoverAsync(filePath, content, 0, 0);

		client.PublishSemanticTokensRefreshRequested();

		Task firstCompletedTask = await Task.WhenAny(firstRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(firstRefresh.Task, firstCompletedTask);

		client.PublishSemanticTokensRefreshRequested();

		Task secondCompletedTask = await Task.WhenAny(secondRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(secondRefresh.Task, secondCompletedTask);

		IReadOnlyList<LuaSemanticToken> semanticTokens = await secondRefresh.Task.ConfigureAwait(false);

		Assert.AreEqual(1, semanticTokens.Count);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/semanticTokens/full",
				"textDocument/semanticTokens/full/delta",
				"textDocument/semanticTokens/full"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task SemanticTokensRefreshRequested_FallsBackToFullRefreshAfterOverlappingDeltaEdits()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"],
			SupportsSemanticTokensDelta = true
		};

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-1"
		}));

		client.EnqueueSemanticTokensDeltaResponse(JsonSerializer.SerializeToElement(new
		{
			resultId = "tokens-2",
			edits = new object[]
			{
				new { start = 0, deleteCount = 2, data = new[] { 0, 6 } },
				new { start = 1, deleteCount = 1, data = new[] { 7 } }
			}
		}));

		client.EnqueueSemanticTokensFullResponse(JsonSerializer.SerializeToElement(new
		{
			data = new[] { 0, 6, 5, 0, 0 },
			resultId = "tokens-3"
		}));

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var firstRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);
		var secondRefresh = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (updatedFilePath, tokens) =>
		{
			if (!string.Equals(updatedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				return;

			if (!firstRefresh.Task.IsCompleted)
				firstRefresh.TrySetResult(tokens);
			else
				secondRefresh.TrySetResult(tokens);
		};

		await provider.GetHoverAsync(filePath, content, 0, 0);

		client.PublishSemanticTokensRefreshRequested();

		Task firstCompletedTask = await Task.WhenAny(firstRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(firstRefresh.Task, firstCompletedTask);

		client.PublishSemanticTokensRefreshRequested();

		Task secondCompletedTask = await Task.WhenAny(secondRefresh.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(secondRefresh.Task, secondCompletedTask);

		IReadOnlyList<LuaSemanticToken> semanticTokens = await secondRefresh.Task.ConfigureAwait(false);

		Assert.AreEqual(1, semanticTokens.Count);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/semanticTokens/full",
				"textDocument/semanticTokens/full/delta",
				"textDocument/semanticTokens/full"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task OpenDocument_WithSemanticTokenLegendButNoFullSupport_DoesNotRequestSemanticTokens()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = false,
			SemanticTokenTypes = ["variable"]
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		CollectionAssert.DoesNotContain(client.GetSentMethodNames(), "textDocument/semanticTokens/full");
	}
}
