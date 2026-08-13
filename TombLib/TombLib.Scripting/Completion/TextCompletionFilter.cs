using Nickelony.LanguageServer.Abstractions.Completion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Scripting.Completion;

/// <summary>
/// Provides completion-item filtering based on the word being typed at the request point.
/// </summary>
public static class TextCompletionFilter
{
	/// <summary>
	/// Reduces a completion item set to the items matching the word being typed at the request point.
	/// </summary>
	/// <remarks>
	/// Matching is case-insensitive and tolerant: an item is kept when its insertion text starts with or
	/// contains the typed word. An empty word (for example on a fresh line or via Ctrl+Space) keeps all items.
	/// </remarks>
	/// <param name="items">The candidate completion items.</param>
	/// <param name="context">The completion request context.</param>
	/// <returns>The filtered completion items.</returns>
	public static IReadOnlyList<TextCompletionItem> FilterByCurrentWord(IReadOnlyList<TextCompletionItem> items, TextCompletionContext context)
	{
		string word = GetCurrentWord(context.DocumentText, context.CaretOffset);

		if (string.IsNullOrEmpty(word))
			return [.. items];

		return [.. items.Where(item => MatchesWord(item, word))];
	}

	private static bool MatchesWord(TextCompletionItem item, string word)
		=> (item.InsertText ?? string.Empty).Contains(word, StringComparison.OrdinalIgnoreCase);

	private static string GetCurrentWord(string documentText, int caretOffset)
	{
		if (string.IsNullOrEmpty(documentText) || caretOffset <= 0 || caretOffset > documentText.Length)
			return string.Empty;

		int start = caretOffset;

		while (start > 0 && IsWordCharacter(documentText[start - 1]))
			start--;

		return documentText[start..caretOffset];
	}

	private static bool IsWordCharacter(char character)
		=> char.IsLetterOrDigit(character) || character == '_';
}
