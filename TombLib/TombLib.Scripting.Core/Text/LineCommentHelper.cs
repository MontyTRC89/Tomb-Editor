using System.Text;

namespace TombLib.Scripting.Text;

/// <summary>
/// Provides line-comment utilities for languages that use a single-character or
/// multi-character line-comment delimiter (e.g. <c>;</c> for ClassicScript, <c>//</c> for GameFlowScript).
/// </summary>
public static class LineCommentHelper
{
    /// <summary>
    /// Finds the start of a line comment in the given text.
    /// The returned offset includes any whitespace immediately preceding the delimiter,
    /// matching the legacy regex <c>\s*;.*$</c> behavior.
    /// </summary>
    /// <param name="text">The text to search. Typically a single line.</param>
    /// <param name="delimiter">The comment delimiter, such as <c>";"</c> or <c>"//"</c>.</param>
    /// <returns>
    /// The zero-based offset of the comment start (including leading whitespace),
    /// or -1 if no comment is found.
    /// </returns>
    public static int FindCommentStart(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter)
    {
        int delimiterIndex = text.IndexOf(delimiter, StringComparison.Ordinal);

        if (delimiterIndex < 0)
            return -1;

        // Match the legacy regex's Unicode whitespace behavior.
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
    /// Removes line comments from the text, matching the legacy multiline regex
    /// <c>\s*;.*$</c> behavior. This can remove preceding whitespace, including
    /// a line ending before a comment-only line.
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
    /// Masks line comments by replacing the legacy regex match with spaces while
    /// preserving the total string length. This exists as a temporary parity bridge
    /// where old algorithms depend on character positions.
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

    private static string TransformLineComments(string text, string delimiter, bool maskComments)
    {
        var result = new StringBuilder(text.Length);
        int sourceOffset = 0;

        while (sourceOffset < text.Length)
        {
            int delimiterOffset = text.AsSpan(sourceOffset).IndexOf(delimiter, StringComparison.Ordinal);

            if (delimiterOffset < 0)
                break;

            delimiterOffset += sourceOffset;
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
