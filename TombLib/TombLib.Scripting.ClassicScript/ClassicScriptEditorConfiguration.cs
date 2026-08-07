using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.ClassicScript
{
	public sealed class ClassicScriptEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Properties

		public bool ShowSectionSeparators { get; set; } = ConfigurationDefaults.ShowSectionSeparators;

		[XmlElement("Tidy_PreEqualSpace")]
		public bool SpaceBeforeEquals { get; set; } = ConfigurationDefaults.SpaceBeforeEquals;

		[XmlElement("Tidy_PostEqualSpace")]
		public bool SpaceAfterEquals { get; set; } = ConfigurationDefaults.SpaceAfterEquals;

		[XmlElement("Tidy_PreCommaSpace")]
		public bool SpaceBeforeComma { get; set; } = ConfigurationDefaults.SpaceBeforeComma;

		[XmlElement("Tidy_PostCommaSpace")]
		public bool SpaceAfterComma { get; set; } = ConfigurationDefaults.SpaceAfterComma;

		[XmlElement("Tidy_ReduceSpaces")]
		public bool CollapseMultipleSpaces { get; set; } = ConfigurationDefaults.CollapseMultipleSpaces;

		#endregion Properties

		#region Color scheme

		private string _selectedColorSchemeName;
		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.ClassicScriptColorConfigsDirectory, value + ScriptingDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = JsonUtils.ReadJsonFile<ColorScheme>(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public ClassicScriptEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

			// These type of brackets aren't being used while writing in Classic Script, therefore auto closing should be disabled for them
			AutoCloseParentheses = false;
			AutoCloseBraces = false;

			SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
		}

		#endregion Construction
	}
}
