using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }
		public ColorScheme ColorScheme = new ColorScheme();

		private string _selectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;

		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = string.IsNullOrWhiteSpace(value)
					? ConfigurationDefaults.SelectedColorSchemeName
					: value;

				ColorScheme = LoadColorScheme(_selectedColorSchemeName);
			}
		}

		public LuaEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);
			SelectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;
		}

		private static ColorScheme LoadColorScheme(string schemeName)
		{
			string schemeFilePath = Path.Combine(
				DefaultPaths.LuaColorConfigsDirectory,
				schemeName + ConfigurationDefaults.ColorSchemeFileExtension);

			return File.Exists(schemeFilePath)
				? XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath)
				: new ColorScheme();
		}
	}
}
