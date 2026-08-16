using Nickelony.LanguageServer.Abstractions.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Threading;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Runs error detection in the background, debounced by an idle timer, and publishes the result
/// through <see cref="RunWorkerCompleted"/>. Detection runs are single-flight: at most one run is
/// active, and content captured while a run is active is coalesced so only the latest content is
/// checked when the active run completes. A newer request cancels the in-flight run's cancellation
/// token, and a result is never published after it has been superseded or the worker disposed.
/// When a silent-session provider is supplied, checks are not started while the session is silent.
/// The worker is created on and confined to the UI thread; the full-document provider call runs on
/// the thread pool because error detection is CPU-bound and the provider contract explicitly
/// permits background execution.
/// </summary>
public sealed class ErrorDetectionWorker : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	// Properties

	/// <summary>
	/// Gets whether a detection run is currently in progress.
	/// </summary>
	public bool IsBusy => _isBusy;

	/// <summary>
	/// Gets or sets the idle debounce interval before a queued check runs.
	/// </summary>
	public TimeSpan IdleDelayInterval
	{
		get => _errorUpdateTimer.Interval;
		set => _errorUpdateTimer.Interval = value;
	}

	/// <summary>
	/// Gets the engine version used for error detection.
	/// </summary>
	public Version EngineVersion { get; }

	// Fields

	private readonly ITextDiagnosticsProvider? _diagnosticsProvider;
	private readonly Dispatcher _dispatcher;
	private readonly DispatcherTimer _errorUpdateTimer = new();
	private readonly Func<bool>? _silentSessionProvider;
	private readonly RequestTokenSource _requestTokens = new();

	private CancellationTokenSource? _requestCancellation;
	private volatile bool _isBusy;
	private string _editorContent = string.Empty;
	private string? _pendingContent;
	private bool _hasPendingContent;
	private bool _isDisposed;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetectionWorker"/> class on the current dispatcher thread.
	/// </summary>
	/// <param name="diagnosticsProvider">The diagnostics provider used to detect errors (optional).</param>
	/// <param name="engineVersion">The engine version used for error detection.</param>
	/// <param name="idleDelayInterval">The idle debounce interval.</param>
	/// <param name="silentSessionProvider">The callback that reports whether the editor is in a silent session (optional).</param>
	public ErrorDetectionWorker(ITextDiagnosticsProvider? diagnosticsProvider, Version engineVersion, TimeSpan idleDelayInterval, Func<bool>? silentSessionProvider = null)
	{
		ArgumentNullException.ThrowIfNull(engineVersion);

		_diagnosticsProvider = diagnosticsProvider;
		_dispatcher = Dispatcher.CurrentDispatcher;
		EngineVersion = engineVersion;
		IdleDelayInterval = idleDelayInterval;
		_silentSessionProvider = silentSessionProvider;

		_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
	}

	// Events

	/// <summary>
	/// Raised when a detection run completes with the resulting diagnostics.
	/// </summary>
	public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

	// Public methods

	/// <summary>
	/// Schedules a detection run after the idle debounce interval. No-op while a silent session is active.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunErrorCheckOnIdle(string? editorContent)
	{
		if (_isDisposed || IsSilentSession())
			return;

		if (_errorUpdateTimer.IsEnabled)
			_errorUpdateTimer.Stop();

		_editorContent = editorContent ?? string.Empty;
		_errorUpdateTimer.Start();
	}

	/// <summary>
	/// Runs a detection check now with the given content, coalescing to the latest content when a
	/// run is already in progress. No-op while a silent session is active.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunErrorCheck(string? editorContent)
	{
		if (_isDisposed || IsSilentSession())
			return;

		if (_diagnosticsProvider is null)
			return;

		_editorContent = editorContent ?? string.Empty;

		// Single-flight policy: an active run is never overlapped; the latest content is retained
		// and checked when the active run completes.
		if (_isBusy)
		{
			_pendingContent = _editorContent;
			_hasPendingContent = true;
			return;
		}

		StartErrorCheck(_editorContent);
	}

	private void ErrorUpdateTimer_Tick(object? sender, EventArgs e)
	{
		_errorUpdateTimer.Stop();

		// Do not start a check when the editor entered a silent session while the timer was pending.
		if (IsSilentSession())
			return;

		RunErrorCheck(_editorContent);
	}

	private bool IsSilentSession()
		=> _silentSessionProvider?.Invoke() ?? false;

	// Private methods

	private void StartErrorCheck(string editorContent)
	{
		_requestCancellation?.Cancel();
		_requestCancellation = new CancellationTokenSource();

		int requestId = _requestTokens.Begin();
		_isBusy = true;

		_ = RunErrorCheckCoreAsync(editorContent, requestId, _requestCancellation.Token);
	}

	private async Task RunErrorCheckCoreAsync(string editorContent, int requestId, CancellationToken cancellationToken)
	{
		Exception? error = null;
		IReadOnlyList<TextEditorDiagnostic> result = [];

		try
		{
			// Full-document error detection is CPU-bound and the provider contract
			// (ITextDiagnosticsProvider) explicitly permits background execution, so the provider
			// runs on the thread pool. Cancellation is cooperative at the run boundary: the token
			// prevents a superseded run from starting and marks a run superseded while in flight.
			result = await Task.Run(() => GetDiagnostics(editorContent), cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Superseded or disposed; a newer request or disposal owns completion.
			return;
		}
		catch (Exception ex)
		{
			Log.Warn(ex, "Error detection failed for request '{RequestId}'.", requestId);
			error = ex;
		}

		await _dispatcher.InvokeAsync(() => CompleteRequest(requestId, result, error));
	}

	private IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string editorContent)
	{
		ITextDiagnosticsProvider? diagnosticsProvider = _diagnosticsProvider;

		return diagnosticsProvider is null
			? []
			: diagnosticsProvider.GetDiagnostics(new TextDiagnosticsRequest(editorContent, EngineVersion));
	}

	private void CompleteRequest(int requestId, object result, Exception? error)
	{
		if (_isDisposed || !_requestTokens.IsCurrent(requestId))
			return;

		_isBusy = false;
		RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(result, error, false));

		// Run the coalesced latest content after the active run, unless the editor entered a
		// silent session while the run was in flight.
		if (_hasPendingContent && !IsSilentSession())
		{
			_hasPendingContent = false;
			string? content = _pendingContent;
			_pendingContent = null;
			StartErrorCheck(content ?? string.Empty);
		}
	}

	// IDisposable

	/// <summary>
	/// Stops error detection and cancels any in-flight run so no completion callback fires afterwards.
	/// </summary>
	public void Dispose()
	{
		_isDisposed = true;
		_errorUpdateTimer.Stop();
		_errorUpdateTimer.Tick -= ErrorUpdateTimer_Tick;
		_isBusy = false;
		_hasPendingContent = false;
		_pendingContent = null;
		_requestCancellation?.Cancel();
		_requestTokens.Invalidate();
	}
}
