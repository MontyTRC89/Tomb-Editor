namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Builds the initialization-options payload sent to the bundled Lua language server.
/// </summary>
internal static class LuaLanguageServerInitializationOptionsFactory
{
	/// <summary>
	/// Builds LuaLS-specific initialization options.
	/// </summary>
	/// <returns>An anonymous initialization-options object serialized into the initialize request.</returns>
	internal static object Create()
	{
		return new
		{
			changeConfiguration = true,
			viewDocument = true,
			trustByClient = false,
			useSemanticByRange = false
		};
	}
}
