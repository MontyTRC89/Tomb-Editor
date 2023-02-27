using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormAnimatedTextures.xaml
/// </summary>
public partial class FormAnimatedTextures : Window
{
	public TombEditor.Forms.FormAnimatedTextures WinFormsLayer { get; }

	public FormAnimatedTextures(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormAnimatedTextures(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		//panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };

		Closed += (sender, args) => WinFormsLayer.Close();
	}
}
