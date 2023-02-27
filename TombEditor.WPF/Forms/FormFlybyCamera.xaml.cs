using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormFlybyCamera.xaml
/// </summary>
public partial class FormFlybyCamera : Window
{
	public TombEditor.Forms.FormFlybyCamera WinFormsLayer { get; }

	public FormFlybyCamera(FlybyCameraInstance flyByCamera)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormFlybyCamera(flyByCamera);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
