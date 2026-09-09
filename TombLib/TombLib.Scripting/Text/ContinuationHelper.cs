using System;

namespace TombLib.Scripting.Text;

/// <summary>
/// Provides continuation-marker utilities for languages that use a single-character
/// continuation marker at the end of a line (e.g. <c>&gt;</c> for ClassicScript).
/// </summary>
public static class ContinuationHelper
{
	/// <summary>
	/// Determines whether the given code span ends with a valid continuation marker.
	/// Trailing comments are ignored; callers should pass the code portion of the line
	/// (after comment removal) or the full line text, which will be evaluated without
	/// its trailing comment.
	/// </summary>
	/// <param name="text">The line text, potentially including a trailing comment.</param>
	/// <param name="commentDelimiter">The comment delimiter for the language.</param>
	/// <param name="continuationMarker">The continuation marker character, e.g. <c>'>'</c>.</param>
	/// <returns>
	/// <see langword="true"/> if the code portion of the line ends with the continuation marker
	/// (ignoring trailing whitespace); otherwise <see langword="false"/>.
	/// </returns>
	public static bool IsValidContinuation(ReadOnlySpan<char> text, ReadOnlySpan<char> commentDelimiter, char continuationMarker)
	{
		// Get the code portion of the line (before any comment).
		TextRange codeRange = LineCommentHelper.GetCodeRange(text, commentDelimiter);
		ReadOnlySpan<char> codeSpan = text.Slice(codeRange.Offset, codeRange.Length);

		// Trim trailing whitespace from the code.
		codeSpan = codeSpan.TrimEnd();

		// A valid continuation requires the code to end with the marker.
		return codeSpan.Length > 0 && codeSpan[^1] == continuationMarker;
	}
}
