using System;
using System.IO;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerSettingsFactory
	{
		public static object Create(string workspaceRootDirectoryPath)
		{
			string apiDirectory = Path.Combine(workspaceRootDirectoryPath, ".API");
			string[] library = Directory.Exists(apiDirectory)
				? new[] { Path.GetFullPath(apiDirectory) }
				: Array.Empty<string>();

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
						checkThirdParty = false,
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
					},
					telemetry = new
					{
						enable = false
					}
				}
			};
		}
	}
}