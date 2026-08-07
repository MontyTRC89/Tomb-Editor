#nullable enable

using System.Windows.Controls;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Rendering;

internal static class TextEditorContextMenuFactory
{
	public static ContextMenu BuildDefault()
	{
		var menu = new ContextMenu();

		var cutItem = new MenuItem { Header = "Cut", Command = ApplicationCommands.Cut };
		var copyItem = new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy };
		var pasteItem = new MenuItem { Header = "Paste", Command = ApplicationCommands.Paste };
		var selectAllItem = new MenuItem { Header = "Select All", Command = ApplicationCommands.SelectAll };

		menu.Items.Add(cutItem);
		menu.Items.Add(copyItem);
		menu.Items.Add(pasteItem);
		menu.Items.Add(new Separator());
		menu.Items.Add(selectAllItem);

		return menu;
	}
}
