using System.Text.Json;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Editing;
using TombLib.Scripting.Hover;

namespace TombLib.LanguageServer.Lua.Tests;

[TestClass]
public partial class LuaLanguageServerIntellisenseProviderTests
{
	[TestMethod]
	public async Task OpenDocument_SendsDidOpenPayloadWithLuaLanguageAndVersion()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, content);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		JsonElement parameters = client.GetLastNotificationParameters("textDocument/didOpen");
		JsonElement textDocument = parameters.GetProperty("textDocument");

		Assert.AreEqual(new Uri(filePath).AbsoluteUri, textDocument.GetProperty("uri").GetString());
		Assert.AreEqual("lua", textDocument.GetProperty("languageId").GetString());
		Assert.AreEqual(1, textDocument.GetProperty("version").GetInt32());
		Assert.AreEqual(content, textDocument.GetProperty("text").GetString());
	}

	[TestMethod]
	public async Task GetHoverAsync_DisposeDuringStartupWait_ReturnsNullWithoutObjectDisposedException()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		SemaphoreSlim startLock = GetProviderStartLock(provider);

		startLock.Wait();

		try
		{
			Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(filePath, "local value = 1", 0, 0);

			await Task.Delay(50).ConfigureAwait(false);
			provider.Dispose();

			Assert.IsNull(await hoverTask.ConfigureAwait(false));
		}
		finally
		{
			startLock.Release();
		}
	}

	[TestMethod]
	public async Task GetHoverAsync_DisposeDuringInFlightRequest_ReturnsNullWithoutLateResult()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

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

		Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(filePath, "local value = 1", 0, 0);

		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/hover", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

		provider.Dispose();
		client.ReleaseHoverRequest();

		Assert.IsNull(await hoverTask.ConfigureAwait(false));
	}

	[TestMethod]
	public async Task GetHoverAsync_DisposeDuringBlockedStart_ReturnsNullWithoutLeakingStartTask()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			IsReady = false,
			HoverResponse = JsonSerializer.SerializeToElement(new
			{
				contents = new
				{
					kind = "markdown",
					value = "Hover docs."
				}
			})
		};

		client.BlockNextStartAsync();

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(filePath, "local value = 1", 0, 0);

		await Task.Delay(50).ConfigureAwait(false);
		provider.Dispose();

		Assert.IsNull(await hoverTask.ConfigureAwait(false));
		Assert.AreEqual(1, client.StartCallCount);
		Assert.AreEqual(1, client.StartCancellationTokenCanBeCanceled.Count);
	}

	[TestMethod]
	public void Dispose_DisposesOwnedCancellationSourceAndUnderlyingClientOnce()
	{
		const string workspaceRoot = @"C:\Workspace";

		var client = new FakeLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		CancellationTokenSource disposeCts = GetProviderDisposeCancellationTokenSource(provider);

		provider.Dispose();
		provider.Dispose();

		Assert.AreEqual(1, client.DisposeCallCount);
		Assert.ThrowsException<ObjectDisposedException>(() => disposeCts.Cancel());
	}

	[TestMethod]
	public async Task GetCompletionItemsAsync_ResolvesCompletionItemDetailsWhenServerSupportsResolve()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			SupportsCompletionResolve = true,
			CompletionResponse = JsonSerializer.SerializeToElement(new
			{
				items = new object[]
				{
					new
					{
						label = "spawn",
						kind = 3,
						insertText = "spawn",
						data = new
						{
							completionId = 7,
							source = "server"
						}
					}
				}
			}),
			CompletionResolveResponse = JsonSerializer.SerializeToElement(new
			{
				label = "spawn",
				kind = 3,
				insertText = "spawn",
				detail = "function",
				documentation = "Spawn docs."
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<TextCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spa", 0, 3);
		TextCompletionItem resolvedItem = await items[0].ResolveAsync();

		Assert.AreEqual(1, items.Count);
		Assert.IsTrue(items[0].CanResolve);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual("Spawn docs.", resolvedItem.Description);
		Assert.AreEqual(7, client.GetLastRequestParameters("completionItem/resolve").GetProperty("data").GetProperty("completionId").GetInt32());

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/completion", "completionItem/resolve" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetCompletionItemsAsync_ResolvePreservesOriginalInsertionMetadata()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			SupportsCompletionResolve = true,
			CompletionResponse = JsonSerializer.SerializeToElement(new
			{
				items = new object[]
				{
					new
					{
						label = "spawn",
						kind = 3,
						textEdit = new
						{
							newText = "spawn",
							range = new
							{
								start = new { line = 0, character = 0 },
								end = new { line = 0, character = 3 }
							}
						}
					}
				}
			}),
			CompletionResolveResponse = JsonSerializer.SerializeToElement(new
			{
				label = "spawn",
				kind = 3,
				insertText = "shouldNotReplaceOriginalInsertText",
				detail = "function",
				documentation = "Spawn docs."
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<TextCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spa", 0, 3);
		TextCompletionItem resolvedItem = await items[0].ResolveAsync();

		Assert.AreEqual("spawn", resolvedItem.InsertText);
		Assert.AreEqual(items[0].TextEdit, resolvedItem.TextEdit);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual("Spawn docs.", resolvedItem.Description);
	}

	[TestMethod]
	public async Task GetCompletionItemsAsync_WithTriggerCharacter_PassesCompletionContext()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			CompletionResponse = JsonSerializer.SerializeToElement(new { items = Array.Empty<object>() })
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<TextCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spawn.", 0, 6, '.');
		JsonElement parameters = client.GetLastRequestParameters("textDocument/completion");

		Assert.AreEqual(0, items.Count);
		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
		Assert.AreEqual(0, parameters.GetProperty("position").GetProperty("line").GetInt32());
		Assert.AreEqual(6, parameters.GetProperty("position").GetProperty("character").GetInt32());
		Assert.AreEqual(2, parameters.GetProperty("context").GetProperty("triggerKind").GetInt32());
		Assert.AreEqual(".", parameters.GetProperty("context").GetProperty("triggerCharacter").GetString());
	}

	[TestMethod]
	public async Task GetCompletionItemsAsync_WhenRequestCrossesTransportBoundary_RetriesOnce()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			TransportChangedRequestFailuresRemaining = 1,
			CompletionResponse = JsonSerializer.SerializeToElement(new
			{
				items = new object[]
				{
					new
					{
						label = "spawn",
						kind = 3,
						insertText = "spawn"
					}
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<TextCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spa", 0, 3).ConfigureAwait(false);

		Assert.AreEqual(1, items.Count);
		Assert.AreEqual("spawn", items[0].Label);
		Assert.AreEqual(2, client.StartCallCount);
		Assert.AreEqual(2, CountSentMethods(client, "textDocument/completion"));
	}

	[TestMethod]
	public async Task GetHoverAsync_WhenRequestTransportFails_RestartsAndRetriesOnce()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			ThrowIOExceptionOnNextRequestMethod = "textDocument/hover",
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

		TextHoverInfo? recoveredHover = await provider.GetHoverAsync(filePath, content, 0, 0).ConfigureAwait(false);

		Assert.IsNotNull(recoveredHover);
		Assert.AreEqual("Hover docs.", recoveredHover.Content);
		Assert.AreEqual(2, client.StartCallCount);
		Assert.AreEqual(2, CountSentMethods(client, "textDocument/hover"));
	}

	[TestMethod]
	public async Task RenameSymbolAsync_WhenRequestTransportFails_RestartsAndRetriesOnce()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\rename.lua";
		const string content = "local tracked_value = 1\r\nreturn tracked_value\r\n";

		using var client = new FakeLanguageServerClient
		{
			ThrowIOExceptionOnNextRequestMethod = "textDocument/rename",
			RenameResponse = JsonSerializer.SerializeToElement(new
			{
				changes = new Dictionary<string, object[]>
				{
					[new Uri(filePath).AbsoluteUri] =
					[
						new
						{
							newText = "renamed_value",
							range = new
							{
								start = new { line = 0, character = 6 },
								end = new { line = 0, character = 19 }
							}
						}
					]
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		TextWorkspaceEdit? recoveredRename = await provider
			.RenameSymbolAsync(new TextRenameRequest(filePath, content, 0, 8, "renamed_value"))
			.ConfigureAwait(false);

		Assert.IsNotNull(recoveredRename);
		Assert.IsTrue(recoveredRename.HasEdits);
		Assert.AreEqual(2, client.StartCallCount);
		Assert.AreEqual(2, CountSentMethods(client, "textDocument/rename"));
	}

	[TestMethod]
	public async Task GetHoverAsync_WhenRequestFailsWithoutTransportLoss_ThrowsWithoutRetrying()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLanguageServerClient
		{
			ThrowInvalidOperationOnNextRequestMethod = "textDocument/hover"
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
			await provider.GetHoverAsync(filePath, content, 0, 0).ConfigureAwait(false)).ConfigureAwait(false);

		Assert.AreEqual(1, client.StartCallCount);
		Assert.AreEqual(1, CountSentMethods(client, "textDocument/hover"));
	}

	[TestMethod]
	public async Task GetHoverAsync_RequestOnlyDocuments_DoNotAccumulateAcrossDistinctFiles()
	{
		const string workspaceRoot = @"C:\Workspace";
		const int maxTrackedRequestOnlyDocuments = 16;

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

		for (int i = 0; i < 20; i++)
		{
			string filePath = $@"C:\Workspace\Scripts\hover_{i}.lua";
			TextHoverInfo? hover = await provider.GetHoverAsync(filePath, "local value = 1", 0, 0);

			Assert.IsNotNull(hover);
		}

		Assert.AreEqual(maxTrackedRequestOnlyDocuments, GetTrackedDocumentCount(provider));
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 4, TimeSpan.FromSeconds(1)));
		Assert.AreEqual(4, CountSentMethods(client, "textDocument/didClose"));
	}

	[TestMethod]
	public async Task GetHoverAsync_RequestOnlyDocument_ReopensLazilyOnNextRequest()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\hover.lua";

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

		Assert.IsNotNull(await provider.GetHoverAsync(filePath, "local value = 1", 0, 0));

		for (int i = 0; i < 20; i++)
		{
			string otherFilePath = $@"C:\Workspace\Scripts\other_{i}.lua";
			Assert.IsNotNull(await provider.GetHoverAsync(otherFilePath, "local value = 1", 0, 0));
		}

		int didOpenCountBeforeReopen = CountSentMethods(client, "textDocument/didOpen");
		int didCloseCountBeforeReopen = CountSentMethods(client, "textDocument/didClose");

		Assert.IsNotNull(await provider.GetHoverAsync(filePath, "local value = 1", 0, 0));

		Assert.AreEqual(16, GetTrackedDocumentCount(provider));
		Assert.AreEqual(didOpenCountBeforeReopen + 1, CountSentMethods(client, "textDocument/didOpen"));
		Assert.AreEqual(didCloseCountBeforeReopen + 1, CountSentMethods(client, "textDocument/didClose"));
	}

	[TestMethod]
	public async Task FormatDocumentAsync_ReturnsFormattingEditsAndPassesEditorOptions()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value=1";

		using var client = new FakeLanguageServerClient
		{
			SupportsFormatting = true,
			FormattingResponse = JsonSerializer.SerializeToElement(new object[]
			{
				new
				{
					range = new
					{
						start = new { line = 0, character = 0 },
						end = new { line = 0, character = 0 }
					},
					newText = "local value = 1\r\n"
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		TextWorkspaceEdit? workspaceEdit = await provider.FormatDocumentAsync(
			new TextFormatRequest(filePath, content, new TextFormattingOptions(tabSize: 3, insertSpaces: false)));

		Assert.IsNotNull(workspaceEdit);
		Assert.AreEqual(1, workspaceEdit.DocumentEdits.Count);
		Assert.AreEqual(filePath, workspaceEdit.DocumentEdits[0].FilePath);
		Assert.AreEqual(1, workspaceEdit.DocumentEdits[0].TextEdits.Count);
		Assert.AreEqual("local value = 1\r\n", workspaceEdit.DocumentEdits[0].TextEdits[0].NewText);

		JsonElement parameters = client.GetLastRequestParameters("textDocument/formatting");

		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
		Assert.AreEqual(3, parameters.GetProperty("options").GetProperty("tabSize").GetInt32());
		Assert.IsFalse(parameters.GetProperty("options").GetProperty("insertSpaces").GetBoolean());

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/formatting" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task RenameSymbolAsync_ReturnsWorkspaceEditFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = target\r\nprint(target)";
		string secondPath = Path.GetFullPath(@"C:\Workspace\Scripts\other.lua");

		using var client = new FakeLanguageServerClient
		{
			SupportsRename = true,
			RenameResponse = JsonSerializer.SerializeToElement(new
			{
				changes = new Dictionary<string, object[]>
				{
					[new Uri(filePath).AbsoluteUri] =
					[
						new
						{
							range = new
							{
								start = new { line = 0, character = 14 },
								end = new { line = 0, character = 20 }
							},
							newText = "renamed"
						}
					]
				},
				documentChanges = new object[]
				{
					new
					{
						textDocument = new { uri = new Uri(secondPath).AbsoluteUri },
						edits = new object[]
						{
							new
							{
								range = new
								{
									start = new { line = 1, character = 6 },
									end = new { line = 1, character = 12 }
								},
								newText = "renamed"
							}
						}
					}
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		TextWorkspaceEdit? workspaceEdit = await provider
			.RenameSymbolAsync(new TextRenameRequest(filePath, content, 0, 14, "renamed"));

		Assert.IsNotNull(workspaceEdit);
		Assert.AreEqual(2, workspaceEdit.DocumentEdits.Count);
		Assert.AreEqual(filePath, workspaceEdit.DocumentEdits[0].FilePath);
		Assert.AreEqual("renamed", workspaceEdit.DocumentEdits[0].TextEdits[0].NewText);
		Assert.AreEqual(secondPath, workspaceEdit.DocumentEdits[1].FilePath);
		Assert.AreEqual("renamed", workspaceEdit.DocumentEdits[1].TextEdits[0].NewText);

		JsonElement parameters = client.GetLastRequestParameters("textDocument/rename");

		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
		Assert.AreEqual("renamed", parameters.GetProperty("newName").GetString());

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/rename" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task FormatDocumentAsync_ReturnsEmptyWhenFormattingIsUnsupported()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
		{
			SupportsFormatting = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		TextWorkspaceEdit? workspaceEdit = await provider.FormatDocumentAsync(
			new TextFormatRequest(filePath, "local value=1", new TextFormattingOptions(tabSize: 4, insertSpaces: true)));

		Assert.IsNull(workspaceEdit);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen" },
			client.GetSentMethodNames());
	}
}
