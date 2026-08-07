using ICSharpCode.AvalonEdit.Document;
using System;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region File I/O

	public new void Load(string filePath)
		=> Load(filePath, false);

	public void Load(string filePath, bool silentSession)
	{
		base.Load(filePath);
		FilePath = filePath;
		_contentPersistenceCoordinator.SetPersistedContent(Content);

		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		IsSilentSession = silentSession;

		_bookmarkCoordinator.Restore(FilePath);
	}

	public void Save()
		=> Save(FilePath);

	public new void Save(string filePath)
	{
		base.Save(filePath);
		_contentPersistenceCoordinator.SetPersistedContent(Content);
		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	internal void SaveBookmarks()
		=> _bookmarkCoordinator.Save(FilePath);

	#endregion File I/O

	#region Content

	public void TryRunContentChangedWorker()
	{
		IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
	}

	public void ApplyPersistedContent(string content)
	{
		SetContent(content);
		_contentPersistenceCoordinator.SetPersistedContent(Content);
		IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
		LastModified = DateTime.Now;
	}

	private void SetContent(string content)
	{
		DocumentLine cachedLine = Document.GetLineByOffset(CaretOffset);

		Document.UndoStack.StartUndoGroup();

		SelectAll();
		SelectedText = content;

		Document.UndoStack.EndUndoGroup();

		if (cachedLine.EndOffset <= Document.TextLength)
			ResetSelectionAt(cachedLine);
		else
			ResetSelection();

		TryRunContentChangedWorker();
	}

	#endregion Content
}
