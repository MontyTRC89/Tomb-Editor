#nullable enable

using Nickelony.LanguageServer.Abstractions.Hover;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.TRX;

public sealed partial class TRXEditor
{
	protected override bool CanShowDiagnosticFallback => true;

	private Task<TextHoverInfo?> RequestHoverAsync(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_hoverService.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
}
