using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor
{
    // Global editor hotkey dispatch for the WPF shell.
    //
    // The legacy WinForms FormMain routed hotkeys through ProcessCmdKey. The WPF MainWindow has no
    // such hook, and keys typed while the WindowsFormsHost-hosted Panel3D (or a WPF panel) has focus
    // never reached the editor command system - so hotkeys did nothing in the 3D view.
    //
    // A thread-wide WinForms IMessageFilter fixes this: WindowsFormsIntegration bridges
    // Application.FilterMessage onto ComponentDispatcher.ThreadFilterMessage, so the filter sees
    // WM_KEYDOWN for every message pumped by the WPF dispatcher - both hosted WinForms HWNDs and the
    // WPF HwndSource. This mirrors how ControlScrollFilter already works in Program.cs.
    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        private EditorHotkeyMessageFilter _hotkeyFilter;

        private void InstallHotkeyFilter()
        {
            _hotkeyFilter = new EditorHotkeyMessageFilter(HandleHotkey);
            System.Windows.Forms.Application.AddMessageFilter(_hotkeyFilter);
        }

        private void RemoveHotkeyFilter()
        {
            if (_hotkeyFilter is null)
                return;

            System.Windows.Forms.Application.RemoveMessageFilter(_hotkeyFilter);
            _hotkeyFilter = null;
        }

        /// <summary>
        /// WPF-shell port of <c>FormMain.ProcessCmdKey</c>. Returns <see langword="true"/> to swallow
        /// the key (so it is not processed further), mirroring the legacy return values.
        /// </summary>
        private bool HandleHotkey(Keys keyData)
        {
            // Only act when this window is the active one; modal dialogs and other windows handle
            // their own keys (the legacy ProcessCmdKey was scoped to FormMain in the same way).
            if (!IsActive)
                return false;

            // Disable all hotkeys in fly mode except ToggleFlyMode
            if (_editor.FlyMode && !_editor.Configuration.UI_Hotkeys["ToggleFlyMode"].Contains(keyData))
                return false;

            if (_editor.CameraPreviewMode != CameraPreviewType.None)
            {
                if (keyData == Keys.Escape)
                {
                    _editor.ToggleCameraPreview(false);
                    return true;
                }

                // Disable all hotkeys in camera preview mode except PreviewCamera
                if (!_editor.Configuration.UI_Hotkeys["PreviewCamera"].Contains(keyData))
                    return true;
            }

            // Don't process reserved camera keys
            if (WinFormsUtils.DirectionalCameraKeys.Contains(keyData))
                return false;

            // Don't process one-key and shift hotkeys if we're focused on a control which allows text input
            if (FocusedControlSupportsTextInput(keyData))
                return false;

            CommandHandler.ExecuteHotkey(new CommandArgs
            {
                Editor = _editor,
                KeyData = keyData,
                Window = WinFormsDialogHelper.GetOpenFormOwner()
            });

            // Don't open menus with the alt key
            if (keyData.HasFlag(Keys.Alt))
                return true;

            return false;
        }

        /// <summary>
        /// WPF-aware equivalent of <c>WinFormsUtils.CurrentControlSupportsInput</c>: returns
        /// <see langword="true"/> when the focused element is a text-entry control that should
        /// receive the key as input instead of having it trigger a hotkey.
        /// </summary>
        private static bool FocusedControlSupportsTextInput(Keys keyData)
        {
            bool textRelevantKey =
                keyData.HasFlag(Keys.Control | Keys.A) ||
                keyData.HasFlag(Keys.Control | Keys.X) ||
                keyData.HasFlag(Keys.Control | Keys.C) ||
                keyData.HasFlag(Keys.Control | Keys.V) ||
                (!keyData.HasFlag(Keys.Control) && !keyData.HasFlag(Keys.Alt));

            if (!textRelevantKey)
                return false;

            // WPF-focused text controls
            var wpfFocused = System.Windows.Input.Keyboard.FocusedElement;
            if (wpfFocused is System.Windows.Controls.Primitives.TextBoxBase ||
                wpfFocused is System.Windows.Controls.PasswordBox ||
                (wpfFocused is System.Windows.Controls.ComboBox comboBox && comboBox.IsEditable))
                return true;

            // Focused WinForms control (e.g. inside the hosted Panel3D region or floating toolboxes)
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle == IntPtr.Zero)
                return false;

            var focusedControl = Control.FromHandle(focusedHandle);
            string controlType = focusedControl?.GetType().Name;

            return controlType is
                "DarkTextBox" or
                "DarkAutocompleteTextBox" or
                "DarkComboBox" or
                "DarkListBox" or
                "UpDownEdit" or
                "ToolStripTextBoxControl" or
                "TextBox" or
                "ComboBox";
        }

        /// <summary>
        /// Thread-wide key message filter that forwards WM_KEYDOWN / WM_SYSKEYDOWN to a handler.
        /// </summary>
        private sealed class EditorHotkeyMessageFilter : IMessageFilter
        {
            private const int WM_KEYDOWN = 0x0100;
            private const int WM_SYSKEYDOWN = 0x0104;

            private readonly Func<Keys, bool> _handler;

            public EditorHotkeyMessageFilter(Func<Keys, bool> handler)
            {
                _handler = handler;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_KEYDOWN && m.Msg != WM_SYSKEYDOWN)
                    return false;

                Keys keyData = (Keys)(int)m.WParam | Control.ModifierKeys;
                return _handler(keyData);
            }
        }
    }
}
