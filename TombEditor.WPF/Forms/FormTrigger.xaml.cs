using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormTrigger.xaml
/// </summary>
public partial class FormTrigger : Window
{
	public TombEditor.Forms.FormTrigger WinFormsLayer { get; }

	public FormTrigger(TriggerInstance trigger, Level level, Action<ObjectInstance> selectObject,
					   Action<Room> selectRoom)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormTrigger(trigger, level, selectObject, selectRoom);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
