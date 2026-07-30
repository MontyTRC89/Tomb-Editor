#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
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
		_host = host ?? throw new ArgumentNullException(nameof(host));
		_defaultBorderBrush = defaultBorderBrush ?? throw new ArgumentNullException(nameof(defaultBorderBrush));
		_defaultBackground = defaultBackground ?? throw new ArgumentNullException(nameof(defaultBackground));
		_defaultForeground = defaultForeground ?? throw new ArgumentNullException(nameof(defaultForeground));
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

	public bool TryOpen(
		IEnumerable<ICompletionData> items,
		int? startOffset = null,
		int? endOffset = null,
		int width = 300,
		int height = 300)
	{
		ICompletionData[] completionItems = items?.ToArray() ?? [];

		if (completionItems.Length == 0)
			return false;

		Initialize(width, height);

		if (_window is null)
			return false;

		if (startOffset.HasValue)
			_window.StartOffset = startOffset.Value;

		if (endOffset.HasValue)
			_window.EndOffset = endOffset.Value;

		foreach (ICompletionData item in completionItems)
			_window.CompletionList.CompletionData.Add(item);

		Show();
		return true;
	}
}
