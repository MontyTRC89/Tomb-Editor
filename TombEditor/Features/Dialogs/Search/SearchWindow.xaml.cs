using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Search;

public partial class SearchWindow : Window
{
	public SearchWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormSearch");
		Closed += (_, _) =>
		{
			if (DataContext is SearchWindowViewModel vm)
				vm.Dispose();
		};
	}
}
