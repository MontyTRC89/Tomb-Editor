using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormTextureRemap.xaml
/// </summary>
public partial class FormTextureRemap : Window
{
	public TombEditor.Forms.FormTextureRemap WinFormsLayer { get; }

	public FormTextureRemap(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormTextureRemap(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
