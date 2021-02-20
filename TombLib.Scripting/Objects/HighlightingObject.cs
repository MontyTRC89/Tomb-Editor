using System;

namespace TombLib.Scripting.Objects
{
	public class HighlightingObject
	{
		public string HtmlColor { get; set; } = "White";
		public bool IsBold { get; set; } = false;
		public bool IsItalic { get; set; } = false;

		#region Operators

		public static bool operator ==(HighlightingObject a, HighlightingObject b) => a.Equals(b);
		public static bool operator !=(HighlightingObject a, HighlightingObject b) => !a.Equals(b);

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is HighlightingObject))
				return false;
			else
			{
				var objectToCompare = obj as HighlightingObject;

				return HtmlColor.Equals(objectToCompare.HtmlColor, StringComparison.OrdinalIgnoreCase)
					&& IsBold == objectToCompare.IsBold
					&& IsItalic == objectToCompare.IsItalic;
			}
		}

		public override int GetHashCode() => ToString().GetHashCode();

		#endregion Operators
	}
}
