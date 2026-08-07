using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.TRX
{
	public sealed class TRXEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Properties

		public bool AutoAddCommas { get; set; } = ConfigurationDefaults.AutoAddCommas;

		#endregion Properties

		#region Color scheme

		private string _selectedColorSchemeName = string.Empty;

		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath = GetExistingColorSchemeFilePath(value);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = ReadColorScheme(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme = new ColorScheme();

		private static ColorScheme ReadColorScheme(string schemeFilePath)
			=> Path.GetExtension(schemeFilePath).Equals(ScriptingDefaults.ColorSchemeFileExtension, StringComparison.OrdinalIgnoreCase)
				? JsonUtils.ReadJsonFile<ColorScheme>(schemeFilePath)
				: XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath);

		#endregion Color scheme

		#region Construction

		public TRXEditorConfiguration()
		{
			DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

			AutoCloseParentheses = false;

			SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
		}

		public static TRXEditorConfiguration LoadWithLegacyFallback()
		{
			var configuration = new TRXEditorConfiguration();
			string legacyPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.LegacyConfigurationFileName);

			if (File.Exists(configuration.DefaultPath))
				return configuration.Load<TRXEditorConfiguration>();

			if (File.Exists(legacyPath))
				return configuration.Load<TRXEditorConfiguration>(legacyPath);

			return configuration;
		}

		public static IReadOnlyList<string> GetAvailableColorSchemeFiles()
		{
			if (!Directory.Exists(DefaultPaths.TRXColorConfigsDirectory))
				return [];

			var filePathsByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (string filePath in Directory.GetFiles(DefaultPaths.TRXColorConfigsDirectory, "*" + ConfigurationDefaults.OldLegacyColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
				filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

			foreach (string filePath in Directory.GetFiles(DefaultPaths.TRXColorConfigsDirectory, "*" + ConfigurationDefaults.LegacyColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
				filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

			foreach (string filePath in Directory.GetFiles(DefaultPaths.TRXColorConfigsDirectory, "*" + ScriptingDefaults.ColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
				filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

			return [..
				filePathsByName
					.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
					.Select(entry => entry.Value)];
		}

		public static IReadOnlyList<string> GetColorSchemeFilePaths(string schemeName)
		{
			ArgumentNullException.ThrowIfNull(schemeName);

			string preferredPath = GetPreferredColorSchemeFilePath(schemeName);
			string legacyPath = Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ConfigurationDefaults.LegacyColorSchemeFileExtension);
			string oldLegacyPath = Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ConfigurationDefaults.OldLegacyColorSchemeFileExtension);

			return [preferredPath, legacyPath, oldLegacyPath];
		}

		public static string GetPreferredColorSchemeFilePath(string schemeName)
		{
			ArgumentNullException.ThrowIfNull(schemeName);

			return Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ScriptingDefaults.ColorSchemeFileExtension);
		}

		public static string GetExistingColorSchemeFilePath(string schemeName)
		{
			ArgumentNullException.ThrowIfNull(schemeName);

			string preferredPath = GetPreferredColorSchemeFilePath(schemeName);

			if (File.Exists(preferredPath))
				return preferredPath;

			string legacyPath = Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ConfigurationDefaults.LegacyColorSchemeFileExtension);

			if (File.Exists(legacyPath))
				return legacyPath;

			return Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ConfigurationDefaults.OldLegacyColorSchemeFileExtension);
		}

		#endregion Construction
	}
}
