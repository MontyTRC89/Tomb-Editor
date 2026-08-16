using System;
using System.Text;

namespace TombLib.Scripting.Text;

/// <summary>
/// Provides line-comment utilities for languages that use a single-character or
/// multi-character line-comment delimiter (e.g. <c>;</c> for ClassicScript, <c>//</c> for GameFlowScript and TRX).
/// For the <c>//</c> delimiter, occurrences inside double-quoted strings are not treated as
/// comments, matching the JSON-like string semantics used by GameFlowScript and TRX.
/// </summary>
public static class LineCommentHelper
{
	/// <summary>
	/// Finds the start of a line comment in the given text.
	/// The returned offset includes any whitespace immediately preceding the delimiter.
	/// </summary>
	/// <param name="text">The text to search. Typically a single line.</param>
	/// <param name="delimiter">The comment delimiter, such as <c>";"</c> or <c>"//"</c>.</param>
	/// <returns>
	/// The zero-based offset of the comment start (including leading whitespace),
	/// or <c>-1</c> if no comment is found.
	/// </returns>
	public static int FindCommentStart(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter)
	{
		int delimiterIndex = FindCommentDelimiter(text, 0, delimiter, IsQuoteAwareDelimiter(delimiter));

		if (delimiterIndex < 0)
			return -1;

		// Include whitespace immediately before the delimiter.
		int commentStart = delimiterIndex;

		while (commentStart > 0 && char.IsWhiteSpace(text[commentStart - 1]))
			commentStart--;

		return commentStart;
	}

	/// <summary>
	/// Gets the code range for a single line of text, excluding the comment.
	/// </summary>
	/// <param name="text">The text to evaluate. Typically a single line.</param>
	/// <param name="delimiter">The comment delimiter.</param>
	/// <returns>
	/// A <see cref="TextRange"/> covering the code portion of the text (before the comment).
	/// If no comment is found, the range covers the entire text.
	/// </returns>
	public static TextRange GetCodeRange(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter)
	{
		int commentStart = FindCommentStart(text, delimiter);

		if (commentStart < 0)
			return new(0, text.Length); // No comment found; the code range is the entire text.

		return new(0, commentStart); // The code range ends where the comment starts (including any preceding whitespace).
	}

	/// <summary>
	/// Removes line comments from the text. Whitespace immediately preceding the
	/// delimiter is removed, including a line ending that precedes a comment-only line.
	/// </summary>
	/// <param name="text">The text to process. May contain multiple lines.</param>
	/// <param name="delimiter">The comment delimiter.</param>
	/// <returns>The text with all line comments removed.</returns>
	public static string RemoveLineComment(string text, string delimiter)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		return TransformLineComments(text, delimiter, false);
	}

	/// <summary>
	/// Masks each line comment and its immediately preceding whitespace with spaces,
	/// preserving the total string length so character offsets remain stable for consumers that rely on them.
	/// </summary>
	/// <param name="text">The text to process. May contain multiple lines.</param>
	/// <param name="delimiter">The comment delimiter.</param>
	/// <returns>The text with comment spans replaced by spaces.</returns>
	public static string MaskLineComment(string text, string delimiter)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		return TransformLineComments(text, delimiter, true);
	}

	// JSON-like `//` comments must ignore delimiters inside quoted strings; Lua uses its own parser for `--` comments.
	private static bool IsQuoteAwareDelimiter(ReadOnlySpan<char> delimiter)
		=> delimiter.SequenceEqual("//");

	private static int FindCommentDelimiter(ReadOnlySpan<char> text, int startIndex, ReadOnlySpan<char> delimiter, bool quoteAware)
	{
		if (!quoteAware)
		{
			// Delimiters without quote-sensitive rules can be found directly.
			int offset = text[startIndex..].IndexOf(delimiter, StringComparison.Ordinal);
			return offset < 0 ? -1 : offset + startIndex;
		}

		// For JSON-like `//` comments, track whether the current position is inside a quoted string.
		bool inQuotes = false;

		for (int i = startIndex; i < text.Length; i++)
		{
			char c = text[i];

			if (c == '\n')
			{
				// A quoted string cannot continue onto the next line in this syntax.
				inQuotes = false;
				continue;
			}

			if (c == '"')
			{
				// Escaped quotes are string content, not quote-state transitions.
				if (!IsEscapedQuote(text, i))
					inQuotes = !inQuotes;

				continue;
			}

			// Only recognize a complete delimiter when it is outside a quoted string.
			if (!inQuotes && c == delimiter[0] && i + delimiter.Length <= text.Length
				&& text.Slice(i, delimiter.Length).SequenceEqual(delimiter))
			{
				return i;
			}
		}

		return -1;
	}

	private static bool IsEscapedQuote(ReadOnlySpan<char> text, int quoteIndex)
	{
		int backslashCount = 0;

		// Count the number of consecutive backslashes immediately preceding the quote.
		for (int i = quoteIndex - 1; i >= 0 && text[i] == '\\'; i--)
			backslashCount++;

		return backslashCount % 2 == 1;
	}

	private static string TransformLineComments(string text, string delimiter, bool maskComments)
	{
		bool quoteAware = IsQuoteAwareDelimiter(delimiter);
		var result = new StringBuilder(text.Length);

		int sourceOffset = 0; // The first character that has not been copied or transformed.

		while (sourceOffset < text.Length)
		{
			int delimiterOffset = FindCommentDelimiter(text, sourceOffset, delimiter, quoteAware);

			if (delimiterOffset < 0)
				break; // No more comments; the untouched remainder is appended after the loop.

			// Include whitespace before the delimiter in the span to remove or mask.
			int commentStart = delimiterOffset;

			while (commentStart > sourceOffset && char.IsWhiteSpace(text[commentStart - 1]))
				commentStart--;

			// Stop at the newline so it can normally be preserved; a later comment-only line may consume it as preceding whitespace.
			int commentEnd = delimiterOffset;

			while (commentEnd < text.Length && text[commentEnd] != '\n')
				commentEnd++;

			// Copy text before the transformed span; the comment and preceding whitespace
			// are omitted or masked below.
			result.Append(text, sourceOffset, commentStart - sourceOffset);

			if (maskComments)
				result.Append(' ', commentEnd - commentStart); // Keep offsets stable for callers that need the original string length.

			// Resume at the newline. If the next comment is on a comment-only line,
			// the next iteration treats this newline as preceding whitespace.
			sourceOffset = commentEnd;
		}

		result.Append(text, sourceOffset, text.Length - sourceOffset);
		return result.ToString();
	}
}
