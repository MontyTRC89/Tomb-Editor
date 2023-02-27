using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormRoomProperties.xaml
/// </summary>
public partial class FormRoomProperties : Window
{
	public TombEditor.Forms.FormRoomProperties WinFormsLayer { get; }

	public FormRoomProperties(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormRoomProperties(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
