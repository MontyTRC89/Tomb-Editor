using System;

namespace TombLib.Scripting.UI.Highlighting
{
	public class HighlightingObject
	{
		public string HtmlColor { get; set; } = "White";
		public bool IsBold { get; set; } = false;
		public bool IsItalic { get; set; } = false;

		#region Operators

		public static bool operator ==(HighlightingObject? a, HighlightingObject? b) => Equals(a, b);

		public static bool operator !=(HighlightingObject? a, HighlightingObject? b) => !Equals(a, b);

		public override bool Equals(object? obj)
		{
			if (obj is not HighlightingObject objectToCompare)
				return false;

			return HtmlColor.Equals(objectToCompare.HtmlColor, StringComparison.OrdinalIgnoreCase)
				&& IsBold == objectToCompare.IsBold
				&& IsItalic == objectToCompare.IsItalic;
		}

		public override int GetHashCode() => ToString().GetHashCode();

		#endregion Operators
	}
}
