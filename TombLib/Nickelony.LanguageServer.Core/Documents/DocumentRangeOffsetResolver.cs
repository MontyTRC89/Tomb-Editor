namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Resolves document offsets for protocol ranges, including stable anchors for zero-width ranges.
/// </summary>
public static class DocumentRangeOffsetResolver
{
	/// <summary>
	/// Resolves start and end offsets for a range within a document.
	/// </summary>
	/// <param name="lineOffsets">The line-offset table for the document.</param>
	/// <param name="startLineIndex">The zero-based start line index.</param>
	/// <param name="startCharacter">The zero-based start character within <paramref name="startLineIndex"/>.</param>
	/// <param name="endLineIndex">The zero-based end line index.</param>
	/// <param name="endCharacter">The zero-based end character within <paramref name="endLineIndex"/>.</param>
	/// <param name="startOffset">Receives the resolved start offset.</param>
	/// <param name="endOffset">Receives the resolved end offset.</param>
	/// <returns><see langword="true"/> when a usable offset range could be resolved.</returns>
	public static bool TryResolveOffsets(DocumentLineOffsets lineOffsets,
		int startLineIndex, int startCharacter, int endLineIndex, int endCharacter,
		out int startOffset, out int endOffset)
	{
		startOffset = 0;
		endOffset = 0;

		if (lineOffsets.LineCount == 0)
			return false;

		startOffset = lineOffsets.GetOffset(startLineIndex, startCharacter);
		endOffset = lineOffsets.GetOffset(endLineIndex, endCharacter);

		if (endOffset > startOffset)
			return true;

		string lineText = lineOffsets.GetLineText(startLineIndex);
		int lineStartOffset = lineOffsets.GetLineStartOffset(startLineIndex);

		if (string.IsNullOrEmpty(lineText))
			return TryGetEmptyLineFallbackOffsets(lineOffsets, startLineIndex, out startOffset, out endOffset);

		int safeCharacter = Math.Max(0, Math.Min(startCharacter, Math.Max(0, lineText.Length - 1)));

		if (TryGetWordBounds(lineText, safeCharacter, out int wordStart, out int wordEnd))
		{
			startOffset = lineStartOffset + wordStart;
			endOffset = lineStartOffset + wordEnd;
			return endOffset > startOffset;
		}

		int trimmedStart = 0;
		int trimmedEnd = lineText.Length;

		while (trimmedStart < trimmedEnd && char.IsWhiteSpace(lineText[trimmedStart]))
			trimmedStart++;

		while (trimmedEnd > trimmedStart && char.IsWhiteSpace(lineText[trimmedEnd - 1]))
			trimmedEnd--;

		if (trimmedEnd > trimmedStart)
		{
			startOffset = lineStartOffset + trimmedStart;
			endOffset = lineStartOffset + trimmedEnd;
			return true;
		}

		startOffset = lineStartOffset + safeCharacter;
		endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
		return endOffset > startOffset;
	}

	/// <summary>
	/// Resolves a fallback anchor range for an empty line by inspecting neighboring non-empty lines.
	/// </summary>
	/// <param name="lineOffsets">The document line-offset table.</param>
	/// <param name="lineIndex">The zero-based empty line index.</param>
	/// <param name="startOffset">Receives the resolved start offset.</param>
	/// <param name="endOffset">Receives the resolved end offset.</param>
	/// <returns><see langword="true"/> when a fallback anchor could be resolved.</returns>
	private static bool TryGetEmptyLineFallbackOffsets(DocumentLineOffsets lineOffsets, int lineIndex,
		out int startOffset, out int endOffset)
	{
		startOffset = 0;
		endOffset = 0;

		for (int nextLineIndex = lineIndex + 1; nextLineIndex < lineOffsets.LineCount; nextLineIndex++)
		{
			if (lineOffsets.GetLineLength(nextLineIndex) == 0)
				continue;

			startOffset = lineOffsets.GetLineStartOffset(nextLineIndex);
			endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
			return endOffset > startOffset;
		}

		for (int previousLineIndex = lineIndex - 1; previousLineIndex >= 0; previousLineIndex--)
		{
			int previousLineLength = lineOffsets.GetLineLength(previousLineIndex);

			if (previousLineLength == 0)
				continue;

			startOffset = lineOffsets.GetLineStartOffset(previousLineIndex) + previousLineLength - 1;
			endOffset = Math.Min(startOffset + 1, lineOffsets.TextLength);
			return endOffset > startOffset;
		}

		return false;
	}

	/// <summary>
	/// Resolves the contiguous word-like span around a candidate character index.
	/// </summary>
	/// <param name="lineText">The line text being inspected.</param>
	/// <param name="index">The preferred anchor index.</param>
	/// <param name="wordStart">Receives the inclusive word start index.</param>
	/// <param name="wordEnd">Receives the exclusive word end index.</param>
	/// <returns><see langword="true"/> when a usable word span could be resolved.</returns>
	private static bool TryGetWordBounds(string lineText, int index, out int wordStart, out int wordEnd)
	{
		wordStart = 0;
		wordEnd = 0;

		if (string.IsNullOrEmpty(lineText))
			return false;

		int safeIndex = Math.Max(0, Math.Min(index, lineText.Length - 1));

		if (!IsRangeAnchorCharacter(lineText[safeIndex]) && safeIndex > 0 && IsRangeAnchorCharacter(lineText[safeIndex - 1]))
			safeIndex--;

		while (safeIndex < lineText.Length && !IsRangeAnchorCharacter(lineText[safeIndex]))
		{
			safeIndex++;

			if (safeIndex >= lineText.Length)
				return false;
		}

		wordStart = safeIndex;
		wordEnd = safeIndex;

		while (wordStart > 0 && IsRangeAnchorCharacter(lineText[wordStart - 1]))
			wordStart--;

		while (wordEnd < lineText.Length && IsRangeAnchorCharacter(lineText[wordEnd]))
			wordEnd++;

		return wordEnd > wordStart;
	}

	/// <summary>
	/// Reports whether a character is considered a stable range anchor for fallback selection.
	/// </summary>
	/// <param name="c">The character to inspect.</param>
	/// <returns><see langword="true"/> when the character can anchor a range.</returns>
	private static bool IsRangeAnchorCharacter(char c)
		=> char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == ':' || c == '\'' || c == '"';
}
