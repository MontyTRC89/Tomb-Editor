using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Coordinates background diagnostics detection for an editor. While the editor is in a silent session,
/// new checks are not started and completed results are discarded, so diagnostics are never replaced with
/// content captured during the session.
/// </summary>
public sealed class TextDiagnosticsCoordinator : IDisposable
{
	private static readonly TimeSpan DefaultIdleDelay = TimeSpan.FromMilliseconds(500.0);

	private readonly TextEditorBase _editor;
	private readonly ErrorDetectionWorker _worker;
	private string? _pendingCheckContent;
	private bool _hasPendingCheck;
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDiagnosticsCoordinator"/> class.
	/// </summary>
	/// <param name="editor">The editor whose diagnostics are coordinated.</param>
	/// <param name="engineVersion">The engine version used for error detection.</param>
	/// <param name="errorDetector">The optional error detector.</param>
	/// <param name="diagnosticsProvider">The optional diagnostics provider.</param>
	/// <param name="idleDelayInterval">The optional idle debounce interval.</param>
	public TextDiagnosticsCoordinator(
		TextEditorBase editor,
		Version engineVersion,
		IErrorDetector? errorDetector = null,
		ITextDiagnosticsProvider? diagnosticsProvider = null,
		TimeSpan? idleDelayInterval = null)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_editor = editor;
		_worker = new ErrorDetectionWorker(errorDetector, engineVersion, idleDelayInterval ?? DefaultIdleDelay, () => _editor.IsSilentSession)
		{
			DiagnosticsProvider = diagnosticsProvider
		};

		_worker.RunWorkerCompleted += ErrorDetectionWorker_RunWorkerCompleted;
	}

	/// <summary>
	/// Gets whether a diagnostics run is currently in progress.
	/// </summary>
	public bool IsBusy => _worker.IsBusy;

	/// <summary>
	/// Schedules a diagnostics run after the idle debounce interval. No-op while the editor is in a silent session.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunOnIdle(string? editorContent)
	{
		if (_isDisposed || _editor.IsSilentSession)
			return;

		_worker.RunErrorCheckOnIdle(editorContent);
	}

	/// <summary>
	/// Runs a diagnostics check now, retaining the latest content when a check is already in progress.
	/// No-op while the editor is in a silent session.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunErrorCheck(string? editorContent)
	{
		if (_isDisposed || _editor.IsSilentSession)
			return;

		// Retain the latest pending request instead of dropping a check issued while one is active.
		if (_worker.IsBusy)
		{
			_pendingCheckContent = editorContent;
			_hasPendingCheck = true;
			return;
		}

		_worker.RunErrorCheck(editorContent);
	}

	// IDisposable

	/// <summary>
	/// Stops background detection and unsubscribes completion callbacks so no diagnostics mutate the editor afterwards.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_worker.RunWorkerCompleted -= ErrorDetectionWorker_RunWorkerCompleted;
		_worker.Dispose();
		_pendingCheckContent = null;
		_hasPendingCheck = false;
	}

	private void ErrorDetectionWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
	{
		// Keep the last known diagnostics when the provider failed; the worker already logged the failure.
		// While a silent session is active, discard the completed result instead of publishing it.
		if (!_isDisposed && !_editor.IsSilentSession && !e.Cancelled && e.Error is null && e.Result is IReadOnlyList<TextEditorDiagnostic> diagnostics)
			_editor.SetDiagnostics(diagnostics);

		RunPendingCheckIfAny();
	}

	private void RunPendingCheckIfAny()
	{
		if (_isDisposed || !_hasPendingCheck)
			return;

		_hasPendingCheck = false;
		string? content = _pendingCheckContent;
		_pendingCheckContent = null;

		// Drop the queued check while a silent session is active; the next content change schedules a fresh run.
		if (_editor.IsSilentSession)
			return;

		_worker.RunErrorCheck(content);
	}
}
