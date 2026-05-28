using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using TombEditor.ViewModels;

namespace TombEditor.Views;

public partial class OperationDialogWindow : Window
{
	public OperationDialogWindow()
	{
		InitializeComponent();
		Loaded += OnLoaded;
		Closing += OnClosing;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is OperationDialogWindowViewModel vm)
		{
			vm.SetWindowHandle(new WindowInteropHelper(this).Handle);
			vm.Start();
		}
	}

	private void OnClosing(object sender, CancelEventArgs e)
	{
		if (DataContext is OperationDialogWindowViewModel vm)
		{
			if (!vm.TryRequestClose())
				e.Cancel = true;
		}
	}
}
