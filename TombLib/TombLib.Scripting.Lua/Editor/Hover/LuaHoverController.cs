using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Lua.Utils;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	protected override async void HandleMouseHover(MouseEventArgs e)
		=> await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);

	private static FrameworkElement CreateHoverToolTipContent(LuaHoverInfo hoverInfo) => hoverInfo.IsMarkdown
		? MarkdownToolTipRenderer.CreateContent(hoverInfo.Content, ToolTipForeground, DefaultToolTipBackground)
		: MarkdownToolTipRenderer.CreatePlainTextContent(hoverInfo.Content, ToolTipForeground);

	/// <summary>
	/// Owns Lua hover request state, request eligibility checks, and hover-versus-diagnostic tooltip presentation.
	/// </summary>
	private sealed class LuaHoverController
	{
		private readonly LuaEditor _editor;
		private CancellationTokenSource? _hoverCancellationTokenSource;
		private int _hoverRequestToken;

		internal LuaHoverController(LuaEditor editor)
		{
			_editor = editor;
		}

		internal async Task HandleMouseHoverAsync(MouseEventArgs e)
		{
			int hoveredOffset = _editor.GetOffsetFromPoint(e.GetPosition(_editor));

			if (hoveredOffset == -1)
				return;

			bool hasDiagnostic = _editor.TryGetDiagnosticInfo(hoveredOffset, out string? diagnosticMessage, out TextEditorDiagnosticSeverity diagnosticSeverity, allowLineFallback: false);
			bool canShowDiagnosticFallback = _editor._completionWindow is null && !_editor._signatureHelpController.IsVisible;

			if (!TryGetRequestOffset(hoveredOffset, out int hoverOffset) || !_editor.IsIntellisenseAvailable())
			{
				ShowDiagnosticToolTipIfAvailable(canShowDiagnosticFallback, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
				return;
			}

			CancellationToken cancellationToken = ResetCancellationTokenSource(ref _hoverCancellationTokenSource);
			int hoverRequestToken = ++_hoverRequestToken;

			try
			{
				LuaHoverInfo? hoverInfo = await RequestAsync(hoverOffset, cancellationToken).ConfigureAwait(true);

				if (cancellationToken.IsCancellationRequested || hoverRequestToken != _hoverRequestToken)
					return;

				int currentHoveredOffset = _editor.GetOffsetFromPoint(Mouse.GetPosition(_editor));

				if (!LuaEditorInteractionRules.TryGetHoverOffset(_editor.Document, currentHoveredOffset, out int currentHoverOffset)
					|| currentHoverOffset != hoverOffset)
				{
					return;
				}

				hasDiagnostic = _editor.TryGetDiagnosticInfo(currentHoveredOffset, out diagnosticMessage, out diagnosticSeverity, allowLineFallback: false);
				ShowBestToolTip(hoverInfo, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				LogEditorFailure("Hover request", exception);
				ShowDiagnosticToolTipIfAvailable(canShowDiagnosticFallback, hasDiagnostic, diagnosticMessage, diagnosticSeverity);
			}
		}

		internal void CancelPendingRequest()
			=> CancelAndDispose(ref _hoverCancellationTokenSource);

		internal void InvalidateRequests()
			=> _hoverRequestToken++;

		private bool TryGetRequestOffset(int hoveredOffset, out int hoverOffset)
		{
			hoverOffset = 0;

			if (!LuaEditorInteractionRules.CanRequestHover(_editor._completionWindow is not null, _editor._signatureHelpController.IsVisible))
				return false;

			if (!LuaEditorInteractionRules.TryGetHoverOffset(_editor.Document, hoveredOffset, out hoverOffset))
				return false;

			return !string.IsNullOrWhiteSpace(_editor.GetWordFromOffset(hoverOffset));
		}

		private void ShowDiagnosticToolTipIfAvailable(bool canShowDiagnosticFallback, bool hasDiagnostic, string? diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
		{
			if (canShowDiagnosticFallback && hasDiagnostic && !string.IsNullOrWhiteSpace(diagnosticMessage))
				_editor.ShowDiagnosticToolTip(diagnosticMessage, diagnosticSeverity);
		}

		private void ShowBestToolTip(LuaHoverInfo? hoverInfo, bool hasDiagnostic, string? diagnosticMessage, TextEditorDiagnosticSeverity diagnosticSeverity)
		{
			bool canShowToolTip = _editor._completionWindow is null && !_editor._signatureHelpController.IsVisible;
			bool hasDisplayableDiagnostic = hasDiagnostic && !string.IsNullOrWhiteSpace(diagnosticMessage);
			string diagnosticText = diagnosticMessage ?? string.Empty;

			if (!canShowToolTip)
				return;

			LuaHoverInfo? displayableHoverInfo = hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content)
				? hoverInfo
				: null;

			if (displayableHoverInfo is not null && hasDisplayableDiagnostic)
				ShowCombinedToolTip(displayableHoverInfo, diagnosticText, diagnosticSeverity);
			else if (displayableHoverInfo is not null)
				ShowHoverToolTip(displayableHoverInfo);
			else if (hasDisplayableDiagnostic)
				_editor.ShowDiagnosticToolTip(diagnosticText, diagnosticSeverity);
		}

		private void ShowCombinedToolTip(LuaHoverInfo hoverInfo, string diagnosticMessage, TextEditorDiagnosticSeverity severity)
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

			_editor.ShowToolTip(panel, DefaultToolTipBorder, DefaultToolTipBackground);
		}

		private void ShowHoverToolTip(LuaHoverInfo hoverInfo)
			=> _editor.ShowToolTip(CreateHoverToolTipContent(hoverInfo), DefaultToolTipBorder, DefaultToolTipBackground);

		private async Task<LuaHoverInfo?> RequestAsync(int offset, CancellationToken cancellationToken)
		{
			if (!_editor.IsIntellisenseAvailable())
				return null;

			var intellisenseProvider = _editor.IntellisenseProvider;

			if (intellisenseProvider is null)
				return null;

			(int line, int column) = _editor.GetPositionFromOffset(offset);

			return await intellisenseProvider
				.GetHoverAsync(_editor.FilePath, _editor.Text, line, column, cancellationToken)
				.ConfigureAwait(true);
		}
	}
}