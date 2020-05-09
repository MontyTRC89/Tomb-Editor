using System;

namespace TombLib.Scripting.TextEditors.ColorSchemes
{
	public sealed class ClassicScriptColorScheme
	{
		public string Sections { get; set; } = "White";
		public string Values { get; set; } = "White";
		public string References { get; set; } = "White";
		public string StandardCommands { get; set; } = "White";
		public string NewCommands { get; set; } = "White";
		public string Comments { get; set; } = "White";

		public string Background { get; set; } = "Black";
		public string Foreground { get; set; } = "White";

		public bool Equals(ClassicScriptColorScheme schemeToCompare)
		{
			return Sections.Equals(schemeToCompare.Sections, StringComparison.OrdinalIgnoreCase)
				&& Values.Equals(schemeToCompare.Values, StringComparison.OrdinalIgnoreCase)
				&& References.Equals(schemeToCompare.References, StringComparison.OrdinalIgnoreCase)
				&& StandardCommands.Equals(schemeToCompare.StandardCommands, StringComparison.OrdinalIgnoreCase)
				&& NewCommands.Equals(schemeToCompare.NewCommands, StringComparison.OrdinalIgnoreCase)
				&& Comments.Equals(schemeToCompare.Comments, StringComparison.OrdinalIgnoreCase)
				&& Background.Equals(schemeToCompare.Background, StringComparison.OrdinalIgnoreCase)
				&& Foreground.Equals(schemeToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
		}
	}
}
