#nullable enable

using System.Windows;
using TombEditor.ViewModels;

namespace TombEditor.Views;

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
