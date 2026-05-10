#nullable enable

using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Objects;

internal sealed record class LuaDiagnosticListItem(
	string FilePath,
	TextEditorDiagnosticSeverity Severity,
	string SeverityLabel,
	int LineNumber,
	int ColumnNumber,
	string Message,
	int StartOffset,
	int EndOffset);