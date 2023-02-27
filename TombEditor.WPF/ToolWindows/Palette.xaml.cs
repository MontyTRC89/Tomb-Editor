using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombEditor.Controls;

namespace TombEditor.WPF.ToolWindows;
/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
public partial class Palette : System.Windows.Controls.UserControl
{
	public TombEditor.ToolWindows.Palette WinFormsLayer { get; }

	public Palette()
	{
		InitializeComponent();
		//WinFormsLayer = new TombEditor.ToolWindows.Palette();

		//var panel = new Panel();
		//panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		PaletteGridHost.Child = new PanelPalette();
	}
}
