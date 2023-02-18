using System.Threading;
using System.Windows;

namespace TombEditor.WPF;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		var editor = new Editor(SynchronizationContext.Current, new Configuration().LoadOrUseDefault<Configuration>());
		Editor.Instance = editor;

		base.OnStartup(e);
	}
}
