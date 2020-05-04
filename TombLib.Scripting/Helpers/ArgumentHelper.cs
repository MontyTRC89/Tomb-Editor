using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Helpers
{
	public static class ArgumentHelper
	{
		public static int GetArgumentIndexAtOffset(TextDocument document, int offset)
		{
			string wholeLineText = CommandHelper.GetWholeCommandLine(document, offset);

			if (wholeLineText == null)
				return -1;

			int totalArgumentCount = wholeLineText.Split(',').Length;

			int commandStartLineNumber = CommandHelper.GetCommandStartLineNumber(document, offset);
			DocumentLine commandStartLine = document.GetLineByNumber(commandStartLineNumber);
			int wholeLineSubstringOffsetIndex = offset - commandStartLine.Offset;

			string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffsetIndex);

			int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

			return totalArgumentCount - argumentCountAfterOffset;
		}
	}
}
