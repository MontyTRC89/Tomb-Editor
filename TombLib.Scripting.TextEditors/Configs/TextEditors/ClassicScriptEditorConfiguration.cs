using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.TextEditors.ColorSchemes;

namespace TombLib.Scripting.TextEditors.Configs
{
	public sealed class ClassicScriptEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		public bool ShowSectionSeparators { get; set; } = true;

		public bool Tidy_PreEqualSpace { get; set; } = false;
		public bool Tidy_PostEqualSpace { get; set; } = true;
		public bool Tidy_PreCommaSpace { get; set; } = false;
		public bool Tidy_PostCommaSpace { get; set; } = true;
		public bool Tidy_ReduceSpaces { get; set; } = true;

		private string _selectedColorSchemeName = "VS15";

		public string SelectedColorSchemeName
		{
			get { return _selectedColorSchemeName; }
			set
			{
				_selectedColorSchemeName = value;

				string colorSchemeFilePath = Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), value + ".cssch");

				if (!File.Exists(colorSchemeFilePath))
					ColorScheme = new ClassicScriptColorScheme();
				else
					ColorScheme = XmlHandling.ReadXmlFile<ClassicScriptColorScheme>(colorSchemeFilePath);
			}
		}

		[XmlIgnore]
		public ClassicScriptColorScheme ColorScheme { get; private set; }

		public ClassicScriptEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.GetTextEditorConfigsPath(), "ClassicScriptConfiguration.xml");

			// These type of brackets aren't being used while writing in Classic Script, therefore auto closing should be disabled for them
			AutoCloseParentheses = false;
			AutoCloseBraces = false;
		}
	}
}
