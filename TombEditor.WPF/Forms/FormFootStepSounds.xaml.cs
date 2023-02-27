using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormFootStepSounds.xaml
/// </summary>
public partial class FormFootStepSounds : Window
{
	public TombEditor.Forms.FormFootStepSounds WinFormsLayer { get; }

	public FormFootStepSounds(Editor editor, LevelTexture texture)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormFootStepSounds(editor, texture);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
