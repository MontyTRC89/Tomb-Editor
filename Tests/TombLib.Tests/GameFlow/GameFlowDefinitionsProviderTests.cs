using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;

namespace TombLib.Tests;

[TestClass]
public class GameFlowDefinitionsProviderTests
{
	[TestMethod]
	public void SectionHover_ProvidesDefinitionIdentifier()
	{
		var hoverProvider = new GameFlowHoverProvider();
		TextHoverInfo? hoverInfo = hoverProvider.GetHoverInfo(new TextHoverRequest("TITLE:\nLEVEL: Caves", 1));

		Assert.IsNotNull(hoverInfo);
		Assert.AreEqual(ObjectType.Section, hoverInfo.Identifier);

		var lineService = new GameFlowScriptLineService();
		var documentService = new GameFlowScriptDocumentService(lineService);
		var definitionProvider = new GameFlowDefinitionProvider(documentService);
		var request = new TextDefinitionRequest("TITLE:\nLEVEL: Caves", hoverInfo.SymbolName!, hoverInfo.Identifier);

		TextDefinitionLocation? definition = definitionProvider.GetDefinition(request);

		Assert.IsNotNull(definition);
		Assert.AreEqual(1, definition.LineNumber);
	}

	[TestMethod]
	public void Definitions_AreLoadedFromBundledJson()
	{
		Assert.IsTrue(GameFlowDefinitionsProvider.Sections.Count > 0);
		Assert.IsTrue(GameFlowDefinitionsProvider.Properties.Count > 0);
		Assert.IsTrue(GameFlowDefinitionsProvider.Constants.Count > 0);
	}
}