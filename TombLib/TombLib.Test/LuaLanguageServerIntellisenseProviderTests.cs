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

	private static JsonElement CreateDiagnostics(string filePath, int version, int startCharacter, int endCharacter, string message)
		=> JsonSerializer.SerializeToElement(new
		{
			uri = new Uri(filePath).AbsoluteUri,
			version,
			diagnostics = new[]
			{
				new
				{
					severity = 2,
					message,
					range = new
					{
						start = new { line = 0, character = startCharacter },
						end = new { line = 0, character = endCharacter }
					}
				}
			}
		});

	private static LuaWorkspaceFileWatcher? GetWorkspaceWatcher(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_workspaceFileWatcher", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_workspaceFileWatcher' was not found.");

		return field.GetValue(provider) as LuaWorkspaceFileWatcher;
	}

	private sealed class FakeLuaLanguageServerClient : ILuaLanguageServerClient
	{
		private readonly List<string> _sentMethodNames = [];
		private TaskCompletionSource<bool>? _openNotificationGate;
		private readonly TaskCompletionSource<bool> _changeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource<bool> _closeNotificationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool IsReady { get; set; } = true;
		public IReadOnlyList<string> SemanticTokenTypes { get; set; } = [];
		public IReadOnlyList<string> SemanticTokenModifiers { get; set; } = [];
		public bool SupportsCompletionResolve => false;
		public bool SupportsSemanticTokensDelta => false;
		public int StartCallCount { get; private set; }

		public event Action<JsonElement>? DiagnosticsPublished;

		public event Action? SemanticTokensRefreshRequested;

		public Task<bool> StartAsync(CancellationToken cancellationToken)
		{
			StartCallCount++;
			IsReady = true;
			return Task.FromResult(true);
		}

		public Task SendNotificationAsync(string method, object parameters, CancellationToken cancellationToken)
		{
			_sentMethodNames.Add(method);

			if (method == "textDocument/didChange")
				_changeNotificationObserved.TrySetResult(true);

			if (method == "textDocument/didClose")
				_closeNotificationObserved.TrySetResult(true);

			if (method == "textDocument/didOpen" && _openNotificationGate is not null)
				return _openNotificationGate.Task;

			return Task.CompletedTask;
		}

		public Task<JsonElement> SendRequestAsync(string method, object parameters, CancellationToken cancellationToken)
		{
			_sentMethodNames.Add(method);

			if (method == "textDocument/semanticTokens/full")
			{
				return Task.FromResult(JsonSerializer.SerializeToElement(new
				{
					data = new[] { 0, 6, 5, 0, 0 },
					resultId = "tokens-1"
				}));
			}

			return Task.FromResult(JsonSerializer.SerializeToElement(new { }));
		}

		public string[] GetSentMethodNames() => [.. _sentMethodNames];

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

		public void PublishDiagnostics(JsonElement parameters)
			=> DiagnosticsPublished?.Invoke(parameters);

		public void PublishSemanticTokensRefreshRequested()
			=> SemanticTokensRefreshRequested?.Invoke();

		public void Dispose()
		{ }
	}
}
