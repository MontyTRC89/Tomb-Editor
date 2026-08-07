using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.GameFlowScript.Documents;

public sealed class GameFlowDocumentLookupService
{
	private readonly IGameFlowScriptDocumentService _documentService;

	public GameFlowDocumentLookupService(IGameFlowScriptDocumentService documentService)
	{
		ArgumentNullException.ThrowIfNull(documentService);
		_documentService = documentService;
	}

	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		return _documentService.IsLevelScriptDefined(source, levelName);
	}
}
