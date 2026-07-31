namespace Nickelony.LanguageServer.Core.Editing;

/// <summary>
/// Represents editor formatting preferences used for a document formatting request.
/// </summary>
public sealed class TextFormattingOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextFormattingOptions"/> class.
	/// </summary>
	/// <param name="tabSize">The preferred indentation width.</param>
	/// <param name="insertSpaces"><see langword="true"/> to indent with spaces; otherwise, tabs.</param>
	public TextFormattingOptions(int tabSize, bool insertSpaces)
	{
		TabSize = tabSize > 0 ? tabSize : 4;
		InsertSpaces = insertSpaces;
	}

	/// <summary>
	/// Gets the preferred indentation width.
	/// </summary>
	public int TabSize { get; }

	/// <summary>
	/// Gets a value indicating whether indentation should use spaces.
	/// </summary>
	public bool InsertSpaces { get; }
}
