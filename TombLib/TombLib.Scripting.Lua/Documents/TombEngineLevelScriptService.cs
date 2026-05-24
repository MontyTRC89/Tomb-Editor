#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua.Documents;

/// <summary>
/// Resolves Tomb Engine level definitions across the shared flow script and language table.
/// </summary>
public sealed partial class TombEngineLevelScriptService
{
	[GeneratedRegex(@"^\s*(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*\{\s*""(?<name>(?:\\.|[^""\\])*)""", RegexOptions.Compiled)]
	private static partial Regex GetLanguageEntryRegex();

	[GeneratedRegex(@"^\s*TEN\.Flow\.AddLevel\s*\(\s*(?<key>[A-Za-z_][A-Za-z0-9_]*)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex GetAddLevelRegex();

	private static readonly Regex LanguageEntryRegex = GetLanguageEntryRegex();
	private static readonly Regex AddLevelRegex = GetAddLevelRegex();

	/// <summary>
	/// Determines whether the specified level has both a language entry and a matching flow-script registration.
	/// </summary>
	/// <param name="scriptDocument">The Tomb Engine flow script document.</param>
	/// <param name="languageDocument">The Tomb Engine language strings document.</param>
	/// <param name="levelName">The display name of the level.</param>
	/// <returns><see langword="true"/> when the level is registered in both documents; otherwise, <see langword="false"/>.</returns>
	public bool IsLevelScriptDefined(TextDocument scriptDocument, TextDocument languageDocument, string levelName)
	{
		ArgumentNullException.ThrowIfNull(scriptDocument);
		ArgumentNullException.ThrowIfNull(languageDocument);
		ArgumentNullException.ThrowIfNull(levelName);

		string? levelKey = TryResolveLevelKey(languageDocument, levelName);

		if (levelKey is null)
			return false;

		return ContainsAddLevelRegistration(scriptDocument, levelKey);
	}

	private static string? TryResolveLevelKey(TextDocument languageDocument, string levelName)
	{
		LuaLineParserState parserState = default;

		foreach (DocumentLine line in languageDocument.Lines)
		{
			string lineText = languageDocument.GetText(line);
			bool insideLongBlockAtLineStart = parserState.Kind != LuaLineParserStateKind.None;
			LuaLineParser.IsInsideCommentOrString(lineText, parserState, out LuaLineParserState nextState);

			if (!insideLongBlockAtLineStart)
			{
				Match match = LanguageEntryRegex.Match(LuaLineParser.StripLineComment(lineText));

				if (match.Success && string.Equals(match.Groups["name"].Value, levelName, StringComparison.Ordinal))
					return match.Groups["key"].Value;
			}

			parserState = nextState;
		}

		return null;
	}

	private static bool ContainsAddLevelRegistration(TextDocument scriptDocument, string levelKey)
	{
		LuaLineParserState parserState = default;

		foreach (DocumentLine line in scriptDocument.Lines)
		{
			string lineText = scriptDocument.GetText(line);
			bool insideLongBlockAtLineStart = parserState.Kind != LuaLineParserStateKind.None;
			LuaLineParser.IsInsideCommentOrString(lineText, parserState, out LuaLineParserState nextState);

			if (!insideLongBlockAtLineStart)
			{
				Match match = AddLevelRegex.Match(LuaLineParser.StripLineComment(lineText));

				if (match.Success && string.Equals(match.Groups["key"].Value, levelKey, StringComparison.Ordinal))
					return true;
			}

			parserState = nextState;
		}

		return false;
	}
}