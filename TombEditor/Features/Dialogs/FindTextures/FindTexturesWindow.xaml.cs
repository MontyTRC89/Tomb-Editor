#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FindTextures;

public partial class FindTexturesWindow : Window
{
    public FindTexturesWindow()
    {
        InitializeComponent();
		this.HookModalAutoClose();
        Closed += OnClosed;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is FindTexturesWindowViewModel vm)
            vm.Cleanup();
    }
}
