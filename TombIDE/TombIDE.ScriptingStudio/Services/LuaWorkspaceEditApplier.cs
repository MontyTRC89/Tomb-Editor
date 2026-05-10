#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services;

internal sealed class LuaWorkspaceEditApplier(EditorTabControl editorTabControl)
{
	private readonly EditorTabControl _editorTabControl = editorTabControl ?? throw new ArgumentNullException(nameof(editorTabControl));

	public LuaWorkspaceEditTransaction Apply(LuaWorkspaceEdit workspaceEdit, LuaWorkspaceEditSelectionState? selectionState = null)
	{
		ArgumentNullException.ThrowIfNull(workspaceEdit);

		if (!workspaceEdit.HasEdits)
			return new LuaWorkspaceEditTransaction([]);

		TabPage? previouslySelectedTab = _editorTabControl.SelectedTab;
		var documentChanges = new List<LuaWorkspaceDocumentChange>();

		try
		{
			foreach (IGrouping<string, LuaDocumentEdit> fileGroup in workspaceEdit.DocumentEdits
				.Where(documentEdit => !string.IsNullOrWhiteSpace(documentEdit.FilePath))
				.GroupBy(documentEdit => documentEdit.FilePath, StringComparer.OrdinalIgnoreCase))
			{
				string filePath = fileGroup.Key;
				TextEditorBase textEditor = GetOrOpenTextEditor(filePath);
				LuaWorkspaceEditSelectionState? selectionStateForFile = selectionState is not null
					&& string.Equals(selectionState.FilePath, filePath, StringComparison.OrdinalIgnoreCase)
						? selectionState
						: null;
				string beforeContent = textEditor.Text;
				List<PreparedTextEdit> preparedTextEdits = PrepareTextEdits(textEditor.Document,
					fileGroup.SelectMany(documentEdit => documentEdit.TextEdits));
				RestoredSelectionState? restoredSelectionState = selectionStateForFile is null
					? null
					: MapSelectionState(selectionStateForFile, preparedTextEdits);

				ApplyPreparedTextEdits(textEditor, preparedTextEdits);
				SynchronizeOpenTabs(filePath, textEditor);

				if (selectionStateForFile is not null && restoredSelectionState is not null)
					RestoreSelectionState(selectionStateForFile.Editor, restoredSelectionState.Value);

				if (!string.Equals(beforeContent, textEditor.Text, StringComparison.Ordinal))
					documentChanges.Add(new LuaWorkspaceDocumentChange(filePath, beforeContent, textEditor.Text));
			}
		}
		finally
		{
			if (previouslySelectedTab is not null && _editorTabControl.TabPages.Contains(previouslySelectedTab))
				_editorTabControl.SelectTab(previouslySelectedTab);
		}

		return new LuaWorkspaceEditTransaction(documentChanges);
	}

	public IReadOnlyList<string> ApplyBeforeSnapshot(LuaWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.BeforeContent);

	public IReadOnlyList<string> ApplyAfterSnapshot(LuaWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.AfterContent);

	private TextEditorBase GetOrOpenTextEditor(string filePath)
	{
		if (!_editorTabControl.FindTabPagesOfFile(filePath).Any() && !File.Exists(filePath))
			throw new FileNotFoundException("Unable to apply a Lua workspace edit because the target file could not be found.", filePath);

		_editorTabControl.OpenFile(filePath);

		if (_editorTabControl.CurrentEditor is not TextEditorBase textEditor)
			throw new InvalidOperationException($"Unable to apply Lua workspace edits to '{filePath}'.");

		return textEditor;
	}

	private static void ApplyPreparedTextEdits(TextEditorBase textEditor, IReadOnlyList<PreparedTextEdit> preparedTextEdits)
	{
		if (preparedTextEdits.Count == 0)
			return;

		textEditor.Document.UndoStack.StartUndoGroup();
		textEditor.Document.BeginUpdate();

		try
		{
			foreach (PreparedTextEdit preparedTextEdit in preparedTextEdits)
				textEditor.Document.Replace(preparedTextEdit.StartOffset, preparedTextEdit.Length, preparedTextEdit.NewText);
		}
		finally
		{
			textEditor.Document.EndUpdate();
			textEditor.Document.UndoStack.EndUndoGroup();
		}

		textEditor.TryRunContentChangedWorker();
	}

	private IReadOnlyList<string> ApplyContentSnapshots(LuaWorkspaceEditTransaction transaction, Func<LuaWorkspaceDocumentChange, string> selectContent)
	{
		ArgumentNullException.ThrowIfNull(transaction);
		ArgumentNullException.ThrowIfNull(selectContent);

		if (!transaction.HasChanges)
			return [];

		TabPage? previouslySelectedTab = _editorTabControl.SelectedTab;
		var updatedFiles = new List<string>(transaction.DocumentChanges.Count);

		try
		{
			foreach (LuaWorkspaceDocumentChange documentChange in transaction.DocumentChanges)
			{
				TextEditorBase textEditor = GetOrOpenTextEditor(documentChange.FilePath);
				ApplyDocumentContent(textEditor, selectContent(documentChange));
				SynchronizeOpenTabs(documentChange.FilePath, textEditor);
				updatedFiles.Add(documentChange.FilePath);
			}
		}
		finally
		{
			if (previouslySelectedTab is not null && _editorTabControl.TabPages.Contains(previouslySelectedTab))
				_editorTabControl.SelectTab(previouslySelectedTab);
		}

		return updatedFiles;
	}

	private static RestoredSelectionState MapSelectionState(LuaWorkspaceEditSelectionState selectionState, IReadOnlyList<PreparedTextEdit> preparedTextEdits)
	{
		PreparedTextEdit[] editsAscending = [.. preparedTextEdits];

		Array.Sort(editsAscending, static (left, right) =>
		{
			int startComparison = left.StartOffset.CompareTo(right.StartOffset);
			return startComparison != 0
				? startComparison
				: left.Length.CompareTo(right.Length);
		});

		int selectionStart = MapOffset(selectionState.SelectionStart, editsAscending);
		int selectionEnd = MapOffset(selectionState.SelectionEnd, editsAscending);
		int caretOffset = MapOffset(selectionState.CaretOffset, editsAscending);

		if (selectionEnd < selectionStart)
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);

		return new RestoredSelectionState(selectionStart, selectionEnd, caretOffset);
	}

	private static int MapOffset(int offset, IReadOnlyList<PreparedTextEdit> preparedTextEdits)
	{
		int cumulativeDelta = 0;

		foreach (PreparedTextEdit preparedTextEdit in preparedTextEdits)
		{
			if (offset < preparedTextEdit.StartOffset)
				break;

			if (offset <= preparedTextEdit.EndOffset)
			{
				int relativeOffset = offset - preparedTextEdit.StartOffset;
				int normalizedRelativeOffset = Math.Min(relativeOffset, preparedTextEdit.NewText.Length);
				return preparedTextEdit.StartOffset + cumulativeDelta + normalizedRelativeOffset;
			}

			cumulativeDelta += preparedTextEdit.NewText.Length - preparedTextEdit.Length;
		}

		return offset + cumulativeDelta;
	}

	private static void RestoreSelectionState(TextEditorBase textEditor, RestoredSelectionState restoredSelectionState)
	{
		int documentLength = textEditor.Document.TextLength;
		int selectionStart = Math.Clamp(restoredSelectionState.SelectionStart, 0, documentLength);
		int selectionEnd = Math.Clamp(restoredSelectionState.SelectionEnd, 0, documentLength);
		int caretOffset = Math.Clamp(restoredSelectionState.CaretOffset, 0, documentLength);

		if (selectionEnd < selectionStart)
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);

		textEditor.Select(selectionStart, selectionEnd - selectionStart);
		textEditor.CaretOffset = caretOffset;
	}

	private static void ApplyDocumentContent(TextEditorBase textEditor, string content)
	{
		if (string.Equals(textEditor.Text, content, StringComparison.Ordinal))
			return;

		textEditor.Content = content;
	}

	private static List<PreparedTextEdit> PrepareTextEdits(TextDocument document, IEnumerable<LuaTextEdit> textEdits)
	{
		var preparedTextEdits = new List<PreparedTextEdit>();

		foreach (LuaTextEdit textEdit in textEdits)
		{
			if (!TryGetOffset(document, textEdit.Range.StartLineNumber, textEdit.Range.StartColumnNumber, out int startOffset)
				|| !TryGetOffset(document, textEdit.Range.EndLineNumber, textEdit.Range.EndColumnNumber, out int endOffset)
				|| endOffset < startOffset)
			{
				throw new InvalidOperationException("LuaLS returned an invalid workspace-edit range.");
			}

			preparedTextEdits.Add(new PreparedTextEdit(startOffset, endOffset - startOffset, textEdit.NewText));
		}

		preparedTextEdits.Sort(static (left, right) =>
		{
			int startComparison = right.StartOffset.CompareTo(left.StartOffset);

			return startComparison != 0
				? startComparison
				: right.Length.CompareTo(left.Length);
		});

		return preparedTextEdits;
	}

	private void SynchronizeOpenTabs(string filePath, TextEditorBase sourceEditor)
	{
		foreach (TabPage tabPage in _editorTabControl.FindTabPagesOfFile(filePath))
		{
			IEditorControl? editor = _editorTabControl.GetEditorOfTab(tabPage);

			if (ReferenceEquals(editor, sourceEditor) || editor is null)
				continue;

			if (!string.Equals(editor.Content, sourceEditor.Content, StringComparison.Ordinal))
				editor.Content = sourceEditor.Content;

			editor.TryRunContentChangedWorker();
		}
	}

	private static bool TryGetOffset(TextDocument document, int lineNumber, int columnNumber, out int offset)
	{
		offset = 0;

		if (lineNumber < 1 || lineNumber > document.LineCount)
			return false;

		DocumentLine line = document.GetLineByNumber(lineNumber);
		int characterOffset = Math.Clamp(columnNumber - 1, 0, line.Length);
		offset = line.Offset + characterOffset;
		return true;
	}

	private readonly record struct PreparedTextEdit(int StartOffset, int Length, string NewText)
	{
		public int EndOffset => StartOffset + Length;
	}

	private readonly record struct RestoredSelectionState(int SelectionStart, int SelectionEnd, int CaretOffset);
}