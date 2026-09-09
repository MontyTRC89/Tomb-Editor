using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows;
using System.Windows.Media;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Owns the completion window lifecycle for a single text area and tracks at most one open window.
/// Creating a new window force-closes any previously tracked window, so the host always reflects the
/// latest completion window only.
/// </summary>
internal sealed class CompletionWindowHost
{
	private readonly TextArea _textArea;
	private EventHandler? _closedHandler;
	private Action? _trackedClosedAction;
	private CompletionWindow? _trackedWindow;

	public CompletionWindowHost(TextArea textArea) => _textArea = textArea;

	/// <summary>
	/// Creates a new completion window. Any previously tracked window is closed first, so only the
	/// most recently created window remains open.
	/// </summary>
	public CompletionWindow Create(double width, double height, Brush borderBrush, Brush background, Brush foreground)
	{
		CloseTrackedWindow();

		return new(_textArea)
		{
			WindowStyle = WindowStyle.None,
			ResizeMode = ResizeMode.NoResize,
			BorderThickness = new Thickness(1.0),
			Background = background,
			Foreground = foreground,
			BorderBrush = borderBrush,
			Width = width,
			Height = height
		};
	}

	public void Show(CompletionWindow completionWindow, Action onClosed)
	{
		TrackWindow(completionWindow, onClosed);

		completionWindow.Show();
	}

	public void Close(CompletionWindow? completionWindow, Action onClosed)
	{
		if (completionWindow is null)
			return;

		UntrackWindow(completionWindow);
		completionWindow.Close();
		onClosed();
	}

	private void TrackWindow(CompletionWindow completionWindow, Action onClosed)
	{
		UntrackWindow(_trackedWindow);

		_trackedWindow = completionWindow;
		_trackedClosedAction = onClosed;
		_closedHandler = (sender, e) =>
		{
			UntrackWindow(completionWindow);
			onClosed();
		};

		completionWindow.Closed += _closedHandler;
	}

	private void UntrackWindow(CompletionWindow? completionWindow)
	{
		if (completionWindow is null || _closedHandler is null || !ReferenceEquals(completionWindow, _trackedWindow))
			return;

		completionWindow.Closed -= _closedHandler;
		_closedHandler = null;
		_trackedClosedAction = null;
		_trackedWindow = null;
	}

	private void CloseTrackedWindow()
	{
		if (_trackedWindow is null)
			return;

		CompletionWindow trackedWindow = _trackedWindow;
		Action? onClosed = _trackedClosedAction;

		UntrackWindow(trackedWindow);
		trackedWindow.Close();
		onClosed?.Invoke();
	}
}
