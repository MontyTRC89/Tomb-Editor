using TombEditor.Controls;
using TombEditor.Controls.Panel3D;
using TombEditor.WPF.ViewModels;
using TombEditor.WPF.Views;

namespace TombEditor.WPF.ToolWindows;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : ViewUserControl<MainViewViewModel>
{
	private readonly Panel2DMap panel2DMap = new();
	private readonly Panel3D panel3D = new();

	public MainView()
	{
		InitializeComponent();

		panel3D.InitializeRendering(Editor.Instance.RenderingDevice, true, TombLib.Controls.ObjectRenderingQuality.High);
		host.Child = panel3D;

		Editor.Instance.EditorEventRaised += Instance_EditorEventRaised;
	}

	private void Instance_EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ModeChangedEvent modeChanged)
		{
			host.Child = modeChanged.Current switch
			{
				EditorMode.Map2D => panel2DMap,
				_ => panel3D
			};
		}
	}
}
