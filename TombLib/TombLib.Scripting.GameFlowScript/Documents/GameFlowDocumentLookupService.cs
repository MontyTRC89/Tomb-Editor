using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.GameFlowScript.Parsers;

namespace TombLib.Scripting.GameFlowScript.Documents;

public sealed class GameFlowDocumentLookupService
{
	public bool IsLevelScriptDefined(TextDocument document, string levelName)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(levelName);

		return DocumentParser.IsLevelScriptDefined(document, levelName);
	}
}
