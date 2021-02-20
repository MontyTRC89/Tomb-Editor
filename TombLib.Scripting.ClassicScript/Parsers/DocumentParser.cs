using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class DocumentParser
	{
		public static bool DocumentContainsSections(TextDocument document)
		{
			foreach (DocumentLine processedLine in document.Lines)
			{
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);

				if (LineParser.IsSectionHeaderLine(processedLineText))
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

		public static int GetSectionsCount(string[] documentLines)
		{
			int sectionsCount = 0;

			foreach (string line in documentLines)
				if (LineParser.IsSectionHeaderLine(line))
					sectionsCount++;

			return sectionsCount;
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

		public static DocumentLine FindDocumentLineOfObject(TextDocument document, string objectName, ObjectType type)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				switch (type)
				{
					case ObjectType.Section:
						if (lineText.StartsWith(objectName))
							return line;
						break;

					case ObjectType.Level:
						if (Regex.Replace(lineText, Patterns.NameCommand, string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
							return line;
						break;

					case ObjectType.Include:
						if (Regex.Replace(lineText, Patterns.IncludeCommand, string.Empty, RegexOptions.IgnoreCase).TrimStart('"').StartsWith(objectName))
							return line;
						break;

					case ObjectType.Define:
						if (Regex.Replace(lineText, Patterns.DefineCommand, string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
							return line;
						break;
				}
			}

			return null;
		}

		public static bool IsLevelScriptDefined(TextDocument document, string levelName)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);
				var regex = new Regex(Patterns.NameCommand, RegexOptions.IgnoreCase);

				if (regex.IsMatch(lineText))
				{
					string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim();

					if (scriptLevelName == levelName)
						return true;
				}
			}

			return false;
		}

		public static bool IsLevelLanguageStringDefined(TextDocument document, string levelName)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);
				string cleanString = LineParser.RemoveComments(LineParser.RemoveNGStringIndex(lineText)).Trim();

				if (cleanString == levelName)
					return true;
			}

			return false;
		}
	}
}
