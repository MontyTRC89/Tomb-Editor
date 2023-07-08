using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DarkUI.WPF.Styles
{
	public partial class DataGrid : ResourceDictionary
	{
		private void TextBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is not TextBox textBox)
				return;

			textBox.Dispatcher.BeginInvoke(
				() => textBox.CaretIndex = textBox.Text.Length,
				DispatcherPriority.Loaded);
		}
	}
}
