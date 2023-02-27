using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormVolume.xaml
/// </summary>
public partial class FormVolume : Window
{
	public TombEditor.Forms.FormVolume WinFormsLayer { get; }

	public FormVolume(VolumeInstance instance)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormVolume(instance);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
