using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Windows.Input;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	// CompletionWindow

	/// <summary>
	/// Initializes the completion window with the given size.
	/// </summary>
	/// <param name="width">The width of the completion window.</param>
	/// <param name="height">The height of the completion window.</param>
	public void InitializeCompletionWindow(int width = 300, int height = 300)
		=> _completionWindowCoordinator.Initialize(width, height);

	/// <summary>
	/// Shows the completion window at the caret position.
	/// </summary>
	public void ShowCompletionWindow()
		=> _completionWindowCoordinator.Show();

	internal CompletionWindow? ActiveCompletionWindow => _completionWindowCoordinator.ActiveWindow;

	/// <summary>
	/// Gets whether the completion window is currently open.
	/// </summary>
	protected bool IsCompletionWindowOpen => _completionWindowCoordinator.IsWindowOpen;

	internal void CloseSharedCompletionWindow()
		=> _completionWindowCoordinator.Close();

	/// <summary>
	/// Handles Ctrl+Space to trigger completion when completion is enabled.
	/// </summary>
	/// <param name="e">The text composition event to inspect.</param>
	/// <param name="onTriggered">The action invoked when completion should be triggered.</param>
	/// <returns>True if the input was handled as a completion trigger; otherwise false.</returns>
	protected bool TryHandleCtrlSpaceCompletion(TextCompositionEventArgs e, Action onTriggered)
	{
		if (!CompletionEnabled || !EditorCompletionTriggerHelper.IsCtrlSpaceInput(e.Text, Keyboard.Modifiers))
			return false;

		if (!IsCompletionWindowOpen)
			onTriggered();

		e.Handled = true;
		return true;
	}
}
