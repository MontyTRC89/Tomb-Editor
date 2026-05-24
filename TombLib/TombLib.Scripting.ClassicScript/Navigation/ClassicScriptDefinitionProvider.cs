using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Navigation;

namespace TombLib.Scripting.ClassicScript.Navigation;

public sealed class ClassicScriptDefinitionProvider : ITextDefinitionProvider
{
	public TextDefinitionLocation? GetDefinition(TextDefinitionRequest request)
	{
		if (request.Identifier is not ObjectType objectType || string.IsNullOrWhiteSpace(request.SymbolName))
			return null;

		var document = new TextDocument(request.DocumentText);
		DocumentLine? line = DocumentParser.FindDocumentLineOfObject(document, request.SymbolName, objectType);

		return line is null ? null : new TextDefinitionLocation(line.LineNumber);
	}
}
