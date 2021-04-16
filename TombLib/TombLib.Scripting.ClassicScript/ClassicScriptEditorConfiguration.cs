using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombLib.Scripting.ClassicScript
{
	public sealed class ClassicScriptEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Properties

		public bool ShowSectionSeparators { get; set; } = ConfigurationDefaults.ShowSectionSeparators;

		public bool Tidy_PreEqualSpace { get; set; } = ConfigurationDefaults.Tidy_PreEqualSpace;
		public bool Tidy_PostEqualSpace { get; set; } = ConfigurationDefaults.Tidy_PostEqualSpace;

		public bool Tidy_PreCommaSpace { get; set; } = ConfigurationDefaults.Tidy_PreCommaSpace;
		public bool Tidy_PostCommaSpace { get; set; } = ConfigurationDefaults.Tidy_PostCommaSpace;

		public bool Tidy_ReduceSpaces { get; set; } = ConfigurationDefaults.Tidy_ReduceSpaces;

		#endregion Properties

		#region Color scheme

		private string _selectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;
		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.ClassicScriptColorConfigsDirectory, value + ConfigurationDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = XmlHandling.ReadXmlFile<ColorScheme>(schemeFilePath);
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
		}

		#endregion Construction

		#region Override methods

		public override void ResetToDefaultSettings()
		{
			ShowSectionSeparators = ConfigurationDefaults.ShowSectionSeparators;

			Tidy_PreEqualSpace = ConfigurationDefaults.Tidy_PreEqualSpace;
			Tidy_PostEqualSpace = ConfigurationDefaults.Tidy_PostEqualSpace;

			Tidy_PreCommaSpace = ConfigurationDefaults.Tidy_PreCommaSpace;
			Tidy_PostCommaSpace = ConfigurationDefaults.Tidy_PostCommaSpace;

			Tidy_ReduceSpaces = ConfigurationDefaults.Tidy_ReduceSpaces;

			base.ResetToDefaultSettings();
		}

		#endregion Override methods
	}
}
