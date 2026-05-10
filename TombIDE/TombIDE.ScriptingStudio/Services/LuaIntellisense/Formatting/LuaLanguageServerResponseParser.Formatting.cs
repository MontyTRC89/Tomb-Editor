#nullable enable

using System.Collections.Generic;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses document-formatting edits from a LuaLS formatting response.
	/// </summary>
	public static IReadOnlyList<LuaTextEdit> ParseDocumentFormattingEdits(JsonElement response)
	{
		if (response.ValueKind != JsonValueKind.Array)
			return [];

		var textEdits = new List<LuaTextEdit>();
		AppendTextEdits(response, textEdits);
		return textEdits;
	}
}