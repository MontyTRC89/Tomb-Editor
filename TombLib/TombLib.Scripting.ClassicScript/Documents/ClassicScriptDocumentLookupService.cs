using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.Documents;

public sealed class ClassicScriptDocumentLookupService
{
	public bool IsLevelScriptDefined(TextDocument document, string levelName)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(levelName);

		return DocumentParser.IsLevelScriptDefined(document, levelName);
	}

	public bool IsLevelLanguageStringDefined(TextDocument document, string levelName)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(levelName);

		return DocumentParser.IsLevelLanguageStringDefined(document, levelName);
	}

	public string? TryGetIncludeFilePath(TextDocument document, int offset)
	{
		ArgumentNullException.ThrowIfNull(document);

		string? includeFilePath = CommandParser.GetFullIncludePath(document, offset);
		return string.IsNullOrWhiteSpace(includeFilePath) ? null : includeFilePath;
	}
}