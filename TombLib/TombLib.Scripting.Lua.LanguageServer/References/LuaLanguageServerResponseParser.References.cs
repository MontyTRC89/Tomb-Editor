using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses reference locations from a LuaLS references response.
	/// </summary>
	public static IReadOnlyList<LuaReferenceLocation> ParseReferenceLocations(IReadOnlyList<LuaReferenceResponse>? response)
	{
		if (response is not { Count: > 0 })
			return [];

		var locations = new List<LuaReferenceLocation>();

		for (int i = 0; i < response.Count; i++)
		{
			LuaReferenceResponse referenceElement = response[i];

			if (string.IsNullOrWhiteSpace(referenceElement.Uri))
				continue;

			if (!Uri.TryCreate(referenceElement.Uri, UriKind.Absolute, out Uri? parsedUri)
				|| parsedUri?.IsFile != true
				|| !TryParseDocumentRange(referenceElement.Range, out LuaDocumentRange? range))
			{
				continue;
			}

			locations.Add(new LuaReferenceLocation(LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri), range));
		}

		return locations;
	}
}
