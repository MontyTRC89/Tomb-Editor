using System.Windows;

namespace DarkUI.WPF.Demo;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		OriginalPreviewHost.Child = new OriginalComparison();
	}
}
