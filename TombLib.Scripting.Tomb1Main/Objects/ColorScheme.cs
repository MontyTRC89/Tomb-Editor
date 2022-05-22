using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Tomb1Main.Objects
{
	public sealed class ColorScheme : ColorSchemeBase
	{
		public HighlightingObject Comments { get; set; } = new HighlightingObject();
		public HighlightingObject Constants { get; set; } = new HighlightingObject();
		public HighlightingObject Collections { get; set; } = new HighlightingObject();
		public HighlightingObject Properties { get; set; } = new HighlightingObject();
		public HighlightingObject Values { get; set; } = new HighlightingObject();
		public HighlightingObject Strings { get; set; } = new HighlightingObject();

		#region Operators

		public static bool operator ==(ColorScheme a, ColorScheme b) => a.Equals(b);

		public static bool operator !=(ColorScheme a, ColorScheme b) => !a.Equals(b);

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ColorScheme))
				return false;
			else
			{
				var objectToCompare = obj as ColorScheme;

				return Comments == objectToCompare.Comments
					&& Constants == objectToCompare.Constants
					&& Collections == objectToCompare.Collections
					&& Properties == objectToCompare.Properties
					&& Values == objectToCompare.Values
					&& Strings == objectToCompare.Strings
					&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
					&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override int GetHashCode() => ToString().GetHashCode();

		#endregion Operators
	}
}
