#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.BumpMaps;

public partial class BumpMapsWindow : Window
{
    public BumpMapsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        DataContextChanged += OnDataContextChanged;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BumpMapsWindowViewModel old)
        {
            old.RequestResetVisibleTexture -= OnResetVisible;
            old.RequestInvalidate -= OnInvalidate;
        }

        if (e.NewValue is BumpMapsWindowViewModel vm)
        {
            vm.RequestResetVisibleTexture += OnResetVisible;
            vm.RequestInvalidate += OnInvalidate;
            MapView.ResetVisibleTexture(vm.SelectedTexture);
        }
    }

    private void OnResetVisible(object? sender, TombLib.LevelData.LevelTexture? texture)
        => MapView.ResetVisibleTexture(texture);

    private void OnInvalidate(object? sender, System.EventArgs e)
        => MapView.InvalidateVisual();

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is BumpMapsWindowViewModel vm)
            vm.Cleanup();
    }
}
