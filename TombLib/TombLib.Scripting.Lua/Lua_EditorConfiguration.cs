using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua
{
	public sealed class Lua_EditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Color scheme

		private string _selectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;
		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.LuaColorConfigsDirectory, value + ConfigurationDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = XmlHandling.ReadXmlFile<ColorScheme>(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public Lua_EditorConfiguration()
			=> DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		#endregion Construction
	}
}
