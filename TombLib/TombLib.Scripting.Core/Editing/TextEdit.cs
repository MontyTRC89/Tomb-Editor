namespace TombLib.Scripting.Editing;

/// <summary>
/// Represents a single text replacement inside a document.
/// </summary>
/// <remarks>
/// Text edits intentionally use <see cref="TextDocumentRange"/> rather than editor-specific segment
/// types so they can describe cross-file or protocol-derived edits without carrying UI dependencies.
/// </remarks>
public sealed class TextEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextEdit"/> class.
	/// </summary>
	/// <param name="range">The range to replace.</param>
	/// <param name="newText">The replacement text.</param>
	public TextEdit(TextDocumentRange range, string newText)
	{
		Range = range;
		NewText = newText;
	}

	/// <summary>
	/// Gets the range to replace.
	/// </summary>
	public TextDocumentRange Range { get; }

	/// <summary>
	/// Gets the replacement text.
	/// </summary>
	public string NewText { get; }
}
