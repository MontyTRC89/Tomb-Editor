using System.IO;
using TombLib.Scripting.GameFlowScript.Highlighting;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Utils;

namespace TombLib.Scripting.GameFlowScript
{
	public sealed class GameFlowEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Color scheme

		private string _selectedColorSchemeName;
		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.GameFlowColorConfigsDirectory, value + ConfigurationDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public GameFlowEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

			AutoCloseParentheses = false;
			AutoCloseBraces = false;
			AutoCloseBrackets = false;
			AutoCloseDoubleQuotes = false;
			AutoCloseSingleQuotes = false;

			SelectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;
		}

		#endregion Construction
	}
}
