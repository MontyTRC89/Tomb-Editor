using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormMemo.xaml
/// </summary>
public partial class FormMemo : Window
{
	public TombEditor.Forms.FormMemo WinFormsLayer { get; }

	public FormMemo(MemoInstance memo)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormMemo(memo);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
