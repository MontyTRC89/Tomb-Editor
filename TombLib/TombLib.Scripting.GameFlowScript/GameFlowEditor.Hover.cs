using Nickelony.LanguageServer.Abstractions.Hover;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Hover;
using TombLib.Scripting.UI.Threading;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
		=> SynchronousRequestAdapter.Adapt(
			() => _languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)),
			cancellationToken);
}
