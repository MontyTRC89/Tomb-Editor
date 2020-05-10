using System;
using TombLib.Scripting.Objects;
using TombLib.Scripting.TextEditors.Configs.Bases;

namespace TombLib.Scripting.TextEditors.ColorSchemes
{
	public sealed class ClassicScriptColorScheme : ColorSchemeBase
	{
		public HighlightingObject Sections { get; set; } = new HighlightingObject();
		public HighlightingObject Values { get; set; } = new HighlightingObject();
		public HighlightingObject References { get; set; } = new HighlightingObject();
		public HighlightingObject StandardCommands { get; set; } = new HighlightingObject();
		public HighlightingObject NewCommands { get; set; } = new HighlightingObject();
		public HighlightingObject Comments { get; set; } = new HighlightingObject();

		#region Operators

		public static bool operator ==(ClassicScriptColorScheme a, ClassicScriptColorScheme b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ClassicScriptColorScheme a, ClassicScriptColorScheme b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ClassicScriptColorScheme))
				return false;
			else
			{
				ClassicScriptColorScheme objectToCompare = obj as ClassicScriptColorScheme;

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

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		#endregion Operators
	}
}
