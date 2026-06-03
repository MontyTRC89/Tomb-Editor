#nullable enable

using System.Windows;
using System.Windows.Controls;
using TombLib.Controls;
using TombLib.WPF;
using DrawingColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.Options
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            this.HookModalAutoClose();
        }

        private void ColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OptionItem item)
                return;

            var current = item.Value is WpfColor c ? c : WpfColor.FromRgb(0, 0, 0);

            using var dialog = new RealtimeColorDialog
            {
                Color = DrawingColor.FromArgb(current.R, current.G, current.B),
                FullOpen = true
            };

            if (dialog.ShowDialog(this.GetWin32Window()) == System.Windows.Forms.DialogResult.OK)
                item.Value = WpfColor.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
        }

        private void Preset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is OptionsWindowViewModel vm && sender is ComboBox combo && combo.SelectedItem is string presetName)
                vm.ApplyColorSchemePreset(presetName);
        }
    }
}
