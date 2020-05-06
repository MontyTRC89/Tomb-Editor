using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Helpers
{
	public static class ArgumentHelper
	{
		public static int GetArgumentIndexAtOffset(TextDocument document, int offset)
		{
			string wholeLineText = CommandHelper.GetWholeCommandLineText(document, offset);

			if (wholeLineText == null)
				return -1;

			int totalArgumentCount = wholeLineText.Split(',').Length;

			DocumentLine commandStartLine = CommandHelper.GetCommandStartLine(document, offset);
			int wholeLineSubstringOffsetIndex = offset - commandStartLine.Offset;

			string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffsetIndex);

			int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

			return totalArgumentCount - argumentCountAfterOffset;
		}
	}
}
