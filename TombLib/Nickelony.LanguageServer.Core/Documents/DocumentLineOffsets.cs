namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Provides a precomputed table of line start offsets and lengths for a document content string.
/// This avoids allocating heavyweight <c>TextDocument</c> instances when parsing LSP payloads such as
/// diagnostics and semantic tokens, which only need to translate between line/character and document offsets.
/// Both <c>\r\n</c> and lone <c>\r</c> are treated as line breaks for parity with LSP positions.
/// </summary>
public sealed class DocumentLineOffsets
{
	private readonly string _content;

	private readonly int[] _lineStartOffsets;
	private readonly int[] _lineLengths;

	private DocumentLineOffsets(string content, int[] lineStartOffsets, int[] lineLengths)
	{
		_content = content;
		_lineStartOffsets = lineStartOffsets;
		_lineLengths = lineLengths;
	}

	/// <summary>
	/// Gets the number of logical lines in the document.
	/// </summary>
	public int LineCount => _lineLengths.Length;

	/// <summary>
	/// Gets the total document length in characters.
	/// </summary>
	public int TextLength => _content.Length;

	/// <summary>
	/// Gets the length of the specified zero-based line.
	/// </summary>
	/// <param name="lineIndex">The zero-based line index.</param>
	/// <returns>The line length in characters.</returns>
	public int GetLineLength(int lineIndex) => _lineLengths[ClampLineIndex(lineIndex)];

	/// <summary>
	/// Gets the absolute document offset where the specified zero-based line starts.
	/// </summary>
	/// <param name="lineIndex">The zero-based line index.</param>
	/// <returns>The absolute document offset.</returns>
	public int GetLineStartOffset(int lineIndex) => _lineStartOffsets[ClampLineIndex(lineIndex)];

	/// <summary>
	/// Returns the offset within the document for the supplied zero-based line and character indices,
	/// clamping the character index to the line length.
	/// </summary>
	/// <param name="lineIndex">The zero-based line index.</param>
	/// <param name="character">The zero-based character index within the line.</param>
	/// <returns>The absolute document offset.</returns>
	public int GetOffset(int lineIndex, int character)
	{
		int safeLine = ClampLineIndex(lineIndex);
		int safeCharacter = Math.Clamp(character, 0, _lineLengths[safeLine]);
		return _lineStartOffsets[safeLine] + safeCharacter;
	}

	/// <summary>
	/// Gets the text of the specified zero-based line.
	/// </summary>
	/// <param name="lineIndex">The zero-based line index.</param>
	/// <returns>The line text without its trailing newline sequence.</returns>
	public string GetLineText(int lineIndex)
	{
		int safeLine = ClampLineIndex(lineIndex);
		return _content.Substring(_lineStartOffsets[safeLine], _lineLengths[safeLine]);
	}

	/// <summary>
	/// Clamps a requested line index into the valid range for the current document.
	/// </summary>
	/// <param name="lineIndex">The requested zero-based line index.</param>
	/// <returns>The nearest valid line index.</returns>
	private int ClampLineIndex(int lineIndex) => Math.Clamp(lineIndex, 0, _lineLengths.Length - 1);

	/// <summary>
	/// Builds the offset table from the given content in a single pass, avoiding the large intermediate
	/// allocations of <see cref="string.Split(char[])"/>.
	/// </summary>
	/// <param name="content">The document text to analyze.</param>
	/// <returns>A line-offset table for the supplied content.</returns>
	public static DocumentLineOffsets Build(string? content)
	{
		string text = content ?? string.Empty;

		if (text.Length == 0)
			return new DocumentLineOffsets(text, [0], [0]);

		var startOffsets = new List<int>(64) { 0 };
		var lengths = new List<int>(64);
		int currentStart = 0;

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];

			if (c == '\r')
			{
				lengths.Add(i - currentStart);

				if (i + 1 < text.Length && text[i + 1] == '\n')
					i++;

				currentStart = i + 1;
				startOffsets.Add(currentStart);
			}
			else if (c == '\n')
			{
				lengths.Add(i - currentStart);
				currentStart = i + 1;
				startOffsets.Add(currentStart);
			}
		}

		lengths.Add(text.Length - currentStart);
		return new DocumentLineOffsets(text, [.. startOffsets], [.. lengths]);
	}
}
