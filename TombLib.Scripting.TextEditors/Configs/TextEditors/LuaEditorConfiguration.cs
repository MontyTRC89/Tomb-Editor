using System.IO;
using TombLib.Scripting.TextEditors.ColorSchemes;
using TombLib.Scripting.TextEditors.Configs.Bases;
using TombLib.Scripting.TextEditors.Configs.Defaults;

namespace TombLib.Scripting.TextEditors.Configs
{
	public sealed class LuaEditorConfiguration : TextEditorConfigurationBase
	{
		public override string DefaultPath { get; }

		#region Color scheme

		private string _selectedColorSchemeName = LuaEditorDefaults.SelectedColorSchemeName;

		public string SelectedColorSchemeName
		{
			get { return _selectedColorSchemeName; }
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), value + LuaEditorDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new LuaColorScheme();
				else
					ColorScheme = XmlHandling.ReadXmlFile<LuaColorScheme>(schemeFilePath);
			}
		}

		public LuaColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public LuaEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.GetTextEditorConfigsPath(), LuaEditorDefaults.ConfigurationFileName);
		}

		#endregion Construction
	}
}
