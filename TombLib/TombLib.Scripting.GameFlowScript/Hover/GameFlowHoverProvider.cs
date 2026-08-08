using Nickelony.LanguageServer.Abstractions.Hover;
using TombLib.Scripting.GameFlowScript.Types;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.GameFlowScript.Hover;

/// <summary>
/// Resolves hover information for GameFlow definitions.
/// </summary>
public sealed class GameFlowHoverProvider : ITextHoverProvider
{
	/// <summary>
	/// Gets the hover information for the given request.
	/// </summary>
	/// <param name="request">The hover request.</param>
	/// <returns>The hover information, or <c>null</c> when the hovered word is not a known definition.</returns>
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		string? hoveredWord = GetWordFromOffset(request.DocumentText, request.HoveredOffset);

		if (string.IsNullOrWhiteSpace(hoveredWord))
			return null;

		if (Contains(GameFlowDefinitionCatalog.Sections, hoveredWord))
			return new TextHoverInfo($"GameFlow section \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: ObjectType.Section);

		if (Contains(GameFlowDefinitionCatalog.SpecialProperties, hoveredWord))
			return new TextHoverInfo($"GameFlow special property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: ObjectType.SpecialProperty);

		if (Contains(GameFlowDefinitionCatalog.Properties, hoveredWord))
			return new TextHoverInfo($"GameFlow property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: ObjectType.Property);

		if (Contains(GameFlowDefinitionCatalog.Constants, hoveredWord))
			return new TextHoverInfo($"GameFlow constant \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: ObjectType.Constant);

		return null;
	}

	private static bool Contains(IReadOnlyList<string> values, string value)
	{
		for (int index = 0; index < values.Count; index++)
		{
			if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	// The word-boundary scan replicates AvalonEdit's TextUtilities.GetNextCaretPosition in
	// WordBorder mode so the hover word is identical without allocating a TextDocument per hover.
	private static string? GetWordFromOffset(string documentText, int offset)
	{
		if (offset < 0 || offset > documentText.Length)
			return null;

		int wordStart = GetNextWordBorder(documentText, offset, forward: false);
		int wordEnd = GetNextWordBorder(documentText, offset, forward: true);

		if (wordStart < 0 || wordEnd < 0 || wordEnd <= wordStart)
			return null;

		return documentText.Substring(wordStart, wordEnd - wordStart).Trim();
	}

	private static int GetNextWordBorder(string text, int offset, bool forward)
	{
		int textLength = text.Length;

		if (textLength <= 0)
			return -1;

		while (true)
		{
			int nextPos = forward ? offset + 1 : offset - 1;

			if (nextPos < 0 || nextPos > textLength)
				return -1;

			if (nextPos == 0)
			{
				return 0;
			}
			else if (nextPos == textLength)
			{
				if (!char.IsWhiteSpace(text[textLength - 1]))
					return textLength;
			}
			else
			{
				if (IsWordBorder(text[nextPos - 1], text[nextPos]))
					return nextPos;
			}

			offset = nextPos;
		}
	}

	private static bool IsWordBorder(char before, char after)
		=> IsWordBorder(GetCharacterClass(before), GetCharacterClass(after));

	private static bool IsWordBorder(WordCharacterClass before, WordCharacterClass after)
	{
		if (before == after)
			return false;

		if (before == WordCharacterClass.LineTerminator || after == WordCharacterClass.LineTerminator)
			return false;

		if (before == WordCharacterClass.Whitespace)
			return false;

		return true;
	}

	private static WordCharacterClass GetCharacterClass(char character)
	{
		if (character == '\r' || character == '\n')
			return WordCharacterClass.LineTerminator;

		if (char.IsWhiteSpace(character))
			return WordCharacterClass.Whitespace;

		if (char.IsLetterOrDigit(character) || character == '_')
			return WordCharacterClass.IdentifierPart;

		return WordCharacterClass.Other;
	}

	private enum WordCharacterClass
	{
		LineTerminator,
		Whitespace,
		IdentifierPart,
		Other
	}
}
