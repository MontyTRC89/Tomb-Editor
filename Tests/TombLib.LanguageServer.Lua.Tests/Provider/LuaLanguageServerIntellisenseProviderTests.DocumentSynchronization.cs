using Nickelony.LanguageServer.Core.Completion;
using Nickelony.LanguageServer.Core.Diagnostics;
using Nickelony.LanguageServer.Core.Hover;
using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerIntellisenseProviderTests
{
	[TestMethod]
	public async Task RenameDocument_MovesDiagnosticsAndSemanticTokensToNewPath()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string oldFilePath = @"C:\Workspace\Scripts\test.lua";
		const string newFilePath = @"C:\Workspace\Scripts\renamed.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"]
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var semanticTokensUpdated = new TaskCompletionSource<IReadOnlyList<LuaSemanticToken>>(TaskCreationOptions.RunContinuationsAsynchronously);

		provider.SemanticTokensUpdated += (filePath, tokens) =>
		{
			if (string.Equals(filePath, oldFilePath, StringComparison.OrdinalIgnoreCase))
				semanticTokensUpdated.TrySetResult(tokens);
		};

		provider.OpenDocument(oldFilePath, content);

		Task completedTask = await Task.WhenAny(semanticTokensUpdated.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
		Assert.AreSame(semanticTokensUpdated.Task, completedTask);

		client.PublishDiagnostics(CreateDiagnostics(oldFilePath, 1, 6, 11, "Current warning."));

		Assert.AreEqual(1, provider.GetDiagnostics(oldFilePath).Count);
		Assert.AreEqual(1, provider.GetSemanticTokens(oldFilePath).Count);

		provider.RenameDocument(oldFilePath, newFilePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)));

		Assert.AreEqual(0, provider.GetDiagnostics(oldFilePath).Count);
		Assert.AreEqual(0, provider.GetSemanticTokens(oldFilePath).Count);
		Assert.AreEqual(1, provider.GetDiagnostics(newFilePath).Count);
		Assert.AreEqual(1, provider.GetSemanticTokens(newFilePath).Count);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/semanticTokens/full",
				"textDocument/didClose",
				"textDocument/didOpen"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task RenameDocument_PreservesOpenReferenceCountsAcrossMultipleTabs()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string oldFilePath = @"C:\Workspace\Scripts\test.lua";
		const string newFilePath = @"C:\Workspace\Scripts\renamed.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(oldFilePath, content);
		provider.OpenDocument(oldFilePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		provider.RenameDocument(oldFilePath, newFilePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)));

		provider.CloseDocument(newFilePath);

		Assert.IsFalse(await client.WaitForMethodCountAsync("textDocument/didClose", 2, TimeSpan.FromMilliseconds(250)));

		provider.CloseDocument(newFilePath);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 2, TimeSpan.FromSeconds(1)));
	}

	[TestMethod]
	public async Task RenameDocument_UpdateOnNewPath_WaitsForRenameReopenToFinish()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string oldFilePath = @"C:\Workspace\Scripts\test.lua";
		const string newFilePath = @"C:\Workspace\Scripts\renamed.lua";
		const string originalContent = "local value = 1";
		const string updatedContent = "local value = 2";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(oldFilePath, originalContent);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		client.BlockNextOpenNotification();

		provider.RenameDocument(oldFilePath, newFilePath, originalContent);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)));

		provider.UpdateDocument(newFilePath, updatedContent);

		Assert.IsFalse(await client.WaitForMethodCountAsync("textDocument/didChange", 1, TimeSpan.FromMilliseconds(250)));

		client.ReleaseOpenNotification();

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didChange", 1, TimeSpan.FromSeconds(1)));

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/didClose",
				"textDocument/didOpen",
				"textDocument/didChange"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task RenameDocument_DisposeDuringBlockedRenameReopen_DoesNotRaiseMovedDiagnosticsUpdated()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string oldFilePath = @"C:\Workspace\Scripts\test.lua";
		const string newFilePath = @"C:\Workspace\Scripts\renamed.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		int movedDiagnosticsUpdatedCount = 0;

		provider.DiagnosticsUpdated += (filePath, _) =>
		{
			if (string.Equals(filePath, newFilePath, StringComparison.OrdinalIgnoreCase))
				movedDiagnosticsUpdatedCount++;
		};

		provider.OpenDocument(oldFilePath, content);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		client.PublishDiagnostics(CreateDiagnostics(oldFilePath, 1, 6, 11, "Current warning."));
		Assert.AreEqual(1, provider.GetDiagnostics(oldFilePath).Count);

		client.BlockNextOpenNotification();
		provider.RenameDocument(oldFilePath, newFilePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		provider.Dispose();
		client.ReleaseOpenNotification();

		await Task.Delay(250).ConfigureAwait(false);

		Assert.AreEqual(0, movedDiagnosticsUpdatedCount);
	}

	[TestMethod]
	public async Task GetHoverAsync_RestartsAfterConsecutiveTimeoutsOnSameTransportGeneration()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			TimedOutHoverRequestsRemaining = 2,
			HoverResponse = JsonSerializer.SerializeToElement(new
			{
				contents = new
				{
					kind = "markdown",
					value = "Hover docs."
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(
			workspaceRoot,
			client,
			requestTimeout: TimeSpan.FromMilliseconds(50),
			requestTimeoutRestartThreshold: 2);

		TextHoverInfo? firstHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		TextHoverInfo? secondHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		TextHoverInfo? thirdHover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNull(firstHover);
		Assert.IsNull(secondHover);
		Assert.IsNotNull(thirdHover);
		Assert.AreEqual("Hover docs.", thirdHover.Content);
		Assert.AreEqual(1, client.MarkTransportUnhealthyCallCount);
		Assert.AreEqual(2, client.StartCallCount);

		client.TimedOutHoverRequestsRemaining = 1;

		TextHoverInfo? fourthHover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNull(fourthHover);
		Assert.AreEqual(1, client.MarkTransportUnhealthyCallCount);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/hover",
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/hover"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetHoverAsync_TimeoutFromSupersededGeneration_DoesNotInvalidateReplacementTransport()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			TimedOutHoverRequestsRemaining = 1,
			HoverResponse = JsonSerializer.SerializeToElement(new
			{
				contents = new
				{
					kind = "markdown",
					value = "Hover docs."
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(
			workspaceRoot,
			client,
			requestTimeout: TimeSpan.FromMilliseconds(200),
			requestTimeoutRestartThreshold: 1);

		Task<TextHoverInfo?> timedOutHoverTask = provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/hover", 1, TimeSpan.FromSeconds(1)));

		client.MarkTransportUnhealthy();

		TextHoverInfo? restartedHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		TextHoverInfo? timedOutHover = await timedOutHoverTask;
		TextHoverInfo? thirdHover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNotNull(restartedHover);
		Assert.AreEqual("Hover docs.", restartedHover.Content);
		Assert.IsNull(timedOutHover);
		Assert.IsNotNull(thirdHover);
		Assert.AreEqual("Hover docs.", thirdHover.Content);
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(2, client.StartCallCount);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/hover"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetHoverAsync_RequestCancellationAfterConnectionDrop_DoesNotForceRestartAttempt()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			FailStartWhenCancellationRequested = true,
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

		client.IsReady = false;

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		TextHoverInfo? hover = await provider.GetHoverAsync(filePath, content, 0, 0, cancellationTokenSource.Token);

		Assert.IsNull(hover);
		Assert.AreEqual(1, client.StartCallCount);
		Assert.AreEqual(1, client.StartCancellationTokenCanBeCanceled.Count);
	}

	[TestMethod]
	public async Task GetHoverAsync_InternalRequestCancellation_DoesNotCountAsTimeoutOrRestart()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			CancelNextHoverRequestWithoutTimeout = true,
			HoverResponse = JsonSerializer.SerializeToElement(new
			{
				contents = new
				{
					kind = "markdown",
					value = "Hover docs."
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(
			workspaceRoot,
			client,
			requestTimeout: TimeSpan.FromMilliseconds(50),
			requestTimeoutRestartThreshold: 1);

		TextHoverInfo? canceledHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		TextHoverInfo? recoveredHover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNull(canceledHover);
		Assert.IsNotNull(recoveredHover);
		Assert.AreEqual("Hover docs.", recoveredHover.Content);
		Assert.AreEqual(0, client.MarkTransportUnhealthyCallCount);
		Assert.AreEqual(1, client.StartCallCount);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/hover"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetHoverAsync_UserCancellation_DoesNotBlockClosingOpenDocument()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			HoverResponse = JsonSerializer.SerializeToElement(new
			{
				contents = new
				{
					kind = "markdown",
					value = "Hover docs."
				}
			})
		};

		client.BlockNextHoverRequest();

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		using var cancellationTokenSource = new CancellationTokenSource();

		provider.OpenDocument(filePath, content);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(filePath, content, 0, 0, cancellationTokenSource.Token);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/hover", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		cancellationTokenSource.Cancel();

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => hoverTask).ConfigureAwait(false);

		provider.CloseDocument(filePath);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		Assert.AreEqual(0, GetTrackedDocumentCount(provider));
	}

	[TestMethod]
	public async Task UpdateDocument_SendsFullTextChangeWhenServerAdvertisesFullSync()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			TextDocumentSyncKind = TextDocumentSyncKind.Full
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, "local value = 1");
		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsTrue(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromSeconds(1)));

		JsonElement parameters = client.GetLastNotificationParameters("textDocument/didChange");
		JsonElement change = parameters.GetProperty("contentChanges")[0];

		Assert.AreEqual("local value = 2", change.GetProperty("text").GetString());
		Assert.IsFalse(change.TryGetProperty("range", out _));
	}

	[TestMethod]
	public async Task UpdateDocument_WithUnchangedContent_DoesNotSendDidChange()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		provider.UpdateDocument(filePath, content);

		Assert.IsFalse(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromMilliseconds(250)));

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task UpdateDocument_CoalescesSupersededQueuedChanges()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			TextDocumentSyncKind = TextDocumentSyncKind.Full
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, "local value = 1");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		client.BlockNextChangeNotification();

		provider.UpdateDocument(filePath, "local value = 2");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didChange", 1, TimeSpan.FromSeconds(1)));

		provider.UpdateDocument(filePath, "local value = 3");
		provider.UpdateDocument(filePath, "local value = 4");

		client.ReleaseChangeNotification();

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didChange", 2, TimeSpan.FromSeconds(1)));
		Assert.IsFalse(await client.WaitForMethodCountAsync("textDocument/didChange", 3, TimeSpan.FromMilliseconds(250)));

		JsonElement parameters = client.GetLastNotificationParameters("textDocument/didChange");
		JsonElement change = parameters.GetProperty("contentChanges")[0];

		Assert.AreEqual("local value = 4", change.GetProperty("text").GetString());

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/didChange", "textDocument/didChange" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task UpdateDocument_BlockedChangeOnOneFile_DoesNotStallOtherFileHover()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string firstFilePath = @"C:\Workspace\Scripts\first.lua";
		const string secondFilePath = @"C:\Workspace\Scripts\second.lua";

		using var client = new FakeLanguageServerClient
		{
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

		provider.OpenDocument(firstFilePath, "local first = 1");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		client.BlockNextChangeNotification();
		provider.UpdateDocument(firstFilePath, "local first = 2");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didChange", 1, TimeSpan.FromSeconds(1)));

		Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(secondFilePath, "local second = 1", 0, 0);
		Task completedTask = await Task.WhenAny(hoverTask, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);

		Assert.AreSame(hoverTask, completedTask);
		Assert.IsNotNull(await hoverTask.ConfigureAwait(false));
		Assert.AreEqual(2, CountSentMethods(client, "textDocument/didOpen"));
		Assert.AreEqual(1, CountSentMethods(client, "textDocument/hover"));

		client.ReleaseChangeNotification();
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didChange", 1, TimeSpan.FromSeconds(1)));
	}

	[TestMethod]
	public async Task UpdateDocument_WithUnchangedContentAfterTransportFailure_ReopensWithFullSemanticTokensRefresh()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			SupportsSemanticTokensFull = true,
			SemanticTokenTypes = ["variable"],
			SupportsSemanticTokensDelta = true
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, "local value = 1");

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/semanticTokens/full", 1, TimeSpan.FromSeconds(1)));

		client.ThrowIOExceptionOnNextDidChange = true;

		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsTrue(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromSeconds(1)));

		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/semanticTokens/full", 2, TimeSpan.FromSeconds(1)));

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/semanticTokens/full",
				"textDocument/didChange",
				"textDocument/didOpen",
				"textDocument/semanticTokens/full"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetHoverAsync_ReplaysTrackedDocumentsAfterLanguageServerRestart()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		await provider.GetHoverAsync(filePath, content, 0, 0);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/hover" },
			client.GetSentMethodNames());

		Assert.AreEqual(1, client.StartCallCount);

		client.IsReady = false;

		await provider.GetHoverAsync(filePath, content, 0, 0);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/didOpen",
				"textDocument/hover"
			},
			client.GetSentMethodNames());

		Assert.AreEqual(2, client.StartCallCount);
	}

	[TestMethod]
	public async Task OpenDocument_DuringStartupFailure_ReplaysTrackedDocumentAfterRecovery()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string openedFilePath = @"C:\Workspace\Scripts\opened.lua";
		const string requestFilePath = @"C:\Workspace\Scripts\request.lua";
		const string openedContent = "local opened = 1";

		using var client = new FakeLanguageServerClient
		{
			StartResult = false,
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

		provider.OpenDocument(openedFilePath, openedContent);

		await Task.Delay(100).ConfigureAwait(false);

		Assert.AreEqual(1, client.StartCallCount);
		Assert.AreEqual(0, CountSentMethods(client, "textDocument/didOpen"));
		Assert.AreEqual(1, GetTrackedDocumentCount(provider));

		client.StartResult = true;

		TextHoverInfo? hover = await provider.GetHoverAsync(requestFilePath, "local request = 1", 0, 0);

		Assert.IsNotNull(hover);
		Assert.AreEqual(2, client.StartCallCount);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/didOpen",
				"textDocument/hover"
			},
			client.GetSentMethodNames());

		JsonElement firstDidOpen = client.GetLastNotificationParameters("textDocument/didOpen");
		Assert.AreEqual(new Uri(requestFilePath).AbsoluteUri, firstDidOpen.GetProperty("textDocument").GetProperty("uri").GetString());
	}

	[TestMethod]
	public async Task GetHoverAsync_ReplaysUntouchedTrackedDocumentsAfterFailedRestartRetry()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string firstFilePath = @"C:\Workspace\Scripts\first.lua";
		const string secondFilePath = @"C:\Workspace\Scripts\second.lua";
		const string firstContent = "local first = 1";
		const string secondContent = "local second = 2";

		using var client = new FakeLanguageServerClient
		{
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

		provider.OpenDocument(firstFilePath, firstContent);
		provider.OpenDocument(secondFilePath, secondContent);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 2, TimeSpan.FromSeconds(1)));
		Assert.AreEqual(1, client.StartCallCount);

		client.IsReady = false;
		client.StartResult = false;

		TextHoverInfo? failedHover = await provider.GetHoverAsync(firstFilePath, firstContent, 0, 0);

		Assert.IsNull(failedHover);
		Assert.AreEqual(2, client.StartCallCount);

		client.StartResult = true;

		TextHoverInfo? recoveredHover = await provider.GetHoverAsync(firstFilePath, firstContent, 0, 0);

		Assert.IsNotNull(recoveredHover);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 4, TimeSpan.FromSeconds(1)));
		Assert.AreEqual(3, client.StartCallCount);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/didOpen",
				"textDocument/didOpen",
				"textDocument/didOpen",
				"textDocument/hover"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task DiagnosticsPublished_IgnoresVersionMismatchAndStoresMatchingVersion()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string firstContent = "local value = 1";
		const string secondContent = "local second = 2";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		int diagnosticsUpdatedCount = 0;
		provider.DiagnosticsUpdated += (_, _) => diagnosticsUpdatedCount++;

		await provider.GetHoverAsync(filePath, firstContent, 0, 0);
		await provider.GetHoverAsync(filePath, secondContent, 0, 0);

		client.PublishDiagnostics(CreateDiagnostics(filePath, 1, 6, 12, "Stale warning."));
		Assert.AreEqual(0, diagnosticsUpdatedCount);
		Assert.AreEqual(0, provider.GetDiagnostics(filePath).Count);

		client.PublishDiagnostics(CreateDiagnostics(filePath, 3, 6, 12, "Future warning."));
		Assert.AreEqual(0, diagnosticsUpdatedCount);
		Assert.AreEqual(0, provider.GetDiagnostics(filePath).Count);

		client.PublishDiagnostics(CreateDiagnostics(filePath, 2, 6, 12, "Current warning."));

		IReadOnlyList<TextEditorDiagnostic> diagnostics = provider.GetDiagnostics(filePath);

		Assert.AreEqual(1, diagnosticsUpdatedCount);
		Assert.AreEqual(1, diagnostics.Count);
		Assert.AreEqual(6, diagnostics[0].StartOffset);
		Assert.AreEqual(12, diagnostics[0].EndOffset);
	}

	[TestMethod]
	public async Task DiagnosticsPublished_WithoutVersion_StoresFallbackDiagnosticsForTrackedDocument()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		int diagnosticsUpdatedCount = 0;

		provider.DiagnosticsUpdated += (_, _) => diagnosticsUpdatedCount++;

		await provider.GetHoverAsync(filePath, content, 0, 0);

		client.PublishDiagnostics(CreateDiagnostics(filePath, version: null, 6, 11, "Fallback warning."));

		IReadOnlyList<TextEditorDiagnostic> diagnostics = provider.GetDiagnostics(filePath);

		Assert.AreEqual(1, diagnosticsUpdatedCount);
		Assert.AreEqual(1, diagnostics.Count);
		Assert.AreEqual(TextEditorDiagnosticSeverity.Warning, diagnostics[0].Severity);
		Assert.AreEqual(6, diagnostics[0].StartOffset);
		Assert.AreEqual(11, diagnostics[0].EndOffset);
	}

	[TestMethod]
	public async Task DiagnosticsPublished_OneSubscriberExceptionDoesNotSuppressLaterSubscribers()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		int notifiedSubscribers = 0;

		provider.DiagnosticsUpdated += (_, _) =>
		{
			notifiedSubscribers++;
			throw new InvalidOperationException("Simulated diagnostics subscriber failure.");
		};

		provider.DiagnosticsUpdated += (_, _) => notifiedSubscribers++;

		await provider.GetHoverAsync(filePath, content, 0, 0);
		client.PublishDiagnostics(CreateDiagnostics(filePath, 1, 6, 12, "Current warning."));

		Assert.AreEqual(2, notifiedSubscribers);
	}

	[TestMethod]
	public async Task DiagnosticsPublished_DisposeInFirstSubscriber_DoesNotNotifyLaterSubscribers()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		int firstSubscriberCalls = 0;
		int secondSubscriberCalls = 0;

		provider.DiagnosticsUpdated += (_, _) =>
		{
			firstSubscriberCalls++;
			provider.Dispose();
		};

		provider.DiagnosticsUpdated += (_, _) => secondSubscriberCalls++;

		await provider.GetHoverAsync(filePath, content, 0, 0);
		client.PublishDiagnostics(CreateDiagnostics(filePath, 1, 6, 12, "Current warning."));

		Assert.AreEqual(1, firstSubscriberCalls);
		Assert.AreEqual(0, secondSubscriberCalls);
	}

	[TestMethod]
	public async Task UpdateDocument_WaitsForEarlierOpenNotificationToFinish()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		client.BlockNextOpenNotification();

		provider.OpenDocument(filePath, "local value = 1");
		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsFalse(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromMilliseconds(250)));

		client.ReleaseOpenNotification();

		Assert.IsTrue(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromSeconds(1)));

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/didChange" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task UpdateDocument_DisposeBeforeQueuedLatestUpdateRuns_DoesNotSendLateDidChange()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		client.BlockNextOpenNotification();

		provider.OpenDocument(filePath, "local value = 1");
		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsFalse(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromMilliseconds(250)).ConfigureAwait(false));

		provider.Dispose();
		client.ReleaseOpenNotification();

		Assert.IsFalse(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromMilliseconds(250)).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task GetHoverAsync_ReopensDocumentAfterIncrementalChangeTransportFailure()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			ThrowIOExceptionOnNextDidChange = true
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		await provider.GetHoverAsync(filePath, "local value = 1", 0, 0);
		provider.UpdateDocument(filePath, "local value = 2");

		Assert.IsTrue(await client.WaitForNotificationAsync("textDocument/didChange", TimeSpan.FromSeconds(1)));

		await provider.GetHoverAsync(filePath, "local value = 2", 0, 0);

		CollectionAssert.AreEqual(
			new[]
			{
				"textDocument/didOpen",
				"textDocument/hover",
				"textDocument/didChange",
				"textDocument/didOpen",
				"textDocument/hover"
			},
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task CloseDocument_WaitsForQueuedOpenNotificationToFinish()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		client.BlockNextOpenNotification();

		provider.OpenDocument(filePath, "local value = 1");
		provider.CloseDocument(filePath);

		Assert.IsFalse(await client.WaitForNotificationAsync("textDocument/didClose", TimeSpan.FromMilliseconds(250)));

		client.ReleaseOpenNotification();

		Assert.IsTrue(await client.WaitForNotificationAsync("textDocument/didClose", TimeSpan.FromSeconds(1)));

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/didClose" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task CloseDocument_SendsDidClosePayloadWithDocumentUri()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, "local value = 1");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		provider.CloseDocument(filePath);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)));

		JsonElement parameters = client.GetLastNotificationParameters("textDocument/didClose");
		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
	}
}
