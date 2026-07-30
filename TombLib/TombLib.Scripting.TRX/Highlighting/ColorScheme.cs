using System;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.TRX.Highlighting
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

		public override bool Equals(object? obj)
		{
			if (obj is not ColorScheme other)
				return false;

			return Comments == other.Comments
				&& Constants == other.Constants
				&& Collections == other.Collections
				&& Properties == other.Properties
				&& Values == other.Values
				&& Strings == other.Strings
				&& Background.Equals(other.Background, StringComparison.OrdinalIgnoreCase)
				&& Foreground.Equals(other.Foreground, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode() => ToString().GetHashCode();

		#endregion Operators
	}
}
