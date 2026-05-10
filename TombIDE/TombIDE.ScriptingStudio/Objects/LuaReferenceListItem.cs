#nullable enable

using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Objects;

internal sealed record class LuaReferenceListItem(
	string FilePath,
	LuaDocumentRange Range,
	int LineNumber,
	int ColumnNumber,
	string PreviewText)
{
	public string DisplayText => $"{LineNumber}:{ColumnNumber}  {PreviewText}";
}