using System.Windows.Forms.Integration;
using TombEditor.Controls;
using TombEditor.Controls.Panel3D;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView2 : System.Windows.Controls.UserControl
{
	private Panel3D panel3D = new();
	private Panel2DMap panel2DMap = new();

	public MainView2()
	{
		InitializeComponent();
		Content = new WindowsFormsHost { Child = panel2DMap };
	}
}
