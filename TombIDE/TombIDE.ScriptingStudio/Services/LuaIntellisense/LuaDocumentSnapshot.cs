#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed class LuaDocumentSnapshot(string filePath, string uri, string? content, int version)
{
	public string FilePath { get; } = filePath;
	public string Uri { get; } = uri;
	public string Content { get; } = content ?? string.Empty;
	public int Version { get; } = version;
}
