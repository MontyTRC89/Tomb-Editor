using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormKeyboardLayout.xaml
/// </summary>
public partial class FormKeyboardLayout : Window
{
	public TombEditor.Forms.FormKeyboardLayout WinFormsLayer { get; }

	public FormKeyboardLayout(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormKeyboardLayout(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
