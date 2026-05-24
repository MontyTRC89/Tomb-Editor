using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Completion;
using System.Windows.Documents;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowCompletionSessionCoordinator(GameFlowAutocompleteService autocompleteService)
{
	private readonly GameFlowAutocompleteService _autocompleteService = autocompleteService;

	public TextCompletionSessionDecision GetCtrlSpaceDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
		=> GetOpenDecision(document, caretOffset, completionWindowIsOpen);

	public TextCompletionSessionDecision GetTextEnteredDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
		=> GetOpenDecision(document, caretOffset, completionWindowIsOpen);

	private TextCompletionSessionDecision GetOpenDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || !_autocompleteService.ShouldShowAutocomplete(document, caretOffset))
			return TextCompletionSessionDecision.None;

		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);
		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		int startOffset = word.StartsWith(':') ? caretOffset : wordStartOffset;
		var items = _autocompleteService.GetCompletionItems(document, caretOffset);

		return items.Count == 0
			? TextCompletionSessionDecision.None
			: TextCompletionSessionDecision.Open(items, startOffset, caretOffset);
	}
}
