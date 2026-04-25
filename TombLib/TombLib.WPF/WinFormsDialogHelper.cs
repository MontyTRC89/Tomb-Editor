#nullable enable

using System.Windows.Forms;

namespace TombLib.WPF;

/// <summary>
/// Provides helpers for WinForms dialogs hosted from WPF views.
/// </summary>
public static class WinFormsDialogHelper
{
    /// <summary>
    /// Gets the first open WinForms window that can own modal dialogs.
    /// </summary>
    public static IWin32Window? GetOpenFormOwner()
    {
        if (Form.ActiveForm is not null)
            return Form.ActiveForm;

        return Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
    }
}
