using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Signatures;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Signatures;

public sealed class ClassicScriptSignatureHelpProvider : ITextSignatureHelpProvider
{
	private readonly IClassicScriptCommandService _commandService;

	public ClassicScriptSignatureHelpProvider(IClassicScriptCommandService commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public TextSignatureHelpInfo? GetSignatureHelp(TextSignatureHelpRequest request)
	{
		var source = new StringTextSnapshot(request.DocumentText);
		string? syntax = _commandService.GetCommandSyntax(source, request.CaretOffset);

		if (string.IsNullOrWhiteSpace(syntax))
			return null;

		int activeParameterIndex = _commandService.GetArgumentIndexAtOffset(source, request.CaretOffset);
		return new TextSignatureHelpInfo(syntax, activeParameterIndex);
	}
}
