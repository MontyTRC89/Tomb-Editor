#nullable enable

using System;
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

		if (!response.TryGetProperty("data", out JsonElement dataElement) || dataElement.ValueKind != JsonValueKind.Array)
			return [];

		string[] lines = NormalizeLineEndings(document.Content).Split('\n');
		var semanticTokens = new List<LuaSemanticToken>();
		var rawData = new List<int>();

		foreach (JsonElement item in dataElement.EnumerateArray())
		{
			if (item.TryGetInt32(out int value))
				rawData.Add(value);
		}

		int line = 0;
		int character = 0;

		for (int i = 0; i + 4 < rawData.Count; i += 5)
		{
			int deltaLine = rawData[i];
			int deltaCharacter = rawData[i + 1];
			int length = rawData[i + 2];
			int tokenTypeIndex = rawData[i + 3];
			int modifierMask = rawData[i + 4];

			line += deltaLine;

			if (deltaLine == 0)
				character += deltaCharacter;
			else
				character = deltaCharacter;

			if (line < 0 || line >= lines.Length || tokenTypeIndex < 0 || tokenTypeIndex >= tokenTypes.Count)
				continue;

			int lineLength = lines[line].Length;
			int safeCharacter = Math.Max(0, Math.Min(character, lineLength));
			int safeLength = Math.Max(0, Math.Min(length, lineLength - safeCharacter));

			if (safeLength == 0)
				continue;

			semanticTokens.Add(new LuaSemanticToken(
				line,
				safeCharacter,
				safeLength,
				tokenTypes[tokenTypeIndex],
				DecodeModifiers(modifierMask, tokenModifiers)));
		}

		return semanticTokens;
	}

	private static IReadOnlyList<string> DecodeModifiers(int modifierMask, IReadOnlyList<string>? tokenModifiers)
	{
		if (modifierMask == 0 || tokenModifiers is null || tokenModifiers.Count == 0)
			return [];

		var modifiers = new List<string>();

		for (int bitIndex = 0; bitIndex < tokenModifiers.Count; bitIndex++)
		{
			if ((modifierMask & (1 << bitIndex)) != 0)
				modifiers.Add(tokenModifiers[bitIndex]);
		}

		return modifiers;
	}

	private static string NormalizeLineEndings(string? content)
		=> (content ?? string.Empty)
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace('\r', '\n');
}
