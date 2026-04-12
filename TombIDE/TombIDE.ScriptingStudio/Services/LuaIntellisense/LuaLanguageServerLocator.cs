using System.IO;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerLocator
	{
		private const string ExecutableFileName = "lua-language-server.exe";

		public static string ResolveExecutablePath()
		{
			string bundledExecutablePath = Path.Combine(DefaultPaths.TIDEDirectory, "LuaLS", "bin", ExecutableFileName);

			return !string.IsNullOrWhiteSpace(bundledExecutablePath) && File.Exists(bundledExecutablePath)
				? bundledExecutablePath
				: null;
		}
	}
}