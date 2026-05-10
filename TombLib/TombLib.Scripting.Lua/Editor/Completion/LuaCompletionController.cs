using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Utils;
using TombLib.WPF;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private void CloseCompletionWindow()
		=> _completionController.CloseWindow();

	private void ScheduleCompletionRequest()
		=> _completionController.ScheduleRequest();

	private void CancelPendingCompletionRequest()
		=> _completionController.CancelPendingRequest();

	private Task RequestCompletionAsync(int offset, char? triggerCharacter)
		=> _completionController.RequestAsync(offset, triggerCharacter);

	private bool CanApplyCompletionItem(LuaCompletionItem item)
		=> IsCompletionItemCurrent(item.RequestDocumentVersion, _editorDocumentVersion,
			item.RequestGeneration, _editorRequestGeneration, IsLoaded, IsIntellisenseAvailable());

	private void RebaseOpenCompletionItems()
		=> _completionController.RebaseOpenCompletionItems();

	private static bool IsCompletionItemCurrent(int? requestDocumentVersion,
		int currentDocumentVersion,
		int? requestGeneration,
		int currentGeneration,
		bool isEditorLoaded,
		bool isIntellisenseAvailable)
	{
		if (!isEditorLoaded || !isIntellisenseAvailable)
			return false;

		if (!requestDocumentVersion.HasValue && !requestGeneration.HasValue)
			return true;

		if (!requestDocumentVersion.HasValue || !requestGeneration.HasValue)
			return false;

		return requestDocumentVersion.Value == currentDocumentVersion
			&& requestGeneration.Value == currentGeneration;
	}

	private void ScheduleCloseIfEmpty()
		=> _completionController.ScheduleCloseIfEmpty();

	private sealed class LuaCompletionController
	{
		private const double CompletionRequestDebounceDelayInMilliseconds = 120.0;
		private const int CompletionWindowMinWidth = 420;
		private const int CompletionWindowMaxWidth = 920;
		private const int CompletionWindowHeight = 320;
		private const int CompletionWidthMeasurementSampleCount = 80;
		private const double CompletionToolTipHorizontalOffset = 10.0;
		private const double CompletionToolTipResolveDelayInMilliseconds = 120.0;
		private const double CompletionWindowHorizontalChrome = 52.0;
		private const double CompletionItemIconWidth = 24.0;
		private const double CompletionItemDetailSpacing = 12.0;

		private static readonly Lazy<FieldInfo?> CompletionToolTipFieldAccessor = new(() =>
		{
			FieldInfo? field = typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);

			if (field is null)
				Log?.Debug("AvalonEdit completion tooltip styling is unavailable because the internal tooltip field could not be found.");

			return field;
		});

		private static FieldInfo? CompletionToolTipField => CompletionToolTipFieldAccessor.Value;

		private readonly LuaEditor _editor;
		private readonly DispatcherTimer _completionRequestTimer = new();
		private readonly DispatcherTimer _completionToolTipUpdateTimer = new();
		private int _completionRequestToken;
		private int _completionToolTipUpdateToken;
		private ToolTip? _pendingCompletionToolTip;

		public LuaCompletionController(LuaEditor editor)
		{
			_editor = editor;
		}

		public void InitializeScheduling()
		{
			_completionRequestTimer.Interval = TimeSpan.FromMilliseconds(CompletionRequestDebounceDelayInMilliseconds);
			_completionRequestTimer.Tick -= CompletionRequestTimer_Tick;
			_completionRequestTimer.Tick += CompletionRequestTimer_Tick;

			_completionToolTipUpdateTimer.Interval = TimeSpan.FromMilliseconds(CompletionToolTipResolveDelayInMilliseconds);
			_completionToolTipUpdateTimer.Tick -= CompletionToolTipUpdateTimer_Tick;
			_completionToolTipUpdateTimer.Tick += CompletionToolTipUpdateTimer_Tick;
		}

		public void CloseWindow()
		{
			InvalidateRequests();
			CloseWindowCore();
		}

		public void CloseWindowForRefresh()
			=> CloseWindowCore();

		public void ScheduleRequest()
		{
			_completionRequestTimer.Stop();
			_completionRequestTimer.Start();
		}

		public void CancelPendingRequest()
			=> _completionRequestTimer.Stop();

		public async Task RequestAsync(int offset, char? triggerCharacter)
		{
			CancellationToken cancellationToken = CancellationToken.None;
			int requestToken = ++_completionRequestToken;
			int requestDocumentVersion = _editor._editorDocumentVersion;
			int requestGeneration = _editor._editorRequestGeneration;

			try
			{
				if (!_editor.IsIntellisenseAvailable())
					return;

				var intellisenseProvider = _editor.IntellisenseProvider;

				if (intellisenseProvider is null)
					return;

				_editor.DismissSignatureHelp();
				_editor.CloseDefinitionToolTip(true);

				(int line, int column) = _editor.GetPositionFromOffset(offset);

				IReadOnlyList<LuaCompletionItem> items = await intellisenseProvider
					.GetCompletionItemsAsync(_editor.FilePath, _editor.Text, line, column, triggerCharacter, cancellationToken)
					.ConfigureAwait(true);

				if (!_editor.IsAsyncEditorResultCurrent(cancellationToken, requestToken, _completionRequestToken,
					requestDocumentVersion, requestGeneration))
				{
					return;
				}

				if (items.Count == 0)
				{
					CloseWindow();
					return;
				}

				var completionDataItems = new LuaCompletionData[items.Count];
				LuaThemeBrushSet brushSet = _editor.GetThemeBrushSet();

				for (int i = 0; i < items.Count; i++)
				{
					LuaCompletionItem completionItem = items[i].WithRequestContext(requestDocumentVersion, requestGeneration);
					completionDataItems[i] = new LuaCompletionData(completionItem, brushSet, _editor.CanApplyCompletionItem);
				}

				if (!_editor.IsAsyncEditorResultCurrent(cancellationToken, requestToken, _completionRequestToken,
					requestDocumentVersion, requestGeneration))
				{
					return;
				}

				CloseWindowForRefresh();
				InitializeWindow();
				ResizeWindow(completionDataItems);
				SetWindowOffsets(offset);

				foreach (LuaCompletionData completionDataItem in completionDataItems)
					_editor._completionWindow.CompletionList.CompletionData.Add(completionDataItem);

				if (_editor._completionWindow.CompletionList.CompletionData.Count > 0)
				{
					_editor.ShowCompletionWindow();
					ScheduleInitialSelection();
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				CloseWindow();
				LogEditorFailure("Completion request", exception);
			}
		}

		public void RebaseOpenCompletionItems()
		{
			if (_editor._completionWindow?.CompletionList?.CompletionData is null)
				return;

			for (int i = 0; i < _editor._completionWindow.CompletionList.CompletionData.Count; i++)
			{
				if (_editor._completionWindow.CompletionList.CompletionData[i] is LuaCompletionData completionData)
					completionData.RebaseForCurrentDocument(_editor._editorDocumentVersion, _editor._editorRequestGeneration);
			}
		}

		public Task UpdateTooltipAsync(ToolTip tooltip, int updateToken)
			=> UpdateTooltipCoreAsync(tooltip, updateToken);

		public void ScheduleCloseIfEmpty()
			=> _editor.Dispatcher.BeginInvoke(new Action(() => CloseWindowIfEmpty()), DispatcherPriority.Background);

		public void InvalidateRequests()
			=> _completionRequestToken++;

		private void InitializeWindow()
		{
			_editor.InitializeCompletionWindow(CompletionWindowMinWidth, CompletionWindowHeight);
			LuaCompletionWindowStyle.Apply(_editor._completionWindow, _editor.GetThemeBrushSet());
			StyleTooltip();
			MakeWindowNonActivatable();
		}

		private void CloseWindowCore()
		{
			CancelPendingRequest();
			CancelTooltipUpdate();

			if (_editor._completionWindow is null)
				return;

			if (CompletionToolTipField?.GetValue(_editor._completionWindow) is ToolTip tooltip)
				tooltip.IsOpen = false;

			_editor._completionWindow.Close();
			_editor._completionWindow = null;
		}

		private async void CompletionRequestTimer_Tick(object? sender, EventArgs e)
		{
			_completionRequestTimer.Stop();

			if (!_editor.AutocompleteEnabled || !_editor.IsIntellisenseAvailable())
				return;

			if (!LuaEditorInteractionRules.IsValidAutocompleteContext(_editor.Document, _editor.CaretOffset, triggerCharacter: null))
				return;

			await RequestAsync(_editor.CaretOffset, null).ConfigureAwait(true);
		}

		private void StyleTooltip()
		{
			if (_editor._completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			if (CompletionToolTipField?.GetValue(_editor._completionWindow) is not ToolTip tooltip)
				return;

			tooltip.Background = DefaultToolTipBackground;
			tooltip.BorderBrush = DefaultToolTipBorder;
			tooltip.BorderThickness = new Thickness(0.0);
			tooltip.Padding = new Thickness(0.0);
			tooltip.PlacementTarget = listBox;
			tooltip.Placement = PlacementMode.Right;
			tooltip.HorizontalOffset = CompletionToolTipHorizontalOffset;
			tooltip.StaysOpen = true;

			listBox.SelectionChanged += (s, e) => ScheduleTooltipUpdate(tooltip);
			listBox.PreviewMouseLeftButtonUp += (s, e) => HandleCompletionListClick(listBox, tooltip, e);
		}

		private void HandleCompletionListClick(ListBox listBox, ToolTip tooltip, MouseButtonEventArgs e)
		{
			ListBoxItem? listBoxItem = (e.OriginalSource as DependencyObject)?.FindVisualAncestorOrSelf<ListBoxItem>();

			if (listBoxItem is null)
				return;

			if (!ReferenceEquals(listBox.SelectedItem, listBoxItem.DataContext))
				listBox.SelectedItem = listBoxItem.DataContext;

			listBox.ScrollIntoView(listBoxItem.DataContext);
			ScheduleTooltipUpdate(tooltip);
		}

		private void ScheduleTooltipUpdate(ToolTip tooltip)
		{
			_pendingCompletionToolTip = tooltip;
			_completionToolTipUpdateToken++;
			_completionToolTipUpdateTimer.Stop();
			_completionToolTipUpdateTimer.Start();
		}

		private async void CompletionToolTipUpdateTimer_Tick(object? sender, EventArgs e)
		{
			_completionToolTipUpdateTimer.Stop();

			if (_pendingCompletionToolTip is not ToolTip tooltip)
				return;

			await UpdateTooltipCoreAsync(tooltip, _completionToolTipUpdateToken).ConfigureAwait(true);
		}

		private async Task UpdateTooltipCoreAsync(ToolTip tooltip, int updateToken)
		{
			if (_editor._completionWindow?.CompletionList.ListBox is not ListBox listBox)
				return;

			if (listBox.SelectedItem is not ICompletionData item)
			{
				tooltip.IsOpen = false;
				return;
			}

			try
			{
				object? description = item.Description;

				if (description is not null)
					ApplyTooltipContent(tooltip, description);
				else
					tooltip.IsOpen = false;

				if (item is LuaCompletionData luaCompletionData && luaCompletionData.CanResolve)
				{
					if (updateToken != _completionToolTipUpdateToken)
						return;

					object? resolvedDescription = await luaCompletionData.GetDescriptionAsync().ConfigureAwait(true);

					if (updateToken != _completionToolTipUpdateToken)
						return;

					if (_editor._completionWindow?.CompletionList.ListBox is not ListBox currentListBox
						|| !ReferenceEquals(currentListBox, listBox)
						|| !ReferenceEquals(currentListBox.SelectedItem, item))
					{
						return;
					}

					if (resolvedDescription is not null)
						ApplyTooltipContent(tooltip, resolvedDescription);
					else
						tooltip.IsOpen = false;
				}
			}
			catch (Exception exception)
			{
				tooltip.IsOpen = false;
				LogEditorFailure("Completion tooltip update", exception);
			}
		}

		private void ResizeWindow(LuaCompletionData[] completionDataItems)
		{
			if (_editor._completionWindow is null || completionDataItems.Length == 0)
				return;

			double requiredWidth = CompletionWindowMinWidth;
			int measurementCount = Math.Min(completionDataItems.Length, CompletionWidthMeasurementSampleCount);
			var textWidthCache = new Dictionary<string, double>(StringComparer.Ordinal);

			for (int i = 0; i < measurementCount; i++)
				requiredWidth = Math.Max(requiredWidth, MeasureItemWidth(completionDataItems[i], textWidthCache));

			_editor._completionWindow.Width = Math.Max(
				CompletionWindowMinWidth,
				Math.Min(CompletionWindowMaxWidth, requiredWidth + CompletionWindowHorizontalChrome));
		}

		private double MeasureItemWidth(LuaCompletionData completionData, IDictionary<string, double> textWidthCache)
		{
			double width = CompletionItemIconWidth + MeasureTextWidth(completionData.DisplayText, textWidthCache);

			if (!string.IsNullOrWhiteSpace(completionData.DisplayDetail))
				width += CompletionItemDetailSpacing + MeasureTextWidth(completionData.DisplayDetail, textWidthCache);

			return width;
		}

		private double MeasureTextWidth(string text, IDictionary<string, double> textWidthCache)
		{
			if (string.IsNullOrWhiteSpace(text))
				return 0.0;

			if (textWidthCache.TryGetValue(text, out double cachedWidth))
				return cachedWidth;

			double pixelsPerDip = VisualTreeHelper.GetDpi(_editor).PixelsPerDip;

			var formattedText = new FormattedText(
				text,
				CultureInfo.CurrentUICulture,
				FlowDirection.LeftToRight,
				new Typeface(_editor.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
				_editor.FontSize,
				_editor.Foreground,
				pixelsPerDip);

			double width = formattedText.WidthIncludingTrailingWhitespace;
			textWidthCache[text] = width;
			return width;
		}

		private static void ApplyTooltipContent(ToolTip tooltip, object content)
		{
			tooltip.Content = content;

			if (!tooltip.IsOpen)
			{
				tooltip.IsOpen = true;
			}
			else
			{
				tooltip.InvalidateMeasure();
				tooltip.InvalidateVisual();
			}
		}

		public void CancelTooltipUpdate()
		{
			_completionToolTipUpdateToken++;
			_pendingCompletionToolTip = null;
			_completionToolTipUpdateTimer.Stop();
		}

		private void ScheduleInitialSelection()
			=> _editor.Dispatcher.BeginInvoke(new Action(SelectInitialItem), DispatcherPriority.ContextIdle);

		private void SelectInitialItem()
		{
			if (_editor._completionWindow is null)
				return;

			_editor._completionWindow.CompletionList.SelectItem(GetCompletionWindowQuery());
			CloseWindowIfEmpty();
		}

		private void MakeWindowNonActivatable()
		{
			_editor._completionWindow.SourceInitialized += (s, e) =>
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

		private bool CloseWindowIfEmpty()
		{
			if (_editor._completionWindow is null)
				return false;

			ListBox listBox = _editor._completionWindow.CompletionList.ListBox;

			if (listBox?.HasItems != false)
				return false;

			CloseWindow();
			return true;
		}

		private string GetCompletionWindowQuery()
		{
			if (_editor._completionWindow is null || _editor.Document is null)
				return string.Empty;

			int startOffset = Math.Max(0, Math.Min(_editor._completionWindow.StartOffset, _editor.Document.TextLength));
			int endOffset = Math.Max(startOffset, Math.Min(_editor._completionWindow.EndOffset, _editor.Document.TextLength));

			return endOffset > startOffset
				? _editor.Document.GetText(startOffset, endOffset - startOffset)
				: string.Empty;
		}

		private void SetWindowOffsets(int offset)
		{
			int startOffset = Math.Max(0, Math.Min(offset, _editor.Document.TextLength));

			while (startOffset > 0)
			{
				char currentChar = _editor.Document.GetCharAt(startOffset - 1);

				if (LuaLineParser.IsIdentifierCharacter(currentChar))
					startOffset--;
				else
					break;
			}

			_editor._completionWindow.StartOffset = startOffset;
			_editor._completionWindow.EndOffset = Math.Max(0, Math.Min(offset, _editor.Document.TextLength));
		}
	}
}