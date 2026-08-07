using ICSharpCode.AvalonEdit.Document;
using Nickelony.LanguageServer.Abstractions.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Applies shared workspace edits through a host-provided text-editor seam.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditApplier
{
	private readonly ITextEditorHost _textEditorHost;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextWorkspaceEditApplier"/> class.
	/// </summary>
	/// <param name="textEditorHost">The host used to open and synchronize text editors.</param>
	public TextWorkspaceEditApplier(ITextEditorHost textEditorHost)
	{
		ArgumentNullException.ThrowIfNull(textEditorHost);
		_textEditorHost = textEditorHost;
	}

	/// <summary>
	/// Applies a workspace edit and captures the resulting document changes.
	/// </summary>
	/// <param name="workspaceEdit">The workspace edit to apply.</param>
	/// <param name="selectionState">The optional selection state to restore for the initiating editor.</param>
	/// <returns>The transaction describing the applied document changes.</returns>
	public TextWorkspaceEditTransaction Apply(TextWorkspaceEdit workspaceEdit, TextWorkspaceEditSelectionState? selectionState = null)
	{
		ArgumentNullException.ThrowIfNull(workspaceEdit);

		if (!workspaceEdit.HasEdits)
			return new TextWorkspaceEditTransaction([]);

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var documentChanges = new List<TextWorkspaceDocumentChange>();

			foreach (IGrouping<string, TextDocumentEdit> fileGroup in workspaceEdit.DocumentEdits
				.Where(documentEdit => !string.IsNullOrWhiteSpace(documentEdit.FilePath))
				.GroupBy(documentEdit => documentEdit.FilePath, StringComparer.OrdinalIgnoreCase))
			{
				string filePath = fileGroup.Key;
				TextEditorBase textEditor = _textEditorHost.OpenTextEditor(filePath);
				TextWorkspaceEditSelectionState? selectionStateForFile = selectionState is not null
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
				SynchronizeOpenEditors(filePath, textEditor);

				if (selectionStateForFile is not null && restoredSelectionState is not null)
					RestoreSelectionState(selectionStateForFile.Editor, restoredSelectionState.Value);

				if (!string.Equals(beforeContent, textEditor.Text, StringComparison.Ordinal))
					documentChanges.Add(new TextWorkspaceDocumentChange(filePath, beforeContent, textEditor.Text));
			}

			return new TextWorkspaceEditTransaction(documentChanges);
		});
	}

	/// <summary>
	/// Applies the before snapshots from a transaction.
	/// </summary>
	/// <param name="transaction">The transaction whose before snapshots should be restored.</param>
	/// <returns>The file paths whose contents changed.</returns>
	public IReadOnlyList<string> ApplyBeforeSnapshot(TextWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.BeforeContent);

	/// <summary>
	/// Applies the after snapshots from a transaction.
	/// </summary>
	/// <param name="transaction">The transaction whose after snapshots should be restored.</param>
	/// <returns>The file paths whose contents changed.</returns>
	public IReadOnlyList<string> ApplyAfterSnapshot(TextWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.AfterContent);

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

	private IReadOnlyList<string> ApplyContentSnapshots(TextWorkspaceEditTransaction transaction, Func<TextWorkspaceDocumentChange, string> selectContent)
	{
		ArgumentNullException.ThrowIfNull(transaction);
		ArgumentNullException.ThrowIfNull(selectContent);

		if (!transaction.HasChanges)
			return [];

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var updatedFiles = new List<string>(transaction.DocumentChanges.Count);

			foreach (TextWorkspaceDocumentChange documentChange in transaction.DocumentChanges)
			{
				TextEditorBase textEditor = _textEditorHost.OpenTextEditor(documentChange.FilePath);
				ApplyDocumentContent(textEditor, selectContent(documentChange));
				SynchronizeOpenEditors(documentChange.FilePath, textEditor);
				updatedFiles.Add(documentChange.FilePath);
			}

			return (IReadOnlyList<string>)updatedFiles;
		});
	}

	private static RestoredSelectionState MapSelectionState(TextWorkspaceEditSelectionState selectionState, IReadOnlyList<PreparedTextEdit> preparedTextEdits)
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

	private static List<PreparedTextEdit> PrepareTextEdits(TextDocument document, IEnumerable<TextEdit> textEdits)
	{
		var preparedTextEdits = new List<PreparedTextEdit>();

		foreach (TextEdit textEdit in textEdits)
		{
			if (!TryGetOffset(document, textEdit.Range.StartLineNumber, textEdit.Range.StartColumnNumber, out int startOffset)
				|| !TryGetOffset(document, textEdit.Range.EndLineNumber, textEdit.Range.EndColumnNumber, out int endOffset)
				|| endOffset < startOffset)
			{
				throw new InvalidOperationException("The edit provider returned an invalid workspace-edit range.");
			}

			preparedTextEdits.Add(new PreparedTextEdit(startOffset, endOffset - startOffset, textEdit.NewText));
		}

		preparedTextEdits.Sort(static (left, right) => right.StartOffset.CompareTo(left.StartOffset));
		return preparedTextEdits;
	}

	private static bool TryGetOffset(TextDocument document, int lineNumber, int columnNumber, out int offset)
	{
		offset = 0;

		if (lineNumber < 1 || lineNumber > document.LineCount)
			return false;

		DocumentLine line = document.GetLineByNumber(lineNumber);
		int columnOffset = Math.Max(0, columnNumber - 1);

		if (columnOffset > line.Length)
			return false;

		offset = line.Offset + columnOffset;
		return true;
	}

	private void SynchronizeOpenEditors(string filePath, TextEditorBase sourceEditor)
	{
		foreach (IEditorControl editorControl in _textEditorHost.GetOpenEditors(filePath))
		{
			if (editorControl is not TextEditorBase editor)
				continue;

			if (ReferenceEquals(editor, sourceEditor))
				continue;

			ApplyDocumentContent(editor, sourceEditor.Text);
		}
	}

	private readonly record struct PreparedTextEdit(int StartOffset, int Length, string NewText)
	{
		public int EndOffset => StartOffset + Length;
	}

	private readonly record struct RestoredSelectionState(int SelectionStart, int SelectionEnd, int CaretOffset);
}
