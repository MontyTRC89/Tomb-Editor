using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Helpers
{
	public static class DocumentHelper
	{
		public static bool DocumentContainsSections(TextDocument document)
		{
			foreach (DocumentLine processedLine in document.Lines)
			{
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);

				if (LineHelper.IsSectionHeaderLine(processedLineText))
					return true;
			}

			return false;
		}

		public static string GetSectionName(TextDocument document, int offset)
		{
			DocumentLine sectionStartLine = GetSectionStartLine(document, offset);

			if (sectionStartLine == null)
				return null;

			return document.GetText(sectionStartLine.Offset, sectionStartLine.Length).Split('[')[1].Split(']')[0];
		}

		public static DocumentLine GetSectionStartLine(TextDocument document, int offset)
		{
			DocumentLine offsetLine = document.GetLineByOffset(offset);

			for (int i = offsetLine.LineNumber; i >= 1; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.StartsWith("["))
					return currentLine;
			}

			return null;
		}
	}
}
