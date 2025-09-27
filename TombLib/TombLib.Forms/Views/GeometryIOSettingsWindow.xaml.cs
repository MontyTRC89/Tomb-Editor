using System.Windows;
using TombLib.Forms.ViewModels;
using TombLib.WPF;

namespace TombLib.Forms.Views;

public partial class GeometryIOSettingsWindow : Window
{
	public GeometryIOSettingsWindow()
	{
		InitializeComponent();
		Loaded += GeometryIOSettingsWindow_Loaded;
	}

	private void GeometryIOSettingsWindow_Loaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is GeometryIOSettingsWindowViewModel vm)
		{
			Title = vm.InternalSettings.Export
				? Localizer.Instance["TombLib.GeometryIOSettingsWindow.TitleExport"]
				: Localizer.Instance["TombLib.GeometryIOSettingsWindow.TitleImport"];
		}
	}
}
