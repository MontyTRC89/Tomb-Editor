using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Windows.Input;

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

	public void HandleTextEntering(
		TextEditor editor,
		TextCompositionEventArgs e,
		TextAutoClosingOptions options,
		Action<string>? onElementSkipped = null)
	{
		if (!TryGetAction(editor.Document, editor.CaretOffset, e.Text, options, out TextAutoClosingAction action))
			return;

		ApplyAction(editor, e, action, onElementSkipped);
	}

	private static void ApplyAction(TextEditor editor, TextCompositionEventArgs e, TextAutoClosingAction action, Action<string>? onElementSkipped)
	{
		switch (action.Kind)
		{
			case TextAutoClosingActionKind.InsertClosingElement:
				editor.SelectedText += action.Element;
				editor.CaretOffset -= action.Element.Length;
				editor.SelectionStart = editor.CaretOffset;
				editor.SelectionLength = 0;
				break;

			case TextAutoClosingActionKind.SkipExistingClosingElement:
				editor.CaretOffset++;
				e.Handled = true;
				onElementSkipped?.Invoke(action.Element);
				break;
		}
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
	{
		return caretOffset < document.TextLength
			&& !string.IsNullOrEmpty(closingToken)
			&& document.GetCharAt(caretOffset) == closingToken[0];
	}
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
