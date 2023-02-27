using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormSelectRoomByTags.xaml
/// </summary>
public partial class FormSelectRoomByTags : Window
{
	public TombEditor.Forms.FormSelectRoomByTags WinFormsLayer { get; }

	public FormSelectRoomByTags(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormSelectRoomByTags(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
