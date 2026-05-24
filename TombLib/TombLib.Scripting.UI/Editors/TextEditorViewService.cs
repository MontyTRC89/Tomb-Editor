#nullable enable

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Editors;

internal sealed class TextEditorViewService
{
	private readonly TextEditor _editor;

	public TextEditorViewService(TextEditor editor)
	{
		_editor = editor ?? throw new ArgumentNullException(nameof(editor));
	}

	public int GetOffsetFromPoint(Point point)
	{
		TextViewPosition? position = GetTextViewPosition(point);

		if (position is null)
			return -1;

		DocumentLine pointLine = _editor.Document.GetLineByNumber(position.Value.Line);
		int offset = pointLine.Offset + Math.Min(pointLine.Length, Math.Max(0, position.Value.Column - 1));

		return offset > _editor.Document.TextLength ? -1 : offset;
	}

	public string? GetWordFromOffset(int offset)
	{
		int wordStart = TextUtilities.GetNextCaretPosition(_editor.Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
		int wordEnd = TextUtilities.GetNextCaretPosition(_editor.Document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

		return wordStart >= 0 && wordEnd >= 0
			? _editor.Document.GetText(wordStart, wordEnd - wordStart)
			: null;
	}

	public void ReplaceContent(string newContent)
	{
		_editor.SelectAll();
		_editor.SelectedText = newContent;
		ResetSelection();
	}

	public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
	{
		SelectLine(line);
		_editor.SelectedText = replacement;

		if (deselectAfterwards)
			ResetSelection();
	}

	public void ResetSelection()
	{
		int offset = _editor.Document.TextLength > 0 ? _editor.Document.TextLength - 1 : 0;
		_editor.Select(offset, 0);
	}

	public void ResetSelectionAt(DocumentLine line)
		=> _editor.Select(line.EndOffset, 0);

	public void SelectLine(DocumentLine line)
		=> _editor.Select(line.Offset, line.Length);

	public bool TryMoveCaretToMousePosition()
	{
		if (!string.IsNullOrEmpty(_editor.SelectedText))
			return false;

		TextViewPosition? position = GetTextViewPosition(Mouse.GetPosition(_editor));

		if (position is null)
			return false;

		int offset = _editor.Document.GetOffset(new TextLocation(position.Value.Line, position.Value.Column));
		_editor.Select(offset, 0);
		return true;
	}

	private TextViewPosition? GetTextViewPosition(Point point)
	{
		if (_editor.TextArea?.TextView is null)
			return null;

		Point textViewPoint = _editor.TranslatePoint(point, _editor.TextArea.TextView);
		return _editor.TextArea.TextView.GetPosition(textViewPoint + _editor.TextArea.TextView.ScrollOffset);
	}
}