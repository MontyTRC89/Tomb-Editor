using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormMoveable.xaml
/// </summary>
public partial class FormMoveable : Window
{
	public TombEditor.Forms.FormMoveable WinFormsLayer { get; }

	public FormMoveable(MoveableInstance moveable)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormMoveable(moveable);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
