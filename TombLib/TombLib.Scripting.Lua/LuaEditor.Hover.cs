using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Utils;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Rendering;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private CancellationTokenSource? _hoverCancellationTokenSource;
		private int _hoverRequestToken;

		protected override async void HandleMouseHover(MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity);

			bool isCompletionWindowOpen = _completionWindow is not null;

			if (!LuaEditorInteractionRules.CanRequestHover(Document, hoveredOffset, isCompletionWindowOpen, _signaturePopup.IsOpen))
			{
				if (!isCompletionWindowOpen && !_signaturePopup.IsOpen && hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);

				return;
			}

			if (!IsIntellisenseAvailable())
			{
				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);

				return;
			}

			string hoveredWord = GetWordFromOffset(hoveredOffset);

			if (string.IsNullOrWhiteSpace(hoveredWord))
			{
				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);

				return;
			}

			CancellationToken cancellationToken = ResetCancellationTokenSource(ref _hoverCancellationTokenSource);
			int hoverRequestToken = ++_hoverRequestToken;

			try
			{
				LuaHoverInfo? hoverInfo = await RequestHoverAsync(hoveredOffset, cancellationToken).ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested || hoverRequestToken != _hoverRequestToken)
					return;

				int currentHoveredOffset = GetOffsetFromPoint(Mouse.GetPosition(this));

				if (currentHoveredOffset != hoveredOffset)
					return;

				if (hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content) && hasDiagnostic)
					ShowCombinedHoverAndDiagnosticToolTip(hoverInfo, diagnosticMessage, diagnosticSeverity);
				else if (hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content))
					ShowHoverToolTip(hoverInfo);
				else if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				WriteDebugFailure("Hover request", exception);

				if (hasDiagnostic)
					ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
			}
		}

		private void ShowCombinedHoverAndDiagnosticToolTip(LuaHoverInfo hoverInfo, string diagnosticMessage, TextEditorDiagnosticSeverity severity)
		{
			GetDiagnosticToolTipColors(severity, out SolidColorBrush diagnosticBorder, out SolidColorBrush diagnosticBackground);

			var panel = new StackPanel { MaxWidth = ToolTipTextMaxWidth };

			panel.Children.Add(CreateHoverToolTipContent(hoverInfo));

			panel.Children.Add(new Border
			{
				Background = diagnosticBackground,
				BorderBrush = diagnosticBorder,
				BorderThickness = new Thickness(1.0),
				CornerRadius = new CornerRadius(3.0),
				Padding = new Thickness(8.0, 4.0, 8.0, 4.0),
				Margin = new Thickness(0.0, 6.0, 0.0, 0.0),
				Child = new TextBlock
				{
					Text = diagnosticMessage,
					Foreground = ToolTipForeground,
					FontFamily = SystemFonts.MessageFontFamily,
					FontSize = ToolTipTextFontSize,
					TextWrapping = TextWrapping.Wrap,
					MaxWidth = ToolTipTextMaxWidth
				}
			});

			ShowToolTip(panel, DefaultToolTipBorder, DefaultToolTipBackground);
		}

		private void ShowHoverToolTip(LuaHoverInfo hoverInfo)
			=> ShowToolTip(CreateHoverToolTipContent(hoverInfo), DefaultToolTipBorder, DefaultToolTipBackground);

		private FrameworkElement CreateHoverToolTipContent(LuaHoverInfo hoverInfo)
		{
			if (hoverInfo.IsMarkdown)
				return MarkdownToolTipRenderer.CreateContent(hoverInfo.Content, ToolTipForeground, DefaultToolTipBackground);

			return MarkdownToolTipRenderer.CreatePlainTextContent(hoverInfo.Content, ToolTipForeground);
		}

		private async Task<LuaHoverInfo?> RequestHoverAsync(int offset, CancellationToken cancellationToken)
		{
			if (!IsIntellisenseAvailable())
				return null;

			(int line, int column) = GetPositionFromOffset(offset);

			return await IntellisenseProvider
				.GetHoverAsync(FilePath, Text, line, column, cancellationToken)
				.ConfigureAwait(true);
		}
	}
}