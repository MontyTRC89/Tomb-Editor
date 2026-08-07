using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.GameFlowScript.Documents;

/// <summary>
/// Boundary facade over <see cref="IGameFlowScriptDocumentService"/> that answers
/// document-level lookups (level-script existence) on behalf of host consumers such as
/// TombIDE. Retained as a thin abstraction so host consumers do not depend on the
/// document service surface directly; it is consumed by TombIDE and tests.
/// </summary>
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
