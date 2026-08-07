using Nickelony.LanguageServer.Abstractions.Hover;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.GameFlowScript;

public sealed partial class GameFlowEditor
{
	private Task<TextHoverInfo?> RequestHover(int hoveredOffset, CancellationToken cancellationToken)
		=> Task.FromResult(_languageServices.HoverProvider.GetHoverInfo(new TextHoverRequest(Document.Text, hoveredOffset)));
}
