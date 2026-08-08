using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Windows.Media;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Coordinates the completion window lifecycle: create, show, close, and
/// item-population orchestration, extracted from <see cref="Bases.TextEditorBase"/>.
/// </summary>
internal sealed class CompletionWindowCoordinator
{
	private readonly CompletionWindowHost _host;
	private readonly Brush _defaultBorderBrush;
	private readonly Brush _defaultBackground;
	private readonly Brush _defaultForeground;
	private CompletionWindow? _window;

	public CompletionWindowCoordinator(
		CompletionWindowHost host,
		Brush defaultBorderBrush,
		Brush defaultBackground,
		Brush defaultForeground)
	{
		ArgumentNullException.ThrowIfNull(host);
		_host = host;
		ArgumentNullException.ThrowIfNull(defaultBorderBrush);
		_defaultBorderBrush = defaultBorderBrush;
		ArgumentNullException.ThrowIfNull(defaultBackground);
		_defaultBackground = defaultBackground;
		ArgumentNullException.ThrowIfNull(defaultForeground);
		_defaultForeground = defaultForeground;
	}

	public CompletionWindow? ActiveWindow => _window;

	public bool IsWindowOpen => _window is not null;

	public void Initialize(int width = 300, int height = 300)
		=> _window = _host.Create(width, height, _defaultBorderBrush, _defaultBackground, _defaultForeground);

	public void Show()
	{
		if (_window is null)
			return;

		_host.Show(_window, () => _window = null);
	}

	public void Close()
		=> _host.Close(_window, () => _window = null);

	public void Dispose()
		=> Close();
}
