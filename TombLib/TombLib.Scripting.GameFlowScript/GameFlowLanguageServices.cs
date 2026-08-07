using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Documents;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;

namespace TombLib.Scripting.GameFlowScript;

public sealed class GameFlowLanguageServices
{
	public GameFlowLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		GameFlowCompletionProvider completionProvider,
		IGameFlowScriptLineService lineService,
		IGameFlowScriptDocumentService documentService,
		GameFlowDocumentLookupService documentLookupService)
	{
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);
		ArgumentNullException.ThrowIfNull(completionProvider);
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(documentService);
		ArgumentNullException.ThrowIfNull(documentLookupService);

		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		CompletionProvider = completionProvider;
		LineService = lineService;
		DocumentService = documentService;
		DocumentLookupService = documentLookupService;
	}

	public ITextDefinitionProvider DefinitionProvider { get; }

	public ITextHoverProvider HoverProvider { get; }

	public GameFlowCompletionProvider CompletionProvider { get; }

	public IGameFlowScriptLineService LineService { get; }

	public IGameFlowScriptDocumentService DocumentService { get; }

	public GameFlowDocumentLookupService DocumentLookupService { get; }
}
