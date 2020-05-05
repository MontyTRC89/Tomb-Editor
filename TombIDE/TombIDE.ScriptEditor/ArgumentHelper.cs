using ICSharpCode.AvalonEdit.Document;

namespace TombIDE.ScriptEditor
{
	internal class ArgumentHelper
	{
		public static int GetCurrentArgumentIndex(TextDocument sourceDocument, int caretOffset)
		{
			DocumentLine currentLine = sourceDocument.GetLineByOffset(caretOffset);
			string lineText = sourceDocument.GetText(currentLine.Offset, currentLine.Length);

			int totalArgumentCount = lineText.Split(',').Length - 1;

			string textAfterCaret = sourceDocument.GetText(caretOffset, currentLine.EndOffset - caretOffset);

			int argumentCountAfterCaret = textAfterCaret.Split(',').Length - 1;

			return totalArgumentCount - argumentCountAfterCaret;
		}
	}
}
