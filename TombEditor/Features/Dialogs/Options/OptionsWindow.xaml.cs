#nullable enable

using System.Windows;
using System.Windows.Controls;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Options;

public partial class OptionsWindow : Window
{
    public OptionsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }

    private void Preset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is OptionsWindowViewModel vm && sender is ComboBox combo && combo.SelectedItem is string presetName)
            vm.ApplyColorSchemePreset(presetName);
    }
}
