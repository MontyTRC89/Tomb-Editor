using System.Windows.Controls;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

/// <summary>
/// Interaction logic for RoomOptions.xaml
/// </summary>
public partial class RoomOptions : UserControl
{
	public RoomOptions()
	{
		InitializeComponent();
		DataContext = new RoomOptionsViewModel(Editor.Instance);
	}
}
