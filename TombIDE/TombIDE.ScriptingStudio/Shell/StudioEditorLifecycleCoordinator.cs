#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioEditorLifecycleCoordinator : IEditorLifecycleService
{
	private readonly IEditorDocumentController _documentController;
	private readonly IMessenger _messenger;
	private readonly Action<IEditorControl> _applyUserSettings;
	private readonly Func<UICommand, bool> _canExecuteCommand;
	private readonly Action<UICommand> _executeCommand;
	private readonly IShortcutBindingService _shortcutBindings;

	public StudioEditorLifecycleCoordinator(
		IEditorDocumentController documentController,
		IMessenger messenger,
		Action<IEditorControl> applyUserSettings,
		Action<UICommand> executeCommand,
		Func<UICommand, bool> canExecuteCommand,
		IShortcutBindingService shortcutBindings)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
		_applyUserSettings = applyUserSettings ?? throw new ArgumentNullException(nameof(applyUserSettings));
		_canExecuteCommand = canExecuteCommand ?? throw new ArgumentNullException(nameof(canExecuteCommand));
		_executeCommand = executeCommand ?? throw new ArgumentNullException(nameof(executeCommand));
		_shortcutBindings = shortcutBindings ?? throw new ArgumentNullException(nameof(shortcutBindings));
	}

	public void Attach()
	{
		Detach();
		_documentController.FileOpened += DocumentController_FileOpened;
	}

	public void Detach()
	{
		_documentController.FileOpened -= DocumentController_FileOpened;

		foreach (IEditorControl editor in _documentController.GetOpenEditors())
			DetachEditor(editor);
	}

	public void Dispose()
		=> Detach();

	private void DocumentController_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl editor)
			return;

		AttachEditor(editor);
		_applyUserSettings(editor);
		_messenger.Send(new ShellUiRefreshMessage());
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
		=> _messenger.Send(new CommandStateRefreshMessage());

	private void TextEditor_TextChanged(object? sender, EventArgs e)
		=> _messenger.Send(new CommandStateRefreshMessage());

	private void TextEditor_KeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
	{
		ShortcutKey? shortcut = ShortcutKey.FromKeyEventArgs(e);

		if (shortcut is null)
			return;

		if (!_shortcutBindings.TryGetCommand(shortcut.Value, out UICommand command) || !_canExecuteCommand(command))
			return;

		_executeCommand(command);
		e.Handled = true;
	}
}
