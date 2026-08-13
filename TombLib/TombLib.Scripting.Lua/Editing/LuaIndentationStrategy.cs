using System;
using System.Collections.Generic;
using System.Text;
using TombLib.Scripting.Lua.Completion;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Computes Lua-specific newline indentation and multiline completion normalization.
/// </summary>
internal static class LuaIndentationStrategy
{
	private readonly record struct TextLine(string Content, string Delimiter, int StartOffset);

	/// <summary>
	/// Creates the indentation unit that should be appended for one extra indent level.
	/// </summary>
	public static string CreateIndentationUnit(bool convertTabsToSpaces, int indentationSize, int tabSize)
	{
		if (!convertTabsToSpaces)
			return "\t";

		int size = indentationSize > 0
			? indentationSize
			: tabSize > 0
				? tabSize
				: 4;

		return new string(' ', size);
	}

	/// <summary>
	/// Gets the leading whitespace prefix for the supplied line.
	/// </summary>
	public static string GetLeadingWhitespace(string lineText)
	{
		return string.IsNullOrEmpty(lineText)
			? string.Empty
			: lineText[..GetLeadingWhitespaceLength(lineText)];
	}

	/// <summary>
	/// Computes the indentation that should be applied to the current line based on the previous Lua line.
	/// </summary>
	public static string GetDesiredIndentation(
		string previousLineText,
		string currentLineText,
		string previousLineIndentation,
		string indentationUnit,
		bool useSmartIndent)
	{
		string indentation = previousLineIndentation;

		if (!useSmartIndent)
			return indentation;

		if (ShouldIncreaseIndentAfterLine(previousLineText))
			indentation += indentationUnit;

		if (StartsWithDedentToken(currentLineText))
			indentation = RemoveSingleIndentLevel(indentation, indentationUnit);

		return indentation;
	}

	/// <summary>
	/// Builds the text that should be inserted when Enter is pressed inside Lua code.
	/// </summary>
	public static LuaEnterInsertionResult BuildEnterInsertion(
		string lineTextBeforeCaret,
		string lineTextAfterCaret,
		string currentLineIndentation,
		string indentationUnit,
		string newLineText,
		bool useSmartIndent)
	{
		newLineText = string.IsNullOrEmpty(newLineText) ? Environment.NewLine : newLineText;

		string nextLineIndentation = currentLineIndentation;

		if (useSmartIndent && ShouldIncreaseIndentAfterLine(lineTextBeforeCaret))
			nextLineIndentation += indentationUnit;

		bool shouldSplitBeforeDedent = useSmartIndent
			&& nextLineIndentation.Length > currentLineIndentation.Length
			&& StartsWithDedentToken(lineTextAfterCaret);

		if (!shouldSplitBeforeDedent)
		{
			string text = newLineText + nextLineIndentation;
			return new LuaEnterInsertionResult(text, text.Length, 0);
		}

		string splitText = newLineText + nextLineIndentation + newLineText + currentLineIndentation;
		return new LuaEnterInsertionResult(
			splitText,
			newLineText.Length + nextLineIndentation.Length,
			GetLeadingWhitespaceLength(lineTextAfterCaret));
	}

	/// <summary>
	/// Normalizes multiline completion insertion relative to the current line indentation.
	/// </summary>
	public static LuaCompletionNormalizationResult NormalizeCompletionInsertion(
		string text,
		int? caretOffset,
		string currentLineIndentation,
		string indentationUnit)
	{
		if (string.IsNullOrEmpty(text) || !ContainsLineBreak(text))
			return new LuaCompletionNormalizationResult(text, caretOffset);

		List<TextLine> lines = SplitLines(text);
		var builder = new StringBuilder(text.Length + Math.Max(0, lines.Count - 1) * currentLineIndentation.Length);
		int? normalizedCaretOffset = null;
		int relativeIndentLevel = 0;

		for (int i = 0; i < lines.Count; i++)
		{
			TextLine line = lines[i];
			int originalLeadingWhitespaceLength = GetLeadingWhitespaceLength(line.Content);
			string trimmedContent = line.Content[originalLeadingWhitespaceLength..];
			int currentIndentLevel = i == 0
				? 0
				: Math.Max(0, relativeIndentLevel - GetDedentLevel(trimmedContent));
			string normalizedIndentation = i == 0
				? string.Empty
				: BuildIndentation(currentLineIndentation, indentationUnit, currentIndentLevel);
			string normalizedLineContent = trimmedContent.Length == 0
				? normalizedIndentation
				: normalizedIndentation + trimmedContent;

			if (caretOffset.HasValue
				&& caretOffset.Value >= line.StartOffset
				&& caretOffset.Value <= line.StartOffset + line.Content.Length)
			{
				int caretColumn = caretOffset.Value - line.StartOffset;
				int normalizedLeadingWhitespaceLength = normalizedLineContent.Length - trimmedContent.Length;
				int contentColumn = Math.Max(0, caretColumn - originalLeadingWhitespaceLength);
				normalizedCaretOffset = builder.Length + normalizedLeadingWhitespaceLength + contentColumn;
			}

			builder.Append(normalizedLineContent);
			builder.Append(line.Delimiter);
			relativeIndentLevel = currentIndentLevel + GetIndentIncrease(trimmedContent);
		}

		if (caretOffset == text.Length)
			normalizedCaretOffset = builder.Length;

		return new LuaCompletionNormalizationResult(builder.ToString(), normalizedCaretOffset ?? caretOffset);
	}

	private static string BuildIndentation(string currentLineIndentation, string indentationUnit, int indentLevel)
	{
		if (indentLevel <= 0)
			return currentLineIndentation;

		var builder = new StringBuilder(currentLineIndentation.Length + indentationUnit.Length * indentLevel);
		builder.Append(currentLineIndentation);

		for (int i = 0; i < indentLevel; i++)
			builder.Append(indentationUnit);

		return builder.ToString();
	}

	private static string RemoveSingleIndentLevel(string indentation, string indentationUnit)
	{
		if (string.IsNullOrEmpty(indentation))
			return string.Empty;

		if (indentationUnit == "\t" && indentation[^1] == '\t')
			return indentation[..^1];

		int removeLength = indentationUnit.Length > 0
			? Math.Min(indentationUnit.Length, indentation.Length)
			: 1;

		return indentation[..^removeLength];
	}

	private static int GetDedentLevel(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).TrimStart();

		if (string.IsNullOrEmpty(codeText))
			return 0;

		return StartsWithDedentToken(codeText) ? 1 : 0;
	}

	private static int GetIndentIncrease(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).Trim();

		if (string.IsNullOrEmpty(codeText))
			return 0;

		return ShouldIncreaseIndentAfterCode(codeText) ? 1 : 0;
	}

	private static bool ShouldIncreaseIndentAfterLine(string lineText)
		=> ShouldIncreaseIndentAfterCode(LuaLineParser.ExtractCodeText(lineText).Trim());

	private static bool ShouldIncreaseIndentAfterCode(string codeText)
	{
		if (string.IsNullOrEmpty(codeText))
			return false;

		if (StartsWithWord(codeText, "repeat")
			|| StartsWithWord(codeText, "else")
			|| StartsWithWord(codeText, "elseif")
			|| EndsWithWord(codeText, "then")
			|| EndsWithWord(codeText, "do")
			|| ContainsWord(codeText, "function"))
		{
			return true;
		}

		return HasPositiveDelimiterBalance(codeText);
	}

	private static bool StartsWithDedentToken(string lineText)
	{
		string codeText = LuaLineParser.ExtractCodeText(lineText).TrimStart();

		if (string.IsNullOrEmpty(codeText))
			return false;

		return StartsWithWord(codeText, "end")
			|| StartsWithWord(codeText, "until")
			|| StartsWithWord(codeText, "else")
			|| StartsWithWord(codeText, "elseif")
			|| StartsWithClosingDelimiter(codeText[0]);
	}

	private static bool HasPositiveDelimiterBalance(string codeText)
	{
		int balance = 0;

		foreach (char character in codeText)
		{
			balance += character switch
			{
				'(' or '{' or '[' => 1,
				')' or '}' or ']' => -1,
				_ => 0
			};
		}

		return balance > 0;
	}

	private static bool StartsWithClosingDelimiter(char character)
		=> character is ')' or '}' or ']';

	private static bool StartsWithWord(string text, string word)
	{
		return text.StartsWith(word, StringComparison.Ordinal)
			&& (text.Length == word.Length || !LuaLineParser.IsIdentifierCharacter(text[word.Length]));
	}

	private static bool EndsWithWord(string text, string word)
	{
		if (!text.EndsWith(word, StringComparison.Ordinal))
			return false;

		int wordStart = text.Length - word.Length;
		return wordStart == 0 || !LuaLineParser.IsIdentifierCharacter(text[wordStart - 1]);
	}

	private static bool ContainsWord(string text, string word)
	{
		int searchIndex = 0;

		while (searchIndex < text.Length)
		{
			int wordIndex = text.IndexOf(word, searchIndex, StringComparison.Ordinal);

			if (wordIndex < 0)
				return false;

			bool hasLeadingBoundary = wordIndex == 0 || !LuaLineParser.IsIdentifierCharacter(text[wordIndex - 1]);
			int wordEnd = wordIndex + word.Length;
			bool hasTrailingBoundary = wordEnd == text.Length || !LuaLineParser.IsIdentifierCharacter(text[wordEnd]);

			if (hasLeadingBoundary && hasTrailingBoundary)
				return true;

			searchIndex = wordIndex + word.Length;
		}

		return false;
	}

	private static bool ContainsLineBreak(string text)
		=> text.IndexOfAny(['\r', '\n']) >= 0;

	private static int GetLeadingWhitespaceLength(string text)
	{
		int length = 0;

		while (length < text.Length && char.IsWhiteSpace(text[length]) && text[length] != '\r' && text[length] != '\n')
			length++;

		return length;
	}

	private static List<TextLine> SplitLines(string text)
	{
		var lines = new List<TextLine>();
		int lineStart = 0;
		int index = 0;

		while (index < text.Length)
		{
			if (text[index] == '\r' || text[index] == '\n')
			{
				int delimiterStart = index;

				if (text[index] == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
					index++;

				lines.Add(new TextLine(
					text[lineStart..delimiterStart],
					text[delimiterStart..(index + 1)],
					lineStart));

				index++;
				lineStart = index;
				continue;
			}

			index++;
		}

		lines.Add(new TextLine(text[lineStart..], string.Empty, lineStart));
		return lines;
	}
}
