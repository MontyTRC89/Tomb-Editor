using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormSprite.xaml
/// </summary>
public partial class FormSprite : Window
{
	public TombEditor.Forms.FormSprite WinFormsLayer { get; }

	public FormSprite(SpriteInstance instance)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormSprite(instance);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
