#nullable enable

using System.Windows.Controls;

namespace TombEditor.Features.DockableViews.TexturePanel;

public partial class TexturePanelView : UserControl
{
	private readonly TexturePanelViewModel _viewModel;
	private readonly TexturePanel.PanelTextureMapMain _panelTextureMap;

	public TexturePanelView()
	{
		InitializeComponent();

		_panelTextureMap = new TexturePanel.PanelTextureMapMain();
		ViewportHost.Child = _panelTextureMap;

		// Bridge the WinForms 2D texture map selection back to Editor state.
		_panelTextureMap.SelectedTextureChanged += (_, _) =>
			Editor.Instance.SelectedTexture = _panelTextureMap.SelectedTexture;

		_viewModel = new TexturePanelViewModel(Editor.Instance);
		_viewModel.RequestResetVisibleTexture += (_, tex) => _panelTextureMap.ResetVisibleTexture(tex);
		_viewModel.RequestShowTexture += (_, area) => _panelTextureMap.ShowTexture(area);
		_viewModel.RequestInvalidate += (_, _) => _panelTextureMap.Invalidate();

		DataContext = _viewModel;
	}

	public void Cleanup() => _viewModel.Cleanup();
}
