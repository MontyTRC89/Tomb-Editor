#nullable enable

using System.IO;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static class LuaLanguageServerSettingsFactory
{
	/// <summary>
	/// Builds the Lua language server settings payload for the active script workspace.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The root directory of the current Lua script workspace.</param>
	/// <returns>An anonymous settings object serialized into the LuaLS configuration request.</returns>
	public static object Create(string workspaceRootDirectoryPath)
	{
		string apiDirectory = Path.Combine(workspaceRootDirectoryPath, ".API");
		string[] library = Directory.Exists(apiDirectory) ? [apiDirectory] : [];

		return new
		{
			Lua = new
			{
				runtime = new
				{
					version = "Lua 5.4"
				},
				workspace = new
				{
					checkThirdParty = "Disable",
					library
				},
				completion = new
				{
					callSnippet = "Disable"
				},
				semantic = new
				{
					enable = true,
					annotation = true,
					variable = true,
					keyword = false
				},
				diagnostics = new
				{
					disable = new[] { "duplicate-set-field" }
				}
			}
		};
	}
}
