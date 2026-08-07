using System;
using System.IO;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Configuration for the TRX editor, including its color scheme.
/// </summary>
public sealed class TRXEditorConfiguration : ColorSchemeConfigBase<ColorScheme>
{
	/// <summary>
	/// Gets the default path used to persist this configuration.
	/// </summary>
	public override string DefaultPath { get; }

	// Properties

	/// <summary>
	/// Gets or sets whether commas are added automatically after closing braces and brackets.
	/// </summary>
	public bool AutoAddCommas { get; set; } = ConfigurationDefaults.AutoAddCommas;

	// Color scheme

	/// <inheritdoc />
	protected override string GetSchemeFilePath(string schemeName)
		=> GetExistingColorSchemeFilePath(schemeName);

	/// <inheritdoc />
	protected override ColorScheme ReadSchemeFile(string schemeFilePath)
		=> Path.GetExtension(schemeFilePath).Equals(ScriptingDefaults.ColorSchemeFileExtension, StringComparison.OrdinalIgnoreCase)
			? JsonUtils.ReadJsonFile<ColorScheme>(schemeFilePath)
			: XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath);

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXEditorConfiguration"/> class.
	/// </summary>
	public TRXEditorConfiguration()
	{
		DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		AutoCloseParentheses = false;

		SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
	}

	/// <summary>
	/// Loads the configuration from the current or legacy path, falling back to defaults.
	/// </summary>
	/// <returns>The loaded configuration.</returns>
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

	/// <summary>
	/// Gets the preferred (current) color scheme file path for the given scheme name.
	/// </summary>
	/// <param name="schemeName">The name of the color scheme.</param>
	/// <returns>The preferred color scheme file path.</returns>
	public static string GetPreferredColorSchemeFilePath(string schemeName)
	{
		ArgumentNullException.ThrowIfNull(schemeName);

		return Path.Combine(DefaultPaths.TRXColorConfigsDirectory, schemeName + ScriptingDefaults.ColorSchemeFileExtension);
	}

	/// <summary>
	/// Gets an existing color scheme file path for the given scheme name, preferring newer formats.
	/// </summary>
	/// <param name="schemeName">The name of the color scheme.</param>
	/// <returns>The first existing color scheme file path, falling back to the oldest legacy path.</returns>
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

}
