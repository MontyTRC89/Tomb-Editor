using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class GlobalParser
	{
		public static int GetNextFreeIndex(TextDocument document, int offset)
		{
			string commandKey = CommandParser.GetCommandKey(document, offset);
			return GetNextFreeIndex(document, offset, commandKey);
		}

		public static int GetNextFreeIndex(TextDocument document, int offset, string commandKey)
		{
			if (string.IsNullOrEmpty(commandKey))
				return -1;

			if (StringHelper.BulkStringComparision(commandKey, StringComparison.OrdinalIgnoreCase,
				"AddEffect", "ColorRGB", "GlobalTrigger", "Image", "ItemGroup", "MultiEnvCondition",
				"Organizer", "Parameters", "TestPosition", "TriggerGroup", "Plugin"))
			{
				IEnumerable<int> takenIndicesList;

				if (DocumentParser.DocumentContainsSections(document))
				{
					DocumentLine sectionStartLine = DocumentParser.GetStartLineOfCurrentSection(document, offset);

					if (sectionStartLine == null)
						return -1;

					int sectionStartLineNumber = sectionStartLine.LineNumber;
					takenIndicesList = GetTakenIndicesList(document, commandKey, sectionStartLineNumber + 1);
				}
				else
					takenIndicesList = GetTakenIndicesList(document, commandKey, 1);

				int nextFreeIndex = 1;

				while (takenIndicesList.Contains(nextFreeIndex))
					nextFreeIndex++;

				return nextFreeIndex;
			}

			return -1;
		}

		private static IEnumerable<int> GetTakenIndicesList(TextDocument document, string commandKey, int loopStartLine)
		{
			for (int i = loopStartLine; i <= document.LineCount; i++)
			{
				DocumentLine processedLine = document.GetLineByNumber(i);
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);
				processedLineText = LineParser.EscapeComments(processedLineText);

				string command = CommandParser.GetCommandKey(document, processedLine.Offset);

				if (string.IsNullOrEmpty(command) || !processedLineText.Contains("="))
					continue;

				if (command.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
					if (int.TryParse(processedLineText.Split('=')[1].Split(',')[0].Trim(), out int takenIndex))
						yield return takenIndex;
			}
		}
	}
}
