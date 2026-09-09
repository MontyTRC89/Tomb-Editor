using Nickelony.LanguageServer.Abstractions.Hover;
using TombLib.Scripting.Diagnostics;

namespace TombLib.Scripting.Presentation;

/// <summary>
/// Describes the current state of the hover presentation.
/// </summary>
/// <param name="HoveredOffset">The document offset under the mouse, or <c>-1</c> when none.</param>
/// <param name="RequestOffset">The document offset of the hover request, or <c>-1</c> when none.</param>
/// <param name="HoverInfo">The resolved hover information, when available.</param>
/// <param name="DiagnosticInfo">The diagnostic information at the hovered offset, when available.</param>
/// <param name="CanShowToolTip">Whether the hover tooltip can be shown.</param>
/// <param name="CanShowDiagnosticFallback">Whether the diagnostic tooltip can be shown as a fallback.</param>
public readonly record struct TextHoverPresentationState(
	int HoveredOffset,
	int RequestOffset,
	TextHoverInfo? HoverInfo,
	TextEditorDiagnosticInfo? DiagnosticInfo,
	bool CanShowToolTip,
	bool CanShowDiagnosticFallback)
{
	/// <summary>
	/// Creates an empty hover presentation state.
	/// </summary>
	/// <param name="hoveredOffset">The initial hovered offset, or <c>-1</c> for none.</param>
	/// <returns>The empty hover presentation state.</returns>
	public static TextHoverPresentationState Empty(int hoveredOffset = -1) => new(
		HoveredOffset: hoveredOffset,
		RequestOffset: -1,
		HoverInfo: null,
		DiagnosticInfo: null,
		CanShowToolTip: false,
		CanShowDiagnosticFallback: false);

	/// <summary>
	/// Gets a value indicating whether a non-empty hover tooltip can be shown.
	/// </summary>
	public bool HasDisplayableHover => HoverInfo is not null && !string.IsNullOrWhiteSpace(HoverInfo.Content);

	/// <summary>
	/// Gets a value indicating whether a non-empty diagnostic tooltip can be shown.
	/// </summary>
	public bool HasDisplayableDiagnostic
		=> DiagnosticInfo is not null && !string.IsNullOrWhiteSpace(DiagnosticInfo.Message);
}
