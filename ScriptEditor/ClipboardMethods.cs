using FastColoredTextBoxNS;
using System.Windows.Forms;

namespace ScriptEditor
{
	public class ClipboardMethods
	{
		public static void Cut(FastColoredTextBox textEditor)
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
				textEditor.SelectedText = string.Empty;
			}
		}

		public static void Copy(FastColoredTextBox textEditor)
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
			}
		}

		public static void Paste(FastColoredTextBox textEditor)
		{
			textEditor.SelectedText = Clipboard.GetText();
		}
	}
}
