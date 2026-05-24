using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts
{
	public sealed class StudioShortcutBindingService
	{
		private static readonly StudioCommandDescriptor[] DefaultCommandDescriptors =
		[
			new(UICommand.NewFile, new StudioShortcutBinding(Keys.Control | Keys.N)),
			new(UICommand.Save, new StudioShortcutBinding(Keys.Control | Keys.S)),
			new(UICommand.SaveAll, new StudioShortcutBinding(Keys.Control | Keys.Shift | Keys.S)),
			new(UICommand.Build, new StudioShortcutBinding(Keys.F9)),
			new(UICommand.Exit, new StudioShortcutBinding(Keys.Alt | Keys.F4)),
			new(UICommand.Undo, new StudioShortcutBinding(Keys.Control | Keys.Z)),
			new(UICommand.Redo, new StudioShortcutBinding(Keys.Control | Keys.Y)),
			new(UICommand.Cut, new StudioShortcutBinding(Keys.Control | Keys.X)),
			new(UICommand.Copy, new StudioShortcutBinding(Keys.Control | Keys.C)),
			new(UICommand.Paste, new StudioShortcutBinding(Keys.Control | Keys.V)),
			new(UICommand.Find,
				new StudioShortcutBinding(Keys.Control | Keys.F),
				new StudioShortcutBinding(Keys.Control | Keys.H)),
			new(UICommand.SelectAll, new StudioShortcutBinding(Keys.Control | Keys.A)),
			new(UICommand.Reindent, new StudioShortcutBinding(Keys.Control | Keys.R)),
			new(UICommand.TrimWhiteSpace, new StudioShortcutBinding(Keys.Control | Keys.Shift | Keys.R)),
			new(UICommand.ToggleComment, new StudioShortcutBinding(Keys.Control | Keys.OemQuestion, "Ctrl+/")),
			new(UICommand.CommentOut, new StudioShortcutBinding(Keys.Control | Keys.Shift | Keys.C)),
			new(UICommand.Uncomment, new StudioShortcutBinding(Keys.Control | Keys.Shift | Keys.U)),
			new(UICommand.ToggleBookmark, new StudioShortcutBinding(Keys.Control | Keys.B)),
			new(UICommand.PrevBookmark, new StudioShortcutBinding(Keys.Control | Keys.Oemcomma, "Ctrl+Comma")),
			new(UICommand.NextBookmark, new StudioShortcutBinding(Keys.Control | Keys.OemPeriod, "Ctrl+Period")),
			new(UICommand.ClearBookmarks, new StudioShortcutBinding(Keys.Control | Keys.Shift | Keys.B)),
			new(UICommand.PrevSection, new StudioShortcutBinding(Keys.Control | Keys.Left)),
			new(UICommand.NextSection, new StudioShortcutBinding(Keys.Control | Keys.Right)),
			new(UICommand.ClearString, new StudioShortcutBinding(Keys.Delete)),
			new(UICommand.RemoveLastString, new StudioShortcutBinding(Keys.Control | Keys.Delete)),
			new(UICommand.NavigateBack, new StudioShortcutBinding(Keys.Alt | Keys.Left)),
			new(UICommand.NavigateForward, new StudioShortcutBinding(Keys.Alt | Keys.Right)),
			new(UICommand.GoToDefinition, new StudioShortcutBinding(Keys.F12)),
			new(UICommand.FindReferences, new StudioShortcutBinding(Keys.Shift | Keys.F12)),
			new(UICommand.RenameSymbol, new StudioShortcutBinding(Keys.F2)),
			new(UICommand.TypeFirstAvailableId, new StudioShortcutBinding(Keys.F1)),
			new(UICommand.NewFileAtCaret, new StudioShortcutBinding(Keys.Control | Keys.F5))
		];

		private readonly Dictionary<UICommand, IReadOnlyList<StudioShortcutBinding>> _bindingsByCommand;
		private readonly Dictionary<Keys, UICommand> _commandsByShortcut;

		public StudioShortcutBindingService()
		{
			_bindingsByCommand = DefaultCommandDescriptors.ToDictionary(
				descriptor => descriptor.Command,
				descriptor => descriptor.DefaultBindings,
				EqualityComparer<UICommand>.Default);

			_commandsByShortcut = new Dictionary<Keys, UICommand>();

			foreach ((UICommand command, IReadOnlyList<StudioShortcutBinding> bindings) in _bindingsByCommand)
			{
				foreach (StudioShortcutBinding binding in bindings)
				{
					if (!_commandsByShortcut.ContainsKey(binding.Keys))
						_commandsByShortcut.Add(binding.Keys, command);
				}
			}
		}

		public bool TryGetCommand(Keys keys, out UICommand command)
			=> _commandsByShortcut.TryGetValue(keys, out command);

		public bool TryGetCommand(System.Windows.Input.KeyEventArgs e, out UICommand command)
			=> TryGetCommand(GetKeys(e), out command);

		public bool TryGetPrimaryShortcut(UICommand command, out Keys keys)
		{
			if (_bindingsByCommand.TryGetValue(command, out IReadOnlyList<StudioShortcutBinding> bindings) && bindings.Count > 0)
			{
				keys = bindings[0].Keys;
				return true;
			}

			keys = Keys.None;
			return false;
		}

		public string GetShortcutDisplayText(UICommand command, string fallbackDisplayText = "")
		{
			if (!_bindingsByCommand.TryGetValue(command, out IReadOnlyList<StudioShortcutBinding> bindings) || bindings.Count == 0)
				return fallbackDisplayText;

			return string.Join(" / ", bindings.Select(GetBindingDisplayText));
		}

		private static string GetBindingDisplayText(StudioShortcutBinding binding)
			=> !string.IsNullOrWhiteSpace(binding.DisplayText)
				? binding.DisplayText
				: new KeysConverter().ConvertToString(binding.Keys) ?? string.Empty;

		private static Keys GetKeys(System.Windows.Input.KeyEventArgs e)
		{
			Key key = e.Key == Key.System ? e.SystemKey : e.Key;
			Keys keys = (Keys)KeyInterop.VirtualKeyFromKey(key);
			ModifierKeys modifiers = Keyboard.Modifiers;

			if ((modifiers & ModifierKeys.Control) != 0)
				keys |= Keys.Control;

			if ((modifiers & ModifierKeys.Shift) != 0)
				keys |= Keys.Shift;

			if ((modifiers & ModifierKeys.Alt) != 0)
				keys |= Keys.Alt;

			return keys;
		}
	}
}