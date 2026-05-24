using TombLib.Scripting.Editing;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses document-formatting edits from a LuaLS formatting response.
	/// </summary>
	internal static IReadOnlyList<TextEdit> ParseDocumentFormattingEdits(IReadOnlyList<TextEditPayload>? response)
	{
		if (response is null)
			return [];

		var textEdits = new List<TextEdit>();
		AppendTextEdits(response, textEdits);
		return textEdits;
	}
}
