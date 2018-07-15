using DarkUI.Forms;
using FastColoredTextBoxNS;
using System.Windows.Forms;

namespace ScriptEditor
{
	public class Bookmarks
	{
		public static void ToggleBookmark(FastColoredTextBox textEditor)
		{
			// Get the current line number
			int currentLine = textEditor.Selection.Start.iLine;

			// If there's a bookmark on the current line
			if (textEditor.Bookmarks.Contains(currentLine))
			{
				textEditor.Bookmarks.Remove(currentLine);
			}
			else
			{
				textEditor.Bookmarks.Add(currentLine);
			}
		}

		public static void ClearAllBookmarks(FastColoredTextBox textEditor)
		{
			using (FormMain owner = new FormMain())
			{
				DialogResult result = DarkMessageBox.Show(owner,
					Resources.Messages.ClearBookmarks, "Delete all bookmarks?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					// Clear all bookmarks
					textEditor.Bookmarks.Clear();

					// Redraw the editor
					textEditor.Invalidate();
				}
			}
		}
	}
}
