namespace TombLib.Scripting.Editing;

/// <summary>
/// Describes a rename request against the current document.
/// </summary>
public sealed class TextRenameRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextRenameRequest"/> class.
	/// </summary>
	/// <param name="filePath">The current document file path.</param>
	/// <param name="documentText">The current document content.</param>
	/// <param name="line">The zero-based current line index.</param>
	/// <param name="column">The zero-based current column index.</param>
	/// <param name="newName">The requested replacement name.</param>
	public TextRenameRequest(string filePath, string documentText, int line, int column, string newName)
	{
		FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
		DocumentText = documentText ?? string.Empty;
		Line = Math.Max(0, line);
		Column = Math.Max(0, column);
		NewName = newName ?? throw new ArgumentNullException(nameof(newName));
	}

	/// <summary>
	/// Gets the current document file path.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the current document content.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the zero-based current line index.
	/// </summary>
	public int Line { get; }

	/// <summary>
	/// Gets the zero-based current column index.
	/// </summary>
	public int Column { get; }

	/// <summary>
	/// Gets the requested replacement name.
	/// </summary>
	public string NewName { get; }
}
