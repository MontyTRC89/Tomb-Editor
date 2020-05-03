using System.IO;
using System.Reflection;

namespace TombLib.Scripting.TextEditors
{
	internal static class DefaultPaths
	{
		public static string GetTextEditorColorConfigsPath()
		{
			return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors", "Colors");
		}

		public static string GetTextEditorConfigsPath()
		{
			return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors");
		}

		public static string GetProgramDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
