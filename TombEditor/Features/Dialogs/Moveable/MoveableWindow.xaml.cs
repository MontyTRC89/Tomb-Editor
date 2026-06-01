#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Moveable;

public partial class MoveableWindow : Window
{
	public MoveableWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormMoveable");
	}
}
