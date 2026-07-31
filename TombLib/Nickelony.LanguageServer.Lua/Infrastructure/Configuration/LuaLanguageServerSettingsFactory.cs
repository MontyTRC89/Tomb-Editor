namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Builds the workspace-scoped configuration payload sent to the bundled Lua language server.
/// </summary>
internal static class LuaLanguageServerSettingsFactory
{
	private const string LuaRuntimeVersion = "Lua 5.4";
	private const string DisabledSettingValue = "Disable";
	private static readonly string[] DisabledDiagnostics = ["duplicate-set-field"];

	/// <summary>
	/// Builds the Lua language server settings payload for the active script workspace.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The root directory of the current Lua script workspace.</param>
	/// <returns>An anonymous settings object serialized into the LuaLS configuration request.</returns>
	internal static object Create(string workspaceRootDirectoryPath)
	{
		string apiDirectory = Path.Combine(workspaceRootDirectoryPath, ".API");
		string[] library = Directory.Exists(apiDirectory) ? [apiDirectory] : [];

		return new
		{
			Lua = new
			{
				runtime = new
				{
					version = LuaRuntimeVersion
				},
				workspace = new
				{
					checkThirdParty = DisabledSettingValue,
					library
				},
				completion = new
				{
					callSnippet = DisabledSettingValue
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
					disable = DisabledDiagnostics
				}
			}
		};
	}
}
