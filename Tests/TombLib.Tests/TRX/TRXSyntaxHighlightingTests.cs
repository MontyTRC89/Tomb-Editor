using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests;

[TestClass]
public class TRXSyntaxHighlightingTests
{
	[TestMethod]
	public void HighlightingContract_AllIHighlightingDefinitionMembersAreUsable()
	{
		var schemaService = new GameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());
		var highlighting = new SyntaxHighlighting(new ColorScheme(), schemaService);

		Assert.AreEqual("TRX Rules", highlighting.Name);
		Assert.IsNotNull(highlighting.MainRuleSet);
		Assert.IsFalse(highlighting.NamedHighlightingColors.Any());
		Assert.IsNotNull(highlighting.Properties);
		Assert.AreEqual(0, highlighting.Properties.Count);
		Assert.IsNull(highlighting.GetNamedColor("anything"));
		Assert.IsNull(highlighting.GetNamedRuleSet("DoesNotExist"));
		Assert.IsNotNull(highlighting.GetNamedRuleSet(highlighting.Name));
	}
}
