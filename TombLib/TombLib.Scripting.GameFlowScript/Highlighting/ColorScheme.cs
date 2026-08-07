using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.GameFlowScript.Highlighting;

public sealed class ColorScheme : ColorSchemeBase
{
	public HighlightingObject Comments { get; set; } = new HighlightingObject();
	public HighlightingObject Sections { get; set; } = new HighlightingObject();
	public HighlightingObject SpecialProperties { get; set; } = new HighlightingObject();
	public HighlightingObject Properties { get; set; } = new HighlightingObject();
	public HighlightingObject Constants { get; set; } = new HighlightingObject();
	public HighlightingObject Values { get; set; } = new HighlightingObject();

	// Operators

	public static bool operator ==(ColorScheme? a, ColorScheme? b) => Equals(a, b);

	public static bool operator !=(ColorScheme? a, ColorScheme? b) => !Equals(a, b);

	public override bool Equals(object? obj)
	{
		if (obj is not ColorScheme objectToCompare)
			return false;

		return Comments == objectToCompare.Comments
			&& Sections == objectToCompare.Sections
			&& SpecialProperties == objectToCompare.SpecialProperties
			&& Properties == objectToCompare.Properties
			&& Constants == objectToCompare.Constants
			&& Values == objectToCompare.Values
			&& Background.Equals(objectToCompare.Background, StringComparison.OrdinalIgnoreCase)
			&& Foreground.Equals(objectToCompare.Foreground, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Comments.GetHashCode();
			hashCode = (hashCode * 397) ^ Sections.GetHashCode();
			hashCode = (hashCode * 397) ^ SpecialProperties.GetHashCode();
			hashCode = (hashCode * 397) ^ Properties.GetHashCode();
			hashCode = (hashCode * 397) ^ Constants.GetHashCode();
			hashCode = (hashCode * 397) ^ Values.GetHashCode();
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Background);
			hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Foreground);
			return hashCode;
		}
	}
}
