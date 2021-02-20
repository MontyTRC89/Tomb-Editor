using DarkUI.Controls;
using System;
using System.Windows.Forms;
using TombIDE.ScriptEditor.Properties;
using TombIDE.ScriptEditor.UI;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor.Controls
{
	internal class TextEditorContextMenu : DarkContextMenu
	{
		public event EventHandler ButtonClicked;
		private void OnButtonClicked(object sender, EventArgs e)
			=> ButtonClicked?.Invoke(sender, e);

		#region Items

		private ToolStripItem[] ItemList => new ToolStripItem[]
		{
			new ToolStripButton(Strings.Default.Cut, Resources.Cut_16, OnButtonClicked) { Tag = UIElement.Cut },
			new ToolStripButton(Strings.Default.Copy, Resources.Copy_16, OnButtonClicked) { Tag = UIElement.Copy },
			new ToolStripButton(Strings.Default.Paste, Resources.CopyComments_16, OnButtonClicked) { Tag = UIElement.Paste },
			new ToolStripSeparator(),
			new ToolStripButton(Strings.Default.CommentOut, Resources.Comment_16, OnButtonClicked) { Tag = UIElement.CommentOut },
			new ToolStripButton(Strings.Default.Uncomment, Resources.Uncomment_16, OnButtonClicked) { Tag = UIElement.Uncomment },
			new ToolStripSeparator(),
			new ToolStripButton(Strings.Default.ToggleBookmark, Resources.Bookmark_16, OnButtonClicked) { Tag = UIElement.ToggleBookmark },
		};

		#endregion Items

		public TextEditorContextMenu()
			=> Items.AddRange(ItemList);
	}
}
