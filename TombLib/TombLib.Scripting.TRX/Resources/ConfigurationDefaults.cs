namespace TombLib.Scripting.TRX.Resources
{
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

		public const string LegacyColorSchemeFileExtension = ".trxsch";
		public const string OldLegacyColorSchemeFileExtension = ".t1msch";

		public const bool AutoAddCommas = true;
	}
}
