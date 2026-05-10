using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private async void TextEditor_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape && (_completionWindow is not null || _signatureHelpController.IsVisible || _specialToolTip.IsOpen))
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
			e.Handled = true;
			return;
		}

		if (_signatureHelpController.IsVisible && (e.Key == Key.Back || e.Key == Key.Delete))
			ScheduleSignatureHelpRefresh();

		if (e.Key == Key.F12)
		{
			if (await _definitionNavigationController.TryNavigateAsync(CaretOffset, CancellationToken.None).ConfigureAwait(true))
				e.Handled = true;
		}
	}

	private void TextEditor_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
		}
	}

	private async void TextEditor_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
	{
		if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || e.ChangedButton != MouseButton.Left)
			return;

		int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

		if (hoveredOffset == -1)
			return;

		if (await _definitionNavigationController.TryNavigateAsync(hoveredOffset, CancellationToken.None).ConfigureAwait(true))
			e.Handled = true;
	}

	/// <summary>
	/// Attempts to resolve and navigate to the symbol definition at the current caret position.
	/// </summary>
	/// <returns>A task that completes once the navigation attempt finishes.</returns>
	public Task NavigateToDefinitionAtCaretAsync()
		=> _definitionNavigationController.TryNavigateAsync(CaretOffset, CancellationToken.None);
}
