#nullable enable

using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using Nickelony.LanguageServer.Core.Hover;
using TombLib.Scripting.Hover;
using Nickelony.LanguageServer.Core.Navigation;
using TombLib.Scripting.Navigation;
using Nickelony.LanguageServer.Core.Signatures;
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
