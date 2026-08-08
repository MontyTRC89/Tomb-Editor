using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.ClassicScript;

public sealed partial class ClassicScriptEditor
{
	private TextHoverRequestState BuildHoverRequestState(int hoveredOffset)
	{
		bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string? diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity, allowLineFallback: false);

		return new TextHoverRequestState(
			ShouldRequestHover: true,
			RequestOffset: hoveredOffset,
			CanShowToolTip: true,
			CanShowDiagnosticFallback: false,
			HasDiagnostic: hasDiagnostic,
			DiagnosticMessage: diagnosticMessage,
			DiagnosticSeverity: diagnosticSeverity);
	}

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
}
