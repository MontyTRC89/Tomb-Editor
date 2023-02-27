using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormOptions.xaml
/// </summary>
public partial class FormOptions : Window
{
	public TombEditor.Forms.FormOptions WinFormsLayer { get; }

	public FormOptions(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormOptions(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
