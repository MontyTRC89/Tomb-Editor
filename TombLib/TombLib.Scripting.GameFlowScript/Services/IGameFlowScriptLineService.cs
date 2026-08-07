namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Provides line-level operations for GameFlowScript text, including
/// comment removal, section header recognition, and empty-line detection.
/// </summary>
public interface IGameFlowScriptLineService
{
	/// <summary>
	/// Removes line comments from the text. Delegates to <c>LineCommentHelper.RemoveLineComment</c>
	/// with the <c>"//"</c> delimiter, matching legacy <c>LineParser.RemoveComments</c> behavior.
	/// </summary>
	string RemoveComments(string lineText);

	/// <summary>
	/// Replaces comments with spaces to maintain the original string length.
	/// Delegates to <c>LineCommentHelper.MaskLineComment</c> with the <c>"//"</c> delimiter,
	/// matching legacy <c>LineParser.EscapeComments</c> behavior.
	/// </summary>
	string EscapeComments(string lineText);

	/// <summary>
	/// Returns <c>true</c> when the line is null, empty, whitespace-only, or starts with <c>//</c>.
	/// Matches legacy <c>LineParser.IsEmptyOrComments</c> behavior.
	/// </summary>
	bool IsEmptyOrComments(string? lineText);

	/// <summary>
	/// Returns <c>true</c> when the line matches a GameFlowScript section header keyword
	/// followed by a colon (<c>Keyword:</c>). Uses the legacy <c>Patterns.Sections</c>
	/// regex for parity.
	/// </summary>
	bool IsSectionHeaderLine(string lineText);

	/// <summary>
	/// Extracts the section keyword text from a section header line.
	/// Returns <c>null</c> when the line is not a recognized section header.
	/// </summary>
	string? GetSectionHeaderText(string lineText);
}
