using System.Windows;
using TombLib.Forms.ViewModels;

namespace TombLib.Forms.Views;

public partial class GeometryIOSettingsWindow : Window
{
	public GeometryIOSettingsWindow(GeometryIOSettingsType settingsType)
	{
		InitializeComponent();
		DataContext = new GeometryIOSettingsWindowViewModel(settingsType);
	}
}
