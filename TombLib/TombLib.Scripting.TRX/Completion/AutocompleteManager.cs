#nullable enable

using Nickelony.LanguageServer.Abstractions.Completion;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.TRX.Completion;

public sealed class AutocompleteManager
{
	private readonly ITRXLineService _lineService;

	public AutocompleteManager(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	public IReadOnlyList<TextCompletionItem> FilterCompletions(IReadOnlyList<TextCompletionItem> autocompleteData, string currentWord)
	{
		if (string.IsNullOrEmpty(currentWord))
			return autocompleteData;

		// Extract the actual word content for matching (remove quotes if present)
		string wordForMatching = currentWord.Trim('"');
		string lowerCurrentWord = wordForMatching.ToLower();

		return autocompleteData.Where(item =>
		{
			string lowerItemText = item.InsertText.ToLower();

			// If we have no actual word content (just quotes), show all
			if (string.IsNullOrEmpty(lowerCurrentWord))
				return true;

			// Exact prefix match (highest priority)
			if (lowerItemText.StartsWith('"' + lowerCurrentWord))
				return true;

			// Prefix match without quotes
			if (lowerItemText.StartsWith(lowerCurrentWord))
				return true;

			// Contains match (lower priority)
			if (lowerItemText.Contains(lowerCurrentWord))
				return true;

			return false;
		})
		.OrderBy(item =>
		{
			string lowerItemText = item.InsertText.ToLower();

			// Prioritize exact prefix matches
			return lowerItemText.StartsWith('"' + lowerCurrentWord) || lowerItemText.StartsWith(lowerCurrentWord) ? 0 : 1;
		})
		.ToList();
	}

	public bool ShouldTriggerAutocompleteOnEmptyLine(ITextSnapshot source, int caretOffset)
	{
		ITextLine line = source.GetLineByOffset(caretOffset);
		string lineText = source.GetText(line.Offset, line.Length);
		string currentLineText = _lineService.EscapeComments(lineText).Trim();

		return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText, char.IsLetter)
			|| currentLineText.Equals("\"\"");
	}

	public (int startOffset, int endOffset) GetCompletionWindowOffsets(ITextSnapshot source, int caretOffset, string currentWord)
	{
		if (!string.IsNullOrEmpty(currentWord))
		{
			// Calculate the start offset based on the actual current word (including quotes if present)
			int startOffset = caretOffset - currentWord.Length;

			// Check if there's a quote after the caret that should be included in replacement
			int endOffset = caretOffset < source.TextLength && source.GetCharAt(caretOffset) == '\"'
				? caretOffset + 1
				: caretOffset;

			return (startOffset, endOffset);
		}
		else
		{
			// No current word, start from current position
			return (caretOffset, caretOffset);
		}
	}
}
