using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ErrorDetection
{
	public static class LuaErrorDetection
	{
		public static List<ErrorLine> DetectErrorLines(TextDocument document)
		{
			List<ErrorLine> errorLines = new List<ErrorLine>();

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (LineHelper.IsEmptyLine(lineText))
					continue;

				ErrorLine error = FindErrorsInLine(document, line, lineText);

				if (error != null)
					errorLines.Add(error);
			}

			return errorLines;
		}

		private static ErrorLine FindErrorsInLine(TextDocument document, DocumentLine line, string lineText)
		{
			// TODO

			return null;
		}
	}
}
