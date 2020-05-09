using System;

namespace TombLib.Scripting.TextEditors.ColorSchemes
{
	public sealed class LuaColorScheme
	{
		public string Comments { get; set; }
		public string Values { get; set; }
		public string Statements { get; set; }
		public string Operators { get; set; }
		public string SpecialOperators { get; set; }

		public string Background { get; set; }
		public string Foreground { get; set; }

		public bool Equals(LuaColorScheme schemeToCompare)
		{
			return Comments.Equals(schemeToCompare.Comments, StringComparison.OrdinalIgnoreCase)
				&& Values.Equals(schemeToCompare.Values, StringComparison.OrdinalIgnoreCase)
				&& Statements.Equals(schemeToCompare.Statements, StringComparison.OrdinalIgnoreCase)
				&& Operators.Equals(schemeToCompare.Operators, StringComparison.OrdinalIgnoreCase)
				&& SpecialOperators.Equals(schemeToCompare.SpecialOperators, StringComparison.OrdinalIgnoreCase)
				&& Background.Equals(schemeToCompare.Background, StringComparison.OrdinalIgnoreCase)
				&& Foreground.Equals(schemeToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
		}
	}
}
