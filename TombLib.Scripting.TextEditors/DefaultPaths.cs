using System.IO;
using System.Reflection;

namespace TombLib.Scripting.TextEditors
{
	internal static class DefaultPaths
	{
		public static string GetTextEditorConfigsPath()
		{ return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors"); }

		public static string GetClassicScriptColorConfigsPath()
		{ return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors", "ColorSchemes", "ClassicScript"); }

		public static string GetLuaColorConfigsPath()
		{ return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors", "ColorSchemes", "Lua"); }

		public static string GetProgramDirectory()
		{ return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
	}
}
