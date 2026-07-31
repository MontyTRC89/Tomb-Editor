using ICSharpCode.AvalonEdit.Document;
using System.Windows.Documents;
using Nickelony.LanguageServer.Core.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.UI.Text;

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
		var source = new TextDocumentSnapshot(document);

		if (completionWindowIsOpen || !_autocompleteService.ShouldShowAutocomplete(source, caretOffset))
			return TextCompletionSessionDecision.None;

		// TextUtilities.GetNextCaretPosition is AvalonEdit-specific and must use TextDocument.
		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);
		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		int startOffset = word.StartsWith(':') ? caretOffset : wordStartOffset;
		var items = _autocompleteService.GetCompletionItems(source, caretOffset);

		return items.Count == 0
			? TextCompletionSessionDecision.None
			: TextCompletionSessionDecision.Open(items, startOffset, caretOffset);
	}
}
