using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormImportPrj.xaml
/// </summary>
public partial class FormImportPrj : Window
{
	public TombEditor.Forms.FormImportPrj WinFormsLayer { get; }

	public FormImportPrj(string prjPath, bool respectMousepatch, bool useHalfPixelCorrection)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormImportPrj(prjPath, respectMousepatch, useHalfPixelCorrection);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
