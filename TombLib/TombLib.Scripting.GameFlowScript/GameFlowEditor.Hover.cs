using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Hover;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private TextHoverController CreateHoverController()
		=> new(
			owner: this,
			getOffsetFromPoint: GetOffsetFromPoint,
			buildRequestState: BuildHoverRequestState,
			requestHoverAsync: RequestHoverAsync,
			getCurrentRequestOffset: hoveredOffset => hoveredOffset,
			showDiagnosticToolTip: ShowDiagnosticToolTip,
			showHoverToolTip: ShowHoverToolTip,
			showCombinedToolTip: ShowCombinedToolTip,
			applyHoverState: _ => { });

	private TextHoverRequestState BuildHoverRequestState(int hoveredOffset)
		=> new(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: false,
			HasDiagnostic: false,
			DiagnosticMessage: null,
			DiagnosticSeverity: TextEditorDiagnosticSeverity.Error);

	private Task<TextHoverInfo?> RequestHoverAsync(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));

	private void ShowHoverToolTip(TextHoverInfo hoverInfo)
		=> ShowToolTip(
			TextHoverToolTipContentFactory.CreateHoverContent(hoverInfo, ToolTipForeground, DefaultToolTipBackground),
			DefaultToolTipBorder,
			DefaultToolTipBackground);

	private void ShowCombinedToolTip(TextHoverInfo hoverInfo, string diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
		=> ShowToolTip(
			TextHoverToolTipContentFactory.CreateCombinedContent(
				hoverInfo,
				diagnosticMessage,
				diagnosticSeverity,
				ToolTipForeground,
				DefaultToolTipBackground,
				ToolTipTextMaxWidth,
				ToolTipTextFontSize,
				GetDiagnosticColors),
			DefaultToolTipBorder,
			DefaultToolTipBackground);

	private static (SolidColorBrush Border, SolidColorBrush Background) GetDiagnosticColors(TextEditorDiagnosticSeverity severity)
	{
		GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
		return (border, background);
	}
}