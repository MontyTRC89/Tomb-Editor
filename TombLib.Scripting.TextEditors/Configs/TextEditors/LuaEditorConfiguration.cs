using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.TextEditors.ColorSchemes;

namespace TombLib.Scripting.TextEditors.Configs
{
	public sealed class LuaEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		private string _selectedColorSchemeName = "VS15";

		public string SelectedColorSchemeName
		{
			get { return _selectedColorSchemeName; }
			set
			{
				_selectedColorSchemeName = value;

				string colorSchemeFilePath = Path.Combine(DefaultPaths.GetLuaColorConfigsPath(), value + ".luasch");
				ColorScheme = XmlHandling.ReadXmlFile<LuaColorScheme>(colorSchemeFilePath);
			}
		}

		[XmlIgnore]
		public LuaColorScheme ColorScheme { get; private set; }

		public LuaEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.GetTextEditorConfigsPath(), "LuaConfiguration.xml");
		}
	}
}
