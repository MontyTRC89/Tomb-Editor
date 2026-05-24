#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using TombLib.Scripting.TRX.Parsers;

namespace TombLib.Scripting.TRX.Documents;

public sealed class TRXDocumentLookupService
{
	public bool IsLevelScriptDefined(TextDocument document, string levelName)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(levelName);

		return DocumentParser.IsLevelScriptDefined(document, levelName);
	}
}