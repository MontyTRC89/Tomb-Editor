using TombLib.Scripting.Editing;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses document-formatting edits from a LuaLS formatting response.
	/// </summary>
	/// <param name="response">The text edit payloads from the formatting response, or <see langword="null"/> when unavailable.</param>
	/// <returns>The formatted text edits, or an empty list when no edits are present.</returns>
	internal static IReadOnlyList<TextEdit> ParseDocumentFormattingEdits(IReadOnlyList<TextEditPayload>? response)
	{
		if (response is null)
			return [];

		var textEdits = new List<TextEdit>();
		AppendTextEdits(response, textEdits);
		return textEdits;
	}
}
