using TombLib.Scripting.ClassicScript.Cleaning;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptDocumentFormatterTests
{
	[TestMethod]
	public void FormatDocument_UsesConfiguredSpacingRulesAndTrimsTrailingWhitespace()
	{
		var formatter = new ClassicScriptDocumentFormatter
		{
			PreEqualSpace = true,
			PostEqualSpace = true,
			PreCommaSpace = false,
			PostCommaSpace = true,
			ReduceSpaces = false
		};

		string formatted = formatter.FormatDocument("Customize= CUST_BAR,foo   \r\nLegend =1\t");

		Assert.AreEqual("Customize = CUST_BAR, foo" + Environment.NewLine + "Legend = 1", formatted);
	}

	[TestMethod]
	public void FormatDocument_TrimOnlyOnlyRemovesTrailingWhitespace()
	{
		var formatter = new ClassicScriptDocumentFormatter
		{
			PreEqualSpace = false,
			PostEqualSpace = false,
			PreCommaSpace = false,
			PostCommaSpace = false,
			ReduceSpaces = false
		};

		string formatted = formatter.FormatDocument("Legend = 1   \r\nCustomize = CUST_BAR, foo\t", trimOnly: true);

		Assert.AreEqual("Legend = 1" + Environment.NewLine + "Customize = CUST_BAR, foo", formatted);
	}

	[TestMethod]
	public void FormatCompilerOutput_RemovesSpacesBeforeEqualsWithoutChangingSpacesAfterEquals()
	{
		string formatted = ClassicScriptDocumentFormatter.FormatCompilerOutput("Legend =1   \r\nCustomize =  CUST_BAR\t");

		Assert.AreEqual("Legend=1" + Environment.NewLine + "Customize=  CUST_BAR", formatted);
	}
}