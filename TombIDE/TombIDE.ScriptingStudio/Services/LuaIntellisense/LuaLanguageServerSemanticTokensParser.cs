#nullable enable

using System.Collections.Generic;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static class LuaLanguageServerSemanticTokensParser
{
	/// <summary>
	/// Decodes the LuaLS semantic token payload for a synchronized document snapshot.
	/// </summary>
	public static IReadOnlyList<LuaSemanticToken> Parse(JsonElement response, LuaDocumentSnapshot? document,
		IReadOnlyList<string>? tokenTypes, IReadOnlyList<string>? tokenModifiers)
	{
		if (document is null || tokenTypes is null || tokenTypes.Count == 0)
			return [];

		LuaSemanticTokensDeltaResponse parsedResponse = LuaLanguageServerSemanticTokensDeltaParser.Parse(response);

		if (parsedResponse.Data is not { } data)
			return [];

		return LuaLanguageServerSemanticTokensDecoder.Decode(data, document, tokenTypes, tokenModifiers);
	}
}
