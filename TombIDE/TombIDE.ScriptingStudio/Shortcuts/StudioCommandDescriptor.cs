using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Immutable command catalog entry containing a <see cref="UICommand"/>,
/// its stable serialized identifier, default bindings, and remapping policy.
/// </summary>
public sealed class StudioCommandDescriptor
{
	public StudioCommandDescriptor(
		UICommand command,
		string serializedId,
		bool isRemappable,
		bool isHostReserved,
		params ShortcutKey[] defaultBindings)
	{
		Command = command;
		SerializedId = serializedId ?? throw new ArgumentNullException(nameof(serializedId));
		IsRemappable = isRemappable;
		IsHostReserved = isHostReserved;
		DefaultBindings = Array.AsReadOnly(defaultBindings ?? Array.Empty<ShortcutKey>());
	}

	/// <summary>
	/// The command this descriptor represents. Never <see cref="UICommand.None"/>.
	/// </summary>
	public UICommand Command { get; }

	/// <summary>
	/// Stable serialized identifier used for persistence.
	/// This is the <see cref="UICommand"/> name (e.g. "Save"), not its numeric value.
	/// </summary>
	public string SerializedId { get; }

	/// <summary>
	/// The default bindings defined by the application.
	/// </summary>
	public IReadOnlyList<ShortcutKey> DefaultBindings { get; }

	/// <summary>
	/// Whether the user is permitted to remap this command.
	/// </summary>
	public bool IsRemappable { get; }

	/// <summary>
	/// Whether this command's shortcut is reserved by the host (e.g. Alt+F4 for Exit).
	/// Host-reserved commands always use catalog defaults and ignore overrides.
	/// </summary>
	public bool IsHostReserved { get; }
}
