#nullable enable

using System;
using System.Windows.Input;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Immutable WPF-native shortcut key composed of a <see cref="Key"/> and <see cref="ModifierKeys"/>.
/// Provides value equality, display text, and factory methods from WPF key events.
/// </summary>
public readonly record struct ShortcutKey
{
	public ShortcutKey(Key key, ModifierKeys modifiers)
	{
		if (key == Key.None)
			throw new ArgumentException("A bindable shortcut must include a non-None key.", nameof(key));

		Key = key;
		Modifiers = modifiers;
	}

	/// <summary>
	/// The primary key. Never <see cref="Key.None"/>.
	/// </summary>
	public Key Key { get; }

	/// <summary>
	/// The modifier keys (Control, Shift, Alt, Windows).
	/// </summary>
	public ModifierKeys Modifiers { get; }

	/// <summary>
	/// Creates a <see cref="ShortcutKey"/> from a WPF <see cref="KeyEventArgs"/>.
	/// Normalizes <see cref="Key.System"/> to <see cref="SystemKey"/> and reads modifiers
	/// from the event's keyboard device rather than the global <see cref="Keyboard.Modifiers"/>.
	/// Returns <see langword="null"/> when the resulting key is <see cref="Key.None"/> or
	/// the combination is a modifier-only keystroke.
	/// </summary>
	public static ShortcutKey? FromKeyEventArgs(KeyEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);

		Key key = e.Key == Key.System ? e.SystemKey : e.Key;

		if (key == Key.None)
			return null;

		// Modifier keys alone are not bindable shortcuts.
		if (key == Key.LeftCtrl || key == Key.RightCtrl ||
			key == Key.LeftAlt || key == Key.RightAlt ||
			key == Key.LeftShift || key == Key.RightShift ||
			key == Key.LWin || key == Key.RWin)
		{
			return null;
		}

		ModifierKeys modifiers = ModifierKeys.None;

		if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
			modifiers |= ModifierKeys.Control;

		if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
			modifiers |= ModifierKeys.Shift;

		if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt))
			modifiers |= ModifierKeys.Alt;

		if (e.KeyboardDevice.IsKeyDown(Key.LWin) || e.KeyboardDevice.IsKeyDown(Key.RWin))
			modifiers |= ModifierKeys.Windows;

		return new ShortcutKey(key, modifiers);
	}

	/// <summary>
	/// Returns a localized display string suitable for menu and toolbar presentation.
	/// This is never persisted — it is calculated from <see cref="Key"/> and <see cref="Modifiers"/>.
	/// </summary>
	public string GetDisplayText()
	{
		string keyText = Key switch
		{
			Key.D0 => "0",
			Key.D1 => "1",
			Key.D2 => "2",
			Key.D3 => "3",
			Key.D4 => "4",
			Key.D5 => "5",
			Key.D6 => "6",
			Key.D7 => "7",
			Key.D8 => "8",
			Key.D9 => "9",
			Key.OemPlus => "+",
			Key.OemMinus => "-",
			Key.OemQuestion => "/",
			Key.OemComma => ",",
			Key.OemPeriod => ".",
			Key.OemSemicolon => ";",
			Key.OemOpenBrackets => "[",
			Key.OemCloseBrackets => "]",
			Key.OemPipe => "\\",
			Key.OemQuotes => "\"",
			Key.OemTilde => "`",
			_ => Key.ToString()
		};

		if (Modifiers == ModifierKeys.None)
			return keyText;

		string modifierText = string.Empty;

		if ((Modifiers & ModifierKeys.Control) != ModifierKeys.None)
			modifierText += "Ctrl+";

		if ((Modifiers & ModifierKeys.Shift) != ModifierKeys.None)
			modifierText += "Shift+";

		if ((Modifiers & ModifierKeys.Alt) != ModifierKeys.None)
			modifierText += "Alt+";

		if ((Modifiers & ModifierKeys.Windows) != ModifierKeys.None)
			modifierText += "Win+";

		return modifierText + keyText;
	}
}
