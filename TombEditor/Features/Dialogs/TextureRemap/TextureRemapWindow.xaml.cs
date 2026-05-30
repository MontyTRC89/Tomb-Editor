#nullable enable

using System.Windows;

namespace TombEditor.Features.Dialogs.TextureRemap;

public partial class TextureRemapWindow : Window
{
    public TextureRemapWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TextureRemapWindowViewModel old)
            old.RemapRectanglesChanged -= OnRectanglesChanged;

        if (e.NewValue is TextureRemapWindowViewModel vm)
        {
            SourceMapView.DataContext = vm;
            DestinationMapView.DataContext = vm;
            SourceMapView.ResetVisibleTexture(vm.SourceTexture);
            DestinationMapView.ResetVisibleTexture(vm.DestinationTexture);

            vm.RemapRectanglesChanged += OnRectanglesChanged;
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(vm.SourceTexture))
                    SourceMapView.ResetVisibleTexture(vm.SourceTexture);
                else if (args.PropertyName == nameof(vm.DestinationTexture))
                    DestinationMapView.ResetVisibleTexture(vm.DestinationTexture);
            };
        }
    }

    private void OnRectanglesChanged(object? sender, System.EventArgs e)
    {
        SourceMapView.InvalidateVisual();
        DestinationMapView.InvalidateVisual();
    }
}
