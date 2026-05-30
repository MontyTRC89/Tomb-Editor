using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.RoomProperties;

public partial class RoomPropertiesWindow : Window
{
	public RoomPropertiesWindow()
	{
		InitializeComponent();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormRoomProperties");
		Closed += (_, _) =>
		{
			if (DataContext is RoomPropertiesWindowViewModel vm)
				vm.Dispose();
		};
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
