#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Diagnostics;

public sealed class TextDiagnosticsCoordinator
{
	private static readonly TimeSpan DefaultIdleDelay = TimeSpan.FromMilliseconds(500.0);

	private readonly TextEditorBase _editor;
	private readonly ErrorDetectionWorker _worker;

	public TextDiagnosticsCoordinator(
		TextEditorBase editor,
		Version engineVersion,
		IErrorDetector? errorDetector = null,
		ITextDiagnosticsProvider? diagnosticsProvider = null,
		TimeSpan? idleDelayInterval = null)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_editor = editor;
		_worker = new ErrorDetectionWorker(errorDetector, engineVersion, idleDelayInterval ?? DefaultIdleDelay)
		{
			DiagnosticsProvider = diagnosticsProvider
		};

		_worker.RunWorkerCompleted += ErrorDetectionWorker_RunWorkerCompleted;
	}

	public bool IsBusy => _worker.IsBusy;

	public void RunOnIdle(string? editorContent)
	{
		if (!_editor.IsSilentSession)
			_worker.RunErrorCheckOnIdle(editorContent);
	}

	public void CheckAsync(string? editorContent)
	{
		if (!_editor.IsSilentSession && !_worker.IsBusy)
			_worker.CheckForErrorsAsync(editorContent);
	}

	private void ErrorDetectionWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Result is IReadOnlyList<TextEditorDiagnostic> diagnostics)
			_editor.SetDiagnostics(diagnostics);
	}
}