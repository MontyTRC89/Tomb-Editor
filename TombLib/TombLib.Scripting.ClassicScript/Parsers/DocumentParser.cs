using ICSharpCode.AvalonEdit.Document;
using System;
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

		public static string GetCurrentSectionName(TextDocument document, int offset)
		{
			DocumentLine sectionStartLine = GetStartLineOfCurrentSection(document, offset);

			if (sectionStartLine == null)
				return null;

			return document.GetText(sectionStartLine.Offset, sectionStartLine.Length).Split('[')[1].Split(']')[0];
		}

		public static int GetSectionsCount(TextDocument document)
		{
			int sectionsCount = 0;

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (LineParser.IsSectionHeaderLine(lineText))
					sectionsCount++;
			}

			return sectionsCount;
		}

		public static DocumentLine GetStartLineOfCurrentSection(TextDocument document, int offset)
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

		public static DocumentLine GetLastLineOfCurrentSection(TextDocument document, int offset)
		{
			DocumentLine offsetLine = document.GetLineByOffset(offset);
			DocumentLine sectionStartLine = GetStartLineOfCurrentSection(document, offset);

			for (int i = offsetLine.LineNumber; i <= document.LineCount; i++)
			{
				DocumentLine iline = document.GetLineByNumber(i);
				string ilineText = document.GetText(iline.Offset, iline.Length);

				if (iline != sectionStartLine && (ilineText.StartsWith("[") || i == document.LineCount))
				{
					for (int j = i == document.LineCount ? i : i - 1; j >= 1; j--)
					{
						DocumentLine jline = document.GetLineByNumber(j);
						string jlineText = document.GetText(jline.Offset, jline.Length);

						if (!string.IsNullOrWhiteSpace(LineParser.RemoveComments(jlineText)))
							return jline;
					}

					break;
				}
			}

			return null;
		}

		public static DocumentLine FindDocumentLineOfSection(TextDocument document, string sectionName)
		{
			sectionName = sectionName.Trim('[').Trim(']').Trim();

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (lineText.StartsWith("["))
				{
					string headerText = LineParser.GetSectionHeaderText(lineText);

					if (headerText.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
						return line;
				}
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

		public static bool IsPluginDefined(TextDocument document, string pluginName)
		{
			DocumentLine optionsSectionLine = FindDocumentLineOfSection(document, "Options");

			if (optionsSectionLine == null)
				return false;

			for (int i = optionsSectionLine.LineNumber; i <= document.LineCount; i++)
			{
				DocumentLine line = document.GetLineByNumber(i);
				string commandKey = CommandParser.GetCommandKey(document, line.Offset);

				if (commandKey != null && commandKey.Equals("Plugin", StringComparison.OrdinalIgnoreCase))
				{
					string wholeCommandLineText = CommandParser.GetWholeCommandLineText(document, line.Offset);

					if (wholeCommandLineText == null)
						continue;

					wholeCommandLineText = LineParser.RemoveComments(wholeCommandLineText);

					if (wholeCommandLineText.Contains(","))
					{
						string definedName = wholeCommandLineText.Split(',')[1].Replace("\n", "").Replace("\r", "").Replace(">", "").Trim();

						if (definedName.Equals(pluginName, StringComparison.OrdinalIgnoreCase))
							return true;
					}
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
