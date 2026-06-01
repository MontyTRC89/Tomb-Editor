using System.Windows;
using TombLib.WPF;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace TombEditor.Features.Dialogs.KeyboardLayout;

public partial class KeyboardLayoutWindow : Window
{
	public KeyboardLayoutWindow()
	{
		InitializeComponent();
		this.HookModalAutoClose();
		PreviewKeyDown += OnPreviewKeyDown;
		PreviewKeyUp += OnPreviewKeyUp;
	}

	private KeyboardLayoutWindowViewModel? Vm => DataContext as KeyboardLayoutWindowViewModel;

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (Vm is not { IsListening: true } vm)
		{
			if (e.Key == Key.Delete && Vm?.SelectedRow is { } row)
				Vm?.DeleteHotkeysCommand.Execute(row);
			return;
		}

		var winFormsKey = (WinForms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
		var modifiers = WinForms.Keys.None;
		if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) modifiers |= WinForms.Keys.Control;
		if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) modifiers |= WinForms.Keys.Shift;
		if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0) modifiers |= WinForms.Keys.Alt;
		vm.OnListenKeyDown(winFormsKey | modifiers);
		e.Handled = true;
	}

	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (Vm is { IsListening: true } vm)
		{
			vm.OnListenKeyUp();
			e.Handled = true;
		}
	}

	private void ListenOverlay_MouseDown(object sender, MouseButtonEventArgs e)
		=> Vm?.StopListeningCommand.Execute(null);
}
