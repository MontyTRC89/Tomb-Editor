#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Nickelony.LanguageServer.Core.Completion;
using TombLib.Scripting.Completion;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Resources;
using TombLib.WPF;

namespace TombLib.Scripting.UI.Completion;

public sealed record TextCompletionControllerOptions(
	TimeSpan RequestDebounceDelay,
	TimeSpan ToolTipResolveDelay,
	int WindowMinWidth,
	int WindowMaxWidth,
	int WindowHeight,
	int WidthMeasurementSampleCount,
	double ToolTipHorizontalOffset,
	double WindowHorizontalChrome,
	double ItemIconWidth,
	double ItemDetailSpacing)
{
	public static TextCompletionControllerOptions Default { get; } = new(
		TimeSpan.FromMilliseconds(120.0),
		TimeSpan.FromMilliseconds(120.0),
		420,
		920,
		320,
		80,
		10.0,
		52.0,
		24.0,
		12.0);
}

/// <summary>
/// Coordinates shared completion popup lifecycle, tooltip ownership, sizing, and request scheduling.
/// </summary>
public sealed class TextCompletionController
{
	private static readonly Lazy<FieldInfo?> CompletionToolTipFieldAccessor = new(() =>
		typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance));

	private static FieldInfo? CompletionToolTipField => CompletionToolTipFieldAccessor.Value;

	private readonly TextEditorBase _editor;
	private readonly TextCompletionControllerOptions _options;
	private readonly Action<TextCompletionPresentationState>? _applyPresentationState;
	private readonly Action<CompletionWindow>? _configureWindow;
	private readonly Action<Exception>? _handleRequestFailure;
	private readonly DispatcherTimer _requestTimer = new();
	private readonly DispatcherTimer _toolTipUpdateTimer = new();
	private Func<Task>? _scheduledRequestAsync;
	private ToolTip? _pendingCompletionToolTip;
	private int _requestToken;
	private int _toolTipUpdateToken;

	public TextCompletionController(
		TextEditorBase editor,
		TextCompletionControllerOptions? options = null,
		Action<TextCompletionPresentationState>? applyPresentationState = null,
		Action<CompletionWindow>? configureWindow = null,
		Action<Exception>? handleRequestFailure = null)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_editor = editor;
		_options = options ?? TextCompletionControllerOptions.Default;
		_applyPresentationState = applyPresentationState;
		_configureWindow = configureWindow;
		_handleRequestFailure = handleRequestFailure;
	}

	/// <summary>
	/// Gets the current shared completion presentation state.
	/// </summary>
	public TextCompletionPresentationState CurrentPresentation { get; private set; } = TextCompletionPresentationState.Empty;

	public void InitializeScheduling(Func<Task> scheduledRequestAsync)
	{
		ArgumentNullException.ThrowIfNull(scheduledRequestAsync);

		_scheduledRequestAsync = scheduledRequestAsync;
		_requestTimer.Interval = _options.RequestDebounceDelay;
		_requestTimer.Tick -= RequestTimer_Tick;
		_requestTimer.Tick += RequestTimer_Tick;

		_toolTipUpdateTimer.Interval = _options.ToolTipResolveDelay;
		_toolTipUpdateTimer.Tick -= ToolTipUpdateTimer_Tick;
		_toolTipUpdateTimer.Tick += ToolTipUpdateTimer_Tick;
	}

	public CompletionWindow? ActiveWindow => _editor.ActiveCompletionWindow;

	public int BeginRequest()
		=> ++_requestToken;

	public bool IsRequestCurrent(int requestToken)
		=> requestToken == _requestToken;

	public void InvalidateRequests()
		=> _requestToken++;

	public void ScheduleRequest()
	{
		if (_scheduledRequestAsync is null)
			return;

		_requestTimer.Stop();
		_requestTimer.Start();
		SetRequestScheduled(true);
	}

	public void CancelPendingRequest()
	{
		_requestTimer.Stop();
		SetRequestScheduled(false);
	}

	public void CloseWindow()
	{
		CancelPendingRequest();
		CancelTooltipUpdate();

		if (ActiveWindow is CompletionWindow completionWindow
			&& CompletionToolTipField?.GetValue(completionWindow) is ToolTip tooltip)
		{
			tooltip.IsOpen = false;
		}

		_editor.CloseSharedCompletionWindow();
		SetWindowState(false);
		SetTooltipState(null, false);
	}

	public bool OpenOrRefresh(IEnumerable<CompletionData> items, int? startOffset = null, int? endOffset = null)
	{
		ArgumentNullException.ThrowIfNull(items);

		CompletionData[] completionItems = items.ToArray();

		if (completionItems.Length == 0)
			return false;

		CloseWindow();
		_editor.InitializeCompletionWindow(_options.WindowMinWidth, _options.WindowHeight);

		if (ActiveWindow is not CompletionWindow completionWindow)
			return false;

		TextCompletionWindowStyle.Apply(completionWindow);
		_configureWindow?.Invoke(completionWindow);
		StyleTooltip(completionWindow);
		MakeWindowNonActivatable(completionWindow);
		ResizeWindow(completionWindow, completionItems);

		if (startOffset.HasValue)
			completionWindow.StartOffset = startOffset.Value;

		if (endOffset.HasValue)
			completionWindow.EndOffset = endOffset.Value;

		foreach (CompletionData item in completionItems)
			completionWindow.CompletionList.CompletionData.Add(item);

		_editor.ShowCompletionWindow();
		SetWindowState(true);
		ScheduleInitialSelection();
		return true;
	}

	public bool ApplyDecision(TextCompletionSessionDecision decision, Func<TextCompletionItem, CompletionData>? mapItem = null)
	{
		if (decision.CloseWindow)
			CloseWindow();

		if (decision.Items is null || !decision.StartOffset.HasValue || !decision.EndOffset.HasValue)
			return false;

		mapItem ??= static item => new CompletionData(item);
		CompletionData[] items = decision.Items.Select(mapItem).ToArray();
		return OpenOrRefresh(items, decision.StartOffset.Value, decision.EndOffset.Value);
	}

	public void RebaseOpenCompletionItems(int requestDocumentVersion, int requestGeneration)
	{
		if (ActiveWindow?.CompletionList?.CompletionData is null)
			return;

		for (int i = 0; i < ActiveWindow.CompletionList.CompletionData.Count; i++)
		{
			if (ActiveWindow.CompletionList.CompletionData[i] is CompletionData completionData)
				completionData.RebaseForCurrentDocument(requestDocumentVersion, requestGeneration);
		}
	}

	public void ScheduleCloseIfEmpty()
		=> _editor.Dispatcher.BeginInvoke(new Action(() => CloseWindowIfEmpty()), DispatcherPriority.Background);

	public void CancelTooltipUpdate()
	{
		_toolTipUpdateToken++;
		_pendingCompletionToolTip = null;
		_toolTipUpdateTimer.Stop();
	}

	private async void RequestTimer_Tick(object? sender, EventArgs e)
	{
		_requestTimer.Stop();
		SetRequestScheduled(false);

		if (_scheduledRequestAsync is null)
			return;

		try
		{
			await _scheduledRequestAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			_handleRequestFailure?.Invoke(exception);
		}
	}

	private async void ToolTipUpdateTimer_Tick(object? sender, EventArgs e)
	{
		_toolTipUpdateTimer.Stop();

		if (_pendingCompletionToolTip is not ToolTip tooltip)
			return;

		await UpdateTooltipAsync(tooltip, _toolTipUpdateToken).ConfigureAwait(true);
	}

	private void StyleTooltip(CompletionWindow completionWindow)
	{
		if (completionWindow.CompletionList.ListBox is not ListBox listBox)
			return;

		if (CompletionToolTipField?.GetValue(completionWindow) is not ToolTip tooltip)
			return;

		tooltip.Background = TextEditorColorPalette.ToolTipBackground;
		tooltip.BorderBrush = TextEditorColorPalette.ToolTipBorder;
		tooltip.BorderThickness = new Thickness(0.0);
		tooltip.Padding = new Thickness(0.0);
		tooltip.PlacementTarget = listBox;
		tooltip.Placement = PlacementMode.Right;
		tooltip.HorizontalOffset = _options.ToolTipHorizontalOffset;
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
		_toolTipUpdateToken++;
		_toolTipUpdateTimer.Stop();
		_toolTipUpdateTimer.Start();
	}

	private async Task UpdateTooltipAsync(ToolTip tooltip, int updateToken)
	{
		if (ActiveWindow?.CompletionList.ListBox is not ListBox listBox)
			return;

		if (listBox.SelectedItem is not ICompletionData item)
		{
			tooltip.IsOpen = false;
			SetTooltipState(null, false);
			return;
		}

		try
		{
			object? description = item.Description;

			if (description is not null)
				ApplyTooltipContent(tooltip, description);
			else
			{
				tooltip.IsOpen = false;
				SetTooltipState(null, false);
			}

			if (item is CompletionData completionData && completionData.CanResolve)
			{
				if (updateToken != _toolTipUpdateToken)
					return;

				object? resolvedDescription = await completionData.GetDescriptionAsync().ConfigureAwait(true);

				if (updateToken != _toolTipUpdateToken)
					return;

				if (ActiveWindow?.CompletionList.ListBox is not ListBox currentListBox
					|| !ReferenceEquals(currentListBox, listBox)
					|| !ReferenceEquals(currentListBox.SelectedItem, item))
				{
					return;
				}

				if (resolvedDescription is not null)
					ApplyTooltipContent(tooltip, resolvedDescription);
				else
				{
					tooltip.IsOpen = false;
					SetTooltipState(null, false);
				}
			}
		}
		catch (Exception exception)
		{
			tooltip.IsOpen = false;
			SetTooltipState(null, false);
			_handleRequestFailure?.Invoke(exception);
		}
	}

	private void SetRequestScheduled(bool isRequestScheduled)
		=> ApplyPresentationState(CurrentPresentation with { IsRequestScheduled = isRequestScheduled });

	private void SetWindowState(bool hasOpenWindow)
		=> ApplyPresentationState(CurrentPresentation with { HasOpenWindow = hasOpenWindow });

	private void SetTooltipState(object? toolTipContent, bool isToolTipVisible)
		=> ApplyPresentationState(CurrentPresentation with { ToolTipContent = toolTipContent, IsToolTipVisible = isToolTipVisible });

	private void ApplyPresentationState(TextCompletionPresentationState state)
	{
		CurrentPresentation = state;
		_applyPresentationState?.Invoke(state);
	}

	private void ResizeWindow(CompletionWindow completionWindow, IReadOnlyList<CompletionData> items)
	{
		double requiredWidth = _options.WindowMinWidth;
		int measurementCount = Math.Min(items.Count, _options.WidthMeasurementSampleCount);
		var textWidthCache = new Dictionary<string, double>(StringComparer.Ordinal);

		for (int i = 0; i < measurementCount; i++)
			requiredWidth = Math.Max(requiredWidth, MeasureItemWidth(items[i], textWidthCache));

		completionWindow.Width = Math.Max(
			_options.WindowMinWidth,
			Math.Min(_options.WindowMaxWidth, requiredWidth + _options.WindowHorizontalChrome));
	}

	private double MeasureItemWidth(CompletionData completionData, IDictionary<string, double> textWidthCache)
	{
		double width = _options.ItemIconWidth + MeasureTextWidth(completionData.DisplayText, textWidthCache);

		if (!string.IsNullOrWhiteSpace(completionData.DisplayDetail))
			width += _options.ItemDetailSpacing + MeasureTextWidth(completionData.DisplayDetail, textWidthCache);

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

	private void ApplyTooltipContent(ToolTip tooltip, object content)
	{
		tooltip.Content = content;
		SetTooltipState(content, true);

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

	private void ScheduleInitialSelection()
		=> _editor.Dispatcher.BeginInvoke(new Action(SelectInitialItem), DispatcherPriority.ContextIdle);

	private void SelectInitialItem()
	{
		if (ActiveWindow is not CompletionWindow completionWindow)
			return;

		completionWindow.CompletionList.SelectItem(GetCompletionWindowQuery(completionWindow));
		CloseWindowIfEmpty();
	}

	private void MakeWindowNonActivatable(CompletionWindow completionWindow)
	{
		completionWindow.SourceInitialized += (s, e) =>
		{
			if (s is Window window && PresentationSource.FromVisual(window) is HwndSource source)
				source.AddHook(CompletionWindowWndProc);
		};
	}

	private static IntPtr CompletionWindowWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		const int MA_NOACTIVATE = 3;
		const int WM_MOUSEACTIVATE = 0x0021;

		if (msg == WM_MOUSEACTIVATE)
		{
			handled = true;
			return new IntPtr(MA_NOACTIVATE);
		}

		return IntPtr.Zero;
	}

	private bool CloseWindowIfEmpty()
	{
		if (ActiveWindow is not CompletionWindow completionWindow)
			return false;

		ListBox listBox = completionWindow.CompletionList.ListBox;

		if (listBox?.HasItems != false)
			return false;

		CloseWindow();
		return true;
	}

	private string GetCompletionWindowQuery(CompletionWindow completionWindow)
	{
		if (_editor.Document is null)
			return string.Empty;

		int startOffset = Math.Max(0, Math.Min(completionWindow.StartOffset, _editor.Document.TextLength));
		int endOffset = Math.Max(startOffset, Math.Min(completionWindow.EndOffset, _editor.Document.TextLength));

		return endOffset > startOffset
			? _editor.Document.GetText(startOffset, endOffset - startOffset)
			: string.Empty;
	}
}
