using FastColoredTextBoxNS;

namespace ScriptEditor
{
	public class VisibleSpaces
	{
		public static void HandleVisibleSpaces(FastColoredTextBox textEditor)
		{
			// If "Show Spaces" is enabled and the text contains spaces
			if (Properties.Settings.Default.ShowSpaces && textEditor.Text.Contains(" "))
			{
				// Cache caret position
				Place caretPosition = textEditor.Selection.Start;

				// Scan all lines
				for (int i = 0; i < textEditor.LinesCount; i++)
				{
					// If a line contains whitespace
					if (textEditor.GetLineText(i).Contains(" "))
					{
						textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
						textEditor.InsertText(textEditor.GetLineText(i).Replace(" ", "·"));
					}
				}

				// Restore caret position
				textEditor.Selection.Start = caretPosition;
			}
			else if (!Properties.Settings.Default.ShowSpaces && textEditor.Text.Contains("·"))
			{
				// Cache caret position
				Place caretPosition = textEditor.Selection.Start;

				// Scan all lines
				for (int i = 0; i < textEditor.LinesCount; i++)
				{
					// If a line contains a "whitespace dot"
					if (textEditor.GetLineText(i).Contains("·"))
					{
						textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
						textEditor.InsertText(textEditor.GetLineText(i).Replace("·", " "));
					}
				}

				// Restore caret position
				textEditor.Selection.Start = caretPosition;
			}
		}
	}
}
