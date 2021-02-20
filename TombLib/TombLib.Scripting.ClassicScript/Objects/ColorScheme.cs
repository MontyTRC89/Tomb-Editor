using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Objects
{
	public sealed class ColorScheme : ColorSchemeBase
	{
		public HighlightingObject Sections { get; set; } = new HighlightingObject();
		public HighlightingObject Values { get; set; } = new HighlightingObject();
		public HighlightingObject References { get; set; } = new HighlightingObject();
		public HighlightingObject StandardCommands { get; set; } = new HighlightingObject();
		public HighlightingObject NewCommands { get; set; } = new HighlightingObject();
		public HighlightingObject Comments { get; set; } = new HighlightingObject();

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

				return Sections == objectToCompare.Sections
					&& Values == objectToCompare.Values
					&& References == objectToCompare.References
					&& StandardCommands == objectToCompare.StandardCommands
					&& NewCommands == objectToCompare.NewCommands
					&& Comments == objectToCompare.Comments
					&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
					&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override int GetHashCode() => ToString().GetHashCode();

		#endregion Operators
	}
}
