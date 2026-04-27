#nullable enable

using System.Windows.Forms;

namespace TombLib.WPF;

/// <summary>
/// Provides helpers for WinForms dialogs hosted from WPF views.
/// </summary>
public static class WinFormsDialogHelper
{
    /// <summary>
    /// Gets a WinForms owner for modal dialogs, falling back to a dummy owner when no open form is available.
    /// </summary>
    public static IWin32Window GetOpenFormOwner()
        => WPFUtils.GetWin32WindowOwner();
}
