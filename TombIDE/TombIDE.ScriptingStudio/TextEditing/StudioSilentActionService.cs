#nullable enable

using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.TextEditing;

internal readonly record struct SilentActionFileState(
	string FilePath,
	EditorType EditorType,
	bool OpenSourceView,
	bool WasAlreadyOpen,
	bool WasContentChanged);

internal readonly record struct SilentActionCompletion(
	IEditorControl? Editor,
	bool SaveAffectedFile,
	bool CloseAffectedTab);

internal sealed class StudioSilentActionService
{
	private readonly IEditorDocumentController _documentController;
	private readonly IScriptingHostOperations _hostOperations;

	public StudioSilentActionService(IEditorDocumentController documentController, IScriptingHostOperations hostOperations)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_hostOperations = hostOperations ?? throw new ArgumentNullException(nameof(hostOperations));
	}

	public IEditorControl? RememberSelectedEditor() => _documentController.CurrentEditor;

	public SilentActionFileState CaptureFileState(string filePath, EditorType editorType = EditorType.Default)
	{
		IEditorControl? editor = _documentController.FindEditor(filePath, editorType);
		bool wasAlreadyOpen = editor is not null;
		bool wasContentChanged = editor is not null && editor.IsContentChanged;

		return new SilentActionFileState(filePath, editorType, false, wasAlreadyOpen, wasContentChanged);
	}

	public SilentActionFileState CaptureSourceFileState(string filePath)
	{
		IEditorControl? editor = _documentController.FindSourceEditor(filePath);
		bool wasAlreadyOpen = editor is not null;
		bool wasContentChanged = editor is not null && editor.IsContentChanged;

		return new SilentActionFileState(filePath, EditorType.Default, true, wasAlreadyOpen, wasContentChanged);
	}

	public SilentActionCompletion CreateCompletion(
		SilentActionFileState fileState,
		bool saveAffectedFile = true,
		bool closeAffectedTab = true)
	{
		IEditorControl? editor = fileState.OpenSourceView
			? _documentController.FindSourceEditor(fileState.FilePath)
			: _documentController.FindEditor(fileState.FilePath, fileState.EditorType);

		return new SilentActionCompletion(
			editor,
			saveAffectedFile && !fileState.WasContentChanged,
			closeAffectedTab && !fileState.WasAlreadyOpen);
	}

	public void Complete(IEditorControl? previousEditor, bool indicateChange, params SilentActionCompletion[] completions)
	{
		if (indicateChange && _documentController.CurrentEditor is { } currentEditor)
		{
			currentEditor.LastModified = DateTime.Now;
			_hostOperations.IndicateExternalChange();
		}

		foreach (SilentActionCompletion completion in completions)
		{
			if (completion.SaveAffectedFile && completion.Editor is not null && _documentController.ContainsEditor(completion.Editor))
				_documentController.SaveFile(completion.Editor);
		}

		foreach (SilentActionCompletion completion in completions)
		{
			if (completion.CloseAffectedTab && completion.Editor is not null && _documentController.ContainsEditor(completion.Editor))
				_documentController.TryCloseEditor(completion.Editor);
		}

		_documentController.EnsureTabFileSynchronization();

		if (previousEditor is not null && _documentController.ContainsEditor(previousEditor))
			_documentController.ActivateEditor(previousEditor);
	}
}
