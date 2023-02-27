using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormCamera.xaml
/// </summary>
public partial class FormCamera : Window
{
	public TombEditor.Forms.FormCamera WinFormsLayer { get; }

	public FormCamera(CameraInstance instance)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormCamera(instance);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
