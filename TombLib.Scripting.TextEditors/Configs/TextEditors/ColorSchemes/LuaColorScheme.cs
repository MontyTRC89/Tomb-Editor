using System;

namespace TombLib.Scripting.TextEditors.ColorSchemes
{
	public sealed class LuaColorScheme
	{
		public HighlightingObject Comments { get; set; } = new HighlightingObject();
		public HighlightingObject Values { get; set; } = new HighlightingObject();
		public HighlightingObject Statements { get; set; } = new HighlightingObject();
		public HighlightingObject Operators { get; set; } = new HighlightingObject();
		public HighlightingObject SpecialOperators { get; set; } = new HighlightingObject();

		public string Background { get; set; } = "Black";
		public string Foreground { get; set; } = "White";

		#region Operators

		public static bool operator ==(LuaColorScheme a, LuaColorScheme b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(LuaColorScheme a, LuaColorScheme b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is LuaColorScheme))
				return false;
			else
			{
				LuaColorScheme objectToCompare = obj as LuaColorScheme;

				return Comments == objectToCompare.Comments
					&& Values == objectToCompare.Values
					&& Statements == objectToCompare.Statements
					&& Operators == objectToCompare.Operators
					&& SpecialOperators == objectToCompare.SpecialOperators
					&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
					&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		#endregion Operators
	}
}
