using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.LevelData;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormChooseRoom.xaml
/// </summary>
public partial class FormChooseRoom : Window
{
	public TombEditor.Forms.FormChooseRoom WinFormsLayer { get; }

	public FormChooseRoom(string why, IEnumerable<Room> rooms, Action<Room> roomSelectionChanged)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormChooseRoom(why, rooms, roomSelectionChanged);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
