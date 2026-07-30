#nullable enable

using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Hover;

namespace TombLib.Scripting.TRX;

public sealed partial class TRXEditor
{
	private TextHoverController CreateHoverController()
		=> HoverControllerFactory.Create(
			this,
			BuildHoverRequestState,
			RequestHoverAsync);

	private TextHoverRequestState BuildHoverRequestState(int hoveredOffset)
	{
		bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string? diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity);

		return new TextHoverRequestState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: true,
			HasDiagnostic: hasDiagnostic,
			DiagnosticMessage: diagnosticMessage,
			DiagnosticSeverity: diagnosticSeverity);
	}

	private Task<TextHoverInfo?> RequestHoverAsync(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_hoverService.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
}
