#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Shell-scoped active runtime service for shortcut bindings.
/// Provides lookup, current bindings, display text, validation,
/// apply/clear/reset operations, and a <see cref="BindingsChanged"/> notification.
/// </summary>
public interface IShortcutBindingService : IDisposable
{
	/// <summary>
	/// Raised after an accepted load, apply, clear, or reset operation,
	/// once the runtime maps are complete. Menu, toolbar, and context-menu
	/// presentation may refresh from this notification.
	/// </summary>
	event EventHandler? BindingsChanged;

	/// <summary>
	/// Looks up the <see cref="UICommand"/> currently bound to a shortcut.
	/// Returns <see langword="false"/> when no command is bound.
	/// A collision (two commands sharing a shortcut) is a fatal invariant and
	/// causes the service to throw rather than returning a first-wins result.
	/// </summary>
	bool TryGetCommand(ShortcutKey shortcut, out UICommand command);

	/// <summary>
	/// Returns the current bindings for a command, or an empty list when
	/// the command has no bindings (explicitly unbound) or is not catalogued.
	/// </summary>
	IReadOnlyList<ShortcutKey> GetBindings(UICommand command);

	/// <summary>
	/// Returns the display text for a command's current bindings.
	/// When the command has no bindings, returns <paramref name="fallbackDisplayText"/>.
	/// </summary>
	string GetDisplayText(UICommand command, string fallbackDisplayText = "");

	/// <summary>
	/// Validates a proposed binding set for a command without mutating state.
	/// </summary>
	ShortcutValidationResult Validate(UICommand command, IReadOnlyList<ShortcutKey> bindings);

	/// <summary>
	/// Applies a new binding set for a command. Rejects invalid, reserved, or
	/// non-remappable changes. When <paramref name="replaceConflicts"/> is
	/// <see langword="true"/>, conflicting bindings are removed from the other
	/// commands before the new bindings are applied. Persists the change atomically.
	/// </summary>
	ShortcutValidationResult Apply(UICommand command, IReadOnlyList<ShortcutKey> bindings, bool replaceConflicts);

	/// <summary>
	/// Clears all bindings for a command, storing an explicit empty override.
	/// Rejected for host-reserved commands.
	/// </summary>
	ShortcutValidationResult Clear(UICommand command);

	/// <summary>
	/// Removes the override for a single command, falling back to catalog defaults.
	/// </summary>
	void Reset(UICommand command);

	/// <summary>
	/// Removes all overrides for the active workspace, falling back to catalog defaults.
	/// </summary>
	void ResetAll();
}
