using TombLib.Scripting.Navigation;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses reference locations from a LuaLS references response.
	/// </summary>
	internal static IReadOnlyList<TextReferenceLocation> ParseReferenceLocations(IReadOnlyList<ReferenceResponse>? response)
	{
		if (response is not { Count: > 0 })
			return [];

		var locations = new List<TextReferenceLocation>();

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

			locations.Add(new TextReferenceLocation(
				LanguageServerPathHelper.NormalizeLocalPath(parsedUri),
				range.Value.StartLineNumber,
				range.Value.StartColumnNumber,
				range.Value.EndLineNumber,
				range.Value.EndColumnNumber));
		}

		return locations;
	}
}
