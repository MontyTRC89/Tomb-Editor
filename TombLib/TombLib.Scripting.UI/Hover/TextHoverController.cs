using Nickelony.LanguageServer.Abstractions.Diagnostics;
using Nickelony.LanguageServer.Abstractions.Hover;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Hover;

/// <summary>
/// Coordinates hover requests, request invalidation, and hover-versus-diagnostic tooltip display.
/// </summary>
public sealed class TextHoverController
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly FrameworkElement _owner;
	private readonly Func<Point, int> _getOffsetFromPoint;
	private readonly Func<int, TextHoverRequestState> _buildRequestState;
	private readonly Func<int, CancellationToken, Task<TextHoverInfo?>> _requestHoverAsync;
	private readonly Func<int, int?> _getCurrentRequestOffset;
	private readonly Action<string, TextEditorDiagnosticSeverity> _showDiagnosticToolTip;
	private readonly Action<TextHoverInfo> _showHoverToolTip;
	private readonly Action<TextHoverInfo, string, TextEditorDiagnosticSeverity> _showCombinedToolTip;
	private readonly Action<TextHoverPresentationState>? _applyHoverState;
	private readonly Action<Exception>? _handleRequestFailure;

	private CancellationTokenSource? _hoverCancellationTokenSource;
	private readonly RequestTokenSource _hoverRequestTokens = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="TextHoverController"/> class.
	/// </summary>
	public TextHoverController(
		FrameworkElement owner,
		Func<Point, int> getOffsetFromPoint,
		Func<int, TextHoverRequestState> buildRequestState,
		Func<int, CancellationToken, Task<TextHoverInfo?>> requestHoverAsync,
		Func<int, int?> getCurrentRequestOffset,
		Action<string, TextEditorDiagnosticSeverity> showDiagnosticToolTip,
		Action<TextHoverInfo> showHoverToolTip,
		Action<TextHoverInfo, string, TextEditorDiagnosticSeverity> showCombinedToolTip,
		Action<TextHoverPresentationState>? applyHoverState = null,
		Action<Exception>? handleRequestFailure = null)
	{
		ArgumentNullException.ThrowIfNull(owner);
		ArgumentNullException.ThrowIfNull(getOffsetFromPoint);
		ArgumentNullException.ThrowIfNull(buildRequestState);
		ArgumentNullException.ThrowIfNull(requestHoverAsync);
		ArgumentNullException.ThrowIfNull(getCurrentRequestOffset);
		ArgumentNullException.ThrowIfNull(showDiagnosticToolTip);
		ArgumentNullException.ThrowIfNull(showHoverToolTip);
		ArgumentNullException.ThrowIfNull(showCombinedToolTip);

		_owner = owner;
		_getOffsetFromPoint = getOffsetFromPoint;
		_buildRequestState = buildRequestState;
		_requestHoverAsync = requestHoverAsync;
		_getCurrentRequestOffset = getCurrentRequestOffset;
		_showDiagnosticToolTip = showDiagnosticToolTip;
		_showHoverToolTip = showHoverToolTip;
		_showCombinedToolTip = showCombinedToolTip;
		_applyHoverState = applyHoverState;
		_handleRequestFailure = handleRequestFailure;
	}

	/// <summary>
	/// Gets the current shared hover presentation state.
	/// </summary>
	public TextHoverPresentationState CurrentPresentation { get; private set; } = TextHoverPresentationState.Empty();

	/// <summary>
	/// Handles a mouse-hover event and shows hover or diagnostic content when appropriate.
	/// </summary>
	public async Task HandleMouseHoverAsync(MouseEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);

		int hoveredOffset = -1;
		TextHoverRequestState requestState = default;

		try
		{
			hoveredOffset = _getOffsetFromPoint(e.GetPosition(_owner));

			if (hoveredOffset == -1)
			{
				ApplyHoverState(TextHoverPresentationState.Empty(hoveredOffset));
				return;
			}

			requestState = _buildRequestState(hoveredOffset);

			if (!requestState.ShouldRequestHover)
			{
				ApplyHoverState(CreatePresentationState(hoveredOffset, requestState, null));
				ShowDiagnosticToolTipIfAvailable(requestState);
				return;
			}

			CancellationToken cancellationToken = ResetCancellationTokenSource();
			int hoverRequestToken = _hoverRequestTokens.Begin();

			TextHoverInfo? hoverInfo = await _requestHoverAsync(requestState.RequestOffset, cancellationToken).ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested || !_hoverRequestTokens.IsCurrent(hoverRequestToken))
				return;

			int currentHoveredOffset = _getOffsetFromPoint(Mouse.GetPosition(_owner));

			if (currentHoveredOffset == -1)
				return;

			int? currentRequestOffset = _getCurrentRequestOffset(currentHoveredOffset);

			if (!currentRequestOffset.HasValue || currentRequestOffset.Value != requestState.RequestOffset)
				return;

			TextHoverRequestState displayState = _buildRequestState(currentHoveredOffset);
			ShowBestToolTip(hoverInfo, displayState);
			ApplyHoverState(CreatePresentationState(currentHoveredOffset, displayState, GetDisplayableHoverInfo(hoverInfo)));
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			if (_handleRequestFailure is null)
				Log.Warn(exception, "Hover request failed.");
			else
				_handleRequestFailure(exception);

			if (hoveredOffset >= 0)
			{
				ShowDiagnosticToolTipIfAvailable(requestState);
				ApplyHoverState(CreatePresentationState(hoveredOffset, requestState, null));
			}
		}
	}

	/// <summary>
	/// Cancels the current hover request, if one is still in flight.
	/// </summary>
	public void CancelPendingRequest()
	{
		_hoverCancellationTokenSource?.Cancel();
		_hoverCancellationTokenSource?.Dispose();
		_hoverCancellationTokenSource = null;
	}

	/// <summary>
	/// Marks outstanding hover requests as stale so completed results are ignored.
	/// </summary>
	public void InvalidateRequests()
		=> _hoverRequestTokens.Invalidate();

	private void ApplyHoverState(TextHoverPresentationState state)
	{
		CurrentPresentation = state;
		_applyHoverState?.Invoke(state);
	}

	private static TextHoverPresentationState CreatePresentationState(int hoveredOffset, TextHoverRequestState requestState, TextHoverInfo? hoverInfo)
		=> new(
			HoveredOffset: hoveredOffset,
			RequestOffset: requestState.ShouldRequestHover ? requestState.RequestOffset : -1,
			HoverInfo: hoverInfo,
			HasDiagnostic: requestState.HasDiagnostic,
			DiagnosticMessage: requestState.DiagnosticMessage,
			DiagnosticSeverity: requestState.DiagnosticSeverity,
			CanShowToolTip: requestState.CanShowToolTip,
			CanShowDiagnosticFallback: requestState.CanShowDiagnosticFallback);

	private static TextHoverInfo? GetDisplayableHoverInfo(TextHoverInfo? hoverInfo)
		=> hoverInfo is not null && !string.IsNullOrWhiteSpace(hoverInfo.Content)
			? hoverInfo
			: null;

	private CancellationToken ResetCancellationTokenSource()
	{
		_hoverCancellationTokenSource?.Cancel();
		_hoverCancellationTokenSource?.Dispose();
		_hoverCancellationTokenSource = new CancellationTokenSource();
		return _hoverCancellationTokenSource.Token;
	}

	private void ShowDiagnosticToolTipIfAvailable(TextHoverRequestState requestState)
	{
		if (requestState.CanShowDiagnosticFallback
			&& requestState.HasDiagnostic
			&& !string.IsNullOrWhiteSpace(requestState.DiagnosticMessage))
		{
			_showDiagnosticToolTip(requestState.DiagnosticMessage, requestState.DiagnosticSeverity);
		}
	}

	private void ShowBestToolTip(TextHoverInfo? hoverInfo, TextHoverRequestState requestState)
	{
		if (!requestState.CanShowToolTip)
			return;

		TextHoverInfo? displayableHoverInfo = GetDisplayableHoverInfo(hoverInfo);
		bool hasDisplayableDiagnostic = requestState.HasDiagnostic && !string.IsNullOrWhiteSpace(requestState.DiagnosticMessage);
		string diagnosticText = requestState.DiagnosticMessage ?? string.Empty;

		if (displayableHoverInfo is not null && hasDisplayableDiagnostic)
			_showCombinedToolTip(displayableHoverInfo, diagnosticText, requestState.DiagnosticSeverity);
		else if (displayableHoverInfo is not null)
			_showHoverToolTip(displayableHoverInfo);
		else if (hasDisplayableDiagnostic)
			_showDiagnosticToolTip(diagnosticText, requestState.DiagnosticSeverity);
	}
}
