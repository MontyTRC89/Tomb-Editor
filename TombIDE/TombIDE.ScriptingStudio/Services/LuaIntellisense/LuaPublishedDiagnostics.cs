#nullable enable

using System.Collections.Generic;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed class LuaPublishedDiagnostics(string filePath, IReadOnlyList<TextEditorDiagnostic> diagnostics, int version)
{
	public string FilePath { get; } = filePath;
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; } = diagnostics ?? [];
	public int Version { get; } = version;
}
