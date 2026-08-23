using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using TombLib.Utils;

namespace TombEditor
{
    // This is a temporary bridge to allow WPF to handle hotkeys in the editor, while still using WinForms for the main window.
    // It fixes issues such as Undo/Redo not working when the flyby timeline is focused.
    // Remove this once the editor is fully migrated to WPF.
    internal sealed class WpfEditorShortcutBridge : IDisposable
    {
        private readonly Form _form;
        private readonly Editor _editor;

        public WpfEditorShortcutBridge(Form form, Editor editor)
        {
            _form = form;
            _editor = editor;
            InputManager.Current.PostProcessInput += OnPostProcessInput;
        }

        public void Dispose()
        {
            InputManager.Current.PostProcessInput -= OnPostProcessInput;
        }

        private void OnPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (e.StagingItem.Input is not System.Windows.Input.KeyEventArgs keyEventArgs ||
                keyEventArgs.RoutedEvent != Keyboard.KeyDownEvent ||
                keyEventArgs.Handled ||
                !_form.ContainsFocus)
                return;

            Keys keyData = GetKeyData(keyEventArgs);

            if (WinFormsUtils.CurrentControlSupportsInput(_form, keyData) ||
                !_editor.Configuration.UI_Hotkeys.Any(set => set.Value.Contains(keyData)))
                return;

            CommandHandler.ExecuteHotkey(new CommandArgs
            {
                Editor = _editor,
                KeyData = keyData,
                Window = _form
            });

            keyEventArgs.Handled = true;
        }

        private static Keys GetKeyData(System.Windows.Input.KeyEventArgs keyEventArgs)
        {
            Key key = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            Keys keyData = (Keys)KeyInterop.VirtualKeyFromKey(key);
            ModifierKeys modifiers = keyEventArgs.KeyboardDevice.Modifiers;

            if (modifiers.HasFlag(ModifierKeys.Control))
                keyData |= Keys.Control;

            if (modifiers.HasFlag(ModifierKeys.Shift))
                keyData |= Keys.Shift;

            if (modifiers.HasFlag(ModifierKeys.Alt))
                keyData |= Keys.Alt;

            return keyData;
        }
    }
}
