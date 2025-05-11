using ICSharpCode.AvalonEdit.Document;
using ScriptLib.Core.Extensions;
using ScriptLib.IniScript.Enums;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	#region public

	/// <summary>
	/// Gets the word at the specified document offset.
	/// </summary>
	/// <param name="document">The text document to extract the word from.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>The word at the specified offset, or <see langword="null" /> if no word is found or the offset is at whitespace.</returns>
	public static string? GetWordFromOffset(TextDocument document, int offset)
	{
		DocumentLine? line = document.TryGetLineByOffset(offset);

		if (line is null)
			return null;

		string lineText = document.GetText(line);
		lineText = EscapeComments(lineText);
		int relativeOffset = offset - line.Offset;

		if (char.IsWhiteSpace(lineText[relativeOffset]))
			return null;

		// Find the boundaries of the current word
		int wordStart = FindWordStart(lineText, relativeOffset);
		int wordEnd = FindWordEnd(lineText, relativeOffset);

		if (wordStart < 0 || wordEnd <= wordStart)
			return null;

		return lineText[wordStart..wordEnd];
	}

	/// <summary>
	/// Determines the type of word at the specified document offset.
	/// </summary>
	/// <param name="document">The text document to analyze.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The <see cref="WordType"/> of the word at the specified offset.
	/// Returns <see cref="WordType.Unknown"/> if no word is found or if the word type cannot be determined.
	/// </returns>
	public static WordType GetWordTypeFromOffset(TextDocument document, int offset)
	{
		DocumentLine? line = document.TryGetLineByOffset(offset);

		if (line is null)
			return WordType.Unknown;

		string lineText = document.GetText(line);
		lineText = EscapeComments(lineText);

		int relativeOffset = offset - line.Offset;

		if (char.IsWhiteSpace(lineText[relativeOffset]))
			return WordType.Unknown;

		// Check if we're in a header [...]
		if (IsInsideHeader(lineText, relativeOffset))
			return WordType.Header;

		// Find the boundaries of the current word
		int wordStart = FindWordStart(lineText, relativeOffset);
		int wordEnd = FindWordEnd(lineText, relativeOffset);

		if (wordStart < 0 || wordEnd <= wordStart)
			return WordType.Unknown;

		// Check for command (line contains `=` sign)
		for (int i = wordEnd; i < lineText.Length; i++)
		{
			if (lineText[i] == '=')
				return WordType.Command;
		}

		// Check the word for special characters
		for (int i = wordStart; i < wordEnd; i++)
		{
			char c = lineText[i];

			if (c == '_')
				return WordType.MnemonicConstant;

			if (c == '$')
				return WordType.Hexadecimal;
		}

		return WordType.Unknown;
	}

	#endregion public

	#region private

	private static bool IsInsideHeader(string lineText, int relativeOffset)
	{
		int leftBracket = -1;
		int rightBracket = -1;

		// Find the first non-whitespace character and check if it's a left bracket
		for (int i = 0; i <= relativeOffset; i++)
		{
			char c = lineText[i];

			if (char.IsWhiteSpace(c))
				continue; // Skip leading whitespace

			if (c != '[')
				return false; // Not a header line, first non-whitespace character is not a bracket

			leftBracket = i;
			break; // Found the left bracket
		}

		if (leftBracket < 0)
			return false; // No left bracket found

		// Find the right bracket after the left bracket, starting from the relative offset
		for (int i = relativeOffset; i < lineText.Length; i++)
		{
			if (lineText[i] == ']')
			{
				rightBracket = i;
				break; // Found the right bracket
			}
		}

		return rightBracket > leftBracket;
	}

	private static int FindWordStart(string lineText, int relativeOffset)
	{
		if (relativeOffset == 0)
			return 0;

		for (int i = relativeOffset; i >= 0; i--)
		{
			if (i == 0)
				return 0;

			char c = lineText[i - 1];

			if (char.IsWhiteSpace(c) || c is ',' or '=' or '+' or '-' or '*' or '/' or '(' or '[')
				return i;
		}

		return 0;
	}

	private static int FindWordEnd(string lineText, int relativeOffset)
	{
		for (int i = relativeOffset; i < lineText.Length; i++)
		{
			char c = lineText[i];

			if (char.IsWhiteSpace(c) || c is ',' or '=' or ';' or '+' or '-' or '*' or '/' or ')' or ']')
				return i;
		}

		return lineText.Length;
	}

	#endregion private
}
