using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormImportedGeometry.xaml
/// </summary>
public partial class FormImportedGeometry : Window
{
	public TombEditor.Forms.FormImportedGeometry WinFormsLayer { get; }

	public FormImportedGeometry(ImportedGeometryInstance instance, LevelSettings settings)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormImportedGeometry(instance, settings);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
