using TombLib.Scripting.Extensions;

namespace TombLib.Scripting.UI.Cleaning;

/// <summary>
/// Trims trailing whitespace from each line in a document.
/// </summary>
public sealed class TrimTrailingWhitespaceFormatter : ITextDocumentFormatter
{
	public static TrimTrailingWhitespaceFormatter Instance { get; } = new();

	private TrimTrailingWhitespaceFormatter()
	{ }

	/// <inheritdoc />
	public string FormatDocument(string content, bool trimOnly = false)
		=> content.TrimTrailingWhitespaceOnLines();
}
