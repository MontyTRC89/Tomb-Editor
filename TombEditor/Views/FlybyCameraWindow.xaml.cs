#nullable enable

using System.ComponentModel;
using System.Windows;
using TombEditor.ViewModels;

namespace TombEditor.Views;

public partial class FlybyCameraWindow : Window
{
    public FlybyCameraWindow()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Closing += OnClosing;
        Closed += OnClosedHandler;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.BeginPreview();
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.OnWindowClosing();
    }

    private void OnClosedHandler(object sender, System.EventArgs e)
    {
        if (DataContext is FlybyCameraWindowViewModel vm)
            vm.EndPreview();
    }
}
