#nullable enable

using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.TRX.Parsers;

namespace TombLib.Scripting.TRX.Navigation;

public sealed class TRXDefinitionProvider : ITextDefinitionProvider
{
	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var document = new TextDocument(request.DocumentText);
		DocumentLine? line = DocumentParser.FindDocumentLineOfLevel(document, request.SymbolName);

		return line is null ? null : new TextDefinitionLocation(line.LineNumber);
	}
}