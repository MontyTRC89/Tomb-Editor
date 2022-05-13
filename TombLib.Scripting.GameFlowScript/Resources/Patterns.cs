namespace TombLib.Scripting.GameFlowScript.Resources
{
	public struct Patterns
	{
		public static string Comments => "//.*$";
		public static string Sections => @"^\b(" + string.Join("|", Keywords.Sections) + @")\b:";
		public static string SpecialProperties => @"^\b(" + string.Join("|", Keywords.SpecialProperties) + @")\b:";
		public static string Properties => @"^\s*\b(" + string.Join("|", Keywords.Properties) + @")\b:";
		public static string Constants => @"\b(" + string.Join("|", Keywords.Constants) + @")\b";
		public static string Values => "\\d|\\w|\"|'|\\.|\\\\|/";

		public static string LevelProperty => @"^\bLEVEL:\s*";
	}
}
