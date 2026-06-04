#nullable enable

using System.Windows;

namespace TombEditor.Features.Dialogs.FootStepSounds;

public partial class FootStepSoundsWindow : Window
{
    public FootStepSoundsWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is FootStepSoundsWindowViewModel old)
        {
            old.RequestResetVisibleTexture -= OnResetVisible;
            old.RequestInvalidate -= OnInvalidate;
        }

        if (e.NewValue is FootStepSoundsWindowViewModel vm)
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
}
