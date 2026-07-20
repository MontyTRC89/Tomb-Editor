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
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BumpMapsWindowViewModel old)
        {
            old.RequestResetVisibleTexture -= OnResetVisible;
            old.RequestInvalidate -= OnInvalidate;
            old.MapSelectionProvider = null;
        }

        if (e.NewValue is BumpMapsWindowViewModel vm)
        {
            vm.RequestResetVisibleTexture += OnResetVisible;
            vm.RequestInvalidate += OnInvalidate;
            vm.MapSelectionProvider = () => mapView.SelectedTexture;
            mapView.ResetVisibleTexture(vm.SelectedTexture);
        }
    }

    private void OnResetVisible(object? sender, TombLib.LevelData.LevelTexture? texture)
        => mapView.ResetVisibleTexture(texture);

    private void OnInvalidate(object? sender, System.EventArgs e)
        => mapView.InvalidateVisual();
}
