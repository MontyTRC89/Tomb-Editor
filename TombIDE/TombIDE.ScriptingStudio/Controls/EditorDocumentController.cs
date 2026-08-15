#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Controls;

internal sealed class EditorDocumentController : IEditorDocumentController
{
	private readonly EditorDocumentControllerCore _documentController;
	private readonly FileReloadCoordinator _fileReloadCoordinator = new();
	private readonly IMessageService _messageService;
	private IEditorControl? _currentEditor;
	private ScriptingDocumentContext _currentDocumentContext = ScriptingDocumentContext.Empty;
	private long _documentContextGeneration;
	private string _scriptRootDirectoryPath;

	public EditorDocumentController(Version currentEngineVersion, string scriptRootDirectoryPath, IMessageService? messageService = null)
	{
		ArgumentNullException.ThrowIfNull(currentEngineVersion);

		_documentController = new EditorDocumentControllerCore(currentEngineVersion);
		_messageService = messageService
			?? ServiceLocator.GetService<IMessageService>()
			?? new MessageBoxService();
		_scriptRootDirectoryPath = string.Empty;
		ScriptRootDirectoryPath = scriptRootDirectoryPath;
	}

	public string ScriptRootDirectoryPath
	{
		get => _scriptRootDirectoryPath;
		set
		{
			if (!AskSaveAll())
				return;

			CloseAllEditors();
			_scriptRootDirectoryPath = value ?? string.Empty;
		}
	}

	public IEditorControl? CurrentEditor => _currentEditor;

	public ScriptingDocumentContext CurrentDocumentContext => _currentDocumentContext;

	public ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor)
		=> _documentController.GetDocumentRegistration(editor);

	public IEditorControl? FindEditor(string filePath, EditorType editorType = EditorType.Default)
		=> _documentController.FindEditor(filePath, editorType);

	public IEditorControl? FindSourceEditor(string filePath)
		=> _documentController.FindSourceEditor(filePath);

	public IEnumerable<IEditorControl> GetOpenEditors()
		=> _documentController.GetOpenEditors();

	public IEnumerable<IEditorControl> FindEditorsOfFile(string filePath)
		=> _documentController.FindEditorsOfFile(filePath);

	public bool ContainsEditor(IEditorControl editor)
		=> _documentController.ContainsEditor(editor);

	public void RegisterDocument(ScriptingDocumentRegistration registration)
		=> _documentController.RegisterDocument(registration);

	public void CheckPreviousSession()
	{
		if (string.IsNullOrWhiteSpace(ScriptRootDirectoryPath) || !Directory.Exists(ScriptRootDirectoryPath))
			return;

		string[] files = Directory.GetFiles(ScriptRootDirectoryPath, $"*{SupportedFormats.Backup}", SearchOption.AllDirectories);

		if (files.Length == 0)
			return;

		DialogResult result = _messageService.ShowConfirmation(
			Strings.Default.AskRestoreSession,
			Strings.Default.RestoreSessionMBT,
			DialogResult.Yes,
			DialogResult.No,
			defaultValue: DialogResult.Yes);

		if (result == DialogResult.Yes)
			RestoreSession(files);
		else if (result == DialogResult.No)
			SharedMethods.DeleteFiles(files);
	}

	public void OpenFile(string filePath, EditorType editorType = EditorType.Default, bool silentSession = false)
	{
		IEditorControl? existingEditor = FindEditor(filePath, editorType);

		if (existingEditor is not null)
		{
			ActivateEditor(existingEditor);
			return;
		}

		EditorOpenResult openResult = _documentController.OpenFile(filePath, editorType, silentSession);

		if (openResult.Editor is null)
			return;

		AttachEditor(openResult.Editor);
		ActivateEditor(openResult.Editor);
		OnFileOpened(EventArgs.Empty);
	}

	public void OpenSourceFile(string filePath, bool silentSession = false)
		=> OpenFile(filePath, _documentController.GetSourceViewEditorType(filePath), silentSession);

	public void AddFileToReloadQueue(string filePath)
		=> _fileReloadCoordinator.QueueFile(filePath);

	public void TryRunFileReloadQueue()
		=> _fileReloadCoordinator.ProcessQueuedFiles(filePath => FindEditorsOfFile(filePath).ToList(), ShowFileReloadPrompt);

	public bool AskSaveAll()
	{
		foreach (string path in _documentController.GetFilePaths())
		{
			IEditorControl? mostRecentEditorOfFile = _documentController.GetMostRecentlyModifiedEditorOfFile(path);

			FileSavingResult result = TryAskSaveFile(mostRecentEditorOfFile);

			if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
				return false;
		}

		return true;
	}

	public void SaveAll()
	{
		foreach (string path in _documentController.GetFilePaths())
		{
			IEditorControl? mostRecentEditorOfFile = _documentController.GetMostRecentlyModifiedEditorOfFile(path);

			if (mostRecentEditorOfFile is not null)
				SaveFile(mostRecentEditorOfFile);
		}
	}

	public FileSavingResult SaveFile()
		=> _currentEditor is null ? FileSavingResult.Failed : SaveFile(_currentEditor);

	public FileSavingResult SaveFile(IEditorControl editor)
	{
		if (editor is null)
			return FileSavingResult.Failed;

		try
		{
			editor.Save();
			RaiseEditorTitleChanged(editor);
			SaveOtherEditorsOfFile(editor);
		}
		catch (Exception ex)
		{
			DialogResult result = _messageService.ShowConfirmation(
				ex.Message,
				Strings.Default.Error,
				DialogResult.Retry,
				DialogResult.Cancel,
				defaultValue: DialogResult.Retry);

			if (result == DialogResult.Retry)
				return SaveFile(editor);

			return FileSavingResult.Failed;
		}

		return FileSavingResult.Success;
	}

	public FileSavingResult SaveFileAs()
		=> _currentEditor is null ? FileSavingResult.Failed : SaveFileAs(_currentEditor);

	public FileSavingResult SaveFileAs(IEditorControl editor)
	{
		if (editor is null)
			return FileSavingResult.Failed;

		string oldFilePath = editor.FilePath;
		string[] ignoredPaths = [];

		if (editor.DefaultFileExtension == ".lua")
			ignoredPaths = [@"Scripts\Engine"];

		var fileCreationViewModel = new FileCreationViewModel(
			ScriptRootDirectoryPath,
			FileCreationMode.SavingAs,
			editor.DefaultFileExtension,
			null,
			null,
			ignoredPaths);

		var view = new FileCreationView(fileCreationViewModel);
		string? newFilePath = view.ShowDialogAndGetResult();

		if (newFilePath is null)
			return FileSavingResult.Canceled;

		if (string.IsNullOrWhiteSpace(oldFilePath)
			|| oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
		{
			editor.FilePath = newFilePath;
			RaiseEditorTitleChanged(editor);
			return SaveFile(editor);
		}

		editor.FilePath = newFilePath;
		RaiseEditorTitleChanged(editor);

		FileSavingResult result = SaveFile(editor);

		if (result != FileSavingResult.Success)
		{
			editor.FilePath = oldFilePath;
			RaiseEditorTitleChanged(editor);
			return result;
		}

		if (FindEditorsOfFile(oldFilePath).Any())
		{
			RenameDocument(oldFilePath, newFilePath);
			SaveOtherEditorsOfFile(editor);
		}
		else
		{
			OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
		}

		return result;
	}

	public void CloseInvalidEditors()
	{
		List<IEditorControl> editorsToClose = [];

		foreach (IEditorControl editor in GetOpenEditors().ToList())
		{
			if (File.Exists(editor.FilePath))
				continue;

			if (editor.IsContentChanged)
				editor.FilePath = string.Empty;
			else
				editorsToClose.Add(editor);
		}

		foreach (IEditorControl editor in editorsToClose)
			TryCloseEditor(editor);
	}

	public void ActivateEditor(IEditorControl editor)
	{
		if (editor is null || !_documentController.ContainsEditor(editor))
			return;

		SynchronizeEditorsOfFile(editor.FilePath);
		SetCurrentEditor(editor);
	}

	public bool TryCloseEditor(IEditorControl editor)
		=> CloseEditor(editor, promptToSave: true);

	public bool TryActivatePreviousEditor()
	{
		IEditorControl? previousEditor = GetRelativeEditor(-1);

		if (previousEditor is null)
			return false;

		ActivateEditor(previousEditor);
		return true;
	}

	public bool TryActivateNextEditor()
	{
		IEditorControl? nextEditor = GetRelativeEditor(1);

		if (nextEditor is null)
			return false;

		ActivateEditor(nextEditor);
		return true;
	}

	public string GetDocumentTitle(IEditorControl editor)
	{
		if (editor is null)
			return string.Empty;

		string title = _documentController.GetDocumentTitle(editor);
		return editor.IsContentChanged ? title + "*" : title;
	}

	public void RenameDocument(string oldFilePath, string newFilePath)
	{
		if (string.IsNullOrWhiteSpace(oldFilePath)
			|| string.IsNullOrWhiteSpace(newFilePath)
			|| oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		List<IEditorControl> editors = FindEditorsOfFile(oldFilePath).ToList();

		if (editors.Count == 0)
			return;

		foreach (IEditorControl editor in editors)
		{
			editor.FilePath = newFilePath;
			RaiseEditorTitleChanged(editor);
		}

		OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
	}

	public bool IsEveryDocumentSaved()
	{
		return _documentController.GetFilePaths()
			.Select(_documentController.GetMostRecentlyModifiedEditorOfFile)
			.Where(static editor => editor is not null)
			.All(static editor => !editor!.IsContentChanged);
	}

	public void EnsureTabFileSynchronization()
	{
		foreach (string filePath in _documentController.GetFilePaths())
			SynchronizeEditorsOfFile(filePath);
	}

	public event EventHandler? FileOpened;

	public event EventHandler<ScriptingDocumentContextChangedEventArgs>? CurrentEditorChanged;

	public event EventHandler<EditorControlEventArgs>? EditorClosed;

	public event EventHandler<EditorControlEventArgs>? EditorTitleChanged;

	public event EventHandler<DocumentRenamedEventArgs>? DocumentRenamed;

	private DialogResult ShowFileReloadPrompt(string filePath)
	{
		return _messageService.ShowConfirmation(
			string.Format(Strings.Default.AskFileReload, filePath),
			Strings.Default.FileReloadMBT,
			DialogResult.Yes,
			DialogResult.No,
			defaultValue: DialogResult.Yes);
	}

	private void RestoreSession(IEnumerable<string> files)
	{
		foreach (string file in files)
		{
			if (!File.Exists(file))
				continue;

			string backupFileContent = File.ReadAllText(file);
			string originalFilePath = FileHelper.GetOriginalFilePathFromBackupFile(file);

			OpenFile(originalFilePath);

			if (_currentEditor is not null)
				_currentEditor.Content = backupFileContent;
		}
	}

	private void CloseAllEditors()
	{
		foreach (IEditorControl editor in GetOpenEditors().ToList())
			CloseEditor(editor, promptToSave: false);
	}

	private bool CloseEditor(IEditorControl editor, bool promptToSave)
	{
		if (editor is null || !_documentController.ContainsEditor(editor))
			return false;

		if (promptToSave && FindEditorsOfFile(editor.FilePath).Count() == 1)
		{
			FileSavingResult result = TryAskSaveFile(editor);

			if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
				return false;
		}
		else if (_documentController.IsMostRecentlyModifiedEditorOfFile(editor))
		{
			foreach (IEditorControl fileEditor in FindEditorsOfFile(editor.FilePath))
				fileEditor.Content = editor.Content;
		}

		IEditorControl? nextEditor = GetAdjacentEditor(editor);

		OnEditorClosed(editor);
		DetachEditor(editor);
		_documentController.RemoveEditor(editor);
		editor.Dispose();

		if (ReferenceEquals(_currentEditor, editor))
			SetCurrentEditor(nextEditor, forceRaise: true);

		return true;
	}

	private FileSavingResult TryAskSaveFile(IEditorControl? editor)
	{
		if (editor is null || !editor.IsContentChanged)
			return FileSavingResult.AlreadySaved;

		ActivateEditor(editor);

		string fileName = Path.GetFileName(editor.FilePath);
		DialogResult result = _messageService.ShowConfirmation(
			string.Format(Strings.Default.AskUnsavedChanged, fileName),
			Strings.Default.UnsavedChangedMBT,
			DialogResult.Yes,
			DialogResult.No,
			DialogResult.Cancel,
			DialogResult.Yes);

		if (result == DialogResult.Yes)
			return SaveFile(editor);

		if (result == DialogResult.No)
			return FileSavingResult.Rejected;

		return FileSavingResult.Canceled;
	}

	private void SaveOtherEditorsOfFile(IEditorControl excludedEditor)
	{
		foreach (IEditorControl fileEditor in FindEditorsOfFile(excludedEditor.FilePath))
		{
			if (fileEditor.EditorType == excludedEditor.EditorType)
				continue;

			if (fileEditor.Content != excludedEditor.Content)
				fileEditor.ApplyPersistedContent(excludedEditor.Content);

			fileEditor.RunContentChangedWorker();
			RaiseEditorTitleChanged(fileEditor);
		}
	}

	private void SynchronizeEditorsOfFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			return;

		IEditorControl? mostRecentEditor = _documentController.GetMostRecentlyModifiedEditorOfFile(filePath);

		if (mostRecentEditor is not null)
			SaveOtherEditorsOfFile(mostRecentEditor);
	}

	private IEditorControl? GetRelativeEditor(int offset)
	{
		List<IEditorControl> editors = [.. GetOpenEditors()];

		if (_currentEditor is null || editors.Count == 0)
			return null;

		int currentIndex = editors.IndexOf(_currentEditor);

		if (currentIndex < 0)
			return null;

		int targetIndex = currentIndex + offset;

		if (targetIndex < 0 || targetIndex >= editors.Count)
			return null;

		return editors[targetIndex];
	}

	private IEditorControl? GetAdjacentEditor(IEditorControl editor)
	{
		List<IEditorControl> editors = [.. GetOpenEditors()];
		int editorIndex = editors.IndexOf(editor);

		if (editorIndex < 0 || editors.Count <= 1)
			return null;

		int candidateIndex = editorIndex > 0 ? editorIndex - 1 : 1;
		return candidateIndex >= 0 && candidateIndex < editors.Count ? editors[candidateIndex] : null;
	}

	private void SetCurrentEditor(IEditorControl? editor, bool forceRaise = false)
	{
		if (!forceRaise && ReferenceEquals(_currentEditor, editor))
			return;

		_currentEditor = editor;
		_documentContextGeneration++;
		_currentDocumentContext = new ScriptingDocumentContext(
			_documentContextGeneration,
			editor,
			editor?.FilePath,
			_documentController.GetDocumentRegistration(editor));
		CurrentEditorChanged?.Invoke(this, new ScriptingDocumentContextChangedEventArgs(_currentDocumentContext));
	}

	private void AttachEditor(IEditorControl editor)
		=> editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

	private void DetachEditor(IEditorControl editor)
		=> editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;

	private void Editor_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl senderEditor)
			return;

		foreach (IEditorControl fileEditor in FindEditorsOfFile(senderEditor.FilePath))
		{
			fileEditor.IsContentChanged = senderEditor.IsContentChanged;
			RaiseEditorTitleChanged(fileEditor);
		}
	}

	private void RaiseEditorTitleChanged(IEditorControl editor)
		=> EditorTitleChanged?.Invoke(this, new EditorControlEventArgs(editor));

	private void OnFileOpened(EventArgs e)
		=> FileOpened?.Invoke(_currentEditor, e);

	private void OnEditorClosed(IEditorControl editor)
		=> EditorClosed?.Invoke(this, new EditorControlEventArgs(editor));

	private void OnDocumentRenamed(DocumentRenamedEventArgs e)
		=> DocumentRenamed?.Invoke(this, e);
}
