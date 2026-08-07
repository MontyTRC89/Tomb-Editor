using ICSharpCode.AvalonEdit.Highlighting;
using TombLib.Scripting.ClassicScript.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptSyntaxHighlightingTests
{
	[TestMethod]
	public void MainRuleSet_CommandAndSectionRegexesComeFromCatalog()
	{
		var highlighting = new SyntaxHighlighting(new ColorScheme());
		HighlightingRuleSet ruleSet = highlighting.MainRuleSet;

		HighlightingRule sectionsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("[Level]"));
		HighlightingRule oldCommandsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("Legend=") && !rule.Regex.IsMatch("[Level]"));
		HighlightingRule newCommandsRule = ruleSet.Rules.First(rule => rule.Regex.IsMatch("FMV=") && !rule.Regex.IsMatch("Legend="));

		Assert.IsTrue(sectionsRule.Regex.IsMatch("[Options]"));
		Assert.IsTrue(sectionsRule.Regex.IsMatch("[Title]"));

		Assert.IsTrue(oldCommandsRule.Regex.IsMatch("AnimatingMIP="));
		Assert.IsTrue(oldCommandsRule.Regex.IsMatch("Cut=")); // Legacy array-only name is kept.

		Assert.IsTrue(newCommandsRule.Regex.IsMatch("AddEffect="));
		Assert.IsTrue(newCommandsRule.Regex.IsMatch("FMV=")); // One of the four entries absent from the legacy array.

		Assert.IsFalse(newCommandsRule.Regex.IsMatch("#DEFINE=")); // Directives are not command alternatives.
	}
}
