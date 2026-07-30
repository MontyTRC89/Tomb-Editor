using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowAutocompleteService
{
    private static readonly ITextCompletionProvider Provider = new GameFlowCompletionProvider();
    private readonly IGameFlowScriptLineService _lineService;

    public GameFlowAutocompleteService(IGameFlowScriptLineService lineService)
    {
        ArgumentNullException.ThrowIfNull(lineService);
        _lineService = lineService;
    }

    public bool ShouldShowAutocomplete(ITextSnapshot source, int caretOffset)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.TextLength == 0 || caretOffset < 0 || caretOffset > source.TextLength)
            return false;

        ITextLine line = source.GetLineByOffset(caretOffset);
        string currentLineText = _lineService.EscapeComments(source.GetText(line.Offset, line.Length)).Trim();
        return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText);
    }

    public IReadOnlyList<TextCompletionItem> GetCompletionItems(ITextSnapshot source, int caretOffset)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Provider.GetCompletionItems(new TextCompletionContext(source.GetText(0, source.TextLength), caretOffset));
    }
}
