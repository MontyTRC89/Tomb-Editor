namespace TombLib.Scripting.ClassicScript.Resources
{
	public struct Patterns
	{
		public static string Comments => ";.*$";
		public static string Sections => @"\[\b(" + string.Join("|", Keywords.Sections) + @")\b\]";
		public static string StandardCommands => @"\b(" + string.Join("|", Keywords.OldCommands) + @")\b\s*=";
		public static string NewCommands => @"\b(" + string.Join("|", Keywords.NewCommands) + @")\b\s*=";
		public static string NextLineKey => @">\s*(;.*)?$";
		public static string Mnemonics => @"\b(" + string.Join("|", MnemonicData.AllConstantFlags) + @")\b";
		public static string HexValues => @"\$[a-f0-9]*";
		public static string Directives => @"#(define|first_id|include)\s";
		public static string Values => "\\d|\\w|\"|'|\\.|\\\\";

		/// <summary><c>Name = </c></summary>
		public static string NameCommand => @"^\s*\bName\s*=\s*";

		/// <summary><c>(CMD_...)</c></summary>
		public static string CommandPrefixInParenthesis => @"\(.*_\.*\)";

		/// <summary><c>Customize = CUST_CMD,</c></summary>
		public static string CustomizeCommandWithFirstArg => @"^\s*\bCustomize\s*=\s*\b.*\b\s*,";

		/// <summary><c>Parameters = PARAM_CMD,</c></summary>
		public static string ParametersCommandWithFirstArg => @"^\s*\bParameters\s*=\s*\b.*\b\s*,";

		/// <summary><c>#include </c></summary>
		public static string IncludeCommand => @"^\s*#include\s+";

		/// <summary><c>#define </c></summary>
		public static string DefineCommand => @"^\s*#define\s+";

		/// <summary><c>CONSTANT VALUE // 2 regex groups</c></summary>
		public static string DefineValue => @"(\w*)\s+(\w*)";

		/// <summary><c>"..."</c></summary>
		public static string FilePath => "\".*\"";
	}
}
