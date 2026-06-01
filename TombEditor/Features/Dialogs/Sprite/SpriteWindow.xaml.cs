using System.ComponentModel;
using System.Windows;
using TombLib.WPF;
using WadTool.Controls;

namespace TombEditor.Features.Dialogs.Sprite;

public partial class SpriteWindow : Window
{
	private readonly PanelRenderingSprite _panelRenderingSprite;

	public SpriteWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();

		_panelRenderingSprite = new PanelRenderingSprite();
		previewHost.Child = _panelRenderingSprite;

		Loaded += OnLoaded;
		Closed += OnClosed;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSprite");

		if (DataContext is not SpriteWindowViewModel vm)
			return;

		var editor = Editor.Instance;
		_panelRenderingSprite.InitializeRendering(editor.RenderingDevice, editor.Configuration.RenderingItem_Antialias);
		_panelRenderingSprite.SpriteID = vm.PreviewSpriteIndex;

		vm.PropertyChanged += OnVmPropertyChanged;
	}

	private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SpriteWindowViewModel.PreviewSpriteIndex)
			&& sender is SpriteWindowViewModel vm)
		{
			_panelRenderingSprite.SpriteID = vm.PreviewSpriteIndex;
		}
	}

	private void OnClosed(object sender, System.EventArgs e)
	{
		if (DataContext is SpriteWindowViewModel vm)
		{
			vm.PropertyChanged -= OnVmPropertyChanged;
			vm.Detach();
		}
	}
}
