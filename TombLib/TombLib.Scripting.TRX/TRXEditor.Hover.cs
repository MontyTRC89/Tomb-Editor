using Nickelony.LanguageServer.Abstractions.Hover;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.TRX;

public sealed partial class TRXEditor
{
	/// <inheritdoc />
	protected override bool CanShowDiagnosticFallback => true;

	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_hoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
}
