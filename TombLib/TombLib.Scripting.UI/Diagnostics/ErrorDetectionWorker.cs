using Nickelony.LanguageServer.Abstractions.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.Diagnostics;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Runs error detection in the background, debounced by an idle timer, and publishes the result
/// through <see cref="RunWorkerCompleted"/>. Requests are latest-request-wins: when a newer request
/// is issued before an older one completes, the older result is discarded.
/// </summary>
public class ErrorDetectionWorker
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	// Properties

	public ITextDiagnosticsProvider? DiagnosticsProvider { get; set; }

	public IErrorDetector? ErrorDetector { get; set; }

	public bool IsBusy => _isBusy;

	public TimeSpan IdleDelayInterval
	{
		get => _errorUpdateTimer.Interval;
		set => _errorUpdateTimer.Interval = value;
	}

	public Version EngineVersion { get; set; }

	// Fields

	private readonly Dispatcher _dispatcher;
	private readonly DispatcherTimer _errorUpdateTimer = new();

	private volatile bool _isBusy;
	private int _latestRequestId;
	private string _editorContent = string.Empty;

	// Construction

	public ErrorDetectionWorker(IErrorDetector? errorDetector, Version engineVersion, TimeSpan idleDelayInterval)
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
		ErrorDetector = errorDetector;
		IdleDelayInterval = idleDelayInterval;
		EngineVersion = engineVersion;

		_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
	}

	// Events

	public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

	// Public methods

	public void RunErrorCheckOnIdle(string? editorContent)
	{
		if (_errorUpdateTimer.IsEnabled)
			_errorUpdateTimer.Stop();

		_editorContent = editorContent ?? string.Empty;
		_errorUpdateTimer.Start();
	}

	public void CheckForErrorsAsync(string? editorContent)
	{
		if (ErrorDetector is null && DiagnosticsProvider is null)
			return;

		_editorContent = editorContent ?? string.Empty;
		int requestId = Interlocked.Increment(ref _latestRequestId);
		_isBusy = true;

		_ = RunErrorCheckCoreAsync(_editorContent, requestId);
	}

	private void ErrorUpdateTimer_Tick(object? sender, EventArgs e)
	{
		_errorUpdateTimer.Stop();
		CheckForErrorsAsync(_editorContent);
	}

	// Private methods

	private async Task RunErrorCheckCoreAsync(string editorContent, int requestId)
	{
		Exception? error = null;
		object result = Array.Empty<TextEditorDiagnostic>();

		try
		{
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
		if (requestId != _latestRequestId)
			return;

		_isBusy = false;
		RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(result, error, false));
	}
}
