namespace TombLib.LanguageServer.Lua;

public static class LuaLanguageServerInitializationOptionsFactory
{
	/// <summary>
	/// Builds LuaLS-specific initialization options.
	/// </summary>
	public static object Create()
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
