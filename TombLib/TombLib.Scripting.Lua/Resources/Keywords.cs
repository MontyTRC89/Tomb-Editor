namespace TombLib.Scripting.Lua.Resources
{
	public static class Keywords
	{
		public static readonly string[] Statements = new string[]
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

		public static readonly string[] Values = new string[]
		{
			"true",
			"false",
			"nil"
		};

		public static readonly string[] Operators = new string[]
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

		public static readonly string[] SpecialOperators = new string[]
		{
			"and",
			"or",
			"not",
		};
	}
}
