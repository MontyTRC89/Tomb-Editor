using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;

namespace TombLib.Scripting.ClassicScript.Writers
{
	public static class LanguageStringWriter
	{
		public static void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		{
			if (!AssignStockLevelNameStringSlot(textEditor, levelName))
				WriteNewNGString(textEditor, levelName);
		}

		public static bool WriteNewNGString(TextEditorBase textEditor, string ngString)
		{
			if (!IsNGStringAlreadyDefined(textEditor.Document, ngString))
			{
				DocumentLine extrangSectionStartLine = GetExtraNGSectionStartLine(textEditor.Document);

				for (int i = textEditor.Document.LineCount; i <= extrangSectionStartLine.LineNumber; i--)
				{
					DocumentLine line = textEditor.Document.GetLineByNumber(i);
					string lineText = textEditor.Document.GetText(line.Offset, line.Length);

					if (Regex.IsMatch(lineText, @"^\d+:"))
					{
						textEditor.CaretOffset = line.EndOffset;
						textEditor.TextArea.PerformTextInput(Environment.NewLine);

						int prevNumber = int.Parse(Regex.Replace(lineText, @"^(\d+):", "$1"));

						textEditor.SelectedText = prevNumber + 1 + ": " + ngString;

						textEditor.ScrollToLine(i + 1);
						return true;
					}
					else if (i == extrangSectionStartLine.LineNumber)
					{
						textEditor.CaretOffset = line.EndOffset;
						textEditor.TextArea.PerformTextInput(Environment.NewLine);

						textEditor.SelectedText = "0: " + ngString;

						textEditor.ScrollToLine(i + 1);
						return true;
					}
				}
			}

			return false;
		}

		private static DocumentLine GetExtraNGSectionStartLine(TextDocument document)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (lineText.StartsWith("[extrang]", StringComparison.OrdinalIgnoreCase))
					return line;
			}

			return null;
		}

		private static bool IsNGStringAlreadyDefined(TextDocument document, string ngString)
		{
			DocumentLine extrangSectionStartLine = GetExtraNGSectionStartLine(document);

			for (int i = extrangSectionStartLine.LineNumber + 1; i < document.LineCount; i++)
			{
				DocumentLine line = document.GetLineByNumber(i);
				string lineText = document.GetText(line.Offset, line.Length);

				if (Regex.IsMatch(lineText, $@"^\d+:\s*{ngString}\s*(;.*)?$"))
					return true;
			}

			return false;
		}

		private static bool AssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
		{
			foreach (DocumentLine line in textEditor.Document.Lines)
			{
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);

				if (Regex.IsMatch(lineText, @"Level\sName\s\d+"))
				{
					textEditor.Select(line.Offset, line.Length);
					textEditor.SelectedText = levelName;

					return true;
				}
			}

			return false;
		}
	}
}
