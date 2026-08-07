using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.UI.Hover;

/// <summary>
/// Describes the current hover request and diagnostic display state for a text editor.
/// </summary>
public readonly record struct TextHoverRequestState(
	bool ShouldRequestHover,
	int RequestOffset,
	bool CanShowToolTip,
	bool CanShowDiagnosticFallback,
	bool HasDiagnostic,
	string? DiagnosticMessage,
	TextEditorDiagnosticSeverity DiagnosticSeverity);
