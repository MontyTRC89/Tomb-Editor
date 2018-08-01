using FastColoredTextBoxNS;

namespace ScriptEditor
{
	public class UndoRedoHandling
	{
		public static void HandleUndoRedo(FastColoredTextBox textEditor, int index) // 0 - Undo, 1 - Redo
		{
			// If "Show Spaces" is enabled
			if (Properties.Settings.Default.ShowSpaces)
			{
				TriggerUndoRedo(textEditor, index);

				while (textEditor.Text.Contains(" "))
				{
					TriggerUndoRedo(textEditor, index);
				}
			}
			else
			{
				TriggerUndoRedo(textEditor, index);
			}
		}

		private static void TriggerUndoRedo(FastColoredTextBox textEditor, int index)
		{
			if (index == 0)
			{
				textEditor.Undo();
			}
			else if (index == 1)
			{
				textEditor.Redo();
			}
		}
	}
}
