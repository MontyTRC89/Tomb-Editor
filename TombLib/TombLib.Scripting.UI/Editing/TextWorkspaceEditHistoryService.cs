#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Tracks undo and redo history for workspace-edit transactions.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditHistoryService
{
	private readonly TextWorkspaceEditApplier _workspaceEditApplier;
	private readonly Stack<TextWorkspaceEditTransaction> _undoStack = [];
	private readonly Stack<TextWorkspaceEditTransaction> _redoStack = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="TextWorkspaceEditHistoryService"/> class.
	/// </summary>
	/// <param name="workspaceEditApplier">The applier used to replay workspace-edit snapshots.</param>
	public TextWorkspaceEditHistoryService(TextWorkspaceEditApplier workspaceEditApplier)
	{
		_workspaceEditApplier = workspaceEditApplier ?? throw new ArgumentNullException(nameof(workspaceEditApplier));
	}

	/// <summary>
	/// Gets a value indicating whether an undo operation is available.
	/// </summary>
	public bool CanUndo => _undoStack.Count > 0;

	/// <summary>
	/// Gets a value indicating whether a redo operation is available.
	/// </summary>
	public bool CanRedo => _redoStack.Count > 0;

	/// <summary>
	/// Gets a value indicating whether any history entries are available.
	/// </summary>
	public bool HasEntries => CanUndo || CanRedo;

	/// <summary>
	/// Pushes a transaction onto the undo stack.
	/// </summary>
	/// <param name="transaction">The transaction to store.</param>
	public void Push(TextWorkspaceEditTransaction transaction)
	{
		ArgumentNullException.ThrowIfNull(transaction);

		if (!transaction.HasChanges)
			return;

		_undoStack.Push(transaction);
		_redoStack.Clear();
	}

	/// <summary>
	/// Clears the undo and redo history.
	/// </summary>
	public void Clear()
	{
		_undoStack.Clear();
		_redoStack.Clear();
	}

	/// <summary>
	/// Applies the previous workspace-edit snapshot.
	/// </summary>
	/// <returns>The file paths whose contents changed during the undo operation.</returns>
	public IReadOnlyList<string> Undo()
	{
		if (_undoStack.Count == 0)
			return [];

		TextWorkspaceEditTransaction transaction = _undoStack.Pop();

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

	/// <summary>
	/// Reapplies the next workspace-edit snapshot.
	/// </summary>
	/// <returns>The file paths whose contents changed during the redo operation.</returns>
	public IReadOnlyList<string> Redo()
	{
		if (_redoStack.Count == 0)
			return [];

		TextWorkspaceEditTransaction transaction = _redoStack.Pop();

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

/// <summary>
/// Represents the before and after snapshots of a workspace-edit application.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditTransaction(IReadOnlyList<TextWorkspaceDocumentChange> documentChanges)
{
	/// <summary>
	/// Gets the per-document changes captured in the transaction.
	/// </summary>
	public IReadOnlyList<TextWorkspaceDocumentChange> DocumentChanges { get; } = documentChanges ?? [];

	/// <summary>
	/// Gets a value indicating whether the transaction contains any changes.
	/// </summary>
	public bool HasChanges => DocumentChanges.Count > 0;
}

/// <summary>
/// Represents the before and after contents of a changed document.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceDocumentChange(string filePath, string beforeContent, string afterContent)
{
	/// <summary>
	/// Gets the changed file path.
	/// </summary>
	public string FilePath { get; } = filePath ?? string.Empty;

	/// <summary>
	/// Gets the document content before the change.
	/// </summary>
	public string BeforeContent { get; } = beforeContent ?? string.Empty;

	/// <summary>
	/// Gets the document content after the change.
	/// </summary>
	public string AfterContent { get; } = afterContent ?? string.Empty;
}
