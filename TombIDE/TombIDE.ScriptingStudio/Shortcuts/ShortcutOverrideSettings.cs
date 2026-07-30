#nullable enable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Versioned collection of shortcut overrides for one scripting workspace.
/// Serialized as part of <see cref="Settings.ScriptingStudioShellWorkspaceSettings"/>.
/// </summary>
public sealed class ShortcutOverrideCollection
{
	public ShortcutOverrideCollection()
	{
		Version = 1;
		Overrides = [];
	}

	/// <summary>
	/// Schema version for forward compatibility.
	/// </summary>
	[XmlAttribute("Version")]
	public int Version { get; set; }

	/// <summary>
	/// Per-command override entries. An empty <see cref="ShortcutOverrideEntry.Bindings"/>
	/// list means the user explicitly unbound the command.
	/// </summary>
	[XmlElement("Command")]
	public List<ShortcutOverrideEntry> Overrides { get; set; }
}

/// <summary>
/// A single command override entry in the persisted shortcut settings.
/// </summary>
public sealed class ShortcutOverrideEntry
{
	public ShortcutOverrideEntry()
	{
		CommandId = string.Empty;
		Bindings = [];
	}

	/// <summary>
	/// Stable serialized command identifier (e.g. "Save").
	/// </summary>
	[XmlAttribute("Id")]
	public string CommandId { get; set; }

	/// <summary>
	/// The binding entries for this command.
	/// An empty list means the command is explicitly unbound.
	/// </summary>
	[XmlElement("Binding")]
	public List<ShortcutBindingSettings> Bindings { get; set; }
}

/// <summary>
/// A single key binding in the persisted shortcut settings.
/// Never contains display text — that is calculated at runtime.
/// </summary>
public sealed class ShortcutBindingSettings
{
	public ShortcutBindingSettings()
	{
		KeyName = string.Empty;
	}

	/// <summary>
	/// The <see cref="System.Windows.Input.Key"/> enum name (e.g. "S", "F9", "OemQuestion").
	/// </summary>
	[XmlAttribute("Key")]
	public string KeyName { get; set; }

	/// <summary>
	/// The modifier flags as a bitfield of <see cref="System.Windows.Input.ModifierKeys"/>.
	/// </summary>
	[XmlAttribute("Modifiers")]
	public int Modifiers { get; set; }
}
