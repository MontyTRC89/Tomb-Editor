#nullable enable

using System.Windows;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Minimal themed text prompt (replaces the WinForms <c>FormInputBox</c> for the WPF editor;
    /// <c>TombLib.Forms.InputBoxWindow</c> is unavailable here because TombLib.WPF cannot reference
    /// TombLib.Forms). Bind an <see cref="InputDialogViewModel"/> as DataContext and call ShowDialog().
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
            this.HookModalAutoClose();
            Loaded += (_, _) => { input.Focus(); input.SelectAll(); };
        }
    }
}
