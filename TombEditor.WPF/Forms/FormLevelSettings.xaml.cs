using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormLevelSettings.xaml
/// </summary>
public partial class FormLevelSettings : Window
{
	public TombEditor.Forms.FormLevelSettings WinFormsLayer { get; }

	public FormLevelSettings(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormLevelSettings(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
