using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private const double SignaturePopupFontSize = 14.0;
	private const double SignatureHelpRefreshDebounceDelayInMilliseconds = 50.0;

	private int _signatureRequestToken;
	private int _pendingSignatureHelpOffset = -1;
	private bool _signatureRefreshPending;
	private bool _signatureRequestInFlight;

	private readonly Popup _signaturePopup = new();
	private readonly Border _signaturePopupBorder = new();
	private readonly ContentPresenter _signaturePopupPresenter = new();
	private readonly DispatcherTimer _signatureRefreshTimer = new();

	private void InitializeSignaturePopup()
	{
		_signaturePopup.AllowsTransparency = true;
		_signaturePopup.PopupAnimation = PopupAnimation.None;
		_signaturePopup.StaysOpen = true;
		_signaturePopup.Placement = PlacementMode.RelativePoint;

		_signaturePopupBorder.SnapsToDevicePixels = true;
		_signaturePopupBorder.CornerRadius = new CornerRadius(3.0);
		_signaturePopupBorder.BorderThickness = new Thickness(1.0);
		_signaturePopupBorder.Padding = new Thickness(8.0, 6.0, 8.0, 6.0);
		_signaturePopupBorder.BorderBrush = DefaultToolTipBorder;
		_signaturePopupBorder.Background = DefaultToolTipBackground;
		_signaturePopupBorder.Child = _signaturePopupPresenter;

		_signaturePopup.Child = _signaturePopupBorder;

		_signatureRefreshTimer.Interval = TimeSpan.FromMilliseconds(SignatureHelpRefreshDebounceDelayInMilliseconds);
		_signatureRefreshTimer.Tick -= SignatureRefreshTimer_Tick;
		_signatureRefreshTimer.Tick += SignatureRefreshTimer_Tick;
	}

	private void DismissSignatureHelp()
	{
		CancelPendingSignatureHelpRefresh();
		InvalidateSignatureHelpRequests();

		if (_signaturePopup.IsOpen)
			_signaturePopup.IsOpen = false;

		_signaturePopupPresenter.Content = null;
	}

	private void ShowSignatureToolTip(LuaSignatureInfo signatureInfo)
	{
		double availablePopupWidth = Math.Max(0.0, Math.Min(500.0, ActualWidth - 16.0));

		double popupHorizontalPadding = _signaturePopupBorder.Padding.Left
			+ _signaturePopupBorder.Padding.Right
			+ _signaturePopupBorder.BorderThickness.Left
			+ _signaturePopupBorder.BorderThickness.Right;

		double popupVerticalPadding = _signaturePopupBorder.Padding.Top
			+ _signaturePopupBorder.Padding.Bottom
			+ _signaturePopupBorder.BorderThickness.Top
			+ _signaturePopupBorder.BorderThickness.Bottom;

		double contentMaxWidth = Math.Max(0.0, availablePopupWidth - popupHorizontalPadding);

		StackPanel panel = CreateSignaturePanel(signatureInfo, contentMaxWidth);

		panel.Measure(new Size(contentMaxWidth, double.PositiveInfinity));

		Size popupSize = new(
			Math.Min(availablePopupWidth, panel.DesiredSize.Width + popupHorizontalPadding),
			panel.DesiredSize.Height + popupVerticalPadding);

		_signaturePopupPresenter.Content = panel;
		_signaturePopupPresenter.InvalidateMeasure();
		_signaturePopupBorder.InvalidateMeasure();
		PositionSignaturePopup(popupSize);

		if (!_signaturePopup.IsOpen)
			_signaturePopup.IsOpen = true;
	}

	private StackPanel CreateSignaturePanel(LuaSignatureInfo signatureInfo, double contentMaxWidth)
	{
		LuaThemeBrushSet brushSet = GetThemeBrushSet();
		var panel = new StackPanel { MaxWidth = contentMaxWidth };
		panel.Children.Add(BuildSignatureBlock(signatureInfo, brushSet));

		if (!string.IsNullOrWhiteSpace(signatureInfo.Documentation))
			panel.Children.Add(CreateSignatureDocumentationBlock(signatureInfo.Documentation, brushSet));

		if (signatureInfo.ActiveParameter < signatureInfo.Parameters.Count)
		{
			LuaParameterInfo activeParam = signatureInfo.Parameters[signatureInfo.ActiveParameter];

			if (!string.IsNullOrWhiteSpace(activeParam.Documentation))
				panel.Children.Add(CreateSignatureDocumentationBlock(activeParam.Label + ": " + activeParam.Documentation, brushSet));
		}

		return panel;
	}

	private static TextBlock CreateSignatureDocumentationBlock(string text, LuaThemeBrushSet brushSet) => new()
	{
		Text = text,
		Foreground = brushSet.SignatureParamDocForeground,
		FontFamily = SystemFonts.MessageFontFamily,
		FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
		TextWrapping = TextWrapping.Wrap,
		Margin = new Thickness(0.0, 4.0, 0.0, 0.0)
	};

	private void PositionSignaturePopup(Size popupSize)
	{
		AttachHostWindowHandlers();
		_signaturePopup.PlacementTarget = this;
		TextArea.TextView.EnsureVisualLines();

		Rect caretRectangle = TextArea.Caret.CalculateCaretRectangle();
		Vector scrollOffset = TextArea.TextView.ScrollOffset;

		Point caretViewportPoint = new(
			caretRectangle.X - scrollOffset.X,
			caretRectangle.Y - scrollOffset.Y);

		Point editorPoint = TextArea.TextView.TranslatePoint(caretViewportPoint, this);
		double lineHeight = Math.Max(TextArea.TextView.DefaultLineHeight, caretRectangle.Height);
		double lineSlack = Math.Max(0.0, lineHeight - caretRectangle.Height);
		double horizontalOffset = Math.Max(0.0, editorPoint.X + 2.0);
		double maxHorizontalOffset = Math.Max(0.0, ActualWidth - popupSize.Width - 8.0);
		horizontalOffset = Math.Min(horizontalOffset, maxHorizontalOffset);

		double verticalOffset = editorPoint.Y - popupSize.Height - lineSlack - 8.0;

		if (verticalOffset < 0.0)
			verticalOffset = Math.Min(Math.Max(0.0, ActualHeight - popupSize.Height), editorPoint.Y + lineHeight + 4.0);

		_signaturePopup.HorizontalOffset = horizontalOffset;
		_signaturePopup.VerticalOffset = verticalOffset;
	}

	private async Task RequestSignatureHelpAsync(int offset)
	{
		if (_signatureRequestInFlight)
		{
			_pendingSignatureHelpOffset = offset;
			_signatureRefreshPending = true;
			return;
		}

		_signatureRequestInFlight = true;

		if (!IsIntellisenseAvailable())
		{
			_signatureRequestInFlight = false;
			DismissSignatureHelp();
			return;
		}

		CancellationToken cancellationToken = CancellationToken.None;
		int requestToken = ++_signatureRequestToken;
		int requestDocumentVersion = _editorDocumentVersion;
		int requestGeneration = _editorRequestGeneration;

		try
		{
			(int Line, int Column) = GetPositionFromOffset(offset);

			LuaSignatureInfo? signatureInfo = await IntellisenseProvider
				.GetSignatureHelpAsync(FilePath, Text, Line, Column, cancellationToken)
				.ConfigureAwait(true);

			if (!IsAsyncEditorResultCurrent(cancellationToken, requestToken, _signatureRequestToken, requestDocumentVersion, requestGeneration))
				return;

			if (signatureInfo is null)
			{
				DismissSignatureHelp();
				return;
			}

			ShowSignatureToolTip(signatureInfo);
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			LogEditorFailure("Signature help", exception);
		}
		finally
		{
			_signatureRequestInFlight = false;

			if (_signatureRefreshPending && _pendingSignatureHelpOffset >= 0)
			{
				_signatureRefreshTimer.Stop();
				_signatureRefreshTimer.Start();
			}
		}
	}

	private void ScheduleSignatureHelpRefresh()
	{
		_pendingSignatureHelpOffset = CaretOffset;
		_signatureRefreshPending = true;
		_signatureRefreshTimer.Stop();
		_signatureRefreshTimer.Start();
	}

	private void CancelPendingSignatureHelpRefresh()
	{
		_signatureRefreshTimer.Stop();
		_signatureRefreshPending = false;
		_pendingSignatureHelpOffset = -1;
	}

	private void InvalidateSignatureHelpRequests()
	{
		_signatureRequestToken++;
		_signatureRequestInFlight = false;
	}

	private async void SignatureRefreshTimer_Tick(object? sender, EventArgs e)
	{
		_signatureRefreshTimer.Stop();

		if (!_signatureRefreshPending || _pendingSignatureHelpOffset < 0)
			return;

		if (_signatureRequestInFlight)
			return;

		int offset = _pendingSignatureHelpOffset;
		_signatureRefreshPending = false;
		await RequestSignatureHelpAsync(offset).ConfigureAwait(true);
	}

	private static TextBlock BuildSignatureBlock(LuaSignatureInfo signatureInfo, LuaThemeBrushSet brushSet)
	{
		var textBlock = new TextBlock
		{
			FontFamily = new FontFamily("Consolas"),
			FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
			TextWrapping = TextWrapping.Wrap,
			Foreground = brushSet.SignatureForeground
		};

		string label = signatureInfo.Label;

		if (signatureInfo.Parameters.Count == 0
			|| !TryGetActiveParameterRange(label, signatureInfo, out int activeStart, out int activeEnd))
		{
			textBlock.Text = label;
			return textBlock;
		}

		if (activeStart > 0)
			textBlock.Inlines.Add(new Run(label[..activeStart]));

		textBlock.Inlines.Add(new Run(label[activeStart..activeEnd])
		{
			FontWeight = FontWeights.Bold,
			Foreground = brushSet.SignatureActiveParamForeground
		});

		if (activeEnd < label.Length)
			textBlock.Inlines.Add(new Run(label[activeEnd..]));

		return textBlock;
	}

	private static bool TryGetActiveParameterRange(string label, LuaSignatureInfo signatureInfo, out int activeStart, out int activeEnd)
	{
		activeStart = 0;
		activeEnd = 0;

		int activeIndex = Math.Min(signatureInfo.ActiveParameter, signatureInfo.Parameters.Count - 1);

		if (activeIndex < 0)
			return false;

		string activeLabel = signatureInfo.Parameters[activeIndex].Label;

		if (string.IsNullOrEmpty(activeLabel))
			return false;

		int searchStart = label.IndexOf('(');
		searchStart = searchStart < 0 ? 0 : searchStart + 1;

		int matchIndex = label.IndexOf(activeLabel, searchStart, StringComparison.Ordinal);

		if (matchIndex < 0)
			return false;

		activeStart = matchIndex;
		activeEnd = matchIndex + activeLabel.Length;
		return true;
	}
}
