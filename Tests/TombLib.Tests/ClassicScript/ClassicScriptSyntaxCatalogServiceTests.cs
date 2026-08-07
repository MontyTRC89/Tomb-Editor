using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptSyntaxCatalogServiceTests
{
	[TestMethod]
	public void GetCommandDefinition_UsesCaseInsensitiveLookupAndExposesParsedMetadata()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		ClassicScriptSyntaxDefinition customize = service.GetCommandDefinition("customize")
			?? throw new AssertFailedException("Customize syntax definition was not loaded.");
		ClassicScriptSyntaxDefinition legend = service.GetCommandDefinition("Legend")
			?? throw new AssertFailedException("Legend syntax definition was not loaded.");

		Assert.AreEqual("Level", customize.ApplicableSection);
		Assert.AreEqual(2, customize.ArgumentCount);
		Assert.IsTrue(customize.HasArrayArguments);
		StringAssert.Contains(customize.SyntaxText, "Customize=");

		Assert.AreEqual("Level", legend.ApplicableSection);
		Assert.AreEqual(1, legend.ArgumentCount);
		Assert.IsFalse(legend.HasArrayArguments);
		StringAssert.Contains(legend.SyntaxText, "Legend=");
	}

	[TestMethod]
	public void GetCustomizeAndParameterSyntax_ReturnExpectedEntries()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		string? customizeSyntax = service.GetCustomizeSyntax("cust_bar");
		string? parameterSyntax = service.GetParameterSyntax("param_rect");

		Assert.IsFalse(string.IsNullOrWhiteSpace(customizeSyntax));
		Assert.IsFalse(string.IsNullOrWhiteSpace(parameterSyntax));
		StringAssert.Contains(customizeSyntax, "CUST_BAR");
		StringAssert.Contains(parameterSyntax, "PARAM_RECT");
	}

	[TestMethod]
	public void GetCommandSyntax_ReturnsSyntaxForKnownCommand()
	{
		var service = new ClassicScriptSyntaxCatalogService();

		string? syntax = service.GetCommandSyntax("AddEffect");

		Assert.IsFalse(string.IsNullOrWhiteSpace(syntax));
		StringAssert.Contains(syntax, "AddEffect=");
		StringAssert.Contains(syntax, "(*Array*)");
	}
}