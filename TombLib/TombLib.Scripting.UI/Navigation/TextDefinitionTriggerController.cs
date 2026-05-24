#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Navigation;

/// <summary>
/// Handles shared editor-side definition navigation triggers such as F12 and Ctrl+Click.
/// </summary>
public sealed class TextDefinitionTriggerController
{
	private readonly FrameworkElement _owner;
	private readonly Func<Point, int> _getOffsetFromPoint;
	private readonly Func<int, CancellationToken, Task<bool>> _tryNavigateAsync;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDefinitionTriggerController"/> class.
	/// </summary>
	public TextDefinitionTriggerController(
		FrameworkElement owner,
		Func<Point, int> getOffsetFromPoint,
		Func<int, CancellationToken, Task<bool>> tryNavigateAsync)
	{
		ArgumentNullException.ThrowIfNull(owner);
		ArgumentNullException.ThrowIfNull(getOffsetFromPoint);
		ArgumentNullException.ThrowIfNull(tryNavigateAsync);

		_owner = owner;
		_getOffsetFromPoint = getOffsetFromPoint;
		_tryNavigateAsync = tryNavigateAsync;
	}

	/// <summary>
	/// Handles an F12 key press for definition navigation.
	/// </summary>
	public async Task<bool> TryHandleKeyDownAsync(KeyEventArgs e, int caretOffset, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(e);

		if (e.Key != Key.F12)
			return false;

		if (!await _tryNavigateAsync(caretOffset, cancellationToken).ConfigureAwait(true))
			return false;

		e.Handled = true;
		return true;
	}

	/// <summary>
	/// Handles a Ctrl+LeftClick definition navigation attempt.
	/// </summary>
	public async Task<bool> TryHandlePointerNavigationAsync(MouseButtonEventArgs e, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(e);

		if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || e.ChangedButton != MouseButton.Left)
			return false;

		int hoveredOffset = _getOffsetFromPoint(e.GetPosition(_owner));

		if (hoveredOffset == -1)
			return false;

		if (!await _tryNavigateAsync(hoveredOffset, cancellationToken).ConfigureAwait(true))
			return false;

		e.Handled = true;
		return true;
	}
}