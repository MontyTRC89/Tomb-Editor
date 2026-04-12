using System.Linq;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Test;

[TestClass]
public class LuaThemeConfigurationTests
{
	[TestMethod]
	public void DefaultConfiguration_UsesBundledVsCodeDarkTheme()
	{
		var config = new LuaEditorConfiguration();

		Assert.AreEqual("VSCode Dark+", config.SelectedThemeName);
		Assert.AreEqual("VSCode Dark+", config.Theme.Name);
		Assert.AreEqual("#202020", config.Theme.EditorBackground);
		Assert.AreEqual("#DCDCDC", config.Theme.EditorForeground);
		Assert.IsTrue(config.Theme.TextMateTheme.Rules.Count > 0);
	}

	[TestMethod]
	public void LegacyVs15Alias_MapsToVsCodeDarkTheme()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "VS15"
		};

		Assert.AreEqual("VSCode Dark+", config.SelectedThemeName);
		Assert.AreEqual("VSCode Dark+", config.Theme.Name);
	}

	[TestMethod]
	public void SwitchingTheme_LoadsBundledPreset()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "Visual Studio 15"
		};

		Assert.AreEqual("Visual Studio 2015", config.SelectedThemeName);
		Assert.AreEqual("Visual Studio 2015", config.Theme.Name);
		Assert.AreEqual("#1E1E1E", config.Theme.EditorBackground);
		Assert.AreEqual("#DCDCDC", config.Theme.EditorForeground);
	}

	[TestMethod]
	public void Repository_ExposesBundledThemes()
	{
		string[] themeNames = LuaThemeRepository.GetAvailableThemes().Select(theme => theme.Name).ToArray();

		CollectionAssert.Contains(themeNames, "VSCode Light+");
		CollectionAssert.Contains(themeNames, "NG_CENTER");
		CollectionAssert.Contains(themeNames, "Tomorrow Night");
	}
}