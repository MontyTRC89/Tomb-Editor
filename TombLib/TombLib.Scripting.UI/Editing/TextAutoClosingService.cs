#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;

namespace TombLib.Scripting.UI.Editing;

internal sealed class TextAutoClosingService
{
	public bool TryGetAction(
		TextDocument document,
		int caretOffset,
		string? inputText,
		TextAutoClosingOptions options,
		out TextAutoClosingAction action)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(options);

		action = default;

		if (string.IsNullOrEmpty(inputText))
			return false;

		if (TryGetAction(inputText, "(", options.AutoCloseParentheses, options.ParenthesesClosingString, document, caretOffset, false, out action))
			return true;

		if (TryGetAction(inputText, "{", options.AutoCloseBraces, options.BracesClosingString, document, caretOffset, false, out action))
			return true;

		if (TryGetAction(inputText, "[", options.AutoCloseBrackets, options.BracketsClosingString, document, caretOffset, false, out action))
			return true;

		if (TryGetAction(inputText, "\"", options.AutoCloseDoubleQuotes, options.DoubleQuotesClosingString, document, caretOffset, true, out action))
			return true;

		return TryGetAction(inputText, "'", options.AutoCloseSingleQuotes, options.SingleQuotesClosingString, document, caretOffset, true, out action);
	}

	private static bool TryGetAction(
		string inputText,
		string openingToken,
		bool isEnabled,
		string closingToken,
		TextDocument document,
		int caretOffset,
		bool blockDuplicateInsert,
		out TextAutoClosingAction action)
	{
		action = default;

		if (!isEnabled || string.IsNullOrEmpty(closingToken))
			return false;

		if (inputText == openingToken)
		{
			if (blockDuplicateInsert && IsExistingClosingTokenAtCaret(document, caretOffset, closingToken))
				return false;

			action = TextAutoClosingAction.CreateInsert(closingToken);
			return true;
		}

		if (inputText == closingToken && IsExistingClosingTokenAtCaret(document, caretOffset, closingToken))
		{
			action = TextAutoClosingAction.CreateSkip(closingToken);
			return true;
		}

		return false;
	}

	private static bool IsExistingClosingTokenAtCaret(TextDocument document, int caretOffset, string closingToken)
		=> caretOffset < document.TextLength
			&& !string.IsNullOrEmpty(closingToken)
			&& document.GetCharAt(caretOffset) == closingToken[0];
}

internal sealed record TextAutoClosingOptions(
	bool AutoCloseParentheses,
	bool AutoCloseBraces,
	bool AutoCloseBrackets,
	bool AutoCloseDoubleQuotes,
	bool AutoCloseSingleQuotes,
	string ParenthesesClosingString,
	string BracesClosingString,
	string BracketsClosingString,
	string DoubleQuotesClosingString,
	string SingleQuotesClosingString);

internal readonly record struct TextAutoClosingAction(TextAutoClosingActionKind Kind, string Element)
{
	public static TextAutoClosingAction CreateInsert(string element)
		=> new(TextAutoClosingActionKind.InsertClosingElement, element);

	public static TextAutoClosingAction CreateSkip(string element)
		=> new(TextAutoClosingActionKind.SkipExistingClosingElement, element);
}

internal enum TextAutoClosingActionKind
{
	InsertClosingElement,
	SkipExistingClosingElement
}
