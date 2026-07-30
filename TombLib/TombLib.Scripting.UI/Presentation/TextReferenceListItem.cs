#nullable enable

using TombLib.Scripting.Editing;

namespace TombLib.Scripting.UI.Presentation;

public sealed record class TextReferenceListItem(
	string FilePath,
	TextDocumentRange Range,
	int LineNumber,
	int ColumnNumber,
	string PreviewText)
{
	public string DisplayText => $"{LineNumber}:{ColumnNumber}  {PreviewText}";
}
