using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using TombLib.Forms.ViewModels;

namespace TombLib.Views;

public partial class GeometryIOSettingsWindow : Window
{
	public GeometryIOSettingsWindow()
	{
		InitializeComponent();
		DataContext = new GeometryIOSettingsViewModel(WeakReferenceMessenger.Default, GeometryIOSettingsType.Import);
	}
}
