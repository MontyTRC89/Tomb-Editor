namespace Nickelony.LanguageServer.Core.Editing;

/// <summary>
/// Describes a formatting request against the current document.
/// </summary>
public sealed class TextFormatRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextFormatRequest"/> class.
	/// </summary>
	/// <param name="filePath">The current document file path.</param>
	/// <param name="documentText">The current document content.</param>
	/// <param name="options">The formatting options to apply.</param>
	public TextFormatRequest(string filePath, string documentText, TextFormattingOptions options)
	{
		FilePath = filePath;
		DocumentText = documentText;
		Options = options;
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
	/// Gets the formatting options.
	/// </summary>
	public TextFormattingOptions Options { get; }
}
