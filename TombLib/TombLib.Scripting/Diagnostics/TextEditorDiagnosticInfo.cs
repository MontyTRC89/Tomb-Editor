using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Describes the diagnostic information displayed by a text editor.
/// </summary>
/// <param name="Message">The formatted diagnostic message.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
public sealed record TextEditorDiagnosticInfo(
	string Message,
	TextEditorDiagnosticSeverity Severity);
