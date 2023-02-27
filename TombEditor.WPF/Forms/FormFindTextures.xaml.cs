using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormFindTextures.xaml
/// </summary>
public partial class FormFindTextures : Window
{
	public TombEditor.Forms.FormFindTextures WinFormsLayer { get; }

	public FormFindTextures(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormFindTextures(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
