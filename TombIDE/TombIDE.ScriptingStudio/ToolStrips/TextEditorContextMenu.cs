using DarkUI.Controls;
using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Controls
{
	internal class TextEditorContextMenu : DarkContextMenu
	{
		public event EventHandler ButtonClicked;
		private void OnButtonClicked(object sender, EventArgs e)
			=> ButtonClicked?.Invoke(sender, e);

		#region Items

		private ToolStripItem[] ItemList => new ToolStripItem[]
		{
			new ToolStripButton(Strings.Default.Cut, Resources.Cut_16, OnButtonClicked) { Tag = UICommand.Cut },
			new ToolStripButton(Strings.Default.Copy, Resources.Copy_16, OnButtonClicked) { Tag = UICommand.Copy },
			new ToolStripButton(Strings.Default.Paste, Resources.CopyComments_16, OnButtonClicked) { Tag = UICommand.Paste },
			new ToolStripSeparator(),
			new ToolStripButton(Strings.Default.CommentOut, Resources.Comment_16, OnButtonClicked) { Tag = UICommand.CommentOut },
			new ToolStripButton(Strings.Default.Uncomment, Resources.Uncomment_16, OnButtonClicked) { Tag = UICommand.Uncomment },
			new ToolStripSeparator(),
			new ToolStripButton(Strings.Default.ToggleBookmark, Resources.Bookmark_16, OnButtonClicked) { Tag = UICommand.ToggleBookmark },
		};

		#endregion Items

		public TextEditorContextMenu()
			=> Items.AddRange(ItemList);
	}
}
