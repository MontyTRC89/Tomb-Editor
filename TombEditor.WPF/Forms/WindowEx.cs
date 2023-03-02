using System.Windows;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.Forms;

public class WindowEx<T> : Window where T : ViewModelBase
{
	public T? ViewModel
	{
		get => DataContext as T;
		set => DataContext = value;
	}
}
