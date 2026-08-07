using ICSharpCode.AvalonEdit.Document;
using System.Windows.Documents;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowCompletionSessionCoordinator(GameFlowCompletionProvider completionProvider, IGameFlowScriptLineService lineService)
{
	private readonly GameFlowCompletionProvider _completionProvider = completionProvider;
	private readonly IGameFlowScriptLineService _lineService = lineService;

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
		var items = _completionProvider.GetCompletionItems(new TextCompletionContext(source.GetText(0, source.TextLength), caretOffset));

		return items.Count == 0
			? TextCompletionSessionDecision.None
			: TextCompletionSessionDecision.Open(items, startOffset, caretOffset);
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
