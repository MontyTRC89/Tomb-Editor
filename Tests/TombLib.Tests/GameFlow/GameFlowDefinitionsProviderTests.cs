using TombLib.Scripting.Specifications.GameFlow;

namespace TombLib.Tests;

[TestClass]
public class GameFlowDefinitionsProviderTests
{
	[TestMethod]
	public void Definitions_AreLoadedFromBundledJson()
	{
		Assert.IsTrue(GameFlowDefinitionsProvider.Sections.Count > 0);
		Assert.IsTrue(GameFlowDefinitionsProvider.Properties.Count > 0);
		Assert.IsTrue(GameFlowDefinitionsProvider.Constants.Count > 0);
	}
}