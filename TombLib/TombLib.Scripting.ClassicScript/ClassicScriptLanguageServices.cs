using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Signatures;

namespace TombLib.Scripting.ClassicScript;

public sealed class ClassicScriptLanguageServices
{
	public ClassicScriptLanguageServices(
		ITextDefinitionProvider definitionProvider,
		ITextHoverProvider hoverProvider,
		ITextSignatureHelpProvider signatureHelpProvider,
		ErrorDetector errorDetector,
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		IClassicScriptIndexService indexService)
	{
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);
		ArgumentNullException.ThrowIfNull(signatureHelpProvider);
		ArgumentNullException.ThrowIfNull(errorDetector);
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(commandService);
		ArgumentNullException.ThrowIfNull(indexService);

		DefinitionProvider = definitionProvider;
		HoverProvider = hoverProvider;
		SignatureHelpProvider = signatureHelpProvider;
		ErrorDetector = errorDetector;
		LineService = lineService;
		CommandService = commandService;
		IndexService = indexService;
	}

	public ITextDefinitionProvider DefinitionProvider { get; }

	public ITextHoverProvider HoverProvider { get; }

	public ITextSignatureHelpProvider SignatureHelpProvider { get; }

	public ErrorDetector ErrorDetector { get; }

	public IClassicScriptLineService LineService { get; }

	public IClassicScriptCommandService CommandService { get; }

	public IClassicScriptIndexService IndexService { get; }
}
