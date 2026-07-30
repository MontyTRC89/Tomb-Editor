using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Hover;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private TextHoverController CreateHoverController()
		=> HoverControllerFactory.Create(
			this,
			BuildHoverRequestState,
			RequestHoverAsync);

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
}
