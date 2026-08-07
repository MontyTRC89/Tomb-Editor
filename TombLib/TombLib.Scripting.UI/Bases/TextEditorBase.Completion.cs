using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region CompletionWindow

	public void InitializeCompletionWindow(int width = 300, int height = 300)
		=> _completionWindowCoordinator.Initialize(width, height);

	public void ShowCompletionWindow()
		=> _completionWindowCoordinator.Show();

	internal CompletionWindow? ActiveCompletionWindow => _completionWindowCoordinator.ActiveWindow;

	protected bool IsCompletionWindowOpen => _completionWindowCoordinator.IsWindowOpen;

	internal void CloseSharedCompletionWindow()
		=> _completionWindowCoordinator.Close();

	protected void CloseCompletionWindowCore()
		=> _completionWindowCoordinator.Close();

	protected bool TryOpenCompletionWindow(IEnumerable<ICompletionData> items,
		int? startOffset = null,
		int? endOffset = null,
		int width = 300,
		int height = 300)
		=> _completionWindowCoordinator.TryOpen(items, startOffset, endOffset, width, height);

	protected bool TryHandleCtrlSpaceCompletion(TextCompositionEventArgs e, Action onTriggered)
	{
		if (!AutocompleteEnabled || !EditorCompletionTriggerHelper.IsCtrlSpaceInput(e.Text, Keyboard.Modifiers))
			return false;

		if (!IsCompletionWindowOpen)
			onTriggered();

		e.Handled = true;
		return true;
	}

	#endregion CompletionWindow
}
