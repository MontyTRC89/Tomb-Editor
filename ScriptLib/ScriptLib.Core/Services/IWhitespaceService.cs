using ICSharpCode.AvalonEdit.Document;

namespace ScriptLib.Core.Services;

/// <summary>
/// Interface for managing whitespace in text documents.
/// </summary>
public interface IWhitespaceService
{
	/// <summary>
	/// Converts tab characters to spaces in a text document.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">Specific lines to process. If null, processes all lines.</param>
	/// <param name="tabSize">The number of spaces that represent a tab character.</param>
	/// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
	void ConvertTabsToSpaces(TextDocument document, IEnumerable<DocumentLine>? lines = null, int tabSize = 4);

	/// <summary>
	/// Converts spaces to tab characters in a text document.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">Specific lines to process. If null, processes all lines.</param>
	/// <param name="tabSize">The number of spaces that represent a tab character.</param>
	/// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
	void ConvertSpacesToTabs(TextDocument document, IEnumerable<DocumentLine>? lines = null, int tabSize = 4);

	/// <summary>
	/// Removes trailing whitespace from lines in a text document.
	/// </summary>
	/// <param name="document">The document to process.</param>
	/// <param name="lines">Specific lines to process. If null, processes all lines.</param>
	/// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
	void TrimTrailingWhitespace(TextDocument document, IEnumerable<DocumentLine>? lines = null);
}
