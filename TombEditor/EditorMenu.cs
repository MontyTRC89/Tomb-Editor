#nullable enable

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor;

/// <summary>
/// Attached property that wires a WPF <see cref="MenuItem"/> to a registered
/// <c>CommandHandler</c> command, mirroring the runtime population that
/// <c>FormMain.GenerateMenusRecursive</c> does on the WinForms menu strip.
/// </summary>
/// <remarks>
/// Usage: <c>&lt;MenuItem editor:EditorMenu.Command="NewLevel" /&gt;</c>. The attached property
/// sets the <see cref="MenuItem.Header"/> to <c>command.FriendlyName</c> (or — for
/// <see cref="CommandType.Windows"/> commands — <c>Name.Replace("Show","").SplitCamelcase()</c>),
/// populates <see cref="MenuItem.InputGestureText"/> from the configured hotkeys, and binds
/// <see cref="MenuItem.Command"/> to a <see cref="RelayCommand"/> that invokes the registered
/// <c>CommandObj.Execute</c> with the current editor and active owner window.
/// </remarks>
public static class EditorMenu
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
		"Command",
		typeof(string),
		typeof(EditorMenu),
		new PropertyMetadata(null, OnCommandChanged));

	public static string? GetCommand(DependencyObject obj) => (string?)obj.GetValue(CommandProperty);
	public static void SetCommand(DependencyObject obj, string? value) => obj.SetValue(CommandProperty, value);

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not MenuItem menuItem || e.NewValue is not string commandName || string.IsNullOrWhiteSpace(commandName))
			return;

		CommandObj? cmd;
		try
		{
			cmd = CommandHandler.GetCommand(commandName);
		}
		catch
		{
			// Unknown command: leave the menu entry as a static placeholder so the XAML still parses.
			return;
		}

		if (cmd is null)
			return;

		menuItem.Header = cmd.Type == CommandType.Windows
			? cmd.Name.Replace("Show", string.Empty).SplitCamelcase()
			: cmd.FriendlyName;

		var hotkeys = Editor.Instance.Configuration.UI_Hotkeys[commandName];
		menuItem.InputGestureText = string.Join(", ",
			hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));

		menuItem.Command = new RelayCommand(() =>
		{
			cmd.Execute?.Invoke(new CommandArgs
			{
				Editor = Editor.Instance,
				Window = WPFUtils.GetWin32WindowOwner(),
			});
		});
	}
}
