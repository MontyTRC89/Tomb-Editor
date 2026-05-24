#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using TombLib.Scripting.Completion;

namespace TombLib.Scripting.TRX.Completion;

public sealed class CompletionSessionCoordinator
{
	private readonly ITextCompletionProvider _autocompleteService;
	private readonly TextAnalysisService _textAnalysisService;
	private readonly AutocompleteManager _autocompleteManager;

	public CompletionSessionCoordinator(
		ITextCompletionProvider autocompleteService,
		TextAnalysisService textAnalysisService,
		AutocompleteManager autocompleteManager)
	{
		_autocompleteService = autocompleteService;
		_textAnalysisService = textAnalysisService;
		_autocompleteManager = autocompleteManager;
	}

	public TextCompletionSessionDecision GetCtrlSpaceDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || !_textAnalysisService.IsValidPositionForCtrlSpaceAutocomplete(document, caretOffset))
			return TextCompletionSessionDecision.None;

		string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
		return CreateOpenDecision(document, caretOffset, currentWord);
	}

	public TextCompletionSessionDecision GetTextEnteredDecision(TextDocument document, int caretOffset, string inputText, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return HasMatchingCompletions(document, caretOffset)
				? TextCompletionSessionDecision.None
				: TextCompletionSessionDecision.Close();

		if (ShouldTriggerAutocomplete(inputText))
		{
			if (!_textAnalysisService.IsValidContextForAutocomplete(document, caretOffset))
				return TextCompletionSessionDecision.None;

			string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
			return CreateOpenDecision(document, caretOffset, currentWord);
		}

		if (_autocompleteManager.ShouldTriggerAutocompleteOnEmptyLine(document, caretOffset))
		{
			string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
			return CreateOpenDecision(document, caretOffset, currentWord);
		}

		return TextCompletionSessionDecision.None;
	}

	private static bool ShouldTriggerAutocomplete(string inputText)
		=> inputText == "\"";

	private bool HasMatchingCompletions(TextDocument document, int caretOffset)
	{
		string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
		IReadOnlyList<TextCompletionItem> autocompleteData = _autocompleteService.GetCompletionItems(new TextCompletionContext(document.Text, caretOffset));
		IReadOnlyList<TextCompletionItem> matchingCompletions = _autocompleteManager.FilterCompletions(autocompleteData, currentWord);

		return matchingCompletions.Count > 0;
	}

	private TextCompletionSessionDecision CreateOpenDecision(TextDocument document, int caretOffset, string currentWord)
	{
		IReadOnlyList<TextCompletionItem> autocompleteData = _autocompleteService.GetCompletionItems(new TextCompletionContext(document.Text, caretOffset));

		if (autocompleteData.Count == 0)
			return TextCompletionSessionDecision.None;

		IReadOnlyList<TextCompletionItem> filteredCompletions = _autocompleteManager.FilterCompletions(autocompleteData, currentWord);

		if (filteredCompletions.Count == 0)
			return TextCompletionSessionDecision.None;

		(int startOffset, int endOffset) = _autocompleteManager.GetCompletionWindowOffsets(document, caretOffset, currentWord);
		return TextCompletionSessionDecision.Open(filteredCompletions, startOffset, endOffset);
	}
}