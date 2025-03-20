using System.Linq;

namespace TombLib.Scripting.Tomb1Main.Resources
{
	public struct Patterns
	{
		public static string Comments => "//.*$";
		public static string Constants => "\"\\b(" + string.Join("|", Keywords.Constants) + "|" + string.Join("|", Keywords.RemovedConstants.Select(x => x.Keyword)) + ")\\b\"";
		public static string Collections => "\"\\b(" + string.Join("|", Keywords.Collections) + ")\\b\"";
		public static string Properties => "\"\\b(" + string.Join("|", Keywords.Properties) + "|" + string.Join("|", Keywords.RemovedProperties.Select(x => x.Keyword)) + ")\\b\"";
		public static string Values => @"\b(" + string.Join("|", Keywords.Values) + @")\b";
		public static string Strings => "\"(.+?)\"";

		public static string LevelProperty => "\"title\":\\s*\"";
		public static string LevelCommentName => @"^\s*//\s*Level\s*\d+\s*:\s*(.+)$";
		public static string PhdPathProperty => "\"path\":\\s*\"(.*\\.phd)\"";
	}
}
