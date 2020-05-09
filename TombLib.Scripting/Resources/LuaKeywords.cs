namespace TombLib.Scripting.Resources
{
	public static class LuaKeywords
	{
		public static string[] Statements
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
					@"-",
					@"\*",
					"/",
					@"\^",
					@"\=",
					@"~\=",
					@">",
					@"<",
					@":",
					@"\.",
					@"\[",
					@"\]"
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
	}
}
