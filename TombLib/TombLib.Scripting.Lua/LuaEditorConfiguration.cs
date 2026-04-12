using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		private string _selectedThemeName = ConfigurationDefaults.SelectedThemeName;

		public string SelectedThemeName
		{
			get => _selectedThemeName;
			set
			{
				_selectedThemeName = LuaThemeRepository.ResolveThemeName(value);

				Theme = LuaThemeRepository.GetTheme(_selectedThemeName);
			}
		}

		[XmlIgnore]
		public Objects.LuaTheme Theme { get; private set; } = LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName);

		public string SelectedColorSchemeName
		{
			get => SelectedThemeName;
			set => SelectedThemeName = value;
		}

		public LuaEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);
			SelectedThemeName = ConfigurationDefaults.SelectedThemeName;
		}

		public bool ShouldSerializeSelectedColorSchemeName()
			=> false;
	}
}
