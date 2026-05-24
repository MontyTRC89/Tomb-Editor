using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Signatures;

namespace TombLib.Scripting.ClassicScript.Signatures;

public sealed class ClassicScriptSignatureHelpProvider : ITextSignatureHelpProvider
{
	public TextSignatureHelpInfo? GetSignatureHelp(TextSignatureHelpRequest request)
	{
		var document = new TextDocument(request.DocumentText);
		string? syntax = CommandParser.GetCommandSyntax(document, request.CaretOffset);

		if (string.IsNullOrWhiteSpace(syntax))
			return null;

		int activeParameterIndex = ArgumentParser.GetArgumentIndexAtOffset(document, request.CaretOffset);
		return new TextSignatureHelpInfo(syntax, activeParameterIndex);
	}
}
