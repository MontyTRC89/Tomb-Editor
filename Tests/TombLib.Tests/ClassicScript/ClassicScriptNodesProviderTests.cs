using DarkUI.Controls;
using TombLib.Scripting.ClassicScript.ContentNodes;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptNodesProviderTests
{
	private static ClassicScriptNodesProvider CreateProvider()
		=> new(new ClassicScriptLineService());

	[TestMethod]
	public void GetNodes_ReturnsExpectedGroupsForMatchingContent()
	{
		var provider = CreateProvider();
		const string content = "[Options]\r\nName = Caves ; comment\r\n#include \"strings.txt\"\r\n#define SECRET_FLAG ENABLED\r\n[Level]\r\n";

		IReadOnlyList<DarkTreeNode> nodes = provider.GetNodes(content, string.Empty);

		Assert.AreEqual(4, nodes.Count);
		Assert.AreEqual("Sections", nodes[0].Text);
		Assert.AreEqual("[Options]", nodes[0].Nodes[0].Text);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Section), nodes[0].Nodes[0].Tag);

		Assert.AreEqual("Levels", nodes[1].Text);
		Assert.AreEqual("Caves", nodes[1].Nodes[0].Text);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Level), nodes[1].Nodes[0].Tag);

		Assert.AreEqual("Includes", nodes[2].Text);
		Assert.AreEqual("strings.txt", nodes[2].Nodes[0].Text);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Include), nodes[2].Nodes[0].Tag);

		Assert.AreEqual("Defines", nodes[3].Text);
		Assert.AreEqual("SECRET_FLAG", nodes[3].Nodes[0].Text);
		Assert.AreEqual(new ClassicScriptObjectDiscriminator(ObjectType.Define), nodes[3].Nodes[0].Tag);
	}

	[TestMethod]
	public void GetNodes_FiltersOutUnmatchedGroups()
	{
		var provider = CreateProvider();
		const string content = "[Options]\r\nName = Caves\r\n#include \"scripts.dat\"\r\n#define SECRET_FLAG ENABLED\r\n";

		IReadOnlyList<DarkTreeNode> nodes = provider.GetNodes(content, "script");

		Assert.AreEqual(1, nodes.Count);
		Assert.AreEqual("Includes", nodes[0].Text);
		Assert.AreEqual(1, nodes[0].Nodes.Count);
		Assert.AreEqual("scripts.dat", nodes[0].Nodes[0].Text);
	}
}