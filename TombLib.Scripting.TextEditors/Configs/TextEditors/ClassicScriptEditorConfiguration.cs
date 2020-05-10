using System.IO;
using TombLib.Scripting.TextEditors.ColorSchemes;
using TombLib.Scripting.TextEditors.Configs.Bases;
using TombLib.Scripting.TextEditors.Configs.Defaults;

namespace TombLib.Scripting.TextEditors.Configs
{
	public sealed class ClassicScriptEditorConfiguration : TextEditorConfigurationBase
	{
		public override string DefaultPath { get; }

		#region Properties

		public bool ShowSectionSeparators { get; set; } = ClassicScriptEditorDefaults.ShowSectionSeparators;

		public bool Tidy_PreEqualSpace { get; set; } = ClassicScriptEditorDefaults.Tidy_PreEqualSpace;
		public bool Tidy_PostEqualSpace { get; set; } = ClassicScriptEditorDefaults.Tidy_PostEqualSpace;

		public bool Tidy_PreCommaSpace { get; set; } = ClassicScriptEditorDefaults.Tidy_PreCommaSpace;
		public bool Tidy_PostCommaSpace { get; set; } = ClassicScriptEditorDefaults.Tidy_PostCommaSpace;

		public bool Tidy_ReduceSpaces { get; set; } = ClassicScriptEditorDefaults.Tidy_ReduceSpaces;

		#endregion Properties

		#region Color scheme

		private string _selectedColorSchemeName = ClassicScriptEditorDefaults.SelectedColorSchemeName;

		public string SelectedColorSchemeName
		{
			get { return _selectedColorSchemeName; }
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.GetClassicScriptColorConfigsPath(), value + ClassicScriptEditorDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ClassicScriptColorScheme();
				else
					ColorScheme = XmlHandling.ReadXmlFile<ClassicScriptColorScheme>(schemeFilePath);
			}
		}

		public ClassicScriptColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public ClassicScriptEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.GetTextEditorConfigsPath(), ClassicScriptEditorDefaults.ConfigurationFileName);

			// These type of brackets aren't being used while writing in Classic Script, therefore auto closing should be disabled for them
			AutoCloseParentheses = false;
			AutoCloseBraces = false;
		}

		#endregion Construction

		#region Override methods

		public override void ResetToDefaultSettings()
		{
			ShowSectionSeparators = ClassicScriptEditorDefaults.ShowSectionSeparators;

			Tidy_PreEqualSpace = ClassicScriptEditorDefaults.Tidy_PreEqualSpace;
			Tidy_PostEqualSpace = ClassicScriptEditorDefaults.Tidy_PostEqualSpace;

			Tidy_PreCommaSpace = ClassicScriptEditorDefaults.Tidy_PreCommaSpace;
			Tidy_PostCommaSpace = ClassicScriptEditorDefaults.Tidy_PostCommaSpace;

			Tidy_ReduceSpaces = ClassicScriptEditorDefaults.Tidy_ReduceSpaces;

			base.ResetToDefaultSettings();
		}

		#endregion Override methods
	}
}
