using DarkUI.Controls;
using TombLib.Scripting.GameFlowScript.Enums;
using TombLib.Scripting.GameFlowScript.ContentNodes;

namespace TombLib.Tests;

[TestClass]
public class GameFlowNodesProviderTests
{
	[TestMethod]
	public void GetNodes_ReturnsSectionAndLevelGroupsForMatchingContent()
	{
		var provider = new GameFlowNodesProvider();
		const string content = "TITLE:\r\nLEVEL: Caves // comment\r\nEND:\r\n";

		IReadOnlyList<DarkTreeNode> nodes = provider.GetNodes(content, string.Empty);

		Assert.AreEqual(2, nodes.Count);
		Assert.AreEqual("Sections", nodes[0].Text);
		Assert.AreEqual(1, nodes[0].Nodes.Count);
		Assert.AreEqual("TITLE", nodes[0].Nodes[0].Text);
		Assert.AreEqual(ObjectType.Section, nodes[0].Nodes[0].Tag);

		Assert.AreEqual("Levels", nodes[1].Text);
		Assert.AreEqual(1, nodes[1].Nodes.Count);
		Assert.AreEqual("Caves", nodes[1].Nodes[0].Text);
		Assert.AreEqual(ObjectType.Level, nodes[1].Nodes[0].Tag);
	}

	[TestMethod]
	public void GetNodes_FiltersOutUnmatchedGroups()
	{
		var provider = new GameFlowNodesProvider();
		const string content = "TITLE:\r\nLEVEL: Caves\r\nLEVEL: Venice\r\n";

		IReadOnlyList<DarkTreeNode> nodes = provider.GetNodes(content, "ven");

		Assert.AreEqual(1, nodes.Count);
		Assert.AreEqual("Levels", nodes[0].Text);
		Assert.AreEqual(1, nodes[0].Nodes.Count);
		Assert.AreEqual("Venice", nodes[0].Nodes[0].Text);
	}
}