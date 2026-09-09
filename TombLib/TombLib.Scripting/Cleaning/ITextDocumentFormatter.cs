namespace TombLib.Scripting.Cleaning;

/// <summary>
/// Formats the content of a text document.
/// </summary>
public interface ITextDocumentFormatter
{
	/// <summary>
	/// Formats the supplied document content.
	/// </summary>
	/// <param name="content">The document content to format.</param>
	/// <param name="trimOnly">Whether only trailing whitespace should be trimmed.</param>
	/// <returns>The formatted document content.</returns>
	string FormatDocument(string content, bool trimOnly = false);
}
