#nullable enable

using System.Windows;

namespace TombEditor.Features.Dialogs.FindTextures;

public partial class FindTexturesWindow : Window
{
    public FindTexturesWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is FindTexturesWindowViewModel vm)
            vm.Cleanup();
    }
}
