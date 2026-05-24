#nullable enable

using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Navigation;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaDocumentLifecycleCoordinator : IDisposable
{
	private readonly EditorTabControl _editorTabControl;
	private readonly ILuaIntellisenseProvider _intellisenseProvider;
	private readonly LuaTrackedDocumentStateService _trackedDocumentStateService;
	private readonly Action _selectedIndexChanged;
	private readonly EventHandler _statusChanged;
	private readonly EventHandler _textChanged;
	private readonly Action<TextDefinitionLocation> _definitionNavigationRequested;
	private readonly Action _editorOpened;
	private readonly Action _currentEditorRenamed;

	public LuaDocumentLifecycleCoordinator(
		EditorTabControl editorTabControl,
		ILuaIntellisenseProvider intellisenseProvider,
		LuaTrackedDocumentStateService trackedDocumentStateService,
		Action selectedIndexChanged,
		EventHandler statusChanged,
		EventHandler textChanged,
		Action<TextDefinitionLocation> definitionNavigationRequested,
		Action editorOpened,
		Action currentEditorRenamed)
	{
		_editorTabControl = editorTabControl ?? throw new ArgumentNullException(nameof(editorTabControl));
		_intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));
		_trackedDocumentStateService = trackedDocumentStateService ?? throw new ArgumentNullException(nameof(trackedDocumentStateService));
		_selectedIndexChanged = selectedIndexChanged ?? throw new ArgumentNullException(nameof(selectedIndexChanged));
		_statusChanged = statusChanged ?? throw new ArgumentNullException(nameof(statusChanged));
		_textChanged = textChanged ?? throw new ArgumentNullException(nameof(textChanged));
		_definitionNavigationRequested = definitionNavigationRequested ?? throw new ArgumentNullException(nameof(definitionNavigationRequested));
		_editorOpened = editorOpened ?? throw new ArgumentNullException(nameof(editorOpened));
		_currentEditorRenamed = currentEditorRenamed ?? throw new ArgumentNullException(nameof(currentEditorRenamed));
	}

	public void Attach()
	{
		Detach();

		_editorTabControl.FileOpened += EditorTabControl_FileOpened;
		_editorTabControl.SelectedIndexChanged += EditorTabControl_SelectedIndexChanged;
		_editorTabControl.DocumentRenamed += EditorTabControl_DocumentRenamed;
	}

	public void Detach()
	{
		_editorTabControl.FileOpened -= EditorTabControl_FileOpened;
		_editorTabControl.SelectedIndexChanged -= EditorTabControl_SelectedIndexChanged;
		_editorTabControl.DocumentRenamed -= EditorTabControl_DocumentRenamed;

		foreach (TabPage tabPage in _editorTabControl.TabPages)
		{
			if (_editorTabControl.GetEditorOfTab(tabPage) is LuaEditor editor)
				DetachEditor(editor);
		}
	}

	public void Dispose()
		=> Detach();

	private void EditorTabControl_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not LuaEditor editor)
			return;

		AttachEditor(editor);
		_trackedDocumentStateService.OpenDocument(editor);
		_editorOpened();
	}

	private void EditorTabControl_SelectedIndexChanged(object? sender, EventArgs e)
		=> _selectedIndexChanged();

	private void EditorTabControl_DocumentRenamed(object? sender, DocumentRenamedEventArgs e)
	{
		LuaEditor? editor = null;

		foreach (TabPage tabPage in _editorTabControl.FindTabPagesOfFile(e.NewFilePath))
		{
			if (_editorTabControl.GetEditorOfTab(tabPage) is LuaEditor luaEditor)
			{
				editor = luaEditor;
				break;
			}
		}

		if (editor is null)
			return;

		_trackedDocumentStateService.RenameDocument(e.OldFilePath, e.NewFilePath, editor);

		if (ReferenceEquals(_editorTabControl.CurrentEditor, editor))
			_currentEditorRenamed();
	}

	private void Editor_TextChangedDelayed(object? sender, EventArgs e)
	{
		if (sender is LuaEditor editor)
			_trackedDocumentStateService.UpdateDocument(editor);
	}

	private void AttachEditor(LuaEditor editor)
	{
		editor.IntellisenseProvider = _intellisenseProvider;
		editor.DefinitionNavigationRequested -= _definitionNavigationRequested;
		editor.DefinitionNavigationRequested += _definitionNavigationRequested;
		editor.StatusChanged -= _statusChanged;
		editor.StatusChanged += _statusChanged;
		editor.TextChanged -= _textChanged;
		editor.TextChanged += _textChanged;
		editor.TextChangedDelayed -= Editor_TextChangedDelayed;
		editor.TextChangedDelayed += Editor_TextChangedDelayed;
	}

	private void DetachEditor(LuaEditor editor)
	{
		editor.DefinitionNavigationRequested -= _definitionNavigationRequested;
		editor.StatusChanged -= _statusChanged;
		editor.TextChanged -= _textChanged;
		editor.TextChangedDelayed -= Editor_TextChangedDelayed;
	}
}