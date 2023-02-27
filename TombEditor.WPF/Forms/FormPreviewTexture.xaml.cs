using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormPreviewTexture.xaml
/// </summary>
public partial class FormPreviewTexture : Window
{
	public TombEditor.Forms.FormPreviewTexture WinFormsLayer { get; }

	public FormPreviewTexture(LevelTexture texture)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormPreviewTexture(texture);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
