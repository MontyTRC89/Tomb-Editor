using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a definition location from a LuaLS definition response.
	/// </summary>
	public static LuaDefinitionLocation? ParseDefinitionLocation(DefinitionResponse response)
	{
		if (string.IsNullOrWhiteSpace(response.Uri)
			|| !Uri.TryCreate(response.Uri, UriKind.Absolute, out Uri? parsedUri)
			|| parsedUri?.IsFile != true)
		{
			return null;
		}

		return new LuaDefinitionLocation(
			LanguageServerPathHelper.NormalizeLocalPath(parsedUri),
			response.LineNumber,
			response.ColumnNumber);
	}
}
