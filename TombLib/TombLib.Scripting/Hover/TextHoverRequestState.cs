using TombLib.Scripting.Diagnostics;

namespace TombLib.Scripting.Hover;

/// <summary>
/// Describes the current hover request and diagnostic display state for a text editor.
/// </summary>
/// <param name="ShouldRequestHover">Whether a hover request should be issued for the hovered offset.</param>
/// <param name="RequestOffset">The zero-based document offset for which hover is requested.</param>
/// <param name="CanShowToolTip">Whether the hover tooltip may be shown.</param>
/// <param name="CanShowDiagnosticFallback">Whether the diagnostic tooltip may be shown as a fallback when no hover content is available.</param>
/// <param name="DiagnosticInfo">The diagnostic information at the hovered offset, when available.</param>
public readonly record struct TextHoverRequestState(
	bool ShouldRequestHover,
	int RequestOffset,
	bool CanShowToolTip,
	bool CanShowDiagnosticFallback,
	TextEditorDiagnosticInfo? DiagnosticInfo);
