using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Tomb1Main.Objects;
using TombLib.Scripting.Tomb1Main.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.Tomb1Main
{
	public sealed class T1MEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Properties

		public bool AutoAddCommas { get; set; } = ConfigurationDefaults.AutoAddCommas;

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
					Path.Combine(DefaultPaths.T1MColorConfigsDirectory, value + ConfigurationDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public T1MEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

			AutoCloseParentheses = false;
		}

		#endregion Construction
	}
}
