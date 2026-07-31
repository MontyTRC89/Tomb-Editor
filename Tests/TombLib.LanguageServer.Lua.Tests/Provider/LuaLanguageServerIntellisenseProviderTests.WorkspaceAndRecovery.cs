using Nickelony.LanguageServer.Core.Hover;
using Nickelony.LanguageServer.Core.Navigation;
using Nickelony.LanguageServer.Core.Signatures;
using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerIntellisenseProviderTests
{
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

			using var client = new FakeLanguageServerClient();
			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(apiFilePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

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
	public async Task DispatchWorkspaceFileChangesAsync_ReplaysDeferredChangesAfterStartupRecovery()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceReplay_" + Guid.NewGuid().ToString("N"));
		string apiDirectory = Path.Combine(workspaceRoot, ".API");
		string apiFilePath = Path.Combine(apiDirectory, "Generated.lua");
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(apiDirectory);

			using var client = new FakeLanguageServerClient
			{
				IsReady = false,
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

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(apiFilePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			Assert.AreEqual(0, client.GetSentMethodNames().Length);

			client.StartResult = true;

			TextHoverInfo? hover = await provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0);

			Assert.IsNotNull(hover);

			CollectionAssert.AreEqual(
				new[]
				{
					"workspace/didChangeConfiguration",
					"workspace/didChangeWatchedFiles",
					"textDocument/didOpen",
					"textDocument/hover"
				},
				client.GetSentMethodNames());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchWorkspaceFileChangesAsync_ReplaysBufferedChangesAfterWorkspaceNotificationTransportFailure()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceTransport_" + Guid.NewGuid().ToString("N"));
		string changedFilePath = Path.Combine(workspaceRoot, "Scripts", "generated.lua");
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(changedFilePath) ?? workspaceRoot);

			using var client = new FakeLanguageServerClient
			{
				ThrowIOExceptionOnNextWatchedFilesNotification = true,
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

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(changedFilePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			CollectionAssert.AreEqual(
				new[] { "workspace/didChangeWatchedFiles" },
				client.GetSentMethodNames());

			TextHoverInfo? hover = await provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0);

			Assert.IsNotNull(hover);

			CollectionAssert.AreEqual(
				new[]
				{
					"workspace/didChangeWatchedFiles",
					"workspace/didChangeWatchedFiles",
					"textDocument/didOpen",
					"textDocument/hover"
				},
				client.GetSentMethodNames());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task GetHoverAsync_ReplaysDeferredWorkspaceChangesAfterReplayCancellation()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceCancellation_" + Guid.NewGuid().ToString("N"));
		string changedFilePath = Path.Combine(workspaceRoot, "Scripts", "generated.lua");
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(changedFilePath) ?? workspaceRoot);

			using var client = new FakeLanguageServerClient
			{
				IsReady = false,
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

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(changedFilePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			client.StartResult = true;

			using var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();

			TextHoverInfo? canceledHover = await provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0, cancellationTokenSource.Token);
			TextHoverInfo? recoveredHover = await provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0);

			Assert.IsNull(canceledHover);
			Assert.IsNotNull(recoveredHover);

			CollectionAssert.AreEqual(
				new[]
				{
					"workspace/didChangeWatchedFiles",
					"textDocument/didOpen",
					"textDocument/hover"
				},
				client.GetSentMethodNames());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchWorkspaceFileChangesAsync_UnexpectedNotificationFailureDoesNotMarkTransportUnhealthyAndDropsChanges()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceUnexpected_" + Guid.NewGuid().ToString("N"));
		string changedFilePath = Path.Combine(workspaceRoot, "Scripts", "generated.lua");
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(changedFilePath) ?? workspaceRoot);

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

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(changedFilePath, FileChangeKind.Changed)
			]);

			await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			TextHoverInfo? hover = await provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0);

			Assert.IsNotNull(hover);
			Assert.AreEqual(0, client.MarkTransportUnhealthyCallCount);
			Assert.AreEqual(1, client.StartCallCount);

			CollectionAssert.AreEqual(
				new[]
				{
					"workspace/didChangeWatchedFiles",
					"textDocument/didOpen",
					"textDocument/hover"
				},
				client.GetSentMethodNames());
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchWorkspaceFileChangesAsync_StaleWatcherTransportFailureDoesNotInvalidateRestartedTransport()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceStaleTransport_" + Guid.NewGuid().ToString("N"));
		string changedFilePath = Path.Combine(workspaceRoot, "Scripts", "generated.lua");
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(changedFilePath) ?? workspaceRoot);

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

			client.BlockNextWatchedFilesNotification();
			client.ThrowIOExceptionAfterWatchedFilesNotificationGateRelease = true;

			var batch = new FileChangeBatch(
			[
				new WorkspaceFileChange(changedFilePath, FileChangeKind.Changed)
			]);

			Task dispatchTask = DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

			Assert.IsTrue(await client.WaitForMethodCountAsync("workspace/didChangeWatchedFiles", 1, TimeSpan.FromSeconds(1)).ConfigureAwait(false));

			client.IsReady = false;

			Task<TextHoverInfo?> hoverTask = provider.GetHoverAsync(scriptFilePath, "local value = 1", 0, 0);

			DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1);

			while (client.StartCallCount < 2 && DateTime.UtcNow < deadline)
				await Task.Delay(10).ConfigureAwait(false);

			Assert.AreEqual(2, client.StartCallCount);

			client.ReleaseWatchedFilesNotification();

			await dispatchTask.ConfigureAwait(false);
			Assert.IsNotNull(await hoverTask.ConfigureAwait(false));

			TextHoverInfo? followUpHover = await provider.GetHoverAsync(scriptFilePath, "local value = 2", 0, 0).ConfigureAwait(false);

			Assert.IsNotNull(followUpHover);
			Assert.AreEqual(0, client.MarkTransportUnhealthyCallCount);
			Assert.AreEqual(2, client.StartCallCount);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public async Task DispatchWorkspaceFileChangesAsync_RepeatedTransportFailuresReplayEachBufferedBatchOnce()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaDeferredWorkspaceRepeated_" + Guid.NewGuid().ToString("N"));
		string scriptFilePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(scriptFilePath) ?? workspaceRoot);

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

			for (int i = 1; i <= 3; i++)
			{
				string changedFilePath = Path.Combine(workspaceRoot, "Scripts", $"generated{i}.lua");

				var batch = new FileChangeBatch(
				[
					new WorkspaceFileChange(changedFilePath, FileChangeKind.Changed)
				]);

				client.ThrowIOExceptionOnNextWatchedFilesNotification = true;

				await DispatchWorkspaceFileChangesAsync(provider, batch, CancellationToken.None);

				TextHoverInfo? hover = await provider.GetHoverAsync(scriptFilePath, $"local value = {i}", 0, 0);

				Assert.IsNotNull(hover);
				Assert.AreEqual(i, client.MarkTransportUnhealthyCallCount);
				Assert.AreEqual(i * 2, CountSentMethods(client, "workspace/didChangeWatchedFiles"));

				JsonElement replayPayload = client.GetLastNotificationParameters("workspace/didChangeWatchedFiles").GetProperty("changes");

				Assert.AreEqual(1, replayPayload.GetArrayLength());
				Assert.AreEqual(new Uri(changedFilePath).AbsoluteUri, replayPayload[0].GetProperty("uri").GetString());
			}
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

		using var client = new FakeLanguageServerClient
		{
			IsReady = false,
			StartResult = false
		};

		using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, client);
		var failures = new List<LanguageServerStartupFailure>();

		provider.StartupFailed += failures.Add;

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

		TextHoverInfo? hover = await provider.GetHoverAsync(filePath, content, 0, 0);

		Assert.IsNotNull(hover);
		Assert.AreEqual("Hover docs.", hover.Content);
		Assert.IsTrue(hover.ContentKind == TextHoverContentKind.Markdown);

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

		using var client = new FakeLanguageServerClient
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

		TextDefinitionLocation? definition = await provider.GetDefinitionAsync(filePath, "value", 0, 0);

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

		using var client = new FakeLanguageServerClient
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

		IReadOnlyList<TextReferenceLocation> references = await provider.GetReferencesAsync(filePath, "value", 0, 0);

		Assert.AreEqual(1, references.Count);
		Assert.AreEqual(targetPath, references[0].FilePath);
		Assert.AreEqual(3, references[0].StartLineNumber);
		Assert.AreEqual(5, references[0].StartColumnNumber);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/references" },
			client.GetSentMethodNames());
	}

	[TestMethod]
	public async Task GetSignatureHelpAsync_ReturnsParsedSignatureHelpFromTypedResponse()
	{
		const string workspaceRoot = @"C:\Workspace";
		const string filePath = @"C:\Workspace\Scripts\test.lua";

		using var client = new FakeLanguageServerClient
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

		TextSignatureHelpInfo? signature = await provider.GetSignatureHelpAsync(filePath, "spawn(", 0, 6);

		Assert.IsNotNull(signature);
		Assert.AreEqual("spawn(room, objectName)", signature.Label);
		Assert.AreEqual("Spawns an object.", signature.Documentation);
		Assert.AreEqual(1, signature.ActiveParameterIndex);
		Assert.AreEqual(2, signature.Parameters.Count);
		Assert.AreEqual("objectName", signature.Parameters[1].Label);
		Assert.AreEqual("Object name.", signature.Parameters[1].Documentation);

		CollectionAssert.AreEqual(
			new[] { "textDocument/didOpen", "textDocument/signatureHelp" },
			client.GetSentMethodNames());
	}
}
