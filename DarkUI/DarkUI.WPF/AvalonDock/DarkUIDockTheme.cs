using System;

namespace DarkUI.WPF.AvalonDock;

public sealed class DarkUIDockTheme : global::AvalonDock.Themes.Theme
{
	public override Uri GetResourceUri()
		=> new("/DarkUI.WPF;component/Styles/AvalonDock/Theme.xaml", UriKind.Relative);
}
