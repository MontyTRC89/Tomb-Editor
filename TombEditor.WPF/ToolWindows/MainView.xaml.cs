using System.Windows.Forms.Integration;
using TombEditor.Controls;
using TombEditor.Controls.Panel3D;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : System.Windows.Controls.UserControl
{
	private Panel3D panel3D = new();

	public MainView()
	{
		InitializeComponent();

		panel3D.InitializeRendering(Editor.Instance.RenderingDevice, true, TombLib.Controls.ObjectRenderingQuality.Medium);
		Content = new WindowsFormsHost { Child = panel3D };
	}
}
