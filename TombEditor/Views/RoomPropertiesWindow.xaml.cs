using System.Windows;
using TombEditor.ViewModels;

namespace TombEditor.Views;

public partial class RoomPropertiesWindow : Window
{
	public RoomPropertiesWindow()
	{
		InitializeComponent();
		Closed += (_, _) =>
		{
			if (DataContext is RoomPropertiesWindowViewModel vm)
				vm.Dispose();
		};
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
