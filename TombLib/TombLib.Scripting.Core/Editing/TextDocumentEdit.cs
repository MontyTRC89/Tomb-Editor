namespace TombLib.Scripting.Editing;

/// <summary>
/// Represents the text edits that should be applied to a single document.
/// </summary>
public sealed class TextDocumentEdit
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextDocumentEdit"/> class.
	/// </summary>
	/// <param name="filePath">The file to update.</param>
	/// <param name="textEdits">The edits to apply.</param>
	public TextDocumentEdit(string filePath, IReadOnlyList<TextEdit> textEdits)
	{
		FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
		TextEdits = textEdits ?? [];
	}

	/// <summary>
	/// Gets the file to update.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the edits to apply to the file.
	/// </summary>
	public IReadOnlyList<TextEdit> TextEdits { get; }
}
