namespace Nickelony.LanguageServer.Core.Navigation;

/// <summary>
/// Describes a reference lookup request against the current document.
/// </summary>
/// <remarks>
/// Reference requests use zero-based line and column indices to match the editor's internal
/// coordinate system. When converting to or from LSP positions, the provider boundary is
/// responsible for the zero-based to one-based translation.
/// </remarks>
public sealed class TextReferenceRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextReferenceRequest"/> class.
	/// </summary>
	/// <param name="filePath">The current document file path.</param>
	/// <param name="documentText">The current document content.</param>
	/// <param name="line">The zero-based current line index.</param>
	/// <param name="column">The zero-based current column index.</param>
	/// <param name="includeDeclaration"><see langword="true"/> to include the symbol declaration when available.</param>
	public TextReferenceRequest(string filePath, string documentText, int line, int column, bool includeDeclaration = true)
	{
		FilePath = filePath;
		DocumentText = documentText;
		Line = Math.Max(0, line);
		Column = Math.Max(0, column);
		IncludeDeclaration = includeDeclaration;
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
	/// Gets a value indicating whether the declaration should be included when available.
	/// </summary>
	public bool IncludeDeclaration { get; }
}
