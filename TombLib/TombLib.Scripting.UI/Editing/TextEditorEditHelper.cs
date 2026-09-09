using ICSharpCode.AvalonEdit.Document;
using System;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Applies programmatic document edits without re-entering keyboard-only language handlers.
/// </summary>
public static class TextEditorEditHelper
{
	/// <summary>
	/// Inserts <paramref name="newText"/> at <paramref name="insertOffset"/> as one undo step
	/// and places the caret at <paramref name="caretOffset"/> (defaults to just after the inserted text).
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="insertOffset">The zero-based offset at which to insert the text.</param>
	/// <param name="newText">The text to insert.</param>
	/// <param name="caretOffset">The caret offset after the edit; defaults to just after the inserted text.</param>
	public static void InsertText(TextEditorBase textEditor, int insertOffset, string newText, int? caretOffset = null)
		=> ApplyEdit(textEditor, insertOffset, 0, newText, caretOffset);

	/// <summary>
	/// Replaces the range starting at <paramref name="startOffset"/> with <paramref name="newText"/>
	/// as one undo step and places the caret at <paramref name="caretOffset"/>
	/// (defaults to just after the inserted text).
	/// </summary>
	/// <param name="textEditor">The editor whose document should be updated.</param>
	/// <param name="startOffset">The zero-based start offset of the replaced range.</param>
	/// <param name="length">The length of the replaced range.</param>
	/// <param name="newText">The replacement text.</param>
	/// <param name="caretOffset">The caret offset after the edit; defaults to just after the inserted text.</param>
	public static void ReplaceText(TextEditorBase textEditor, int startOffset, int length, string newText, int? caretOffset = null)
		=> ApplyEdit(textEditor, startOffset, length, newText, caretOffset);

	private static void ApplyEdit(TextEditorBase textEditor, int startOffset, int length, string newText, int? caretOffset)
	{
		ArgumentNullException.ThrowIfNull(textEditor);

		TextDocument document = textEditor.Document;
		document.UndoStack.StartUndoGroup();

		try
		{
			if (length == 0)
				document.Insert(startOffset, newText);
			else
				document.Replace(startOffset, length, newText);

			textEditor.CaretOffset = Math.Min(caretOffset ?? startOffset + newText.Length, document.TextLength);
		}
		finally
		{
			document.UndoStack.EndUndoGroup();
		}

		textEditor.RunContentChangedWorker();
	}
}
