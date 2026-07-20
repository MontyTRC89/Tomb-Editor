#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.RoomsPanel;

public partial class RoomsView : UserControl
{
    private readonly RoomsViewModel _viewModel;

    public RoomsView()
    {
        InitializeComponent();
        _viewModel = new RoomsViewModel(Editor.Instance);
        DataContext = _viewModel;
    }

    public void Cleanup() => _viewModel.Cleanup();
}
