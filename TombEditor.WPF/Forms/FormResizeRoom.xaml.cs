using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;
using TombLib;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormResizeRoom.xaml
/// </summary>
public partial class FormResizeRoom : Window
{
	public TombEditor.Forms.FormResizeRoom WinFormsLayer { get; }

	public FormResizeRoom(Editor editor, Room roomToResize, RectangleInt2 newArea)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormResizeRoom(editor, roomToResize, newArea);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
