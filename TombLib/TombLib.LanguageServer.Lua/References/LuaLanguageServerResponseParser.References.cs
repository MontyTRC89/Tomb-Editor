using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses reference locations from a LuaLS references response.
	/// </summary>
	internal static IReadOnlyList<LuaReferenceLocation> ParseReferenceLocations(IReadOnlyList<ReferenceResponse>? response)
	{
		if (response is not { Count: > 0 })
			return [];

		var locations = new List<LuaReferenceLocation>();

		for (int i = 0; i < response.Count; i++)
		{
			ReferenceResponse referenceElement = response[i];

			if (string.IsNullOrWhiteSpace(referenceElement.Uri))
				continue;

			if (!Uri.TryCreate(referenceElement.Uri, UriKind.Absolute, out Uri? parsedUri)
				|| parsedUri?.IsFile != true
				|| !ProtocolRangeHelper.TryGetOneBasedRange(referenceElement.Range, out OneBasedDocumentRange? range))
			{
				continue;
			}

			locations.Add(new LuaReferenceLocation(
				LanguageServerPathHelper.NormalizeLocalPath(parsedUri),
				new LuaDocumentRange(range.Value.StartLineNumber, range.Value.StartColumnNumber, range.Value.EndLineNumber, range.Value.EndColumnNumber)));
		}

		return locations;
	}
}
