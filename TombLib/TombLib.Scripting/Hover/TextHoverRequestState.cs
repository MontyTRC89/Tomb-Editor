using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.Hover;

/// <summary>
/// Describes the current hover request and diagnostic display state for a text editor.
/// </summary>
/// <param name="ShouldRequestHover">Whether a hover request should be issued for the hovered offset.</param>
/// <param name="RequestOffset">The zero-based document offset for which hover is requested.</param>
/// <param name="CanShowToolTip">Whether the hover tooltip may be shown.</param>
/// <param name="CanShowDiagnosticFallback">Whether the diagnostic tooltip may be shown as a fallback when no hover content is available.</param>
/// <param name="HasDiagnostic">Whether a diagnostic is available at the hovered offset.</param>
/// <param name="DiagnosticMessage">The diagnostic message, or <c>null</c> when no diagnostic is available.</param>
/// <param name="DiagnosticSeverity">The severity of the diagnostic at the hovered offset.</param>
public readonly record struct TextHoverRequestState(
	bool ShouldRequestHover,
	int RequestOffset,
	bool CanShowToolTip,
	bool CanShowDiagnosticFallback,
	bool HasDiagnostic,
	string? DiagnosticMessage,
	TextEditorDiagnosticSeverity DiagnosticSeverity);
