using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Stores user-configurable settings for the Lua editor.
/// </summary>
public sealed class LuaEditorConfiguration : TextEditorConfigBase
{
	/// <summary>
	/// Gets the default file path used to persist this configuration.
	/// </summary>
	public override string DefaultPath { get; }

	private string _selectedThemeName = ConfigurationDefaults.SelectedThemeName;

	/// <summary>
	/// Gets or sets the selected Lua theme name.
	/// </summary>
	public string SelectedThemeName
	{
		get => _selectedThemeName;
		set
		{
			_selectedThemeName = LuaThemeRepository.ResolveThemeName(value);

			Theme = LuaThemeRepository.GetTheme(_selectedThemeName);
		}
	}

	/// <summary>
	/// Gets the resolved theme object for the current selection.
	/// </summary>
	[XmlIgnore]
	public LuaTheme Theme { get; private set; } = LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName);

	/// <summary>
	/// Gets or sets the serialized name of the selected color scheme. This maps to
	/// <see cref="SelectedThemeName"/> so existing serialized configuration data round-trips.
	/// </summary>
	public string SelectedColorSchemeName
	{
		get => SelectedThemeName;
		set => SelectedThemeName = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaEditorConfiguration"/> class.
	/// </summary>
	public LuaEditorConfiguration()
	{
		DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);
		SelectedThemeName = ConfigurationDefaults.SelectedThemeName;
	}
}
