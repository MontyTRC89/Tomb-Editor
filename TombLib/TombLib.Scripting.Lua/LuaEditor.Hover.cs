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

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private CancellationTokenSource? _hoverCancellationTokenSource;
	private int _hoverRequestToken;

	protected override async void HandleMouseHover(MouseEventArgs e)
	{
		int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

		if (hoveredOffset == -1)
			return;

		bool hasDiagnostic = TryGetDiagnosticInfo(hoveredOffset, out string diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity, allowLineFallback: false);
		bool canShowDiagnosticFallback = _completionWindow is null && !_signaturePopup.IsOpen;

		if (!TryGetHoverRequestOffset(hoveredOffset, out int hoverOffset) || !IsIntellisenseAvailable())
		{
			ShowDiagnosticToolTipIfAvailable(canShowDiagnosticFallback, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
			return;
		}

		CancellationToken cancellationToken = ResetCancellationTokenSource(ref _hoverCancellationTokenSource);
		int hoverRequestToken = ++_hoverRequestToken;

		try
		{
			LuaHoverInfo? hoverInfo = await RequestHoverAsync(hoverOffset, cancellationToken).ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested || hoverRequestToken != _hoverRequestToken)
				return;

			int currentHoveredOffset = GetOffsetFromPoint(Mouse.GetPosition(this));

			if (!LuaEditorInteractionRules.TryGetHoverOffset(Document, currentHoveredOffset, out int currentHoverOffset)
				|| currentHoverOffset != hoverOffset)
			{
				return;
			}

			hasDiagnostic = TryGetDiagnosticInfo(currentHoveredOffset, out diagnosticMessage, out diagnosticSeverity, allowLineFallback: false);
			ShowBestHoverToolTip(hoverInfo, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			LogEditorFailure("Hover request", exception);
			ShowDiagnosticToolTipIfAvailable(canShowDiagnosticFallback, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
		}
	}

	private bool TryGetHoverRequestOffset(int hoveredOffset, out int hoverOffset)
	{
		hoverOffset = 0;

		if (!LuaEditorInteractionRules.CanRequestHover(_completionWindow is not null, _signaturePopup.IsOpen))
			return false;

		if (!LuaEditorInteractionRules.TryGetHoverOffset(Document, hoveredOffset, out hoverOffset))
			return false;

		return !string.IsNullOrWhiteSpace(GetWordFromOffset(hoverOffset));
	}

	private void ShowDiagnosticToolTipIfAvailable(bool canShowDiagnosticFallback, bool hasDiagnostic, string diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
	{
		if (canShowDiagnosticFallback && hasDiagnostic)
			ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
	}

	private void ShowBestHoverToolTip(LuaHoverInfo? hoverInfo, bool hasDiagnostic, string diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
	{
		// Recompute eligibility: a completion window or signature popup may have opened during the
		// asynchronous hover request; we must not paint a hover tooltip on top of either of them.
		bool canShowToolTip = _completionWindow is null && !_signaturePopup.IsOpen;

		if (!canShowToolTip)
			return;

		LuaHoverInfo? displayableHoverInfo = hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content)
			? hoverInfo
			: null;

		if (displayableHoverInfo is not null && hasDiagnostic)
			ShowCombinedHoverAndDiagnosticToolTip(displayableHoverInfo, diagnosticMessage, diagnosticSeverity);
		else if (displayableHoverInfo is not null)
			ShowHoverToolTip(displayableHoverInfo);
		else if (hasDiagnostic)
			ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
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

	private static FrameworkElement CreateHoverToolTipContent(LuaHoverInfo hoverInfo) => hoverInfo.IsMarkdown
		? MarkdownToolTipRenderer.CreateContent(hoverInfo.Content, ToolTipForeground, DefaultToolTipBackground)
		: MarkdownToolTipRenderer.CreatePlainTextContent(hoverInfo.Content, ToolTipForeground);

	private async Task<LuaHoverInfo?> RequestHoverAsync(int offset, CancellationToken cancellationToken)
	{
		if (!IsIntellisenseAvailable())
			return null;

		(int Line, int Column) = GetPositionFromOffset(offset);

		return await IntellisenseProvider
			.GetHoverAsync(FilePath, Text, Line, Column, cancellationToken)
			.ConfigureAwait(true);
	}
}
