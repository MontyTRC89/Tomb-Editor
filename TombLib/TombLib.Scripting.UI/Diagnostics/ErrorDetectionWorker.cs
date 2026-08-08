using Nickelony.LanguageServer.Abstractions.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Threading;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Runs error detection in the background, debounced by an idle timer, and publishes the result
/// through <see cref="RunWorkerCompleted"/>. Requests are latest-request-wins: when a newer request
/// is issued before an older one completes, the older result is discarded. When a silent-session
/// provider is supplied, checks are not started while the session is silent.
/// The worker is created on and confined to the UI thread; the full-document provider call runs on
/// the thread pool because error detection is CPU-bound and the provider contract explicitly
/// permits background execution.
/// </summary>
public sealed class ErrorDetectionWorker : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	// Properties

	/// <summary>
	/// Gets or sets the diagnostics provider used to detect errors.
	/// </summary>
	public ITextDiagnosticsProvider? DiagnosticsProvider { get; set; }

	/// <summary>
	/// Gets or sets the error detector used to detect errors.
	/// </summary>
	public IErrorDetector? ErrorDetector { get; set; }

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
	/// Gets or sets the engine version used for error detection.
	/// </summary>
	public Version EngineVersion { get; set; }

	// Fields

	private readonly Dispatcher _dispatcher;
	private readonly DispatcherTimer _errorUpdateTimer = new();
	private readonly Func<bool>? _silentSessionProvider;
	private readonly RequestTokenSource _requestTokens = new();

	private volatile bool _isBusy;
	private string _editorContent = string.Empty;
	private bool _isDisposed;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetectionWorker"/> class on the current dispatcher thread.
	/// </summary>
	/// <param name="errorDetector">The optional error detector.</param>
	/// <param name="engineVersion">The engine version used for error detection.</param>
	/// <param name="idleDelayInterval">The idle debounce interval.</param>
	/// <param name="silentSessionProvider">The callback that reports whether the editor is in a silent session (optional).</param>
	public ErrorDetectionWorker(IErrorDetector? errorDetector, Version engineVersion, TimeSpan idleDelayInterval, Func<bool>? silentSessionProvider = null)
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
		ErrorDetector = errorDetector;
		IdleDelayInterval = idleDelayInterval;
		EngineVersion = engineVersion;
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
	/// Runs a detection check now with the given content. No-op while a silent session is active.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunErrorCheck(string? editorContent)
	{
		if (_isDisposed || IsSilentSession())
			return;

		if (ErrorDetector is null && DiagnosticsProvider is null)
			return;

		_editorContent = editorContent ?? string.Empty;
		int requestId = _requestTokens.Begin();
		_isBusy = true;

		_ = RunErrorCheckCoreAsync(_editorContent, requestId);
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

	private async Task RunErrorCheckCoreAsync(string editorContent, int requestId)
	{
		Exception? error = null;
		object result = Array.Empty<TextEditorDiagnostic>();

		try
		{
			// Full-document error detection is CPU-bound and the provider contracts (ITextDiagnosticsProvider /
			// IErrorDetector) explicitly permit background execution, so the provider runs on the thread pool.
			result = await Task.Run(() => (object)GetDiagnostics(editorContent)).ConfigureAwait(false);
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
		ITextDiagnosticsProvider? diagnosticsProvider = DiagnosticsProvider;

		if (diagnosticsProvider is not null)
			return diagnosticsProvider.GetDiagnostics(new TextDiagnosticsRequest(editorContent, EngineVersion));

		IErrorDetector? errorDetector = ErrorDetector;

		return errorDetector is null
			? []
			: errorDetector.FindErrors(editorContent, EngineVersion);
	}

	private void CompleteRequest(int requestId, object result, Exception? error)
	{
		if (!_requestTokens.IsCurrent(requestId))
			return;

		_isBusy = false;
		RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(result, error, false));
	}

	// IDisposable

	/// <summary>
	/// Stops error detection and invalidates any in-flight request so no completion callback fires afterwards.
	/// </summary>
	public void Dispose()
	{
		_isDisposed = true;
		_errorUpdateTimer.Stop();
		_errorUpdateTimer.Tick -= ErrorUpdateTimer_Tick;
		_isBusy = false;
		_requestTokens.Invalidate();
	}
}
