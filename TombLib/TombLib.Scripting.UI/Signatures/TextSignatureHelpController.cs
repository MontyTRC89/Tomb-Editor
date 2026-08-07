#nullable enable

using Nickelony.LanguageServer.Abstractions.Signatures;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Signatures;

/// <summary>
/// Coordinates shared signature-help request state, refresh scheduling, and optional presentation updates.
/// </summary>
public sealed class TextSignatureHelpController
{
	private readonly Func<int> _getCurrentCaretOffset;
	private readonly Func<int, int, Task<TextSignatureHelpInfo?>> _requestSignatureHelpAsync;
	private readonly Action<TextSignatureHelpInfo> _showSignatureHelp;
	private readonly Action _dismissSignatureHelp;
	private readonly Action<TextSignatureHelpPresentationState>? _applySignatureState;
	private readonly Action<Exception>? _handleRequestFailure;
	private readonly DispatcherTimer _refreshTimer = new();

	private int _signatureRequestToken;
	private int _pendingSignatureHelpOffset = -1;
	private bool _signatureRefreshPending;
	private bool _signatureRequestInFlight;
	private bool _isVisible;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextSignatureHelpController"/> class.
	/// </summary>
	public TextSignatureHelpController(
		Func<int> getCurrentCaretOffset,
		Func<int, int, Task<TextSignatureHelpInfo?>> requestSignatureHelpAsync,
		Action<TextSignatureHelpInfo> showSignatureHelp,
		Action dismissSignatureHelp,
		Action<TextSignatureHelpPresentationState>? applySignatureState = null,
		Action<Exception>? handleRequestFailure = null,
		double refreshDebounceDelayInMilliseconds = 50.0)
	{
		ArgumentNullException.ThrowIfNull(getCurrentCaretOffset);
		ArgumentNullException.ThrowIfNull(requestSignatureHelpAsync);
		ArgumentNullException.ThrowIfNull(showSignatureHelp);
		ArgumentNullException.ThrowIfNull(dismissSignatureHelp);

		_getCurrentCaretOffset = getCurrentCaretOffset;
		_requestSignatureHelpAsync = requestSignatureHelpAsync;
		_showSignatureHelp = showSignatureHelp;
		_dismissSignatureHelp = dismissSignatureHelp;
		_applySignatureState = applySignatureState;
		_handleRequestFailure = handleRequestFailure;

		_refreshTimer.Interval = TimeSpan.FromMilliseconds(refreshDebounceDelayInMilliseconds);
		_refreshTimer.Tick += RefreshTimer_Tick;
	}

	/// <summary>
	/// Gets the currently displayed signature-help state, if any.
	/// </summary>
	public TextSignatureHelpInfo? CurrentSignatureHelp { get; private set; }

	/// <summary>
	/// Gets the current shared signature-help presentation state.
	/// </summary>
	public TextSignatureHelpPresentationState CurrentPresentation { get; private set; } = TextSignatureHelpPresentationState.Empty;

	/// <summary>
	/// Gets a value indicating whether signature help is currently visible.
	/// </summary>
	public bool IsVisible => _isVisible;

	/// <summary>
	/// Gets a value indicating whether signature help is visible or a request is pending.
	/// </summary>
	public bool IsActiveOrPending => _isVisible || _signatureRequestInFlight || _signatureRefreshPending;

	/// <summary>
	/// Dismisses the current signature-help presentation and invalidates pending work.
	/// </summary>
	public void Dismiss()
	{
		CancelPendingRefresh();
		InvalidateRequests();
		DismissPresentation();
	}

	/// <summary>
	/// Requests signature help at the specified offset.
	/// </summary>
	public Task RequestAsync(int offset)
		=> RequestAsyncCore(offset);

	/// <summary>
	/// Schedules a debounced refresh at the current caret offset.
	/// </summary>
	public void ScheduleRefresh()
	{
		_pendingSignatureHelpOffset = _getCurrentCaretOffset();
		_signatureRefreshPending = true;
		_refreshTimer.Stop();
		_refreshTimer.Start();
		ApplyPresentationState();
	}

	/// <summary>
	/// Cancels any pending debounced refresh.
	/// </summary>
	public void CancelPendingRefresh()
	{
		_refreshTimer.Stop();
		_signatureRefreshPending = false;
		_pendingSignatureHelpOffset = -1;
		ApplyPresentationState();
	}

	/// <summary>
	/// Marks outstanding requests as stale so completed results are ignored.
	/// </summary>
	public void InvalidateRequests()
	{
		_signatureRequestToken++;
		_signatureRequestInFlight = false;
		ApplyPresentationState();
	}

	private void ShowSignatureHelp(TextSignatureHelpInfo signatureInfo)
	{
		CurrentSignatureHelp = signatureInfo;
		_showSignatureHelp(signatureInfo);
		_isVisible = true;
		ApplyPresentationState();
	}

	private void DismissPresentation()
	{
		CurrentSignatureHelp = null;
		_dismissSignatureHelp();
		_isVisible = false;
		ApplyPresentationState();
	}

	private void ApplyPresentationState()
	{
		CurrentPresentation = new TextSignatureHelpPresentationState(
			CurrentSignatureHelp,
			_isVisible,
			_signatureRequestInFlight,
			_signatureRefreshPending);
		_applySignatureState?.Invoke(CurrentPresentation);
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
		bool wasVisibleAtRequestStart = _isVisible;
		int requestToken = ++_signatureRequestToken;
		ApplyPresentationState();

		try
		{
			TextSignatureHelpInfo? signatureInfo = await _requestSignatureHelpAsync(offset, requestToken).ConfigureAwait(true);

			if (requestToken != _signatureRequestToken)
				return;

			if (signatureInfo is null)
			{
				if (!wasVisibleAtRequestStart)
					DismissPresentation();

				return;
			}

			ShowSignatureHelp(signatureInfo);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			_handleRequestFailure?.Invoke(exception);
		}
		finally
		{
			_signatureRequestInFlight = false;
			ApplyPresentationState();

			if (_signatureRefreshPending && _pendingSignatureHelpOffset >= 0)
			{
				_refreshTimer.Stop();
				_refreshTimer.Start();
			}
		}
	}

	private async void RefreshTimer_Tick(object? sender, EventArgs e)
	{
		_refreshTimer.Stop();

		if (!_signatureRefreshPending || _pendingSignatureHelpOffset < 0)
			return;

		if (_signatureRequestInFlight)
			return;

		int offset = _pendingSignatureHelpOffset;
		_signatureRefreshPending = false;
		await RequestAsyncCore(offset).ConfigureAwait(true);
	}
}
