using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;

/// <summary>
/// Interaction logic for Lighting.xaml
/// </summary>
public partial class Lighting : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.Lighting WinFormsLayer { get; }

	public Lighting()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.Lighting();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		//Content = new WindowsFormsHost { Child = panel };
	}
}
