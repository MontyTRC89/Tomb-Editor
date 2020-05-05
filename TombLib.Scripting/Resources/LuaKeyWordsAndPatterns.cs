namespace TombLib.Scripting.Resources
{
	public static class LuaKeyWords
	{
		#region Public variables

		public static string[] Keywords
		{
			get
			{
				return new string[]
				{
					"if",
					"then",
					"else",
					"elseif",
					"function",
					"end",
					"for",
					"while",
					"in",
					"repeat",
					"until",
					"break",
					"return",
					"local",
					"print"
				};
			}
		}

		public static string[] Values
		{
			get
			{
				return new string[]
				{
					"true",
					"false",
					"nil"
				};
			}
		}

		public static string[] Operators
		{
			get
			{
				return new string[]
				{
					"%",
					@"\+",
					@"\-",
					@"\*",
					"/",
					@"\^",
					@"\=",
					@"~\=",
					@"\>",
					@"\<",
					@"\:",
					@"\."
				};
			}
		}

		public static string[] SpecialOperators
		{
			get
			{
				return new string[]
				{
					"and",
					"or",
					"not",
				};
			}
		}

		#endregion Public variables
	}

	public struct LuaPatterns
	{
		public static string Comments { get { return @"--.*$"; } }
		public static string Tables { get { return @"\[\]"; } }
		public static string Operators { get { return @"(" + string.Join("|", LuaKeyWords.Operators) + @")"; } }
		public static string SpecialOperators { get { return @"\b(" + string.Join("|", LuaKeyWords.SpecialOperators) + @")\b"; } }
		public static string Keywords { get { return @"\b(" + string.Join("|", LuaKeyWords.Keywords) + @")\b"; } }
		public static string Values { get { return @"\b(" + string.Join("|", LuaKeyWords.Values) + @")\b"; } }
	}
}
