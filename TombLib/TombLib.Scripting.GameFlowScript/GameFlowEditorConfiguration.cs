﻿using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.GameFlowScript.Objects;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.GameFlowScript
{
	public sealed class GameFlowEditorConfiguration : TextEditorConfigBase
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
			AutoCloseQuotes = false;
		}

		#endregion Construction
	}
}
