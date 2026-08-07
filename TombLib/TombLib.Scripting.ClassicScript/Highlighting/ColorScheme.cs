using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.ClassicScript.Highlighting;

public sealed class ColorScheme : ColorSchemeBase
{
	public HighlightingObject Sections { get; set; } = new HighlightingObject();
	public HighlightingObject Values { get; set; } = new HighlightingObject();
	public HighlightingObject References { get; set; } = new HighlightingObject();
	public HighlightingObject StandardCommands { get; set; } = new HighlightingObject();
	public HighlightingObject NewCommands { get; set; } = new HighlightingObject();
	public HighlightingObject Comments { get; set; } = new HighlightingObject();

	// Operators

	public static bool operator ==(ColorScheme? a, ColorScheme? b) => Equals(a, b);
	public static bool operator !=(ColorScheme? a, ColorScheme? b) => !Equals(a, b);

	public override bool Equals(object? obj)
	{
		if (obj is not ColorScheme objectToCompare)
			return false;

		return Sections == objectToCompare.Sections
			&& Values == objectToCompare.Values
			&& References == objectToCompare.References
			&& StandardCommands == objectToCompare.StandardCommands
			&& NewCommands == objectToCompare.NewCommands
			&& Comments == objectToCompare.Comments
			&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
			&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Sections.GetHashCode();
			hashCode = (hashCode * 397) ^ Values.GetHashCode();
			hashCode = (hashCode * 397) ^ References.GetHashCode();
			hashCode = (hashCode * 397) ^ StandardCommands.GetHashCode();
			hashCode = (hashCode * 397) ^ NewCommands.GetHashCode();
			hashCode = (hashCode * 397) ^ Comments.GetHashCode();
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Background);
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Foreground);
			return hashCode;
		}
	}
}
