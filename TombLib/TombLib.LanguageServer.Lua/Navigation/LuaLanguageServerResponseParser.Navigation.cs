using TombLib.Scripting.Navigation;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a definition location from a LuaLS definition response.
	/// </summary>
	/// <param name="response">The definition response payload.</param>
	/// <returns>The resolved definition location, or <see langword="null"/> when the response does not contain a valid file URI.</returns>
	internal static TextDefinitionLocation? ParseDefinitionLocation(DefinitionResponse response)
	{
		if (string.IsNullOrWhiteSpace(response.Uri)
			|| !Uri.TryCreate(response.Uri, UriKind.Absolute, out Uri? parsedUri)
			|| parsedUri?.IsFile != true)
		{
			return null;
		}

		return new TextDefinitionLocation(
			response.LineNumber,
			response.ColumnNumber,
			LanguageServerPathHelper.NormalizeLocalPath(parsedUri));
	}
}
