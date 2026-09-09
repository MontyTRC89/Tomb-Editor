#nullable enable

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Result of a shortcut validation or apply operation.
/// </summary>
public enum ShortcutValidationResult
{
	/// <summary>The operation is valid and was accepted.</summary>
	Valid,

	/// <summary>The command is host-reserved and cannot be remapped.</summary>
	Reserved,

	/// <summary>The command is not remappable.</summary>
	NotRemappable,

	/// <summary>One or more bindings contain an invalid key (e.g. <see cref="System.Windows.Input.Key.None"/>).</summary>
	InvalidKey,

	/// <summary>The same shortcut appears more than once in the proposed binding set.</summary>
	DuplicateInCommand,

	/// <summary>A binding is already assigned to another command. The operation must use explicit conflict replacement.</summary>
	Conflict
}
