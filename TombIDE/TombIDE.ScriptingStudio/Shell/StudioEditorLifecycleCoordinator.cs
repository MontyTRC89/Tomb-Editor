#nullable enable

using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioEditorLifecycleCoordinator : IDisposable
{
	private readonly EditorTabControl _editorTabControl;
	private readonly Action<IEditorControl> _applyUserSettings;
	private readonly Func<UICommand, bool> _canExecuteCommand;
	private readonly Action _updateUi;
	private readonly Action _updateUndoRedoSaveStates;
	private readonly Action<UICommand> _executeCommand;
	private readonly StudioShortcutBindingService _shortcutBindings;

	public StudioEditorLifecycleCoordinator(
		EditorTabControl editorTabControl,
		Action<IEditorControl> applyUserSettings,
		Action updateUi,
		Action updateUndoRedoSaveStates,
		Action<UICommand> executeCommand,
		Func<UICommand, bool> canExecuteCommand,
		StudioShortcutBindingService shortcutBindings)
	{
		_editorTabControl = editorTabControl ?? throw new ArgumentNullException(nameof(editorTabControl));
		_applyUserSettings = applyUserSettings ?? throw new ArgumentNullException(nameof(applyUserSettings));
		_canExecuteCommand = canExecuteCommand ?? throw new ArgumentNullException(nameof(canExecuteCommand));
		_updateUi = updateUi ?? throw new ArgumentNullException(nameof(updateUi));
		_updateUndoRedoSaveStates = updateUndoRedoSaveStates ?? throw new ArgumentNullException(nameof(updateUndoRedoSaveStates));
		_executeCommand = executeCommand ?? throw new ArgumentNullException(nameof(executeCommand));
		_shortcutBindings = shortcutBindings ?? throw new ArgumentNullException(nameof(shortcutBindings));
	}

	public void Attach()
	{
		Detach();
		_editorTabControl.FileOpened += EditorTabControl_FileOpened;
	}

	public void Detach()
	{
		_editorTabControl.FileOpened -= EditorTabControl_FileOpened;

		foreach (TabPage tabPage in _editorTabControl.TabPages)
		{
			if (_editorTabControl.GetEditorOfTab(tabPage) is IEditorControl editor)
				DetachEditor(editor);
		}
	}

	public void Dispose()
		=> Detach();

	private void EditorTabControl_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl editor)
			return;

		AttachEditor(editor);
		_applyUserSettings(editor);
		_updateUi();
	}

	private void AttachEditor(IEditorControl editor)
	{
		editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;
		editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

		if (editor is not TextEditorBase textEditor)
			return;

		textEditor.KeyDown -= TextEditor_KeyDown;
		textEditor.KeyDown += TextEditor_KeyDown;
		textEditor.TextChanged -= TextEditor_TextChanged;
		textEditor.TextChanged += TextEditor_TextChanged;
	}

	private void DetachEditor(IEditorControl editor)
	{
		editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;

		if (editor is not TextEditorBase textEditor)
			return;

		textEditor.KeyDown -= TextEditor_KeyDown;
		textEditor.TextChanged -= TextEditor_TextChanged;
	}

	private void Editor_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		=> _updateUndoRedoSaveStates();

	private void TextEditor_TextChanged(object? sender, EventArgs e)
		=> _updateUndoRedoSaveStates();

	private void TextEditor_KeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
	{
		if (!_shortcutBindings.TryGetCommand(e, out UICommand command) || !_canExecuteCommand(command))
			return;

		_executeCommand(command);
		e.Handled = true;
	}
}