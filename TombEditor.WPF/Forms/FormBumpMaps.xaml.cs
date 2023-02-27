using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormBumpMaps.xaml
/// </summary>
public partial class FormBumpMaps : Window
{
	public TombEditor.Forms.FormBumpMaps WinFormsLayer { get; }

	public FormBumpMaps(Editor editor, LevelTexture texture)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormBumpMaps(editor, texture);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
