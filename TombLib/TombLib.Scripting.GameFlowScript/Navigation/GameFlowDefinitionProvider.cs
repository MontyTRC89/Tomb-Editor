using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.GameFlowScript.Navigation;

public sealed class GameFlowDefinitionProvider : ITextDefinitionProvider
{
    private readonly IGameFlowScriptDocumentService _documentService;

    public GameFlowDefinitionProvider(IGameFlowScriptDocumentService documentService)
    {
        ArgumentNullException.ThrowIfNull(documentService);
        _documentService = documentService;
    }

    public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
    {
        if (request.Identifier is not ObjectType objectType || string.IsNullOrWhiteSpace(request.SymbolName))
            return null;

        var source = new StringTextSnapshot(request.DocumentText);
        int? lineNumber = _documentService.FindDocumentLineOfObject(source, request.SymbolName, objectType);

        return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
    }
}
