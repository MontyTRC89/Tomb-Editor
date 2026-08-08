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
		var codeSpan = text.Slice(codeRange.Offset, codeRange.Length);

		// Trim trailing whitespace from the code.
		codeSpan = codeSpan.TrimEnd();

		// A valid continuation requires the code to end with the marker.
		return codeSpan.Length > 0 && codeSpan[^1] == continuationMarker;
	}

	/// <summary>
	/// Determines whether a code span has exactly one continuation marker at its end.
	/// This is useful for diagnostic purposes to detect duplicated markers (e.g. <c>&gt;&gt;</c>).
	/// </summary>
	/// <param name="text">The line text, potentially including a trailing comment.</param>
	/// <param name="commentDelimiter">The comment delimiter for the language.</param>
	/// <param name="continuationMarker">The continuation marker character.</param>
	/// <returns>
	/// <see langword="true"/> if the code portion ends with exactly one continuation marker
	/// (ignoring trailing whitespace); <see langword="false"/> if there are zero or multiple markers.
	/// </returns>
	public static bool HasSingleContinuationMarker(ReadOnlySpan<char> text, ReadOnlySpan<char> commentDelimiter, char continuationMarker)
	{
		// Get the code portion of the line.
		TextRange codeRange = LineCommentHelper.GetCodeRange(text, commentDelimiter);
		var codeSpan = text.Slice(codeRange.Offset, codeRange.Length);

		// Trim trailing whitespace.
		codeSpan = codeSpan.TrimEnd();

		if (codeSpan.Length == 0 || codeSpan[^1] != continuationMarker)
			return false;

		// Check that the character before the final marker is not also a marker.
		// A line like "cmd = value >>" has a duplicated marker.
		if (codeSpan.Length >= 2 && codeSpan[^2] == continuationMarker)
			return false;

		return true;
	}
}
