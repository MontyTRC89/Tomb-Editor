using Nickelony.LanguageServer.Core.Completion;
using Nickelony.LanguageServer.Core.Editing;
using Nickelony.LanguageServer.Core.Hover;
using Nickelony.LanguageServer.Core.Navigation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;

namespace Nickelony.LanguageServer.Lua.Tests;

/// <summary>
/// Live integration coverage for the bundled Lua language server.
/// These tests require TombIDE/TombIDE.Shared/TIDE/LuaLS.zip to be present in the repository layout
/// reachable from the test output directory, and they typically take several seconds each because they
/// launch a real language-server process, wait for diagnostics and semantic token round-trips, and
/// exercise restart or shutdown behavior.
/// </summary>
[TestClass]
public class LuaLanguageServerRealIntegrationTests
{
	private static readonly TimeSpan IntegrationTimeout = TimeSpan.FromSeconds(20);
	private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(150);

	[TestMethod]
	[TestCategory("Integration")]
	public async Task Provider_WithBundledLuaLanguageServer_HandlesLiveWorkflowConfigurationReloadAndShutdown()
	{
		using var session = new RealLuaLanguageServerTestSession();

		string filePath = Path.Combine(session.WorkspaceRoot, "Scripts", "test.lua");
		string apiDirectoryPath = Path.Combine(session.WorkspaceRoot, ".API");
		string generatedApiFilePath = Path.Combine(apiDirectoryPath, "Generated.lua");

		const string initialContent = "local stable_local =\r\nreturn stable_local\r\n";
		const string updatedContent = "local stable_local = 1\r\nlocal updated_local = stable_local + 1\r\nreturn updated_local\r\nupd";
		const string libraryAwareContent = updatedContent + "\r\ngen";

		Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? session.WorkspaceRoot);
		File.WriteAllText(filePath, initialContent);

		using var provider = new LuaLanguageServerIntellisenseProvider(session.WorkspaceRoot, session.ExecutablePath);

		provider.OpenDocument(filePath, initialContent);

		await WaitForConditionAsync(
			() => provider.GetDiagnostics(filePath).Count > 0,
			IntegrationTimeout,
			"Expected bundled LuaLS to publish diagnostics for the syntax error in the opened document.");

		provider.UpdateDocument(filePath, updatedContent);

		await WaitForConditionAsync(
			() => provider.GetSemanticTokens(filePath).Any(token => token.Line >= 2),
			IntegrationTimeout,
			"Expected semantic tokens to reflect the updated live document content.");

		IReadOnlyList<TextCompletionItem> completionItems = await WaitForCompletionItemsAsync(
			() => provider.GetCompletionItemsAsync(filePath, updatedContent, 3, 3),
			items => items.Any(item => string.Equals(item.Label, "updated_local", StringComparison.Ordinal)),
			IntegrationTimeout,
			"Expected bundled LuaLS to return completion items for the updated local variable.");

		Assert.IsTrue(completionItems.Any(item => string.Equals(item.Label, "updated_local", StringComparison.Ordinal)));

		TextHoverInfo hover = await WaitForHoverAsync(
			() => provider.GetHoverAsync(filePath, updatedContent, 2, 8),
			IntegrationTimeout,
			"Expected bundled LuaLS to return hover information for the updated document.");

		Assert.IsFalse(string.IsNullOrWhiteSpace(hover.Content));

		Directory.CreateDirectory(apiDirectoryPath);

		File.WriteAllText(generatedApiFilePath,
			"---@meta\r\n" +
			"function generated_function() end\r\n");

		await DispatchWorkspaceFileChangeAsync(provider, generatedApiFilePath, FileChangeKind.Created, CancellationToken.None);

		provider.UpdateDocument(filePath, libraryAwareContent);

		IReadOnlyList<TextCompletionItem> libraryItems = await WaitForCompletionItemsAsync(
			() => provider.GetCompletionItemsAsync(filePath, libraryAwareContent, 4, 3),
			items => items.Any(item => item.Label.StartsWith("generated_function", StringComparison.Ordinal)),
			IntegrationTimeout,
			"Expected a newly forwarded .API library symbol to appear in completions after the live workspace change.");

		Assert.IsTrue(libraryItems.Any(item => item.Label.StartsWith("generated_function", StringComparison.Ordinal)));

		int processId = GetRequiredServerProcessId(provider);

		provider.Dispose();

		await WaitForProcessExitAsync(
			processId,
			IntegrationTimeout,
			"Expected disposing the provider to stop the live Lua language-server process.");
	}

	[TestMethod]
	[TestCategory("Integration")]
	public async Task Provider_WithBundledLuaLanguageServer_RestartsAfterLiveServerCrashAndResumesRequests()
	{
		using var session = new RealLuaLanguageServerTestSession();

		string filePath = Path.Combine(session.WorkspaceRoot, "Scripts", "restart.lua");

		const string initialContent = "local restart_probe = 1\r\nres";
		const string restartedContent = "local restart_probe = 1\r\nlocal after_restart = restart_probe + 1\r\naft";

		Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? session.WorkspaceRoot);
		File.WriteAllText(filePath, initialContent);

		using var provider = new LuaLanguageServerIntellisenseProvider(session.WorkspaceRoot, session.ExecutablePath);

		provider.OpenDocument(filePath, initialContent);

		IReadOnlyList<TextCompletionItem> initialItems = await WaitForCompletionItemsAsync(
			() => provider.GetCompletionItemsAsync(filePath, initialContent, 1, 3),
			items => items.Any(item => string.Equals(item.Label, "restart_probe", StringComparison.Ordinal)),
			IntegrationTimeout,
			"Expected bundled LuaLS to return the initial completion before restart.");

		Assert.IsTrue(initialItems.Any(item => string.Equals(item.Label, "restart_probe", StringComparison.Ordinal)));

		LanguageServerClient client = GetRequiredClient(provider);
		long initialGeneration = client.TransportGeneration;
		Process initialProcess = GetRequiredServerProcess(client);
		int initialProcessId = initialProcess.Id;

		initialProcess.Kill(entireProcessTree: true);

		await WaitForProcessExitAsync(
			initialProcessId,
			IntegrationTimeout,
			"Expected the live Lua language-server process to exit after the simulated crash.");

		await WaitForConditionAsync(
			() => !client.IsReady,
			IntegrationTimeout,
			"Expected the client to observe the live LuaLS transport disconnect.");

		provider.UpdateDocument(filePath, restartedContent);

		IReadOnlyList<TextCompletionItem> restartedItems = await WaitForCompletionItemsAsync(
			() => provider.GetCompletionItemsAsync(filePath, restartedContent, 2, 3),
			items => items.Any(item => string.Equals(item.Label, "after_restart", StringComparison.Ordinal)),
			IntegrationTimeout,
			"Expected the provider to restart the live server and resume completions after the crash.");

		Assert.IsTrue(restartedItems.Any(item => string.Equals(item.Label, "after_restart", StringComparison.Ordinal)));

		long restartedGeneration = client.TransportGeneration;
		int restartedProcessId = GetRequiredServerProcessId(provider);

		Assert.IsTrue(restartedGeneration > initialGeneration);
		Assert.AreNotEqual(initialProcessId, restartedProcessId);

		provider.Dispose();

		await WaitForProcessExitAsync(
			restartedProcessId,
			IntegrationTimeout,
			"Expected the restarted live Lua language-server process to stop on provider disposal.");
	}

	[TestMethod]
	[TestCategory("Integration")]
	public async Task Provider_WithBundledLuaLanguageServer_ResolvesDefinitionAndReferencesForLocalSymbol()
	{
		using var session = new RealLuaLanguageServerTestSession();

		string filePath = Path.Combine(session.WorkspaceRoot, "Scripts", "navigation.lua");
		const string content =
			"local tracked_value = 1\r\n" +
			"local combined = tracked_value + tracked_value\r\n" +
			"return combined\r\n";

		Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? session.WorkspaceRoot);
		File.WriteAllText(filePath, content);

		using var provider = new LuaLanguageServerIntellisenseProvider(session.WorkspaceRoot, session.ExecutablePath);

		provider.OpenDocument(filePath, content);

		await WaitForConditionAsync(
			() => provider.SupportsReferences,
			IntegrationTimeout,
			"Expected the bundled Lua language server to advertise reference support.");

		TextDefinitionLocation definition = await WaitForDefinitionAsync(
			() => provider.GetDefinitionAsync(filePath, content, 1, 19),
			IntegrationTimeout,
			"Expected the bundled Lua language server to resolve the local symbol definition.");

		Assert.IsTrue(string.Equals(filePath, definition.FilePath, StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(1, definition.LineNumber);
		Assert.AreEqual(7, definition.ColumnNumber);

		IReadOnlyList<TextReferenceLocation> references = await WaitForReferencesAsync(
			() => provider.GetReferencesAsync(filePath, content, 1, 19),
			referenceLocations => referenceLocations.Count >= 3,
			IntegrationTimeout,
			"Expected the bundled Lua language server to return declaration and usage references for the local symbol.");

		Assert.AreEqual(3, references.Count(location => string.Equals(location.FilePath, filePath, StringComparison.OrdinalIgnoreCase)));
		Assert.IsTrue(references.Any(location => location.StartLineNumber == 1 && location.StartColumnNumber == 7));
		Assert.AreEqual(2, references.Count(location => location.StartLineNumber == 2));
	}

	[TestMethod]
	[TestCategory("Integration")]
	public async Task Provider_WithBundledLuaLanguageServer_ReturnsWorkspaceEditForRename()
	{
		using var session = new RealLuaLanguageServerTestSession();

		string filePath = Path.Combine(session.WorkspaceRoot, "Scripts", "rename.lua");
		const string content =
			"local tracked_value = 1\r\n" +
			"local result = tracked_value + 2\r\n" +
			"return tracked_value, result\r\n";

		Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? session.WorkspaceRoot);
		File.WriteAllText(filePath, content);

		using var provider = new LuaLanguageServerIntellisenseProvider(session.WorkspaceRoot, session.ExecutablePath);

		provider.OpenDocument(filePath, content);

		await WaitForConditionAsync(
			() => provider.SupportsRename,
			IntegrationTimeout,
			"Expected the bundled Lua language server to advertise rename support.");

		TextWorkspaceEdit workspaceEdit = await WaitForWorkspaceEditAsync(
			() => provider.RenameSymbolAsync(new TextRenameRequest(filePath, content, 0, 8, "renamed_value")),
			IntegrationTimeout,
			"Expected the bundled Lua language server to return a rename workspace edit for the local symbol.");

		Assert.IsTrue(workspaceEdit.HasEdits);
		Assert.AreEqual(1, workspaceEdit.DocumentEdits.Count);

		TextDocumentEdit documentEdit = workspaceEdit.DocumentEdits[0];

		Assert.IsTrue(string.Equals(filePath, documentEdit.FilePath, StringComparison.OrdinalIgnoreCase));
		Assert.IsTrue(documentEdit.TextEdits.Count >= 3);
		Assert.IsTrue(documentEdit.TextEdits.All(edit => edit.NewText == "renamed_value"));
	}

	private static async Task DispatchWorkspaceFileChangeAsync(
		LuaLanguageServerIntellisenseProvider provider,
		string filePath,
		FileChangeKind kind,
		CancellationToken cancellationToken)
	{
		var batch = new FileChangeBatch(
		[
			new WorkspaceFileChange(filePath, kind)
		]);

		await LuaLanguageServerIntellisenseProviderTestAccess.DispatchWorkspaceFileChangesAsync(provider, batch, cancellationToken).ConfigureAwait(false);
	}

	private static LanguageServerClient GetRequiredClient(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_client' was not found.");

		return (LanguageServerClient)(field.GetValue(provider)
			?? throw new InvalidOperationException("The live integration test expected a real language-server client instance."));
	}

	private static Process GetRequiredServerProcess(LanguageServerClient client)
	{
		FieldInfo sessionField = typeof(LanguageServerClient).GetField("_activeSession", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_activeSession' was not found.");

		object session = sessionField.GetValue(client)
			?? throw new InvalidOperationException("The live language-server client has no active transport session.");

		PropertyInfo processProperty = session.GetType().GetProperty("Process", BindingFlags.Instance | BindingFlags.Public)
			?? throw new InvalidOperationException("Active transport session property 'Process' was not found.");

		return (Process)(processProperty.GetValue(session)
			?? throw new InvalidOperationException("The active transport session did not expose a live server process."));
	}

	private static int GetRequiredServerProcessId(LuaLanguageServerIntellisenseProvider provider)
		=> GetRequiredServerProcess(GetRequiredClient(provider)).Id;

	private static async Task<IReadOnlyList<TextCompletionItem>> WaitForCompletionItemsAsync(
		Func<Task<IReadOnlyList<TextCompletionItem>>> action,
		Func<IReadOnlyList<TextCompletionItem>, bool> predicate,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		IReadOnlyList<TextCompletionItem> lastResult = [];

		while (stopwatch.Elapsed < timeout)
		{
			lastResult = await action().ConfigureAwait(false);

			if (predicate(lastResult))
				return lastResult;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage + Environment.NewLine + "Last completion labels: "
			+ string.Join(", ", lastResult.Select(item => item.Label)));

		return [];
	}

	private static async Task<TextDefinitionLocation> WaitForDefinitionAsync(
		Func<Task<TextDefinitionLocation?>> action,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		TextDefinitionLocation? lastResult = null;

		while (stopwatch.Elapsed < timeout)
		{
			lastResult = await action().ConfigureAwait(false);

			if (lastResult is not null)
				return lastResult;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage);
		return null;
	}

	private static async Task<TextHoverInfo> WaitForHoverAsync(
		Func<Task<TextHoverInfo?>> action,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		TextHoverInfo? lastResult = null;

		while (stopwatch.Elapsed < timeout)
		{
			lastResult = await action().ConfigureAwait(false);

			if (lastResult is not null)
				return lastResult;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage);
		return null;
	}

	private static async Task<IReadOnlyList<TextReferenceLocation>> WaitForReferencesAsync(
		Func<Task<IReadOnlyList<TextReferenceLocation>>> action,
		Func<IReadOnlyList<TextReferenceLocation>, bool> predicate,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		IReadOnlyList<TextReferenceLocation> lastResult = [];

		while (stopwatch.Elapsed < timeout)
		{
			lastResult = await action().ConfigureAwait(false);

			if (predicate(lastResult))
				return lastResult;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage + Environment.NewLine + "Last reference count: " + lastResult.Count);
		return [];
	}

	private static async Task<TextWorkspaceEdit> WaitForWorkspaceEditAsync(
		Func<Task<TextWorkspaceEdit?>> action,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		TextWorkspaceEdit? lastResult = null;

		while (stopwatch.Elapsed < timeout)
		{
			lastResult = await action().ConfigureAwait(false);

			if (lastResult?.HasEdits == true)
				return lastResult;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage);
		return null;
	}

	private static async Task WaitForConditionAsync(
		Func<bool> predicate,
		TimeSpan timeout,
		string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();

		while (stopwatch.Elapsed < timeout)
		{
			if (predicate())
				return;

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage);
	}

	private static async Task WaitForProcessExitAsync(int processId, TimeSpan timeout, string failureMessage)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();

		while (stopwatch.Elapsed < timeout)
		{
			if (!TryGetProcess(processId, out Process? process))
				return;

			using (process)
			{
				if (process.HasExited)
					return;
			}

			await Task.Delay(PollInterval).ConfigureAwait(false);
		}

		Assert.Fail(failureMessage);
	}

	private static bool TryGetProcess(int processId, [NotNullWhen(true)] out Process? process)
	{
		try
		{
			process = Process.GetProcessById(processId);
			return true;
		}
		catch (ArgumentException)
		{
			process = null;
			return false;
		}
	}

	private sealed class RealLuaLanguageServerTestSession : IDisposable
	{
		private readonly string _extractionRoot;

		public RealLuaLanguageServerTestSession()
		{
			string archivePath = TryFindRepositoryFile(Path.Combine("TombIDE", "TombIDE.Shared", "TIDE", "LuaLS.zip"))
				?? throw new AssertInconclusiveException(
					"Could not locate TombIDE/TombIDE.Shared/TIDE/LuaLS.zip from the test output directory. " +
					"See Tests/TombLib.LanguageServer.Lua.Tests/Integration/LuaLanguageServerIntegrationTests.md for prerequisites.");

			_extractionRoot = Path.Combine(Path.GetTempPath(), "LuaLsExtract_" + Guid.NewGuid().ToString("N"));
			WorkspaceRoot = Path.Combine(Path.GetTempPath(), "LuaLsWorkspace_" + Guid.NewGuid().ToString("N"));

			ZipFile.ExtractToDirectory(archivePath, _extractionRoot);
			Directory.CreateDirectory(WorkspaceRoot);

			string executablePath = Path.Combine(_extractionRoot, "bin", "lua-language-server.exe");

			if (!File.Exists(executablePath))
			{
				throw new AssertInconclusiveException(
					"The bundled LuaLS archive was found, but bin/lua-language-server.exe was missing after extraction.");
			}

			ExecutablePath = executablePath;
		}

		public string ExecutablePath { get; }
		public string WorkspaceRoot { get; }

		public void Dispose()
		{
			TryDeleteDirectory(WorkspaceRoot);
			TryDeleteDirectory(_extractionRoot);
		}
	}

	private static string? TryFindRepositoryFile(string relativePath)
	{
		for (DirectoryInfo? current = new(AppContext.BaseDirectory); current is not null; current = current.Parent)
		{
			string candidatePath = Path.Combine(current.FullName, relativePath);

			if (File.Exists(candidatePath))
				return candidatePath;
		}

		return null;
	}

	private static void TryDeleteDirectory(string path)
	{
		if (!Directory.Exists(path))
			return;

		try
		{
			Directory.Delete(path, recursive: true);
		}
		catch (IOException)
		{ }
		catch (UnauthorizedAccessException)
		{ }
	}
}
