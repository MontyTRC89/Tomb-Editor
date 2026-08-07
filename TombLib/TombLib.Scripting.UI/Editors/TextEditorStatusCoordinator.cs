using ICSharpCode.AvalonEdit.Editing;
using System;

namespace TombLib.Scripting.UI.Editors;

internal sealed class TextEditorStatusCoordinator : IDisposable
{
	private readonly Action _raiseStatusChanged;
	private readonly Action _raiseZoomChanged;
	private readonly TextArea _textArea;

	private int _zoom = 100;

	public TextEditorStatusCoordinator(TextArea textArea, Action raiseStatusChanged, Action raiseZoomChanged)
	{
		ArgumentNullException.ThrowIfNull(textArea);
		_textArea = textArea;
		ArgumentNullException.ThrowIfNull(raiseStatusChanged);
		_raiseStatusChanged = raiseStatusChanged;
		ArgumentNullException.ThrowIfNull(raiseZoomChanged);
		_raiseZoomChanged = raiseZoomChanged;
	}

	public int Zoom
	{
		get => _zoom;
		set => _zoom = value;
	}

	public void Attach()
	{
		_textArea.Caret.PositionChanged += TextArea_PositionChanged;
		_textArea.SelectionChanged += TextArea_SelectionChanged;
	}

	public void Dispose()
	{
		_textArea.Caret.PositionChanged -= TextArea_PositionChanged;
		_textArea.SelectionChanged -= TextArea_SelectionChanged;
	}

	public bool TryHandleZoom(int delta, int minZoom, int maxZoom, int zoomStepSize, double defaultFontSize, Action<double> applyFontSize)
	{
		ArgumentNullException.ThrowIfNull(applyFontSize);

		int nextZoom = _zoom;

		if (delta > 0)
		{
			if (_zoom >= maxZoom)
				return false;

			nextZoom = Math.Min(maxZoom, _zoom + zoomStepSize);
		}
		else if (delta < 0)
		{
			if (_zoom <= minZoom)
				return false;

			nextZoom = Math.Max(minZoom, _zoom - zoomStepSize);
		}
		else
		{
			return false;
		}

		_zoom = nextZoom;
		applyFontSize(defaultFontSize * _zoom / 100);
		_raiseZoomChanged();
		return true;
	}

	private void TextArea_PositionChanged(object? sender, EventArgs e)
		=> _raiseStatusChanged();

	private void TextArea_SelectionChanged(object? sender, EventArgs e)
		=> _raiseStatusChanged();
}
