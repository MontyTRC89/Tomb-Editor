using System.Linq;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Completion;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptCompletionSessionCoordinatorTests
{
	[TestMethod]
	public async Task GetTextEnteredDecisionAsync_AfterEqualsSpace_OpensContextualCompletionAtCaret()
	{
		var coordinator = new ClassicScriptCompletionSessionCoordinator();

		TextCompletionSessionDecision decision = await coordinator.GetTextEnteredDecisionAsync(
			"Horizon= ",
			null,
			9,
			" ",
			false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(decision.Items.Any(item => item.Label == "DISABLED"));
		Assert.AreEqual(9, decision.StartOffset);
		Assert.AreEqual(9, decision.EndOffset);
	}

	[TestMethod]
	public async Task GetTextEnteredDecisionAsync_AfterCommaSpace_OpensContextualCompletionAtCaret()
	{
		var coordinator = new ClassicScriptCompletionSessionCoordinator();

		const string text = "Customize= CUST_LOOK_TRASPARENT, ";

		TextCompletionSessionDecision decision = await coordinator.GetTextEnteredDecisionAsync(
			text,
			null,
			text.Length,
			" ",
			false);

		Assert.IsNotNull(decision.Items);
		Assert.IsTrue(decision.Items.Any(item => item.Label == "ENABLED"));
		Assert.IsTrue(decision.Items.Any(item => item.Label == "DISABLED"));
		Assert.AreEqual(text.Length, decision.StartOffset);
		Assert.AreEqual(text.Length, decision.EndOffset);
	}
}