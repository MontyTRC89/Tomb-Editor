using System.Collections.Generic;
using System.Text;

namespace TombLib.Scripting.Lua.Utils;

/// <summary>
/// Identifies the long-block parser mode that must continue across Lua document lines.
/// </summary>
internal enum LuaLineParserStateKind
{
	None,
	LongString,
	LongComment
}

/// <summary>
/// Stores the parser continuation state needed to evaluate long strings and long comments across line boundaries.
/// </summary>
internal readonly struct LuaLineParserState
{
	public LuaLineParserState(LuaLineParserStateKind kind, int longBracketEqualsCount)
	{
		Kind = kind;
		LongBracketEqualsCount = longBracketEqualsCount;
	}

	public LuaLineParserStateKind Kind { get; }
	public int LongBracketEqualsCount { get; }
}

/// <summary>
/// Provides lightweight line-based parsing helpers for Lua identifiers, comments, and long-bracket strings.
/// </summary>
internal static class LuaLineParser
{
	private enum ParserState
	{
		None,
		SingleQuotedString,
		DoubleQuotedString,
		LongString,
		LongComment
	}

	/// <summary>
	/// Determines whether a character can appear within a Lua identifier.
	/// </summary>
	/// <param name="character">The character to test.</param>
	/// <returns><see langword="true"/> if the character is valid inside an identifier; otherwise, <see langword="false"/>.</returns>
	public static bool IsIdentifierCharacter(char character)
		=> char.IsLetterOrDigit(character) || character == '_';

	/// <summary>
	/// Determines whether a character can start an identifier-triggered autocomplete request.
	/// </summary>
	/// <param name="character">The character to test.</param>
	/// <returns><see langword="true"/> if the character is a valid identifier trigger; otherwise, <see langword="false"/>.</returns>
	public static bool IsIdentifierTriggerCharacter(char character)
		=> char.IsLetter(character) || character == '_';

	/// <summary>
	/// Determines whether the inspected line fragment currently ends inside a comment or string.
	/// </summary>
	/// <param name="lineText">The line text to inspect, typically truncated at the current offset.</param>
	/// <returns><see langword="true"/> if the fragment is inside a comment or string; otherwise, <see langword="false"/>.</returns>
	public static bool IsInsideCommentOrString(string lineText)
		=> IsInsideCommentOrString(lineText, default, out _);

	internal static bool IsInsideCommentOrString(string lineText, LuaLineParserState initialState, out LuaLineParserState finalState)
	{
		ParserState state = GetInitialParserState(initialState);
		int longBracketEqualsCount = initialState.LongBracketEqualsCount;

		if (string.IsNullOrEmpty(lineText))
		{
			finalState = CreateContinuationState(state, longBracketEqualsCount);
			return state != ParserState.None;
		}

		for (int i = 0; i < lineText.Length; i++)
		{
			char currentChar = lineText[i];

			if (state == ParserState.LongString || state == ParserState.LongComment)
			{
				if (TryMatchLongBracketEnd(lineText, i, longBracketEqualsCount, out int endTokenLength))
				{
					i += endTokenLength - 1;
					state = ParserState.None;
				}

				continue;
			}

			if (state == ParserState.SingleQuotedString)
			{
				if (currentChar == '\\' && i + 1 < lineText.Length)
				{
					i++;
					continue;
				}

				if (currentChar == '\'')
					state = ParserState.None;

				continue;
			}

			if (state == ParserState.DoubleQuotedString)
			{
				if (currentChar == '\\' && i + 1 < lineText.Length)
				{
					i++;
					continue;
				}

				if (currentChar == '"')
					state = ParserState.None;

				continue;
			}

			if (TryMatchLongCommentStart(lineText, i, out longBracketEqualsCount, out int longCommentStartLength))
			{
				state = ParserState.LongComment;
				i += longCommentStartLength - 1;
				continue;
			}

			if (IsLineCommentStart(lineText, i))
			{
				finalState = default;
				return true;
			}

			if (TryMatchLongBracketStart(lineText, i, out longBracketEqualsCount, out int longStringStartLength))
			{
				state = ParserState.LongString;
				i += longStringStartLength - 1;
				continue;
			}

			if (currentChar == '\'')
				state = ParserState.SingleQuotedString;
			else if (currentChar == '"')
				state = ParserState.DoubleQuotedString;
		}

		finalState = CreateContinuationState(state, longBracketEqualsCount);
		return state != ParserState.None;
	}

	/// <summary>
	/// Removes a trailing Lua line comment while preserving quoted strings and long-bracket strings.
	/// </summary>
	/// <param name="lineText">The line text to process.</param>
	/// <returns>The line text without a trailing line comment.</returns>
	public static string StripLineComment(string lineText)
	{
		if (string.IsNullOrEmpty(lineText))
			return string.Empty;

		var builder = new StringBuilder(lineText.Length);
		ParserState state = ParserState.None;
		int longBracketEqualsCount = 0;

		for (int i = 0; i < lineText.Length; i++)
		{
			char currentChar = lineText[i];

			if (state == ParserState.LongComment)
			{
				if (TryMatchLongBracketEnd(lineText, i, longBracketEqualsCount, out int endTokenLength))
				{
					i += endTokenLength - 1;
					state = ParserState.None;
				}

				continue;
			}

			if (state == ParserState.LongString)
			{
				if (TryMatchLongBracketEnd(lineText, i, longBracketEqualsCount, out int endTokenLength))
				{
					builder.Append(lineText, i, endTokenLength);
					i += endTokenLength - 1;
					state = ParserState.None;
				}
				else
				{
					builder.Append(currentChar);
				}

				continue;
			}

			if (state == ParserState.SingleQuotedString || state == ParserState.DoubleQuotedString)
			{
				builder.Append(currentChar);

				if (currentChar == '\\' && i + 1 < lineText.Length)
				{
					builder.Append(lineText[i + 1]);
					i++;
					continue;
				}

				if ((state == ParserState.SingleQuotedString && currentChar == '\'')
					|| (state == ParserState.DoubleQuotedString && currentChar == '"'))
				{
					state = ParserState.None;
				}

				continue;
			}

			if (TryMatchLongCommentStart(lineText, i, out longBracketEqualsCount, out int longCommentStartLength))
			{
				state = ParserState.LongComment;
				i += longCommentStartLength - 1;
				continue;
			}

			if (IsLineCommentStart(lineText, i))
				break;

			if (TryMatchLongBracketStart(lineText, i, out longBracketEqualsCount, out int longStringStartLength))
			{
				builder.Append(lineText, i, longStringStartLength);
				state = ParserState.LongString;
				i += longStringStartLength - 1;
				continue;
			}

			if (currentChar == '\'')
				state = ParserState.SingleQuotedString;
			else if (currentChar == '"')
				state = ParserState.DoubleQuotedString;

			builder.Append(currentChar);
		}

		return builder.ToString();
	}

	/// <summary>
	/// Enumerates structural characters that remain after stripping comments and string content from a line.
	/// </summary>
	/// <param name="lineText">The line text to inspect.</param>
	/// <returns>The structural characters that participate in brace and delimiter analysis.</returns>
	public static IEnumerable<char> EnumerateStructuralCharacters(string lineText)
	{
		if (string.IsNullOrEmpty(lineText))
			yield break;

		ParserState state = ParserState.None;
		int longBracketEqualsCount = 0;

		for (int i = 0; i < lineText.Length; i++)
		{
			char currentChar = lineText[i];

			if (state == ParserState.LongComment || state == ParserState.LongString)
			{
				if (TryMatchLongBracketEnd(lineText, i, longBracketEqualsCount, out int endTokenLength))
				{
					i += endTokenLength - 1;
					state = ParserState.None;
				}

				continue;
			}

			if (state == ParserState.SingleQuotedString || state == ParserState.DoubleQuotedString)
			{
				if (currentChar == '\\' && i + 1 < lineText.Length)
				{
					i++;
				}
				else if ((state == ParserState.SingleQuotedString && currentChar == '\'')
					|| (state == ParserState.DoubleQuotedString && currentChar == '"'))
				{
					state = ParserState.None;
				}

				continue;
			}

			if (TryMatchLongCommentStart(lineText, i, out longBracketEqualsCount, out int longCommentStartLength))
			{
				state = ParserState.LongComment;
				i += longCommentStartLength - 1;
				continue;
			}

			if (IsLineCommentStart(lineText, i))
				yield break;

			if (TryMatchLongBracketStart(lineText, i, out longBracketEqualsCount, out int longStringStartLength))
			{
				state = ParserState.LongString;
				i += longStringStartLength - 1;
				continue;
			}

			if (currentChar == '\'')
			{
				state = ParserState.SingleQuotedString;
				continue;
			}

			if (currentChar == '"')
			{
				state = ParserState.DoubleQuotedString;
				continue;
			}

			yield return currentChar;
		}
	}

	private static bool TryMatchLongCommentStart(string lineText, int index, out int equalsCount, out int tokenLength)
	{
		equalsCount = 0;
		tokenLength = 0;

		if (!IsLineCommentStart(lineText, index) || !TryMatchLongBracketStart(lineText, index + 2, out equalsCount, out int bracketTokenLength))
			return false;

		tokenLength = 2 + bracketTokenLength;
		return true;
	}

	private static bool TryMatchLongBracketStart(string lineText, int index, out int equalsCount, out int tokenLength)
	{
		equalsCount = 0;
		tokenLength = 0;

		if (index >= lineText.Length || lineText[index] != '[')
			return false;

		int probeIndex = index + 1;

		while (probeIndex < lineText.Length && lineText[probeIndex] == '=')
		{
			equalsCount++;
			probeIndex++;
		}

		if (probeIndex >= lineText.Length || lineText[probeIndex] != '[')
		{
			equalsCount = 0;
			return false;
		}

		tokenLength = probeIndex - index + 1;
		return true;
	}

	private static bool TryMatchLongBracketEnd(string lineText, int index, int equalsCount, out int tokenLength)
	{
		tokenLength = 0;

		if (index >= lineText.Length || lineText[index] != ']')
			return false;

		int probeIndex = index + 1;

		for (int i = 0; i < equalsCount; i++)
		{
			if (probeIndex >= lineText.Length || lineText[probeIndex] != '=')
				return false;

			probeIndex++;
		}

		if (probeIndex >= lineText.Length || lineText[probeIndex] != ']')
			return false;

		tokenLength = probeIndex - index + 1;
		return true;
	}

	private static bool IsLineCommentStart(string lineText, int index)
		=> lineText[index] == '-' && index + 1 < lineText.Length && lineText[index + 1] == '-';

	private static ParserState GetInitialParserState(LuaLineParserState initialState) => initialState.Kind switch
	{
		LuaLineParserStateKind.LongString => ParserState.LongString,
		LuaLineParserStateKind.LongComment => ParserState.LongComment,
		_ => ParserState.None
	};

	private static LuaLineParserState CreateContinuationState(ParserState state, int longBracketEqualsCount) => state switch
	{
		ParserState.LongString => new LuaLineParserState(LuaLineParserStateKind.LongString, longBracketEqualsCount),
		ParserState.LongComment => new LuaLineParserState(LuaLineParserStateKind.LongComment, longBracketEqualsCount),
		_ => default
	};
}
