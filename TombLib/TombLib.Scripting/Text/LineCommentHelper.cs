using System;
using System.Text;

namespace TombLib.Scripting.Text;

/// <summary>
/// Provides line-comment utilities for languages that use a single-character or
/// multi-character line-comment delimiter (e.g. <c>;</c> for ClassicScript, <c>//</c> for GameFlowScript).
/// For the <c>//</c> delimiter, occurrences inside double-quoted strings are not treated as
/// comments, matching the JSON-like string semantics of GameFlowScript and TRX.
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
	/// or -1 if no comment is found.
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
			return new TextRange(0, text.Length);

		return new TextRange(0, commentStart);
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
	/// Masks line comments by replacing each comment with spaces, preserving the total
	/// string length so character offsets remain stable for consumers that rely on them.
	/// </summary>
	/// <param name="text">The text to process. May contain multiple lines.</param>
	/// <param name="delimiter">The comment delimiter.</param>
	/// <returns>The text with comment characters replaced by spaces.</returns>
	public static string MaskLineComment(string text, string delimiter)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		return TransformLineComments(text, delimiter, true);
	}

	private static bool IsQuoteAwareDelimiter(ReadOnlySpan<char> delimiter)
		=> delimiter.SequenceEqual("//");

	private static int FindCommentDelimiter(ReadOnlySpan<char> text, int startIndex, ReadOnlySpan<char> delimiter, bool quoteAware)
	{
		if (!quoteAware)
		{
			int offset = text.Slice(startIndex).IndexOf(delimiter, StringComparison.Ordinal);
			return offset < 0 ? -1 : offset + startIndex;
		}

		bool inQuotes = false;

		for (int i = startIndex; i < text.Length; i++)
		{
			char c = text[i];

			if (c == '\n')
			{
				inQuotes = false;
				continue;
			}

			if (c == '"')
			{
				if (!IsEscapedQuote(text, i))
					inQuotes = !inQuotes;

				continue;
			}

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

		for (int i = quoteIndex - 1; i >= 0 && text[i] == '\\'; i--)
			backslashCount++;

		return backslashCount % 2 == 1;
	}

	private static string TransformLineComments(string text, string delimiter, bool maskComments)
	{
		bool quoteAware = IsQuoteAwareDelimiter(delimiter);
		var result = new StringBuilder(text.Length);
		int sourceOffset = 0;

		while (sourceOffset < text.Length)
		{
			int delimiterOffset = FindCommentDelimiter(text, sourceOffset, delimiter, quoteAware);

			if (delimiterOffset < 0)
				break;

			int commentStart = delimiterOffset;

			while (commentStart > sourceOffset && char.IsWhiteSpace(text[commentStart - 1]))
				commentStart--;

			int commentEnd = delimiterOffset;

			while (commentEnd < text.Length && text[commentEnd] != '\n')
				commentEnd++;

			result.Append(text, sourceOffset, commentStart - sourceOffset);

			if (maskComments)
				result.Append(' ', commentEnd - commentStart);

			sourceOffset = commentEnd;
		}

		result.Append(text, sourceOffset, text.Length - sourceOffset);
		return result.ToString();
	}
}
