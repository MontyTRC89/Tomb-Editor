#nullable enable

using System.Windows;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Minimal themed text prompt (replaces the WinForms <c>FormInputBox</c> for the WPF editor).
    /// </summary>
    public partial class InputDialog : Window
    {
        public string Value => input.Text;

        public InputDialog(string title, string labelText, string initial)
        {
            InitializeComponent();
            Title = title;
            label.Text = labelText;
            input.Text = initial ?? string.Empty;
            Loaded += (_, _) => { input.Focus(); input.SelectAll(); };
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
