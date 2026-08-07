using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Completion;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Services;

namespace TombLib.Tests;

[TestClass]
public class GameFlowCompletionSessionCoordinatorTests
{
	private static GameFlowCompletionSessionCoordinator CreateCoordinator()
		=> new(new GameFlowCompletionProvider(), new GameFlowScriptLineService());

	[TestMethod]
	public void GetOpenDecision_EmptyDocument_ReturnsNone()
	{
		GameFlowCompletionSessionCoordinator coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = coordinator.GetOpenDecision(new TextDocument(string.Empty), 0, false);

		Assert.AreEqual(TextCompletionSessionDecision.None, decision);
	}

	[TestMethod]
	public void GetOpenDecision_CaretAtStartOfSingleCharacterLine_ReturnsNone()
	{
		GameFlowCompletionSessionCoordinator coordinator = CreateCoordinator();

		TextCompletionSessionDecision decision = coordinator.GetOpenDecision(new TextDocument("A"), 0, false);

		Assert.AreEqual(TextCompletionSessionDecision.None, decision);
	}
}
