using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.MainView WinFormsLayer { get; }

	public MainView()
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.ToolWindows.MainView();

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
