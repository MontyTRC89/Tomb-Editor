namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Computes a minimal single-range edit that transforms <c>oldText</c> into <c>newText</c> using a
/// common-prefix / common-suffix scan. The result is suitable for an LSP <c>textDocument/didChange</c>
/// incremental notification: a server applying the returned range and replacement text to the old
/// content reproduces the new content byte-for-byte.
/// </summary>
public static class DocumentIncrementalEditCalculator
{
	/// <summary>
	/// Computes the minimal single-range edit that transforms one document snapshot into another.
	/// </summary>
	/// <param name="oldText">The previously synchronized document content.</param>
	/// <param name="newText">The updated document content.</param>
	/// <param name="oldOffsets">The line-offset table for <paramref name="oldText"/>.</param>
	/// <returns>The incremental change range to send in <c>textDocument/didChange</c>.</returns>
	public static DocumentChangeRange Compute(string oldText, string newText, DocumentLineOffsets oldOffsets)
	{
		oldText ??= string.Empty;
		newText ??= string.Empty;

		int prefixLength = ComputeCommonPrefixLength(oldText, newText);
		int suffixLength = ComputeCommonSuffixLength(oldText, newText, prefixLength);

		int oldEnd = oldText.Length - suffixLength;
		int newEnd = newText.Length - suffixLength;

		string replacement = newEnd > prefixLength ? newText[prefixLength..newEnd] : string.Empty;

		(int startLine, int startCharacter) = OffsetToPosition(oldOffsets, prefixLength);
		(int endLine, int endCharacter) = OffsetToPosition(oldOffsets, oldEnd);

		return new DocumentChangeRange(startLine, startCharacter, endLine, endCharacter, replacement);
	}

	/// <summary>
	/// Computes the common prefix length shared by two strings.
	/// </summary>
	/// <param name="a">The first string.</param>
	/// <param name="b">The second string.</param>
	/// <returns>The number of leading characters shared by both strings.</returns>
	private static int ComputeCommonPrefixLength(string a, string b)
	{
		int max = Math.Min(a.Length, b.Length);
		int i = 0;

		while (i < max && a[i] == b[i])
			i++;

		return i;
	}

	/// <summary>
	/// Computes the common suffix length shared by two strings after the prefix overlap has been removed.
	/// </summary>
	/// <param name="a">The first string.</param>
	/// <param name="b">The second string.</param>
	/// <param name="prefixLength">The already matched common prefix length.</param>
	/// <returns>The number of trailing characters shared by both strings.</returns>
	private static int ComputeCommonSuffixLength(string a, string b, int prefixLength)
	{
		int max = Math.Min(a.Length, b.Length) - prefixLength;
		int i = 0;

		while (i < max && a[a.Length - 1 - i] == b[b.Length - 1 - i])
			i++;

		return i;
	}

	/// <summary>
	/// Converts a document offset into a zero-based line and character position.
	/// </summary>
	/// <param name="offsets">The line-offset table for the original document.</param>
	/// <param name="offset">The zero-based document offset.</param>
	/// <returns>The zero-based line and character position.</returns>
	private static (int Line, int Character) OffsetToPosition(DocumentLineOffsets offsets, int offset)
	{
		int lineCount = offsets.LineCount;

		if (lineCount == 0)
			return (0, 0);

		int low = 0;
		int high = lineCount - 1;

		while (low < high)
		{
			int mid = (low + high + 1) >>> 1;

			if (offsets.GetLineStartOffset(mid) <= offset)
				low = mid;
			else
				high = mid - 1;
		}

		int lineStart = offsets.GetLineStartOffset(low);
		int character = Math.Max(0, offset - lineStart);

		return (low, character);
	}
}
