using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormPreviewWad.xaml
/// </summary>
public partial class FormPreviewWad : Window
{
	public TombEditor.Forms.FormPreviewWad WinFormsLayer { get; }

	public FormPreviewWad(Wad2 wad, RenderingDevice device, Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormPreviewWad(wad, device, editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
