namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Defines default values used by TRX editor configuration objects.
/// </summary>
public static class ConfigurationDefaults
{
	/// <summary>
	/// Gets the default file name used to persist TRX editor configuration.
	/// </summary>
	public const string ConfigurationFileName = "TRXConfiguration.xml";

	/// <summary>
	/// Gets the legacy file name used to persist TRX editor configuration.
	/// </summary>
	public const string LegacyConfigurationFileName = "Tomb1MainConfiguration.xml";

	/// <summary>
	/// Gets the legacy color scheme file extension used by TRX.
	/// </summary>
	public const string LegacyColorSchemeFileExtension = ".trxsch";

	/// <summary>
	/// Gets the oldest legacy color scheme file extension used by TRX.
	/// </summary>
	public const string OldLegacyColorSchemeFileExtension = ".t1msch";

	/// <summary>
	/// Gets the default value for automatically adding commas after closing braces and brackets.
	/// </summary>
	public const bool AutoAddCommas = true;
}
