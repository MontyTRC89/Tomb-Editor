using System.Text.RegularExpressions;

namespace TombIDE.ScriptingStudio
{
	internal static class RegexPatterns
	{
		public static Regex LevelName => new Regex(@"\bName\s*=\s*", RegexOptions.IgnoreCase);
		public static Regex NGStringID => new Regex(@"^\d+:");

		public static int GetNGStringID(string lineText)
			=> int.Parse(Regex.Replace(lineText, @"^(\d+):", "$1"));
	}
}
