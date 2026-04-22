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
using TombLib.Scripting.Lua.Utils;
using TombLib.WPF;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private const int CompletionWindowMinWidth = 420;
	private const int CompletionWindowMaxWidth = 920;
	private const int CompletionWindowHeight = 320;
	private const int CompletionWidthMeasurementSampleCount = 80;
	private const double CompletionToolTipHorizontalOffset = 10.0;
	private const double CompletionToolTipResolveDelayInMilliseconds = 120.0;
	private const double CompletionWindowHorizontalChrome = 52.0;
	private const double CompletionItemIconWidth = 24.0;
	private const double CompletionItemDetailSpacing = 12.0;

	// Reflection target: AvalonEdit's CompletionWindow keeps the documentation tooltip in a private field named "toolTip".
	// Verified against the AvalonEdit version pinned in TombLib.Scripting.Lua.csproj.
	private static readonly Lazy<FieldInfo?> CompletionToolTipFieldAccessor = new(() =>
	{
		FieldInfo? field = typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);

		if (field is null)
			Log?.Debug("AvalonEdit completion tooltip styling is unavailable because the internal tooltip field could not be found.");

		return field;
	});

	private static FieldInfo? CompletionToolTipField => CompletionToolTipFieldAccessor.Value;

	private CancellationTokenSource? _completionCancellationTokenSource;
	private CancellationTokenSource? _completionToolTipCancellationTokenSource;
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
		InitializeCompletionWindow(CompletionWindowMinWidth, CompletionWindowHeight);
		LuaCompletionWindowStyle.Apply(_completionWindow, GetThemeBrushSet());
		StyleCompletionTooltip();
		MakeCompletionWindowNonActivatable();
	}

	private async Task RequestCompletionAsync(int offset, char? triggerCharacter)
	{
		CancellationToken cancellationToken = ResetCancellationTokenSource(ref _completionCancellationTokenSource);
		int requestToken = ++_completionRequestToken;

		try
		{
			// Validate the request context up front, then dismiss competing transient UI before asking LuaLS.
			if (!IsIntellisenseAvailable())
				return;

			DismissSignatureHelp();
			CloseDefinitionToolTip(true);

			(int Line, int Column) = GetPositionFromOffset(offset);

			var items = await IntellisenseProvider
				.GetCompletionItemsAsync(FilePath, Text, Line, Column, triggerCharacter, cancellationToken)
				.ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
				return;

			if (items is null || items.Count == 0)
			{
				CloseCompletionWindow();
				return;
			}

			// Materialize the provider response into AvalonEdit completion rows using the current theme.
			var completionDataItems = new LuaCompletionData[items.Count];
			var brushSet = GetThemeBrushSet();

			for (int i = 0; i < items.Count; i++)
				completionDataItems[i] = new LuaCompletionData(items[i], brushSet);

			if (cancellationToken.IsCancellationRequested || requestToken != _completionRequestToken)
				return;

			// Recreate the popup from scratch so stale selection and tooltip state never leaks between requests.
			CloseCompletionWindow();

			InitializeLuaCompletionWindow();
			ResizeCompletionWindow(completionDataItems);
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
		{ }
		catch (Exception exception)
		{
			CloseCompletionWindow();
			LogEditorFailure("Completion request", exception);
		}
	}

	private void StyleCompletionTooltip()
	{
		if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
			return;

		if (CompletionToolTipField?.GetValue(_completionWindow) is not ToolTip tooltip)
			return;

		tooltip.Background = DefaultToolTipBackground;
		tooltip.BorderBrush = DefaultToolTipBorder;
		tooltip.BorderThickness = new Thickness(0.0);
		tooltip.Padding = new Thickness(0.0);
		tooltip.PlacementTarget = listBox;
		tooltip.Placement = PlacementMode.Right;
		tooltip.HorizontalOffset = CompletionToolTipHorizontalOffset;
		tooltip.StaysOpen = true;

		listBox.SelectionChanged += (s, e) => ScheduleCompletionTooltipUpdate(tooltip);
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
		ScheduleCompletionTooltipUpdate(tooltip);
	}

	private void ScheduleCompletionTooltipUpdate(ToolTip tooltip)
	{
		int updateToken = ++_completionToolTipUpdateToken;
		CancellationToken cancellationToken = ResetCancellationTokenSource(ref _completionToolTipCancellationTokenSource);

		Dispatcher.BeginInvoke(
			new Action(() => _ = UpdateCompletionTooltipAsync(tooltip, updateToken, cancellationToken)),
			DispatcherPriority.Background);
	}

	private async Task UpdateCompletionTooltipAsync(ToolTip tooltip, int updateToken, CancellationToken cancellationToken)
	{
		if (_completionWindow?.CompletionList.ListBox is not ListBox listBox)
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
				ApplyCompletionToolTipContent(tooltip, description);
			else
				tooltip.IsOpen = false;

			if (item is LuaCompletionData luaCompletionData && luaCompletionData.CanResolve)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(CompletionToolTipResolveDelayInMilliseconds), cancellationToken).ConfigureAwait(true);

				if (updateToken != _completionToolTipUpdateToken)
					return;

				object? resolvedDescription = await luaCompletionData.GetDescriptionAsync(cancellationToken).ConfigureAwait(true);

				if (updateToken != _completionToolTipUpdateToken)
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
			// Newer selection or close superseded this tooltip update.
		}
		catch (Exception exception)
		{
			tooltip.IsOpen = false;
			LogEditorFailure("Completion tooltip update", exception);
		}
	}

	private void ResizeCompletionWindow(LuaCompletionData[] completionDataItems)
	{
		if (_completionWindow is null || completionDataItems is null || completionDataItems.Length == 0)
			return;

		double requiredWidth = CompletionWindowMinWidth;
		int measurementCount = Math.Min(completionDataItems.Length, CompletionWidthMeasurementSampleCount);
		var textWidthCache = new Dictionary<string, double>(StringComparer.Ordinal);

		for (int i = 0; i < measurementCount; i++)
			requiredWidth = Math.Max(requiredWidth, MeasureCompletionItemWidth(completionDataItems[i], textWidthCache));

		_completionWindow.Width = Math.Max(
			CompletionWindowMinWidth,
			Math.Min(CompletionWindowMaxWidth, requiredWidth + CompletionWindowHorizontalChrome));
	}

	private double MeasureCompletionItemWidth(LuaCompletionData completionData, IDictionary<string, double> textWidthCache)
	{
		double width = CompletionItemIconWidth + MeasureCompletionTextWidth(completionData.DisplayText, textWidthCache);

		if (!string.IsNullOrWhiteSpace(completionData.DisplayDetail))
			width += CompletionItemDetailSpacing + MeasureCompletionTextWidth(completionData.DisplayDetail, textWidthCache);

		return width;
	}

	private double MeasureCompletionTextWidth(string text, IDictionary<string, double> textWidthCache)
	{
		if (string.IsNullOrWhiteSpace(text))
			return 0.0;

		if (textWidthCache.TryGetValue(text, out double cachedWidth))
			return cachedWidth;

		double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

		var formattedText = new FormattedText(
			text,
			CultureInfo.CurrentUICulture,
			FlowDirection.LeftToRight,
			new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
			FontSize,
			Foreground,
			pixelsPerDip);

		double width = formattedText.WidthIncludingTrailingWhitespace;
		textWidthCache[text] = width;
		return width;
	}

	private static void ApplyCompletionToolTipContent(ToolTip tooltip, object content)
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

	private void CancelCompletionToolTipUpdate()
	{
		_completionToolTipUpdateToken++;
		_completionToolTipCancellationTokenSource?.Cancel();
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

		if (listBox?.HasItems != false)
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

			if (LuaLineParser.IsIdentifierCharacter(currentChar))
				startOffset--;
			else
				break;
		}

		_completionWindow.StartOffset = startOffset;
		_completionWindow.EndOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
	}
}
