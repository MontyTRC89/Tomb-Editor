using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private CancellationTokenSource? _signatureCancellationTokenSource;

	private readonly Popup _signaturePopup = new();
	private readonly Border _signaturePopupBorder = new();
	private readonly ContentPresenter _signaturePopupPresenter = new();

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
	}

	private void DismissSignatureHelp()
	{
		CancelAndDispose(ref _signatureCancellationTokenSource);

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

		if (_signaturePopup.IsOpen)
			_signaturePopup.IsOpen = false;

		_signaturePopupPresenter.Content = panel;
		_signaturePopupPresenter.InvalidateMeasure();
		_signaturePopupBorder.InvalidateMeasure();
		PositionSignaturePopup(popupSize);

		_signaturePopup.IsOpen = true;
	}

	private StackPanel CreateSignaturePanel(LuaSignatureInfo signatureInfo, double contentMaxWidth)
	{
		LuaThemeBrushSet brushSet = GetThemeBrushSet();
		var panel = new StackPanel { MaxWidth = contentMaxWidth };
		panel.Children.Add(BuildSignatureBlock(signatureInfo, brushSet));

		if (!string.IsNullOrWhiteSpace(signatureInfo.Documentation))
		{
			panel.Children.Add(new TextBlock
			{
				Text = signatureInfo.Documentation,
				Foreground = brushSet.SignatureParamDocForeground,
				FontFamily = SystemFonts.MessageFontFamily,
				FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0),
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0.0, 4.0, 0.0, 0.0)
			});
		}

		if (signatureInfo.ActiveParameter < signatureInfo.Parameters.Count)
		{
			LuaParameterInfo activeParam = signatureInfo.Parameters[signatureInfo.ActiveParameter];

			if (!string.IsNullOrWhiteSpace(activeParam.Documentation))
			{
				panel.Children.Add(new TextBlock
				{
					Text = activeParam.Label + ": " + activeParam.Documentation,
					Foreground = brushSet.SignatureParamDocForeground,
					FontFamily = SystemFonts.MessageFontFamily,
					FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0),
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(0.0, 4.0, 0.0, 0.0)
				});
			}
		}

		return panel;
	}

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
		double horizontalOffset = Math.Max(0.0, editorPoint.X + 2.0f);
		double maxHorizontalOffset = Math.Max(0.0, ActualWidth - popupSize.Width - 8.0f);
		horizontalOffset = Math.Min(horizontalOffset, maxHorizontalOffset);

		double verticalOffset = editorPoint.Y - popupSize.Height - lineSlack - 8.0f;

		if (verticalOffset < 0.0)
			verticalOffset = Math.Min(Math.Max(0.0, ActualHeight - popupSize.Height), editorPoint.Y + lineHeight + 4.0f);

		_signaturePopup.HorizontalOffset = horizontalOffset;
		_signaturePopup.VerticalOffset = verticalOffset;
	}

	private async Task RequestSignatureHelpAsync(int offset)
	{
		if (!IsIntellisenseAvailable())
		{
			DismissSignatureHelp();
			return;
		}

		CancellationToken cancellationToken = ResetCancellationTokenSource(ref _signatureCancellationTokenSource);

		try
		{
			(int line, int column) = GetPositionFromOffset(offset);

			LuaSignatureInfo? signatureInfo = await IntellisenseProvider
				.GetSignatureHelpAsync(FilePath, Text, line, column, cancellationToken)
				.ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested)
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
			WriteDebugFailure("Signature help", exception);
		}
	}

	private void ScheduleSignatureHelpRefresh()
		=> Dispatcher.BeginInvoke(new Action(() => _ = RequestSignatureHelpAsync(CaretOffset)));

	private static TextBlock BuildSignatureBlock(LuaSignatureInfo signatureInfo, LuaThemeBrushSet brushSet)
	{
		var textBlock = new TextBlock
		{
			FontFamily = new FontFamily("Consolas"),
			FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0),
			TextWrapping = TextWrapping.Wrap,
			Foreground = brushSet.SignatureForeground
		};

		string label = signatureInfo.Label;
		int paramStart = label.IndexOf('(');

		if (paramStart < 0 || signatureInfo.Parameters.Count == 0)
		{
			textBlock.Text = label;
			return textBlock;
		}

		textBlock.Inlines.Add(new Run(label[..(paramStart + 1)]));

		string paramSection = label[(paramStart + 1)..];
		int closingParen = paramSection.LastIndexOf(')');

		if (closingParen >= 0)
			paramSection = paramSection[..closingParen];

		string[] parameterNames = paramSection.Split(',');

		for (int i = 0; i < parameterNames.Length; i++)
		{
			if (i > 0)
				textBlock.Inlines.Add(new Run(", "));

			var run = new Run(parameterNames[i].Trim());

			if (i == signatureInfo.ActiveParameter)
			{
				run.FontWeight = FontWeights.Bold;
				run.Foreground = brushSet.SignatureActiveParamForeground;
			}

			textBlock.Inlines.Add(run);
		}

		textBlock.Inlines.Add(new Run(")"));
		return textBlock;
	}
}
