using System.Windows;
using System.Windows.Input;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Search;

public partial class SearchWindow : Window
{
	public SearchWindow()
	{
		InitializeComponent();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSearch");
		Closed += (_, _) =>
		{
			if (DataContext is SearchWindowViewModel vm)
				vm.Dispose();
		};
		PreviewKeyDown += OnPreviewKeyDown;
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Delete && DataContext is SearchWindowViewModel vm && vm.SelectedRow is not null)
		{
			vm.DeleteSelectedCommand.Execute(null);
			e.Handled = true;
		}
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
