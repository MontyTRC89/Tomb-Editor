namespace TombLib.Scripting.Resources
{
	public struct ScriptPatterns
	{
		public static string Comments { get { return ";.*$"; } }
		public static string Sections { get { return @"^\[\b(" + string.Join("|", ScriptKeywords.Sections) + @")\b\]\s*?(;.*)?$"; } }
		public static string StandardCommands { get { return @"\b(" + string.Join("|", ScriptKeywords.OldCommands) + @")\b\s*?="; } }
		public static string NewCommands { get { return @"\b(" + string.Join("|", ScriptKeywords.NewCommands) + @")\b\s*?="; } }
		public static string NextLineKey { get { return @">\s*?(;.*)?$"; } }
		public static string Comma { get { return ","; } }
		public static string Mnemonics { get { return @"\b(" + string.Join("|", ScriptKeywords.AllMnemonics) + @")\b"; } }
		public static string HexValues { get { return @"\$[a-f0-9]*"; } }
		public static string Directives { get { return @"#(define|first_id|include)\s"; } }
		public static string Values { get { return "\\d|\\w|\"|'|\\.|\\\\|/"; } }
	}
}
