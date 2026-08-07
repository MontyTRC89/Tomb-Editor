using System.Linq;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Resources;

namespace TombLib.Tests;

// Provider layer tests: prove every scripting language exposes its color schemes or
// themes through the same ITextEditorColorProvider contract used by the settings UI.
[TestClass]
public class TextEditorColorProviderTests
{
	private static readonly string[] BundledSchemeNames = ["VS15", "Obsidian", "NG_Center", "Monokai"];

	[TestMethod]
	public void ClassicScriptProvider_ListsBundledSchemes()
	{
		var provider = new ClassicScriptColorSchemeProvider();

		CollectionAssert.AreEquivalent(BundledSchemeNames, provider.GetAvailableNames().ToArray());
	}

	[TestMethod]
	public void GameFlowProvider_ListsBundledSchemes()
	{
		var provider = new GameFlowColorSchemeProvider();

		CollectionAssert.AreEquivalent(BundledSchemeNames, provider.GetAvailableNames().ToArray());
	}

	[TestMethod]
	public void TrxProvider_ListsBundledSchemes()
	{
		var provider = new TRXColorSchemeProvider();

		CollectionAssert.AreEquivalent(BundledSchemeNames, provider.GetAvailableNames().ToArray());
	}

	[TestMethod]
	public void LuaProvider_ListsBundledThemes()
	{
		var provider = new LuaThemeProvider();

		CollectionAssert.Contains(provider.GetAvailableNames().ToArray(), "SharpLua Classic");
		CollectionAssert.Contains(provider.GetAvailableNames().ToArray(), "Tomorrow Night");
	}

	[TestMethod]
	public void Providers_ReadAndWriteSelectedNameThroughConfig()
	{
		var classicConfig = new ClassicScriptEditorConfiguration();
		var classicProvider = new ClassicScriptColorSchemeProvider();
		classicProvider.SetSelectedName(classicConfig, "Monokai");

		Assert.AreEqual("Monokai", classicProvider.GetSelectedName(classicConfig));
		Assert.AreEqual("#2C2C2A", classicConfig.ColorScheme.Background);

		var luaConfig = new LuaEditorConfiguration();
		var luaProvider = new LuaThemeProvider();
		luaProvider.SetSelectedName(luaConfig, "VS15");

		Assert.AreEqual("VSCode Dark+", luaProvider.GetSelectedName(luaConfig));
	}
}
