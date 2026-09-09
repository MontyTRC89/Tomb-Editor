using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text;
using TombLib.Scripting.UI.Documents;

namespace TombLib.Scripting.UI.Editing;

internal sealed class TextLineCommentService
{
	public bool TryCreateEdit(
		TextDocument document,
		int selectionStart,
		int selectionLength,
		string commentPrefix,
		TextLineCommentAction action,
		out TextLineCommentEdit edit)
	{
		ArgumentNullException.ThrowIfNull(document);

		edit = default;

		if (string.IsNullOrWhiteSpace(commentPrefix) || document.TextLength == 0)
			return false;

		int safeSelectionStart = document.ClampOffset(selectionStart);
		int safeSelectionEnd = Math.Max(safeSelectionStart, document.ClampOffset(selectionStart + selectionLength));

		DocumentLine startLine = document.GetLineByOffset(safeSelectionStart);
		DocumentLine endLine = document.GetLineByOffset(safeSelectionEnd);

		TextLineCommentAction effectiveAction = action;

		if (action == TextLineCommentAction.Toggle)
			effectiveAction = ShouldUncommentSelectedLines(document, startLine, endLine, commentPrefix)
				? TextLineCommentAction.Uncomment
				: TextLineCommentAction.Comment;

		int totalLineLength = 0;
		var builder = new StringBuilder();

		for (int lineNumber = startLine.LineNumber; lineNumber <= endLine.LineNumber; lineNumber++)
		{
			DocumentLine currentLine = document.GetLineByNumber(lineNumber);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

			builder.AppendLine(TransformLine(currentLineText, commentPrefix, effectiveAction));
			totalLineLength += currentLine.TotalLength;
		}

		string replacementText = builder.ToString();
		edit = new TextLineCommentEdit(
			startLine.Offset,
			totalLineLength,
			replacementText,
			startLine.Offset,
			Math.Max(0, replacementText.Length - 1));

		return true;
	}

	public void ApplyEdit(TextEditor editor, string commentPrefix, TextLineCommentAction action)
	{
		if (!TryCreateEdit(editor.Document, editor.SelectionStart, editor.SelectionLength, commentPrefix, action, out TextLineCommentEdit edit))
			return;

		editor.Select(edit.ReplaceOffset, edit.ReplaceLength);
		editor.SelectedText = edit.ReplacementText;
		editor.Select(edit.SelectionStart, edit.SelectionLength);
	}

	private static bool ShouldUncommentSelectedLines(TextDocument document, DocumentLine startLine, DocumentLine endLine, string commentPrefix)
	{
		bool foundCommentableLine = false;

		for (int lineNumber = startLine.LineNumber; lineNumber <= endLine.LineNumber; lineNumber++)
		{
			DocumentLine currentLine = document.GetLineByNumber(lineNumber);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);
			string trimmedLineText = currentLineText.TrimStart();

			if (string.IsNullOrWhiteSpace(trimmedLineText))
				continue;

			foundCommentableLine = true;

			if (!trimmedLineText.StartsWith(commentPrefix, StringComparison.Ordinal))
				return false;
		}

		return foundCommentableLine;
	}

	private static string TransformLine(string currentLineText, string commentPrefix, TextLineCommentAction action)
	{
		return action == TextLineCommentAction.Uncomment
			? UncommentLine(currentLineText, commentPrefix)
			: CommentLine(currentLineText, commentPrefix);
	}

	private static string CommentLine(string currentLineText, string commentPrefix)
	{
		string leadingWhitespace = GetLeadingWhitespace(currentLineText);

		return !string.IsNullOrWhiteSpace(currentLineText)
			? leadingWhitespace + commentPrefix + currentLineText.TrimStart()
			: leadingWhitespace;
	}

	private static string UncommentLine(string currentLineText, string commentPrefix)
	{
		string leadingWhitespace = GetLeadingWhitespace(currentLineText);
		string trimmedLineText = currentLineText.TrimStart();

		return trimmedLineText.StartsWith(commentPrefix, StringComparison.Ordinal)
			? leadingWhitespace + trimmedLineText.Remove(0, commentPrefix.Length)
			: currentLineText;
	}

	private static string GetLeadingWhitespace(string currentLineText)
	{
		var whitespaceBuilder = new StringBuilder();

		for (int index = 0; index < currentLineText.Length; index++)
		{
			char character = currentLineText[index];

			if (char.IsWhiteSpace(character))
				whitespaceBuilder.Append(character);
			else
				break;
		}

		return whitespaceBuilder.ToString();
	}
}

internal enum TextLineCommentAction
{
	Comment,
	Uncomment,
	Toggle
}

internal readonly record struct TextLineCommentEdit(
	int ReplaceOffset,
	int ReplaceLength,
	string ReplacementText,
	int SelectionStart,
	int SelectionLength);
