using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowAutocompleteService
{
	private static readonly ITextCompletionProvider Provider = new GameFlowCompletionProvider();

	public bool ShouldShowAutocomplete(TextDocument document, int caretOffset)
	{
		ArgumentNullException.ThrowIfNull(document);

		if (document.TextLength == 0 || caretOffset < 0 || caretOffset > document.TextLength)
			return false;

		string currentLineText = LineParser.EscapeComments(document.GetText(document.GetLineByOffset(caretOffset))).Trim();
		return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText);
	}

	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextDocument document, int caretOffset)
	{
		ArgumentNullException.ThrowIfNull(document);

		return Provider.GetCompletionItems(new TextCompletionContext(document.Text, caretOffset));
	}
}