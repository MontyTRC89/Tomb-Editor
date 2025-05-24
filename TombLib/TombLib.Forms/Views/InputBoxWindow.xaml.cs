using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using TombLib.Forms.ViewModels;

namespace TombLib.Views;

public partial class InputBoxWindow : Window
{
	public InputBoxWindow()
	{
		InitializeComponent();
		DataContext = new InputBoxViewModel(WeakReferenceMessenger.Default);
		Loaded += InputBoxWindow_Loaded;
	}

	private void InputBoxWindow_Loaded(object sender, RoutedEventArgs e)
	{
		textBox_Value.Focus();
		Dispatcher.BeginInvoke(textBox_Value.SelectAll);
	}
}
