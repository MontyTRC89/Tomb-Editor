using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for ToolPaletteFloating.xaml
/// </summary>
public partial class ToolPaletteFloating : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.ToolPaletteFloating WinFormsLayer { get; }

	public ToolPaletteFloating()
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.ToolWindows.ToolPaletteFloating();

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
