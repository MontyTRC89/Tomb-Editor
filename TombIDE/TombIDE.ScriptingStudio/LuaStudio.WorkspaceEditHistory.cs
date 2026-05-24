#nullable enable

using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private readonly TextWorkspaceEditHistoryService _workspaceEditHistory;
	private int _workspaceEditHistoryApplyDepth;

	protected override bool CanExecuteUndo()
		=> _workspaceEditHistory.CanUndo || base.CanExecuteUndo();

	protected override bool CanExecuteRedo()
		=> _workspaceEditHistory.CanRedo || base.CanExecuteRedo();

	protected override void ExecuteUndo()
	{
		if (TryExecuteWorkspaceUndo())
			return;

		base.ExecuteUndo();
	}

	protected override void ExecuteRedo()
	{
		if (TryExecuteWorkspaceRedo())
			return;

		base.ExecuteRedo();
	}

	private bool IsApplyingWorkspaceEditHistory => _workspaceEditHistoryApplyDepth > 0;

	private void PushWorkspaceEditTransaction(TextWorkspaceEditTransaction transaction)
	{
		if (!transaction.HasChanges)
			return;

		_workspaceEditHistory.Push(transaction);
		UpdateUndoRedoSaveStates();
	}

	private void InvalidateWorkspaceEditHistory()
	{
		if (IsApplyingWorkspaceEditHistory || !_workspaceEditHistory.HasEntries)
			return;

		_workspaceEditHistory.Clear();
		UpdateUndoRedoSaveStates();
	}

	private bool TryExecuteWorkspaceUndo()
	{
		if (!_workspaceEditHistory.CanUndo)
			return false;

		ApplyWorkspaceEditHistoryOperation(_workspaceEditHistory.Undo, Strings.Default.Undo);
		return true;
	}

	private bool TryExecuteWorkspaceRedo()
	{
		if (!_workspaceEditHistory.CanRedo)
			return false;

		ApplyWorkspaceEditHistoryOperation(_workspaceEditHistory.Redo, Strings.Default.Redo);
		return true;
	}

	private void ApplyWorkspaceEditHistoryOperation(Func<IReadOnlyList<string>> operation, string caption)
	{
		try
		{
			_workspaceEditHistoryApplyDepth++;
			HandleWorkspaceDocumentsChanged(operation());
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			_workspaceEditHistoryApplyDepth--;
			UpdateUndoRedoSaveStates();
		}
	}

	private void HandleWorkspaceDocumentsChanged(IEnumerable<string> filePaths)
	{
		string[] changedFiles = [.. filePaths
			.Where(filePath => !string.IsNullOrWhiteSpace(filePath))
			.Distinct(StringComparer.OrdinalIgnoreCase)];

		foreach (string filePath in changedFiles)
		{
			if (TryGetOpenLuaEditor(filePath, out LuaEditor? updatedEditor) && updatedEditor is not null)
				_trackedDocumentStateService.UpdateDocument(updatedEditor);
		}

		if (CurrentEditor is LuaEditor currentEditor
			&& changedFiles.Any(filePath => string.Equals(filePath, currentEditor.FilePath, StringComparison.OrdinalIgnoreCase)))
		{
			RefreshLuaDiagnosticsView(isPending: true);
		}
	}
}