#nullable enable

using Nickelony.LanguageServer.Core.Diagnostics;
using Nickelony.LanguageServer.Core.Hover;

namespace TombLib.Scripting.UI.Presentation;

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

	public bool HasDisplayableHover => HoverInfo is not null && !string.IsNullOrWhiteSpace(HoverInfo.Content);

	public bool HasDisplayableDiagnostic => HasDiagnostic && !string.IsNullOrWhiteSpace(DiagnosticMessage);
}
