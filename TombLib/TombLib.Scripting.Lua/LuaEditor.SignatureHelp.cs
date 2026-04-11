using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private static readonly SolidColorBrush SignatureParamDocForeground = CreateFrozenBrush(Color.FromRgb(180, 180, 180));
		private static readonly SolidColorBrush SignatureActiveParamForeground = CreateFrozenBrush(Color.FromRgb(86, 180, 235));
		private static readonly SolidColorBrush SignatureForeground = CreateFrozenBrush(Colors.Gainsboro);

		private readonly Popup _signaturePopup = new Popup();
		private readonly Border _signaturePopupBorder = new Border();
		private readonly ContentPresenter _signaturePopupPresenter = new ContentPresenter();

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
			_signatureCancellationTokenSource?.Cancel();

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

			var panel = new StackPanel { MaxWidth = contentMaxWidth };
			panel.Children.Add(BuildSignatureBlock(signatureInfo));

			if (signatureInfo.ActiveParameter < signatureInfo.Parameters.Count)
			{
				LuaParameterInfo activeParam = signatureInfo.Parameters[signatureInfo.ActiveParameter];

				if (!string.IsNullOrWhiteSpace(activeParam.Documentation))
				{
					var parameterDocumentation = new TextBlock
					{
						Text = activeParam.Label + ": " + activeParam.Documentation,
						Foreground = SignatureParamDocForeground,
						FontFamily = SystemFonts.MessageFontFamily,
						FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0),
						TextWrapping = TextWrapping.Wrap,
						Margin = new Thickness(0.0, 4.0, 0.0, 0.0)
					};

					panel.Children.Add(parameterDocumentation);
				}
			}

			panel.Measure(new Size(contentMaxWidth, double.PositiveInfinity));
			Size popupSize = new Size(
				Math.Min(availablePopupWidth, panel.DesiredSize.Width + popupHorizontalPadding),
				panel.DesiredSize.Height + popupVerticalPadding);

			if (_signaturePopup.IsOpen)
				_signaturePopup.IsOpen = false;

			_signaturePopupPresenter.Content = panel;
			_signaturePopupPresenter.InvalidateMeasure();
			_signaturePopupBorder.InvalidateMeasure();
			AttachHostWindowHandlers();
			_signaturePopup.PlacementTarget = this;
			TextArea.TextView.EnsureVisualLines();

			Rect caretRectangle = TextArea.Caret.CalculateCaretRectangle();
			Vector scrollOffset = TextArea.TextView.ScrollOffset;
			Point caretViewportPoint = new Point(
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
			_signaturePopup.IsOpen = true;
		}

		private static TextBlock BuildSignatureBlock(LuaSignatureInfo signatureInfo)
		{
			var textBlock = new TextBlock
			{
				FontFamily = new FontFamily("Consolas"),
				FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, 14.0),
				TextWrapping = TextWrapping.Wrap,
				Foreground = SignatureForeground
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
					run.Foreground = SignatureActiveParamForeground;
				}

				textBlock.Inlines.Add(run);
			}

			textBlock.Inlines.Add(new Run(")"));
			return textBlock;
		}
	}
}