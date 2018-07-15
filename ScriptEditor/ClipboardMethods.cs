using FastColoredTextBoxNS;
using System.Windows.Forms;

namespace ScriptEditor
{
	public class ClipboardMethods
	{
		public static void CutToClipboard(FastColoredTextBox textEditor)
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
				textEditor.SelectedText = string.Empty;
			}
		}

		public static void CopyToClipboard(FastColoredTextBox textEditor)
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
			}
		}

		public static void PasteFromClipboard(FastColoredTextBox textEditor)
		{
			textEditor.SelectedText = Clipboard.GetText();
		}
	}
}
