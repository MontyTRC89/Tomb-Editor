using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;

namespace TombLib.Scripting.Presentation;

/// <summary>
/// Describes the current state of the hover presentation.
/// </summary>
/// <param name="HoveredOffset">The document offset under the mouse, or <c>-1</c> when none.</param>
/// <param name="RequestOffset">The document offset of the hover request, or <c>-1</c> when none.</param>
/// <param name="HoverInfo">The resolved hover information, when available.</param>
/// <param name="HasDiagnostic">Whether a diagnostic is available at the hovered offset.</param>
/// <param name="DiagnosticMessage">The diagnostic message, when a diagnostic is available.</param>
/// <param name="DiagnosticSeverity">The severity of the diagnostic at the hovered offset.</param>
/// <param name="CanShowToolTip">Whether the hover tool tip can be shown.</param>
/// <param name="CanShowDiagnosticFallback">Whether the diagnostic tool tip can be shown as a fallback.</param>
public readonly record struct TextHoverPresentationState(
	int HoveredOffset,
	int RequestOffset,
	TextHoverInfo? HoverInfo,
	bool HasDiagnostic,
	string? DiagnosticMessage,
	TextEditorDiagnosticSeverity DiagnosticSeverity,
	bool CanShowToolTip,
	bool CanShowDiagnosticFallback)
{
	/// <summary>
	/// Creates an empty hover presentation state.
	/// </summary>
	/// <param name="hoveredOffset">The initial hovered offset, or <c>-1</c> for none.</param>
	/// <returns>The empty hover presentation state.</returns>
	public static TextHoverPresentationState Empty(int hoveredOffset = -1)
		=> new(
			HoveredOffset: hoveredOffset,
			RequestOffset: -1,
			HoverInfo: null,
			HasDiagnostic: false,
			DiagnosticMessage: null,
			DiagnosticSeverity: default,
			CanShowToolTip: false,
			CanShowDiagnosticFallback: false);

	/// <summary>
	/// Gets a value indicating whether a non-empty hover tool tip can be shown.
	/// </summary>
	public bool HasDisplayableHover => HoverInfo is not null && !string.IsNullOrWhiteSpace(HoverInfo.Content);

	/// <summary>
	/// Gets a value indicating whether a non-empty diagnostic tool tip can be shown.
	/// </summary>
	public bool HasDisplayableDiagnostic => HasDiagnostic && !string.IsNullOrWhiteSpace(DiagnosticMessage);
}
