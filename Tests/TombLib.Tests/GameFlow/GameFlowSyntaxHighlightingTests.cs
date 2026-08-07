using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.GameFlowScript.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class GameFlowSyntaxHighlightingTests
{
	[TestMethod]
	public void HighlightingContract_AllIHighlightingDefinitionMembersAreUsable()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());

		Assert.AreEqual("GameFlowScript Rules", highlighting.Name);
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsFalse(highlighting.NamedHighlightingColors.Any());
		Assert.IsNotNull(highlighting.Properties);
		Assert.AreEqual(0, highlighting.Properties.Count);
		Assert.IsNull(highlighting.GetNamedColor("anything"));
		Assert.IsNull(highlighting.GetNamedRuleSet("DoesNotExist"));
		Assert.IsNotNull(highlighting.GetNamedRuleSet(highlighting.Name));
	}
}
