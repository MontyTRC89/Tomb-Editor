using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerIntellisenseProviderTests
{
	[TestMethod]
	public async Task OpenDocument_SendsDidOpenPayloadWithLuaLanguageAndVersion()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task GetCompletionItemsAsync_ResolvesCompletionItemDetailsWhenServerSupportsResolve()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
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
						insertText = "spawn"
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

		IReadOnlyList<LuaCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spa", 0, 3);
		LuaCompletionItem resolvedItem = await items[0].ResolveAsync();

		Assert.AreEqual(1, items.Count);
		Assert.IsTrue(items[0].CanResolve);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual("Spawn docs.", resolvedItem.Description);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/completion", "completionItem/resolve" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetCompletionItemsAsync_ResolvePreservesOriginalInsertionMetadata()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
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

		IReadOnlyList<LuaCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spa", 0, 3);
		LuaCompletionItem resolvedItem = await items[0].ResolveAsync();

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

		using var client = new FakeLuaLanguageServerClient
		{
			CompletionResponse = JsonSerializer.SerializeToElement(new { items = Array.Empty<object>() })
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<LuaCompletionItem> items = await provider.GetCompletionItemsAsync(filePath, "spawn.", 0, 6, '.');
		JsonElement parameters = client.GetLastRequestParameters("textDocument/completion");

		Assert.AreEqual(0, items.Count);
		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
		Assert.AreEqual(0, parameters.GetProperty("position").GetProperty("line").GetInt32());
		Assert.AreEqual(6, parameters.GetProperty("position").GetProperty("character").GetInt32());
		Assert.AreEqual(2, parameters.GetProperty("context").GetProperty("triggerKind").GetInt32());
		Assert.AreEqual(".", parameters.GetProperty("context").GetProperty("triggerCharacter").GetString());
	}

	[TestMethod]
	public async Task FormatDocumentAsync_ReturnsFormattingEditsAndPassesEditorOptions()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value=1";

		using var client = new FakeLuaLanguageServerClient
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

		IReadOnlyList<LuaTextEdit> edits = await provider.FormatDocumentAsync(filePath, content,
			new LuaFormattingOptions(tabSize: 3, insertSpaces: false));

		Assert.AreEqual(1, edits.Count);
		Assert.AreEqual("local value = 1\r\n", edits[0].NewText);

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

		using var client = new FakeLuaLanguageServerClient
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

		LuaWorkspaceEdit? workspaceEdit = await provider.RenameSymbolAsync(filePath, content, 0, 14, "renamed");

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

		using var client = new FakeLuaLanguageServerClient
		{
			SupportsFormatting = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<LuaTextEdit> edits = await provider.FormatDocumentAsync(filePath, "local value=1",
			new LuaFormattingOptions(tabSize: 4, insertSpaces: true));

		Assert.AreEqual(0, edits.Count);
		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task DispatchWorkspaceFileChangesAsync_RefreshesConfigurationWhenApiLibraryChanges()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaConfigRefresh_" + Guid.NewGuid().ToString("N"));
		string apiDirectory = Path.Combine(workspaceRoot, ".API");
		string apiFilePath = Path.Combine(apiDirectory, "Generated.lua");

		try
		{
			Directory.CreateDirectory(apiDirectory);
			File.WriteAllText(apiFilePath, "return {}");

			using var client = new FakeLuaLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
			var batch = new FileChangeBatch();

			batch.Add(apiFilePath, FileChangeKind.Changed);

			await InvokePrivateTaskAsync(provider, "DispatchWorkspaceFileChangesAsync", batch, CancellationToken.None);

			CollectionAssert.AreEqual(
				new[] { "workspace/didChangeConfiguration", "workspace/didChangeWatchedFiles" },
				client.GetSentMethodNames());

			JsonElement settings = client.GetLastNotificationParameters("workspace/didChangeConfiguration")
				.GetProperty("settings")
				.GetProperty("Lua")
				.GetProperty("workspace")
				.GetProperty("library");

			Assert.AreEqual(1, settings.GetArrayLength());
			Assert.AreEqual(apiDirectory, settings[0].GetString());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task GetHoverAsync_RaisesTransientAndPermanentStartupFailuresOnceEach()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
		{
			IsReady = false,
			StartResult = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var failures = new List<LuaLanguageServerStartupFailure>();

		provider.StartupFailed += failure => failures.Add(failure);

		await provider.GetHoverAsync(filePath, content, 0, 0);
		await provider.GetHoverAsync(filePath, content, 0, 0);
		await provider.GetHoverAsync(filePath, content, 0, 0);
		await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.AreEqual(2, failures.Count);
		Assert.IsFalse(failures[0].IsPersistent);
		Assert.IsTrue(failures[1].IsPersistent);
		Assert.AreEqual(3, client.StartCallCount);
	}

	[TestMethod]
	public async Task GetHoverAsync_ReturnsParsedHoverInfoFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
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

		LuaHoverInfo? hover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNotNull(hover);
		Assert.AreEqual("Hover docs.", hover.Content);
		Assert.IsTrue(hover.IsMarkdown);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/hover" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetDefinitionAsync_ReturnsParsedDefinitionLocationFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\definitions.lua");

		using var client = new FakeLuaLanguageServerClient
		{
			DefinitionResponse = JsonSerializer.SerializeToElement(new
			{
				uri = new Uri(targetPath).AbsoluteUri,
				range = new
				{
					start = new { line = 4, character = 2 },
					end = new { line = 4, character = 7 }
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		LuaDefinitionLocation? definition = await provider.GetDefinitionAsync(filePath, "value", 0, 0);

		Assert.IsNotNull(definition);
		Assert.AreEqual(targetPath, definition.FilePath);
		Assert.AreEqual(5, definition.LineNumber);
		Assert.AreEqual(3, definition.ColumnNumber);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/definition" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetReferencesAsync_ReturnsParsedReferenceLocationsFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		string targetPath = Path.GetFullPath(@"C:\Workspace\Scripts\references.lua");

		using var client = new FakeLuaLanguageServerClient
		{
			ReferencesResponse = JsonSerializer.SerializeToElement(new object[]
			{
				new
				{
					uri = new Uri(targetPath).AbsoluteUri,
					range = new
					{
						start = new { line = 2, character = 4 },
						end = new { line = 2, character = 9 }
					}
				},
				new
				{
					uri = "https://example.com/not-a-file.lua",
					range = new
					{
						start = new { line = 0, character = 0 },
						end = new { line = 0, character = 1 }
					}
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		IReadOnlyList<LuaReferenceLocation> references = await provider.GetReferencesAsync(filePath, "value", 0, 0);

		Assert.AreEqual(1, references.Count);
		Assert.AreEqual(targetPath, references[0].FilePath);
		Assert.AreEqual(3, references[0].Range.StartLineNumber);
		Assert.AreEqual(5, references[0].Range.StartColumnNumber);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/references" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetSignatureHelpAsync_ReturnsParsedSignatureHelpFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
		{
			SignatureHelpResponse = JsonSerializer.SerializeToElement(new
			{
				activeSignature = 0,
				activeParameter = 1,
				signatures = new[]
				{
					new
					{
						label = "spawn(room, objectName)",
						documentation = new
						{
							kind = "markdown",
							value = "Spawns an object."
						},
						parameters = new object[]
						{
							new
							{
								label = new[] { 6, 10 },
								documentation = "Room id."
							},
							new
							{
								label = new[] { 12, 22 },
								documentation = "Object name."
							}
						}
					}
				}
			})
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		LuaSignatureInfo? signature = await provider.GetSignatureHelpAsync(filePath, "spawn(", 0, 6);

		Assert.IsNotNull(signature);
		Assert.AreEqual("spawn(room, objectName)", signature.Label);
		Assert.AreEqual("Spawns an object.", signature.Documentation);
		Assert.AreEqual(1, signature.ActiveParameter);
		Assert.AreEqual(2, signature.Parameters.Count);
		Assert.AreEqual("objectName", signature.Parameters[1].Label);
		Assert.AreEqual("Object name.", signature.Parameters[1].Documentation);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/signatureHelp" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task RenameDocument_MovesDiagnosticsAndSemanticTokensToNewPath()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string oldFilePath = @"C:\Workspace\Scripts\test.lua";
		const string newFilePath = @"C:\Workspace\Scripts\renamed.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
		{
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

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task GetHoverAsync_RestartsAfterConsecutiveTimeoutsOnSameTransportGeneration()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
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

		LuaHoverInfo? firstHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		LuaHoverInfo? secondHover = await provider.GetHoverAsync(filePath, content, 0, 0);
		LuaHoverInfo? thirdHover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNull(firstHover);
		Assert.IsNull(secondHover);
		Assert.IsNotNull(thirdHover);
		Assert.AreEqual("Hover docs.", thirdHover.Content);
		Assert.AreEqual(1, client.MarkTransportUnhealthyCallCount);
		Assert.AreEqual(2, client.StartCallCount);

		client.TimedOutHoverRequestsRemaining = 1;

		LuaHoverInfo? fourthHover = await provider.GetHoverAsync(filePath, content, 0, 0);

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
	public async Task GetHoverAsync_ShieldsRestartFromRequestCancellationAfterConnectionDrop()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
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

		await provider.GetHoverAsync(filePath, content, 0, 0, cancellationTokenSource.Token);

		Assert.AreEqual(2, client.StartCallCount);
		Assert.AreEqual(2, client.StartCancellationTokenCanBeCanceled.Count);
		Assert.IsFalse(client.StartCancellationTokenCanBeCanceled[1]);
	}

	[TestMethod]
	public async Task UpdateDocument_SendsFullTextChangeWhenServerAdvertisesFullSync()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
		{
			TextDocumentSyncKind = LuaTextDocumentSyncKind.Full
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

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task UpdateDocument_WithUnchangedContentAfterTransportFailure_ReopensWithFullSemanticTokensRefresh()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
		{
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

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task GetHoverAsync_ReplaysUntouchedTrackedDocumentsAfterFailedRestartRetry()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string firstFilePath = @"C:\Workspace\Scripts\first.lua";
		const string secondFilePath = @"C:\Workspace\Scripts\second.lua";
		const string firstContent = "local first = 1";
		const string secondContent = "local second = 2";

		using var client = new FakeLuaLanguageServerClient
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

		LuaHoverInfo? failedHover = await provider.GetHoverAsync(firstFilePath, firstContent, 0, 0);

		Assert.IsNull(failedHover);
		Assert.AreEqual(2, client.StartCallCount);

		client.StartResult = true;

		LuaHoverInfo? recoveredHover = await provider.GetHoverAsync(firstFilePath, firstContent, 0, 0);

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

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task DiagnosticsPublished_OneSubscriberExceptionDoesNotSuppressLaterSubscribers()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task UpdateDocument_WaitsForEarlierOpenNotificationToFinish()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient();
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
	public async Task GetHoverAsync_ReopensDocumentAfterIncrementalChangeTransportFailure()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLuaLanguageServerClient
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

		using var client = new FakeLuaLanguageServerClient();
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

		using var client = new FakeLuaLanguageServerClient();
		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

		provider.OpenDocument(filePath, "local value = 1");
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didOpen", 1, TimeSpan.FromSeconds(1)));

		provider.CloseDocument(filePath);
		Assert.IsTrue(await client.WaitForMethodCountAsync("textDocument/didClose", 1, TimeSpan.FromSeconds(1)));

		JsonElement parameters = client.GetLastNotificationParameters("textDocument/didClose");
		Assert.AreEqual(new Uri(filePath).AbsoluteUri, parameters.GetProperty("textDocument").GetProperty("uri").GetString());
	}

	[TestMethod]
	public async Task GetHoverAsync_RetriesWorkspaceWatcherStartAfterWorkspaceDirectoryAppears()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherRetry_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			using var client = new FakeLuaLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			Assert.IsNull(GetWorkspaceWatcher(provider));

			Directory.CreateDirectory(workspaceRoot);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			Assert.IsNotNull(GetWorkspaceWatcher(provider));
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task WorkspaceWatcherFailure_IsRaisedOnceAndStopsWatching()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherFailure_" + Guid.NewGuid().ToString("N"));
		string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
		const string content = "local value = 1";

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var client = new FakeLuaLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
			var failures = new List<LuaWorkspaceWatcherFailure>();

			provider.WorkspaceWatcherFailed += failure => failures.Add(failure);

			await provider.GetHoverAsync(filePath, content, 0, 0);

			LuaWorkspaceFileWatcher watcher = GetWorkspaceWatcher(provider)
				?? throw new AssertFailedException("Expected the workspace watcher to start.");

			Assert.IsTrue(watcher.HasActiveWatchers);

			watcher.ReportErrorForTest(new IOException("Simulated watcher failure."));
			watcher.ReportErrorForTest(new IOException("Simulated watcher failure."));

			Assert.AreEqual(1, failures.Count);
			Assert.IsFalse(watcher.HasActiveWatchers);
			Assert.IsTrue(failures[0].Message.Contains("disabled", StringComparison.OrdinalIgnoreCase));
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

		using var client = new FakeLuaLanguageServerClient
		{
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
	public async Task OpenDocument_SemanticTokensFullRequest_OmitsPreviousResultId()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";
		const string content = "local value = 1";

		using var client = new FakeLuaLanguageServerClient
		{
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

		using var client = new FakeLuaLanguageServerClient
		{
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

	private static LuaPublishDiagnosticsParams CreateDiagnostics(string filePath, int version, int startCharacter, int endCharacter, string message)
		=> new(
			new Uri(filePath).AbsoluteUri,
			version,
			[
				new LuaDiagnosticPayload(
					new LuaProtocolRangePayload(
						new LuaProtocolNullablePosition(0, startCharacter),
						new LuaProtocolNullablePosition(0, endCharacter)),
					2,
					message,
					null,
					null)
			]);

	private static LuaWorkspaceFileWatcher? GetWorkspaceWatcher(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_workspaceFileWatcher", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_workspaceFileWatcher' was not found.");

		return field.GetValue(provider) as LuaWorkspaceFileWatcher;
	}

	private static async Task InvokePrivateTaskAsync(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		Task task = (Task)(method.Invoke(instance, parameters)
			?? throw new InvalidOperationException($"Private method '{methodName}' returned null instead of a Task."));

		await task.ConfigureAwait(false);
	}

	private sealed class FakeLuaLanguageServerClient : ILuaLanguageServerClient
	{
		private readonly object _syncRoot = new();
		private readonly List<(string Method, JsonElement Parameters)> _sentNotifications = [];
		private readonly List<(string Method, JsonElement Parameters)> _sentRequests = [];
		private readonly List<string> _sentMethodNames = [];
		private readonly Queue<JsonElement> _semanticTokensDeltaResponses = [];
		private readonly Queue<JsonElement> _semanticTokensFullResponses = [];
		private TaskCompletionSource<bool>? _openNotificationGate;
		private readonly TaskCompletionSource<bool> _changeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource<bool> _closeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool IsReady { get; set; } = true;
		public long TransportGeneration { get; private set; }
		public bool StartResult { get; set; } = true;
		public JsonElement CompletionResponse { get; set; }
		public JsonElement CompletionResolveResponse { get; set; }
		public JsonElement DefinitionResponse { get; set; }
		public JsonElement FormattingResponse { get; set; }
		public JsonElement HoverResponse { get; set; }
		public JsonElement ReferencesResponse { get; set; }
		public JsonElement RenameResponse { get; set; }
		public JsonElement SignatureHelpResponse { get; set; }
		public LuaTextDocumentSyncKind TextDocumentSyncKind { get; set; } = LuaTextDocumentSyncKind.Incremental;
		public IReadOnlyList<string> SemanticTokenTypes { get; set; } = [];
		public IReadOnlyList<string> SemanticTokenModifiers { get; set; } = [];
		public bool SupportsCompletionResolve { get; set; }
		public bool SupportsReferences { get; set; } = true;
		public bool SupportsRename { get; set; } = true;
		public bool SupportsFormatting { get; set; } = true;
		public bool SupportsSemanticTokensDelta { get; set; }
		public bool FailStartWhenCancellationRequested { get; set; }
		public int StartCallCount { get; private set; }
		public int MarkTransportUnhealthyCallCount { get; private set; }
		public int TimedOutHoverRequestsRemaining { get; set; }
		public bool ThrowIOExceptionOnNextDidChange { get; set; }
		public List<bool> StartCancellationTokenCanBeCanceled { get; } = [];

		public event Action<LuaPublishDiagnosticsParams>? DiagnosticsPublished;

		public event Action? SemanticTokensRefreshRequested;

		public Task<bool> StartAsync(CancellationToken cancellationToken)
		{
			StartCallCount++;
			StartCancellationTokenCanBeCanceled.Add(cancellationToken.CanBeCanceled);

			if (FailStartWhenCancellationRequested && cancellationToken.IsCancellationRequested)
				throw new OperationCanceledException(cancellationToken);

			IsReady = StartResult;

			if (StartResult)
				TransportGeneration++;

			return Task.FromResult(StartResult);
		}

		public void MarkTransportUnhealthy()
		{
			MarkTransportUnhealthyCallCount++;
			IsReady = false;
		}

		public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
		{
			lock (_syncRoot)
			{
				_sentMethodNames.Add(method);
				_sentNotifications.Add((method, JsonSerializer.SerializeToElement(parameters)));
			}

			if (method == "textDocument/didChange")
			{
				_changeNotificationObserved.TrySetResult(true);

				if (ThrowIOExceptionOnNextDidChange)
				{
					ThrowIOExceptionOnNextDidChange = false;
					throw new IOException("Simulated didChange transport failure.");
				}
			}

			if (method == "textDocument/didClose")
				_closeNotificationObserved.TrySetResult(true);

			if (method == "textDocument/didOpen" && _openNotificationGate is not null)
				return _openNotificationGate.Task;

			return Task.CompletedTask;
		}

		public Task<TResult> SendRequestAsync<TResult>(string method, object parameters, CancellationToken cancellationToken)
		{
			RecordRequest(method, parameters);

			if (method == "textDocument/hover")
			{
				if (TimedOutHoverRequestsRemaining > 0)
				{
					TimedOutHoverRequestsRemaining--;
					return WaitForCancellationAsync<TResult>(cancellationToken);
				}

				if (HoverResponse.ValueKind != JsonValueKind.Undefined)
					return DeserializeResponseAsync<TResult>(HoverResponse);
			}

			if (method == "textDocument/completion" && CompletionResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(CompletionResponse);

			if (method == "completionItem/resolve" && CompletionResolveResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(CompletionResolveResponse);

			if (method == "textDocument/definition" && DefinitionResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(DefinitionResponse);

			if (method == "textDocument/references" && ReferencesResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(ReferencesResponse);

			if (method == "textDocument/rename" && RenameResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(RenameResponse);

			if (method == "textDocument/formatting" && FormattingResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(FormattingResponse);

			if (method == "textDocument/semanticTokens/full/delta")
			{
				if (_semanticTokensDeltaResponses.Count > 0)
					return DeserializeResponseAsync<TResult>(_semanticTokensDeltaResponses.Dequeue());

				return DeserializeResponseAsync<TResult>(JsonSerializer.SerializeToElement(new
				{
					edits = Array.Empty<object>(),
					resultId = "tokens-delta"
				}));
			}

			if (method == "textDocument/semanticTokens/full")
			{
				if (_semanticTokensFullResponses.Count > 0)
					return DeserializeResponseAsync<TResult>(_semanticTokensFullResponses.Dequeue());

				return DeserializeResponseAsync<TResult>(JsonSerializer.SerializeToElement(new
				{
					data = new[] { 0, 6, 5, 0, 0 },
					resultId = "tokens-1"
				}));
			}

			if (method == "textDocument/signatureHelp" && SignatureHelpResponse.ValueKind != JsonValueKind.Undefined)
				return DeserializeResponseAsync<TResult>(SignatureHelpResponse);

			if (typeof(TResult) == typeof(JsonElement))
				return Task.FromResult((TResult)(object)JsonSerializer.SerializeToElement(new { }));

			return Task.FromResult(CreateDefaultResponse<TResult>());
		}

		public string[] GetSentMethodNames()
		{
			lock (_syncRoot)
				return [.. _sentMethodNames];
		}

		public JsonElement GetLastNotificationParameters(string method)
		{
			lock (_syncRoot)
			{
				for (int i = _sentNotifications.Count - 1; i >= 0; i--)
				{
					if (string.Equals(_sentNotifications[i].Method, method, StringComparison.Ordinal))
						return _sentNotifications[i].Parameters;
				}
			}

			throw new InvalidOperationException($"Notification '{method}' was not observed.");
		}

		public JsonElement GetLastRequestParameters(string method)
		{
			lock (_syncRoot)
			{
				for (int i = _sentRequests.Count - 1; i >= 0; i--)
				{
					if (string.Equals(_sentRequests[i].Method, method, StringComparison.Ordinal))
						return _sentRequests[i].Parameters;
				}
			}

			throw new InvalidOperationException($"Request '{method}' was not observed.");
		}

		public void BlockNextOpenNotification()
			=> _openNotificationGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		public void ReleaseOpenNotification()
			=> _openNotificationGate?.TrySetResult(true);

		public async Task<bool> WaitForNotificationAsync(string method, TimeSpan timeout)
		{
			Task observedNotification = method switch
			{
				"textDocument/didChange" => _changeNotificationObserved.Task,
				"textDocument/didClose" => _closeNotificationObserved.Task,
				_ => Task.CompletedTask
			};

			Task completedTask = await Task.WhenAny(observedNotification, Task.Delay(timeout)).ConfigureAwait(false);
			return ReferenceEquals(completedTask, observedNotification);
		}

		public async Task<bool> WaitForMethodCountAsync(string method, int expectedCount, TimeSpan timeout)
		{
			DateTime deadline = DateTime.UtcNow + timeout;

			while (DateTime.UtcNow < deadline)
			{
				if (GetSentMethodCount(method) >= expectedCount)
					return true;

				await Task.Delay(10).ConfigureAwait(false);
			}

			return GetSentMethodCount(method) >= expectedCount;
		}

		public void PublishDiagnostics(LuaPublishDiagnosticsParams parameters)
			=> DiagnosticsPublished?.Invoke(parameters);

		public void PublishSemanticTokensRefreshRequested()
			=> SemanticTokensRefreshRequested?.Invoke();

		public void EnqueueSemanticTokensFullResponse(JsonElement response)
			=> _semanticTokensFullResponses.Enqueue(response);

		public void EnqueueSemanticTokensDeltaResponse(JsonElement response)
			=> _semanticTokensDeltaResponses.Enqueue(response);

		private int GetSentMethodCount(string method)
		{
			int count = 0;

			lock (_syncRoot)
			{
				for (int i = 0; i < _sentMethodNames.Count; i++)
				{
					if (string.Equals(_sentMethodNames[i], method, StringComparison.Ordinal))
						count++;
				}
			}

			return count;
		}

		private void RecordRequest(string method, object parameters)
		{
			lock (_syncRoot)
			{
				_sentMethodNames.Add(method);
				_sentRequests.Add((method, JsonSerializer.SerializeToElement(parameters)));
			}
		}

		private static Task<TResult> DeserializeResponseAsync<TResult>(JsonElement response)
		{
			if (typeof(TResult) == typeof(JsonElement))
				return Task.FromResult((TResult)(object)response);

			return Task.FromResult(DeserializeResponse<TResult>(response));
		}

		[return: MaybeNull]
		private static TResult DeserializeResponse<TResult>(JsonElement response)
			=> JsonSerializer.Deserialize<TResult>(response.GetRawText());

		[return: MaybeNull]
		private static TResult CreateDefaultResponse<TResult>()
			=> default;

		private static async Task<JsonElement> WaitForCancellationAsync(CancellationToken cancellationToken)
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			return default;
		}

		private static async Task<TResult> WaitForCancellationAsync<TResult>(CancellationToken cancellationToken)
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			return CreateDefaultResponse<TResult>();
		}

		public void Dispose()
		{ }
	}
}
