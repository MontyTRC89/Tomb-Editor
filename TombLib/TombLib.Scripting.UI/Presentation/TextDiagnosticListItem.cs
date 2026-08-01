#nullable enable

using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.UI.Presentation;

public sealed record class TextDiagnosticListItem(
	string FilePath,
	TextEditorDiagnosticSeverity Severity,
	string SeverityLabel,
	int LineNumber,
	int ColumnNumber,
	string Message,
	int StartOffset,
	int EndOffset);
