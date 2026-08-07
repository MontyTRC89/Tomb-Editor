using Nickelony.LanguageServer.Abstractions.Navigation;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Navigation;

public sealed class ClassicScriptDefinitionProvider : ITextDefinitionProvider
{
	private readonly IClassicScriptCommandService _commandService;

	public ClassicScriptDefinitionProvider(IClassicScriptCommandService commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (request.Identifier is not ObjectType objectType || string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var source = new StringTextSnapshot(request.DocumentText);
		int? lineNumber = _commandService.FindDocumentLineOfObject(source, request.SymbolName, objectType);

		return lineNumber is null ? null : new TextDefinitionLocation(lineNumber.Value);
	}
}
