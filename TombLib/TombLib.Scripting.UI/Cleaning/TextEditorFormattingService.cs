using System;
using System.Windows;
using TombLib.Scripting.Cleaning;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Cleaning;

/// <summary>
/// Applies formatter output to an editor as a single undo operation while preserving the current
/// caret line and scroll position. Because a full-document rewrite changes every offset, the
/// selection is collapsed to the preserved caret rather than re-mapped to the formatted text.
/// </summary>
public sealed class TextEditorFormattingService
{
	/// <summary>
	/// Formats the current editor content as one undo step.
	/// </summary>
	/// <param name="editor">The editor to update.</param>
	/// <param name="formatter">The formatter to apply.</param>
	/// <param name="trimOnly">Whether only trailing whitespace should be trimmed.</param>
	public void FormatDocument(TextEditorBase editor, ITextDocumentFormatter formatter, bool trimOnly = false)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(formatter);

		string formattedContent = formatter.FormatDocument(editor.Text, trimOnly);

		if (string.Equals(editor.Text, formattedContent, StringComparison.Ordinal))
			return;

		Vector scrollOffset = editor.TextArea.TextView.ScrollOffset;
		int caretLineNumber = editor.Document.GetLineByOffset(editor.CaretOffset).LineNumber;

		editor.Document.UndoStack.StartUndoGroup();

		try
		{
			editor.SelectAll();
			editor.SelectedText = formattedContent;
		}
		finally
		{
			editor.Document.UndoStack.EndUndoGroup();
		}

		if (caretLineNumber <= editor.Document.LineCount)
			editor.ResetSelectionAt(caretLineNumber);
		else
			editor.ResetSelection();

		editor.ScrollToHorizontalOffset(scrollOffset.X);
		editor.ScrollToVerticalOffset(scrollOffset.Y);
	}
}
