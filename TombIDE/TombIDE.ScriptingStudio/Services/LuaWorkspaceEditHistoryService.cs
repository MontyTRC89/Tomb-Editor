#nullable enable

using System;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.Services;

internal sealed class LuaWorkspaceEditHistoryService(LuaWorkspaceEditApplier workspaceEditApplier)
{
	private readonly LuaWorkspaceEditApplier _workspaceEditApplier = workspaceEditApplier ?? throw new ArgumentNullException(nameof(workspaceEditApplier));
	private readonly Stack<LuaWorkspaceEditTransaction> _undoStack = [];
	private readonly Stack<LuaWorkspaceEditTransaction> _redoStack = [];

	public bool CanUndo => _undoStack.Count > 0;
	public bool CanRedo => _redoStack.Count > 0;
	public bool HasEntries => CanUndo || CanRedo;

	public void Push(LuaWorkspaceEditTransaction transaction)
	{
		ArgumentNullException.ThrowIfNull(transaction);

		if (!transaction.HasChanges)
			return;

		_undoStack.Push(transaction);
		_redoStack.Clear();
	}

	public void Clear()
	{
		_undoStack.Clear();
		_redoStack.Clear();
	}

	public IReadOnlyList<string> Undo()
	{
		if (_undoStack.Count == 0)
			return [];

		LuaWorkspaceEditTransaction transaction = _undoStack.Pop();

		try
		{
			IReadOnlyList<string> changedFiles = _workspaceEditApplier.ApplyBeforeSnapshot(transaction);
			_redoStack.Push(transaction);
			return changedFiles;
		}
		catch
		{
			_undoStack.Push(transaction);
			throw;
		}
	}

	public IReadOnlyList<string> Redo()
	{
		if (_redoStack.Count == 0)
			return [];

		LuaWorkspaceEditTransaction transaction = _redoStack.Pop();

		try
		{
			IReadOnlyList<string> changedFiles = _workspaceEditApplier.ApplyAfterSnapshot(transaction);
			_undoStack.Push(transaction);
			return changedFiles;
		}
		catch
		{
			_redoStack.Push(transaction);
			throw;
		}
	}
}

internal sealed class LuaWorkspaceEditTransaction(IReadOnlyList<LuaWorkspaceDocumentChange> documentChanges)
{
	public IReadOnlyList<LuaWorkspaceDocumentChange> DocumentChanges { get; } = documentChanges ?? [];

	public bool HasChanges => DocumentChanges.Count > 0;
}

internal sealed class LuaWorkspaceDocumentChange(string filePath, string beforeContent, string afterContent)
{
	public string FilePath { get; } = filePath ?? string.Empty;
	public string BeforeContent { get; } = beforeContent ?? string.Empty;
	public string AfterContent { get; } = afterContent ?? string.Empty;
}