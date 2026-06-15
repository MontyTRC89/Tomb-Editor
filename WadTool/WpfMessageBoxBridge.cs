#nullable enable

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using CustomMessageBox.WPF;
using DarkUI.Forms;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace WadTool;

/// <summary>
/// Routes <see cref="DarkMessageBox"/> calls (used throughout WadActions and the shared editor
/// code) onto the WPF <see cref="CMessageBox"/> so message boxes match the WPF shell instead of
/// opening the WinForms dialog. Installed by <see cref="Program"/> for the WPF shell only; the
/// legacy --winforms shell keeps the WinForms dialog.
/// </summary>
internal static class WpfMessageBoxBridge
{
    public static void Install()
    {
        DarkMessageBox.ShowOverride = Show;
    }

    private static DialogResult Show(IWin32Window? owner, string message, string caption,
        MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
    {
        var cButtons = buttons switch
        {
            MessageBoxButtons.OKCancel => CMessageBoxButtons.OKCancel,
            MessageBoxButtons.YesNo => CMessageBoxButtons.YesNo,
            MessageBoxButtons.YesNoCancel => CMessageBoxButtons.YesNoCancel,
            MessageBoxButtons.RetryCancel => CMessageBoxButtons.RetryCancel,
            MessageBoxButtons.AbortRetryIgnore => CMessageBoxButtons.AbortRetryIgnore,
            _ => CMessageBoxButtons.OK,
        };

        var cIcon = icon switch
        {
            MessageBoxIcon.Information => CMessageBoxIcon.Information,
            MessageBoxIcon.Warning => CMessageBoxIcon.Warning,
            MessageBoxIcon.Error => CMessageBoxIcon.Error,
            MessageBoxIcon.Question => CMessageBoxIcon.Question,
            _ => CMessageBoxIcon.None,
        };

        var cDefault = defaultButton switch
        {
            MessageBoxDefaultButton.Button2 => CMessageBoxDefaultButton.Button2,
            MessageBoxDefaultButton.Button3 => CMessageBoxDefaultButton.Button3,
            _ => CMessageBoxDefaultButton.Button1,
        };

        CMessageBoxResult result = FindWpfWindow(owner) is { } wpfOwner
            ? CMessageBox.Show(wpfOwner, message, caption, cButtons, cIcon, cDefault)
            : CMessageBox.Show(message, caption, cButtons, cIcon, cDefault);

        return result switch
        {
            CMessageBoxResult.OK => DialogResult.OK,
            CMessageBoxResult.Cancel => DialogResult.Cancel,
            CMessageBoxResult.Yes => DialogResult.Yes,
            CMessageBoxResult.No => DialogResult.No,
            CMessageBoxResult.Abort => DialogResult.Abort,
            CMessageBoxResult.Retry => DialogResult.Retry,
            CMessageBoxResult.Ignore => DialogResult.Ignore,
            _ => DialogResult.None,
        };
    }

    private static Window? FindWpfWindow(IWin32Window? owner)
    {
        if (System.Windows.Application.Current is not { } app)
            return null;

        if (owner is not null && owner.Handle != IntPtr.Zero)
        {
            foreach (Window window in app.Windows)
            {
                if (new WindowInteropHelper(window).Handle == owner.Handle)
                    return window;
            }
        }

        // Owner unset or a hosted WinForms control: fall back to the active WPF window so the
        // box still centers sensibly.
        foreach (Window window in app.Windows)
        {
            if (window.IsActive)
                return window;
        }

        return app.MainWindow;
    }
}
