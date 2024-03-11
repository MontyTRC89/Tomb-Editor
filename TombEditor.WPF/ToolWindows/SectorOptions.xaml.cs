using System.Windows.Controls;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

public partial class SectorOptions : UserControl
{
	public SectorOptions()
	{
		InitializeComponent();
		DataContext = new SectorOptionsViewModel(Editor.Instance);
	}
}
