using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Utils;
using TombLib.WPF;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private static readonly FieldInfo CompletionToolTipField =
			typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);

		private static bool _completionToolTipFieldLoggedMissing;

		private CancellationTokenSource _completionCancellationTokenSource;
		private CancellationTokenSource _completionToolTipCancellationTokenSource;
		private int _completionRequestToken;
		private int _completionToolTipUpdateToken;

		private void CloseCompletionWindow()
		{
			CancelCompletionToolTipUpdate();

			if (_completionWindow is null)
				return;

			if (CompletionToolTipField?.GetValue(_completionWindow) is ToolTip tooltip)
				tooltip.IsOpen = false;

			_completionWindow.Close();
			_completionWindow = null;
		}

		private void InitializeLuaCompletionWindow()
		{
			InitializeCompletionWindow(420, 320);
			LuaCompletionWindowStyle.Apply(_completionWindow);
			StyleCompletionTooltip();
			MakeCompletionWindowNonActivatable();
		}

		private async Task RequestCompletionAsync(int offset, char? triggerCharacter)
		{
			CancellationToken cancellationToken = ResetCancellationTokenSource(ref _completionCancellationTokenSource);
			int requestToken = ++_completionRequestToken;

			try
			{
				if (!IsIntellisenseAvailable())
					return;

				(int line, int column) = GetPositionFromOffset(offset);
				var items = await IntellisenseProvider
					.GetCompletionItemsAsync(FilePath, Text, line, column, triggerCharacter, cancellationToken)
					.ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
					return;

				if (items is null || items.Count == 0)
				{
					CloseCompletionWindow();
					return;
				}

				var completionDataItems = new LuaCompletionData[items.Count];

				for (int i = 0; i < items.Count; i++)
					completionDataItems[i] = new LuaCompletionData(items[i]);

				if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
					return;

				CloseCompletionWindow();

				InitializeLuaCompletionWindow();
				SetCompletionWindowOffsets(offset);

				foreach (LuaCompletionData completionDataItem in completionDataItems)
					_completionWindow.CompletionList.CompletionData.Add(completionDataItem);

				if (_completionWindow.CompletionList.CompletionData.Count > 0)
				{
					ShowCompletionWindow();
					ScheduleInitialSelection();
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				CloseCompletionWindow();
				WriteDebugFailure("Completion request", exception);
			}
		}

		private bool IsValidAutocompleteContext(int offset, char? triggerCharacter)
		{
			if (offset <= 0 || Document is null || Document.TextLength == 0)
				return false;

			if (IsInsideCommentOrString(offset))
				return false;

			if (triggerCharacter is '.' || triggerCharacter is ':')
				return true;

			char typedCharacter = Document.GetCharAt(offset - 1);

			if (!IsIdentifierCharacter(typedCharacter))
				return false;

			if (offset >= 2)
			{
				char previousCharacter = Document.GetCharAt(offset - 2);

				if (previousCharacter == '.')
					return false;
			}

			return true;
		}

		private bool IsInsideCommentOrString(int offset)
		{
			DocumentLine currentLine = Document.GetLineByOffset(Math.Max(0, Math.Min(offset, Document.TextLength)));
			int lineStart = currentLine.Offset;
			int inspectedLength = Math.Max(0, Math.Min(offset, currentLine.EndOffset) - lineStart);
			string lineText = Document.GetText(lineStart, inspectedLength);

			return LuaLineParser.IsInsideCommentOrString(lineText);
		}

		private void StyleCompletionTooltip()
		{
			if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			if (CompletionToolTipField?.GetValue(_completionWindow) is not ToolTip tooltip)
			{
				if (!_completionToolTipFieldLoggedMissing)
				{
					_completionToolTipFieldLoggedMissing = true;
					Debug.WriteLine("[Lua] AvalonEdit completion tooltip styling is unavailable because the internal tooltip field could not be found.");
				}

				return;
			}

			tooltip.Background = DefaultToolTipBackground;
			tooltip.BorderBrush = DefaultToolTipBorder;
			tooltip.BorderThickness = new Thickness(0.0);
			tooltip.Padding = new Thickness(0.0);
			tooltip.PlacementTarget = listBox;
			tooltip.Placement = PlacementMode.Right;
			tooltip.HorizontalOffset = 10.0;
			tooltip.StaysOpen = true;

			listBox.SelectionChanged += (s, e) => ScheduleCompletionTooltipUpdate(tooltip);
			listBox.PreviewMouseLeftButtonUp += (s, e) => HandleCompletionListClick(listBox, tooltip, e);
		}

		private void HandleCompletionListClick(ListBox listBox, ToolTip tooltip, MouseButtonEventArgs e)
		{
			ListBoxItem listBoxItem = (e.OriginalSource as DependencyObject)?.FindVisualAncestorOrSelf<ListBoxItem>();

			if (listBoxItem is null)
				return;

			if (!ReferenceEquals(listBox.SelectedItem, listBoxItem.DataContext))
				listBox.SelectedItem = listBoxItem.DataContext;

			listBox.ScrollIntoView(listBoxItem.DataContext);
			ScheduleCompletionTooltipUpdate(tooltip);
		}

		private void ScheduleCompletionTooltipUpdate(ToolTip tooltip)
		{
			CancellationToken cancellationToken = ResetCancellationTokenSource(ref _completionToolTipCancellationTokenSource);
			int updateToken = ++_completionToolTipUpdateToken;

			Dispatcher.BeginInvoke(
				new Action(() => _ = UpdateCompletionTooltipAsync(tooltip, cancellationToken, updateToken)),
				DispatcherPriority.Background);
		}

		private async Task UpdateCompletionTooltipAsync(ToolTip tooltip, CancellationToken cancellationToken, int updateToken)
		{
			if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			ICompletionData item = listBox.SelectedItem as ICompletionData;

			if (item is null)
			{
				tooltip.IsOpen = false;
				return;
			}

			try
			{
				object description = item.Description;

				if (description is not null)
					ApplyCompletionToolTipContent(tooltip, description);
				else
					tooltip.IsOpen = false;

				if (item is LuaCompletionData luaCompletionData && luaCompletionData.CanResolve)
				{
					object resolvedDescription = await luaCompletionData.GetDescriptionAsync(cancellationToken).ConfigureAwait(true);

					if (cancellationToken.IsCancellationRequested || updateToken != _completionToolTipUpdateToken)
						return;

					if (_completionWindow?.CompletionList.ListBox is not ListBox currentListBox
						|| !ReferenceEquals(currentListBox, listBox)
						|| !ReferenceEquals(currentListBox.SelectedItem, item))
					{
						return;
					}

					if (resolvedDescription is not null)
						ApplyCompletionToolTipContent(tooltip, resolvedDescription);
					else
						tooltip.IsOpen = false;
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				tooltip.IsOpen = false;
				WriteDebugFailure("Completion tooltip update", exception);
			}
		}

		private static void ApplyCompletionToolTipContent(ToolTip tooltip, object content)
		{
			tooltip.Content = content;
			if (!tooltip.IsOpen)
				tooltip.IsOpen = true;
			else
			{
				tooltip.InvalidateMeasure();
				tooltip.InvalidateVisual();
			}
		}

		private void CancelCompletionToolTipUpdate()
		{
			CancelAndDispose(ref _completionToolTipCancellationTokenSource);
			_completionToolTipUpdateToken++;
		}

		private void ScheduleCloseIfEmpty()
			=> Dispatcher.BeginInvoke(new Action(() => CloseCompletionWindowIfEmpty()), DispatcherPriority.Background);

		private void ScheduleInitialSelection()
			=> Dispatcher.BeginInvoke(new Action(SelectInitialItem), DispatcherPriority.ContextIdle);

		private void SelectInitialItem()
		{
			if (_completionWindow is null)
				return;

			_completionWindow.CompletionList.SelectItem(GetCompletionWindowQuery());
			CloseCompletionWindowIfEmpty();
		}

		private void MakeCompletionWindowNonActivatable()
		{
			_completionWindow.SourceInitialized += (s, e) =>
			{
				if (s is Window window && PresentationSource.FromVisual(window) is HwndSource source)
					source.AddHook(CompletionWindowWndProc);
			};
		}

		private static IntPtr CompletionWindowWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_MOUSEACTIVATE = 0x0021;
			const int MA_NOACTIVATE = 3;

			if (msg == WM_MOUSEACTIVATE)
			{
				handled = true;
				return new IntPtr(MA_NOACTIVATE);
			}

			return IntPtr.Zero;
		}

		private bool CloseCompletionWindowIfEmpty()
		{
			if (_completionWindow is null)
				return false;

			ListBox listBox = _completionWindow.CompletionList.ListBox;

			if (listBox is null || listBox.HasItems)
				return false;

			CloseCompletionWindow();
			return true;
		}

		private string GetCompletionWindowQuery()
		{
			if (_completionWindow is null || Document is null)
				return string.Empty;

			int startOffset = Math.Max(0, Math.Min(_completionWindow.StartOffset, Document.TextLength));
			int endOffset = Math.Max(startOffset, Math.Min(_completionWindow.EndOffset, Document.TextLength));

			return endOffset > startOffset
				? Document.GetText(startOffset, endOffset - startOffset)
				: string.Empty;
		}

		private void SetCompletionWindowOffsets(int offset)
		{
			int startOffset = Math.Max(0, Math.Min(offset, Document.TextLength));

			while (startOffset > 0)
			{
				char currentChar = Document.GetCharAt(startOffset - 1);

				if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
					startOffset--;
				else
					break;
			}

			_completionWindow.StartOffset = startOffset;
			_completionWindow.EndOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
		}
	}
}