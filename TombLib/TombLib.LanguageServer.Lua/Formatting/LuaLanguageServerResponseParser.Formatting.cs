using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses document-formatting edits from a LuaLS formatting response.
	/// </summary>
	public static IReadOnlyList<LuaTextEdit> ParseDocumentFormattingEdits(IReadOnlyList<TextEditPayload>? response)
	{
		if (response is null)
			return [];

		var textEdits = new List<LuaTextEdit>();
		AppendTextEdits(response, textEdits);
		return textEdits;
	}
}
