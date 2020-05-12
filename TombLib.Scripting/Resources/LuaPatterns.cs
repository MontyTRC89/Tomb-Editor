namespace TombLib.Scripting.Resources
{
	public struct LuaPatterns
	{
		public static string Comments { get { return @"--.*$"; } }
		public static string Operators { get { return @"(" + string.Join("|", LuaKeywords.Operators) + @")"; } }
		public static string SpecialOperators { get { return @"\b(" + string.Join("|", LuaKeywords.SpecialOperators) + @")\b"; } }
		public static string Statements { get { return @"\b(" + string.Join("|", LuaKeywords.Statements) + @")\b"; } }
		public static string Values { get { return @"\b(" + string.Join("|", LuaKeywords.Values) + @")\b"; } }
	}
}
