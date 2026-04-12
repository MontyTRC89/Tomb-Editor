using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Utils;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private async void TextEditor_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && (_completionWindow is not null || _signaturePopup.IsOpen || _specialToolTip.IsOpen))
			{
				CloseCompletionWindow();
				DismissTransientToolTips();
				e.Handled = true;
				return;
			}

			if (_signaturePopup.IsOpen && (e.Key == Key.Back || e.Key == Key.Delete))
				ScheduleSignatureHelpRefresh();

			if (e.Key == Key.F12)
			{
				if (await TryNavigateToDefinitionAsync(CaretOffset, CancellationToken.None).ConfigureAwait(true))
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

			if (await TryNavigateToDefinitionAsync(hoveredOffset, CancellationToken.None).ConfigureAwait(true))
				e.Handled = true;
		}

		private async Task<bool> TryNavigateToDefinitionAsync(int offset, CancellationToken cancellationToken)
		{
			if (!IsIntellisenseAvailable())
				return false;

			try
			{
				if (!LuaEditorInteractionRules.TryGetDefinitionStartOffset(Document, offset, out int definitionOffset))
					return false;

				(int line, int column) = GetPositionFromOffset(definitionOffset);

				LuaDefinitionLocation? definitionLocation = await IntellisenseProvider
					.GetDefinitionAsync(FilePath, Text, line, column, cancellationToken)
					.ConfigureAwait(true);

				if (definitionLocation is null)
					return false;

				DefinitionNavigationRequested?.Invoke(definitionLocation);
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Debug.WriteLine($"[Lua] Go to definition failed: {exception}");
				return false;
			}
		}

		public async void NavigateToDefinitionAtCaretAsync()
			=> await TryNavigateToDefinitionAsync(CaretOffset, CancellationToken.None).ConfigureAwait(true);
	}
}