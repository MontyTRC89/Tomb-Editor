namespace TombLib.Scripting.Tomb1Main.Resources
{
	public struct Patterns
	{
		public static string Comments => "//.*$";
		public static string Constants => "\"\\b(" + string.Join("|", Keywords.Constants) + ")\\b\"";
		public static string Collections => "\"\\b(" + string.Join("|", Keywords.Collections) + ")\\b\"";
		public static string Properties => "\"\\b(" + string.Join("|", Keywords.Properties) + ")\\b\"";
		public static string Values => @"\b(" + string.Join("|", Keywords.Values) + @")\b";
		public static string Strings => "\"(.*)\"";

		public static string LevelProperty => "\"title\":\\s*";
	}
}
