using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormReplaceObject.xaml
/// </summary>
public partial class FormReplaceObject : Window
{
	public TombEditor.Forms.FormReplaceObject WinFormsLayer { get; }

	public FormReplaceObject(Editor editor, bool fromContext = false)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormReplaceObject(editor, fromContext);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
