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

	private void DismissSignatureHelp()
		=> _signatureHelpController.Dismiss();

	private Task RequestSignatureHelpAsync(int offset)
		=> _signatureHelpController.RequestAsync(offset);

	private void ScheduleSignatureHelpRefresh()
		=> _signatureHelpController.ScheduleRefresh();

	private static TextBlock CreateSignatureDocumentationBlock(string text, LuaThemeBrushSet brushSet) => new()
	{
		Text = text,
		Foreground = brushSet.SignatureParamDocForeground,
		FontFamily = SystemFonts.MessageFontFamily,
		FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
		TextWrapping = TextWrapping.Wrap,
		Margin = new Thickness(0.0, 4.0, 0.0, 0.0)
	};

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

	private sealed class LuaSignatureHelpController
	{
		private const double SignatureHelpRefreshDebounceDelayInMilliseconds = 50.0;

		private readonly LuaEditor _editor;
		private int _signatureRequestToken;
		private int _pendingSignatureHelpOffset = -1;
		private bool _signatureRefreshPending;
		private bool _signatureRequestInFlight;

		private readonly Popup _signaturePopup = new();
		private readonly Border _signaturePopupBorder = new();
		private readonly ContentPresenter _signaturePopupPresenter = new();
		private readonly DispatcherTimer _signatureRefreshTimer = new();

		public LuaSignatureHelpController(LuaEditor editor)
		{
			_editor = editor;
		}

		public bool IsVisible => _signaturePopup.IsOpen;

		public bool IsActiveOrPending => _signaturePopup.IsOpen || _signatureRequestInFlight || _signatureRefreshPending;

		public void InitializePopup()
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

		public void Dismiss()
		{
			CancelPendingRefresh();
			InvalidateRequests();

			if (_signaturePopup.IsOpen)
				_signaturePopup.IsOpen = false;

			_signaturePopupPresenter.Content = null;
		}

		public Task RequestAsync(int offset)
			=> RequestAsyncCore(offset);

		public void ScheduleRefresh()
		{
			_pendingSignatureHelpOffset = _editor.CaretOffset;
			_signatureRefreshPending = true;
			_signatureRefreshTimer.Stop();
			_signatureRefreshTimer.Start();
		}

		public void CancelPendingRefresh()
		{
			_signatureRefreshTimer.Stop();
			_signatureRefreshPending = false;
			_pendingSignatureHelpOffset = -1;
		}

		public void InvalidateRequests()
		{
			_signatureRequestToken++;
			_signatureRequestInFlight = false;
		}

		public void HandleRefreshTimerTick(object? sender, EventArgs e)
			=> SignatureRefreshTimer_Tick(sender, e);

		private void ShowToolTip(LuaSignatureInfo signatureInfo)
		{
			double availablePopupWidth = Math.Max(0.0, Math.Min(500.0, _editor.ActualWidth - 16.0));

			double popupHorizontalPadding = _signaturePopupBorder.Padding.Left
				+ _signaturePopupBorder.Padding.Right
				+ _signaturePopupBorder.BorderThickness.Left
				+ _signaturePopupBorder.BorderThickness.Right;

			double popupVerticalPadding = _signaturePopupBorder.Padding.Top
				+ _signaturePopupBorder.Padding.Bottom
				+ _signaturePopupBorder.BorderThickness.Top
				+ _signaturePopupBorder.BorderThickness.Bottom;

			double contentMaxWidth = Math.Max(0.0, availablePopupWidth - popupHorizontalPadding);

			StackPanel panel = CreatePanel(signatureInfo, contentMaxWidth);

			panel.Measure(new Size(contentMaxWidth, double.PositiveInfinity));

			Size popupSize = new(
				Math.Min(availablePopupWidth, panel.DesiredSize.Width + popupHorizontalPadding),
				panel.DesiredSize.Height + popupVerticalPadding);

			_signaturePopupPresenter.Content = panel;
			_signaturePopupPresenter.InvalidateMeasure();
			_signaturePopupBorder.InvalidateMeasure();
			PositionPopup(popupSize);

			if (!_signaturePopup.IsOpen)
				_signaturePopup.IsOpen = true;
		}

		private StackPanel CreatePanel(LuaSignatureInfo signatureInfo, double contentMaxWidth)
		{
			LuaThemeBrushSet brushSet = _editor.GetThemeBrushSet();
			var panel = new StackPanel { MaxWidth = contentMaxWidth };
			panel.Children.Add(BuildSignatureBlock(signatureInfo, brushSet));

			if (!string.IsNullOrWhiteSpace(signatureInfo.Documentation))
				panel.Children.Add(CreateSignatureDocumentationBlock(signatureInfo.Documentation, brushSet));

			if (signatureInfo.ActiveParameter < signatureInfo.Parameters.Count)
			{
				LuaParameterInfo activeParameter = signatureInfo.Parameters[signatureInfo.ActiveParameter];

				if (!string.IsNullOrWhiteSpace(activeParameter.Documentation))
					panel.Children.Add(CreateSignatureDocumentationBlock(activeParameter.Label + ": " + activeParameter.Documentation, brushSet));
			}

			return panel;
		}

		private void PositionPopup(Size popupSize)
		{
			_editor.AttachHostWindowHandlers();
			_signaturePopup.PlacementTarget = _editor;
			_editor.TextArea.TextView.EnsureVisualLines();

			Rect caretRectangle = _editor.TextArea.Caret.CalculateCaretRectangle();
			Vector scrollOffset = _editor.TextArea.TextView.ScrollOffset;

			Point caretViewportPoint = new(
				caretRectangle.X - scrollOffset.X,
				caretRectangle.Y - scrollOffset.Y);

			Point editorPoint = _editor.TextArea.TextView.TranslatePoint(caretViewportPoint, _editor);
			double lineHeight = Math.Max(_editor.TextArea.TextView.DefaultLineHeight, caretRectangle.Height);
			double lineSlack = Math.Max(0.0, lineHeight - caretRectangle.Height);
			double horizontalOffset = Math.Max(0.0, editorPoint.X + 2.0);
			double maxHorizontalOffset = Math.Max(0.0, _editor.ActualWidth - popupSize.Width - 8.0);
			horizontalOffset = Math.Min(horizontalOffset, maxHorizontalOffset);

			double verticalOffset = editorPoint.Y - popupSize.Height - lineSlack - 8.0;

			if (verticalOffset < 0.0)
				verticalOffset = Math.Min(Math.Max(0.0, _editor.ActualHeight - popupSize.Height), editorPoint.Y + lineHeight + 4.0);

			_signaturePopup.HorizontalOffset = horizontalOffset;
			_signaturePopup.VerticalOffset = verticalOffset;
		}

		private async Task RequestAsyncCore(int offset)
		{
			if (_signatureRequestInFlight)
			{
				_pendingSignatureHelpOffset = offset;
				_signatureRefreshPending = true;
				return;
			}

			_signatureRequestInFlight = true;

			if (!_editor.IsIntellisenseAvailable())
			{
				_signatureRequestInFlight = false;
				Dismiss();
				return;
			}

			var intellisenseProvider = _editor.IntellisenseProvider;

			if (intellisenseProvider is null)
			{
				_signatureRequestInFlight = false;
				Dismiss();
				return;
			}

			CancellationToken cancellationToken = CancellationToken.None;
			int requestToken = ++_signatureRequestToken;
			int requestDocumentVersion = _editor._editorDocumentVersion;
			int requestGeneration = _editor._editorRequestGeneration;

			try
			{
				(int line, int column) = _editor.GetPositionFromOffset(offset);

				LuaSignatureInfo? signatureInfo = await intellisenseProvider
					.GetSignatureHelpAsync(_editor.FilePath, _editor.Text, line, column, cancellationToken)
					.ConfigureAwait(true);

				if (!_editor.IsAsyncEditorResultCurrent(cancellationToken, requestToken, _signatureRequestToken,
					requestDocumentVersion, requestGeneration))
				{
					return;
				}

				if (signatureInfo is null)
				{
					Dismiss();
					return;
				}

				ShowToolTip(signatureInfo);
			}
			catch (OperationCanceledException)
			{
			}
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

		private async void SignatureRefreshTimer_Tick(object? sender, EventArgs e)
		{
			_signatureRefreshTimer.Stop();

			if (!_signatureRefreshPending || _pendingSignatureHelpOffset < 0)
				return;

			if (_signatureRequestInFlight)
				return;

			int offset = _pendingSignatureHelpOffset;
			_signatureRefreshPending = false;
			await RequestAsyncCore(offset).ConfigureAwait(true);
		}
	}
}