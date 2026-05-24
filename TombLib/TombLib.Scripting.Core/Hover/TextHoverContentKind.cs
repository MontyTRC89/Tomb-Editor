namespace TombLib.Scripting.Hover;

/// <summary>
/// Describes how hover content should be interpreted by the editor UI.
/// </summary>
public enum TextHoverContentKind
{
	/// <summary>
	/// Treat the hover content as plain text.
	/// </summary>
	PlainText,

	/// <summary>
	/// Treat the hover content as Markdown.
	/// </summary>
	Markdown
}
