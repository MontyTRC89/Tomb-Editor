using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormAnimatFormSoundSourceedTextures.xaml
/// </summary>
public partial class FormSoundSource : Window
{
	public TombEditor.Forms.FormSoundSource WinFormsLayer { get; }

	public FormSoundSource(SoundSourceInstance soundSource)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormSoundSource(soundSource);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
