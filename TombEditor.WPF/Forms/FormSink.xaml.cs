using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormSink.xaml
/// </summary>
public partial class FormSink : Window
{
	public TombEditor.Forms.FormSink WinFormsLayer { get; }

	public FormSink(SinkInstance sink)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormSink(sink);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
