using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Threading;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private TextHoverRequestState BuildHoverRequestState(int hoveredOffset)
	{
		TryGetDiagnosticInfo(hoveredOffset, out TextEditorDiagnosticInfo? diagnosticInfo, allowLineFallback: false);

		return new TextHoverRequestState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: false,
			DiagnosticMessage: diagnosticInfo?.Message,
			DiagnosticSeverity: diagnosticInfo?.Severity ?? TextEditorDiagnosticSeverity.None);
	}

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
	{
		return SynchronousRequestAdapter.Adapt(
			() => _languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)),
			cancellationToken);
	}
}
