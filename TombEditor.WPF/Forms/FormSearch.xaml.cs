using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormSearch.xaml
/// </summary>
public partial class FormSearch : Window
{
	public TombEditor.Forms.FormSearch WinFormsLayer { get; }

	public FormSearch(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormSearch(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
