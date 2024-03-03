using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for ToolPalette.xaml
/// </summary>
public partial class ToolPalette : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.ToolPalette WinFormsLayer { get; }

	public ToolPalette()
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.ToolWindows.ToolPalette();

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
