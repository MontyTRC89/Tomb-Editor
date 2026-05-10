using System.IO.Compression;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaLanguageServerRealIntegrationTests
{
	[TestMethod]
	[TestCategory("Integration")]
	public async Task GetCompletionItemsAsync_WithBundledLuaLanguageServer_ReturnsLocalVariableCompletion()
	{
		string archivePath = TryFindRepositoryFile(Path.Combine("TombIDE", "TombIDE.Shared", "TIDE", "LuaLS.zip"))
			?? throw new AssertInconclusiveException("Could not locate the bundled LuaLS archive from the test output directory.");

		string extractionRoot = Path.Combine(Path.GetTempPath(), "LuaLsExtract_" + Guid.NewGuid().ToString("N"));
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaLsWorkspace_" + Guid.NewGuid().ToString("N"));

		try
		{
			ZipFile.ExtractToDirectory(archivePath, extractionRoot);
			string executablePath = Path.Combine(extractionRoot, "bin", "lua-language-server.exe");

			Assert.IsTrue(File.Exists(executablePath), "The extracted LuaLS executable was not found.");

			string filePath = Path.Combine(workspaceRoot, "Scripts", "test.lua");
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? workspaceRoot);
			const string content = "local spawn_room = 1\r\nsp";
;
			File.WriteAllText(filePath, content);

			using var provider = new LuaLanguageServerIntellisenseProvider(workspaceRoot, executablePath);

			IReadOnlyList<LuaCompletionItem> items = [];

			for (int attempt = 0; attempt < 4; attempt++)
			{
				items = await provider.GetCompletionItemsAsync(filePath, content, 1, 2).ConfigureAwait(false);

				if (items.Any(item => string.Equals(item.Label, "spawn_room", StringComparison.Ordinal)))
					break;

				await Task.Delay(250).ConfigureAwait(false);
			}

			Assert.IsTrue(
				items.Any(item => string.Equals(item.Label, "spawn_room", StringComparison.Ordinal)),
				"The real LuaLS server did not return the expected local variable completion.");
		}
		finally
		{
			TryDeleteDirectory(extractionRoot);
			TryDeleteDirectory(workspaceRoot);
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
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}
}