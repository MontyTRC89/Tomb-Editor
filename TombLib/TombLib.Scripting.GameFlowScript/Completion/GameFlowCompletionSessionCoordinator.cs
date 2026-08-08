using ICSharpCode.AvalonEdit.Document;
using System.Windows.Documents;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.GameFlowScript.Completion;

/// <summary>
/// Coordinates completion session decisions for the GameFlow editor.
/// </summary>
/// <param name="completionProvider">The completion provider used to source completion items.</param>
/// <param name="lineService">The line service used to analyze document lines.</param>
public sealed class GameFlowCompletionSessionCoordinator(GameFlowCompletionProvider completionProvider, IGameFlowScriptLineService lineService)
{
	private readonly GameFlowCompletionProvider _completionProvider = completionProvider;
	private readonly IGameFlowScriptLineService _lineService = lineService;

	/// <summary>
	/// Gets the decision for whether a completion session should open at the caret.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The caret offset.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision.</returns>
	public TextCompletionSessionDecision GetOpenDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		var source = new TextDocumentSnapshot(document);

		if (completionWindowIsOpen || !ShouldShowCompletion(source, caretOffset))
			return TextCompletionSessionDecision.None;

		// TextUtilities.GetNextCaretPosition is AvalonEdit-specific and must use TextDocument.
		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

		if (wordStartOffset < 0)
			return TextCompletionSessionDecision.None;

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		int startOffset = word.StartsWith(':') ? caretOffset : wordStartOffset;
		var context = new TextCompletionContext(source.GetText(0, source.TextLength), caretOffset);
		var items = _completionProvider.GetCompletionItems(context);
		var filteredItems = TextCompletionFilter.FilterByCurrentWord(items, context);

		return filteredItems.Count == 0
			? TextCompletionSessionDecision.None
			: TextCompletionSessionDecision.Open(filteredItems, startOffset, caretOffset);
	}

	private bool ShouldShowCompletion(ITextSnapshot source, int caretOffset)
	{
		if (source.TextLength == 0 || caretOffset < 0 || caretOffset > source.TextLength)
			return false;

		ITextLine line = source.GetLineByOffset(caretOffset);
		string currentLineText = _lineService.EscapeComments(source.GetText(line.Offset, line.Length)).Trim();
		return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText);
	}
}
