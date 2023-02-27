using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormStatic.xaml
/// </summary>
public partial class FormStatic : Window
{
	public TombEditor.Forms.FormStatic WinFormsLayer { get; }

	public FormStatic(StaticInstance staticMesh)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormStatic(staticMesh);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
