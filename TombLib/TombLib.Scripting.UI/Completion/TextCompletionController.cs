using ICSharpCode.AvalonEdit.CodeCompletion;
using Nickelony.LanguageServer.Abstractions.Completion;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Presentation;
using TombLib.Scripting.Threading;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Resources;
using TombLib.WPF;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Carries the sizing and timing options used by the <see cref="TextCompletionController"/>.
/// </summary>
/// <param name="RequestDebounceDelay">The debounce delay before a scheduled completion request runs.</param>
/// <param name="ToolTipResolveDelay">The delay before a completion tooltip update is resolved.</param>
/// <param name="WindowMinWidth">The minimum width of the completion window.</param>
/// <param name="WindowMaxWidth">The maximum width of the completion window.</param>
/// <param name="WindowHeight">The height of the completion window.</param>
/// <param name="WidthMeasurementSampleCount">The number of samples used to measure the completion item width.</param>
/// <param name="ToolTipHorizontalOffset">The horizontal offset of the completion tooltip.</param>
/// <param name="WindowHorizontalChrome">The horizontal chrome width of the completion window.</param>
/// <param name="ItemIconWidth">The width reserved for the completion item icon.</param>
/// <param name="ItemDetailSpacing">The spacing between completion item details.</param>
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
	/// <summary>
	/// Gets the default completion controller options.
	/// </summary>
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
/// The popup is owned through <see cref="CompletionWindowHost"/>, which tracks at most one window:
/// opening a new completion window force-closes the previous one.
/// </summary>
public sealed class TextCompletionController : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly TextEditorBase _editor;
	private readonly TextCompletionControllerOptions _options;
	private readonly Action<TextCompletionPresentationState>? _applyPresentationState;
	private readonly Action<CompletionWindow>? _configureWindow;
	private readonly Action<Exception>? _handleRequestFailure;
	private readonly DispatcherTimer _requestTimer = new();
	private readonly DispatcherTimer _toolTipUpdateTimer = new();
	private Func<Task>? _scheduledRequestAsync;
	private ToolTip? _pendingCompletionToolTip;
	private readonly RequestTokenSource _requestTokens = new();
	private int _toolTipUpdateToken;
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextCompletionController"/> class.
	/// </summary>
	/// <param name="editor">The editor the controller serves.</param>
	/// <param name="options">The controller options, or <c>null</c> to use the defaults.</param>
	/// <param name="applyPresentationState">An optional callback that applies the completion presentation state.</param>
	/// <param name="configureWindow">An optional callback that configures a completion window before it is shown.</param>
	/// <param name="handleRequestFailure">An optional callback that handles completion request failures.</param>
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

	/// <summary>
	/// Initializes the request scheduling with the callback used to run a scheduled request.
	/// </summary>
	/// <param name="scheduledRequestAsync">The callback that runs a scheduled completion request.</param>
	public void InitializeScheduling(Func<Task> scheduledRequestAsync)
	{
		ArgumentNullException.ThrowIfNull(scheduledRequestAsync);

		if (_isDisposed)
			return;

		_scheduledRequestAsync = scheduledRequestAsync;
		_requestTimer.Interval = _options.RequestDebounceDelay;
		_requestTimer.Tick -= RequestTimer_Tick;
		_requestTimer.Tick += RequestTimer_Tick;

		_toolTipUpdateTimer.Interval = _options.ToolTipResolveDelay;
		_toolTipUpdateTimer.Tick -= ToolTipUpdateTimer_Tick;
		_toolTipUpdateTimer.Tick += ToolTipUpdateTimer_Tick;
	}

	/// <summary>
	/// Gets the currently active completion window, or <c>null</c> when none is open.
	/// </summary>
	public CompletionWindow? ActiveWindow => _isDisposed ? null : _editor.ActiveCompletionWindow;

	/// <summary>
	/// Begins a new request and returns its token.
	/// </summary>
	/// <returns>The token of the new request.</returns>
	public int BeginRequest()
	{
		if (_isDisposed)
			return -1;

		return _requestTokens.Begin();
	}

	/// <summary>
	/// Determines whether the given request token is still the current request.
	/// </summary>
	/// <param name="requestToken">The request token to check.</param>
	/// <returns><c>true</c> when the token is current; otherwise, <c>false</c>.</returns>
	public bool IsRequestCurrent(int requestToken)
		=> !_isDisposed && _requestTokens.IsCurrent(requestToken);

	/// <summary>
	/// Invalidates all in-flight completion requests.
	/// </summary>
	public void InvalidateRequests()
	{
		if (_isDisposed)
			return;

		_requestTokens.Invalidate();
	}

	/// <summary>
	/// Schedules a completion request to run after the debounce delay.
	/// </summary>
	public void ScheduleRequest()
	{
		if (_isDisposed || _scheduledRequestAsync is null)
			return;

		_requestTimer.Stop();
		_requestTimer.Start();
		SetRequestScheduled(true);
	}

	/// <summary>
	/// Cancels any pending scheduled completion request.
	/// </summary>
	public void CancelPendingRequest()
	{
		if (_isDisposed)
			return;

		CancelPendingRequestCore();
	}

	private void CancelPendingRequestCore()
	{
		_requestTimer.Stop();
		SetRequestScheduled(false);
	}

	/// <summary>
	/// Closes the active completion window and any completion tooltip.
	/// </summary>
	public void CloseWindow()
	{
		if (_isDisposed)
			return;

		CloseWindowCore();
	}

	private void CloseWindowCore()
	{
		CancelPendingRequestCore();
		CancelTooltipUpdateCore();

		if (_editor.ActiveCompletionWindow is CompletionWindow completionWindow
			&& CompletionWindowToolTipAccess.TryGetToolTip(completionWindow, out ToolTip? tooltip))
		{
			tooltip.IsOpen = false;
		}

		_editor.CloseSharedCompletionWindow();
		SetWindowState(false);
		SetTooltipState(null, false);
	}

	/// <summary>
	/// Opens a completion window with the given items, or refreshes the current window.
	/// </summary>
	/// <param name="items">The completion items to show.</param>
	/// <param name="startOffset">The optional start offset of the replaced word.</param>
	/// <param name="endOffset">The optional end offset of the replaced word.</param>
	/// <returns><c>true</c> when the window was opened; otherwise, <c>false</c>.</returns>
	public bool OpenOrRefresh(IEnumerable<CompletionData> items, int? startOffset = null, int? endOffset = null)
	{
		ArgumentNullException.ThrowIfNull(items);

		if (_isDisposed)
			return false;

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

	/// <summary>
	/// Applies a completion session decision to the completion window.
	/// </summary>
	/// <param name="decision">The decision to apply.</param>
	/// <param name="mapItem">An optional mapper from provider items to completion data.</param>
	/// <returns><c>true</c> when a completion window was opened; otherwise, <c>false</c>.</returns>
	public bool ApplyDecision(TextCompletionSessionDecision decision, Func<TextCompletionItem, CompletionData>? mapItem = null)
	{
		if (_isDisposed)
			return false;

		if (decision.CloseWindow)
			CloseWindow();

		if (decision.Items is null || !decision.StartOffset.HasValue || !decision.EndOffset.HasValue)
			return false;

		mapItem ??= static item => new CompletionData(item);
		CompletionData[] items = decision.Items.Select(mapItem).ToArray();
		return OpenOrRefresh(items, decision.StartOffset.Value, decision.EndOffset.Value);
	}

	/// <summary>
	/// Rebaselines the open completion items for the current document version and generation.
	/// </summary>
	/// <param name="requestDocumentVersion">The document version of the request.</param>
	/// <param name="requestGeneration">The generation of the request.</param>
	public void RebaseOpenCompletionItems(int requestDocumentVersion, int requestGeneration)
	{
		if (_isDisposed || ActiveWindow?.CompletionList?.CompletionData is null)
			return;

		for (int i = 0; i < ActiveWindow.CompletionList.CompletionData.Count; i++)
		{
			if (ActiveWindow.CompletionList.CompletionData[i] is CompletionData completionData)
				completionData.RebaseForCurrentDocument(requestDocumentVersion, requestGeneration);
		}
	}

	/// <summary>
	/// Schedules the completion window to close if it is empty.
	/// </summary>
	public void ScheduleCloseIfEmpty()
	{
		if (_isDisposed)
			return;

		_editor.Dispatcher.BeginInvoke(new Action(() => CloseWindowIfEmpty()), DispatcherPriority.Background);
	}

	/// <summary>
	/// Cancels any pending completion tooltip update.
	/// </summary>
	public void CancelTooltipUpdate()
	{
		if (_isDisposed)
			return;

		CancelTooltipUpdateCore();
	}

	private void CancelTooltipUpdateCore()
	{
		_toolTipUpdateToken++;
		_pendingCompletionToolTip = null;
		_toolTipUpdateTimer.Stop();
	}

	/// <summary>
	/// Stops completion scheduling and closes any open completion window or tooltip.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_requestTimer.Stop();
		_requestTimer.Tick -= RequestTimer_Tick;
		_toolTipUpdateTimer.Stop();
		_toolTipUpdateTimer.Tick -= ToolTipUpdateTimer_Tick;
		_requestTokens.Invalidate();
		CloseWindowCore();
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
			if (_isDisposed)
				return;

			if (_handleRequestFailure is null)
				Log.Warn(exception, "Completion request failed.");
			else
				_handleRequestFailure(exception);
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

		if (!CompletionWindowToolTipAccess.TryGetToolTip(completionWindow, out ToolTip? tooltip))
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
		if (_isDisposed || ActiveWindow?.CompletionList.ListBox is not ListBox listBox)
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

				if (_isDisposed || updateToken != _toolTipUpdateToken)
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
			if (_isDisposed)
				return;

			tooltip.IsOpen = false;
			SetTooltipState(null, false);

			if (_handleRequestFailure is null)
				Log.Warn(exception, "Failed to resolve completion tooltip content.");
			else
				_handleRequestFailure(exception);
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

		int startOffset = _editor.Document.ClampOffset(completionWindow.StartOffset);
		int endOffset = Math.Max(startOffset, Math.Min(completionWindow.EndOffset, _editor.Document.TextLength));

		return endOffset > startOffset
			? _editor.Document.GetText(startOffset, endOffset - startOffset)
			: string.Empty;
	}
}
