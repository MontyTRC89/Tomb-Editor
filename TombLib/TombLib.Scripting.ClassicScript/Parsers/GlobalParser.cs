using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
				int loopStartLine = 1;

				if (DocumentParser.DocumentContainsSections(document))
				{
					DocumentLine sectionStartLine = DocumentParser.GetStartLineOfCurrentSection(document, offset);

					if (sectionStartLine == null)
						return -1;

					loopStartLine = sectionStartLine.LineNumber + 1;
				}

				int firstId = GetFirstId(document, commandKey, loopStartLine);
				IEnumerable<int> takenIndicesList = GetTakenIndicesList(document, commandKey, loopStartLine);

				int nextFreeIndex = firstId;

				while (takenIndicesList.Contains(nextFreeIndex))
					nextFreeIndex++;

				return nextFreeIndex;
			}

			return -1;
		}

		private static int GetFirstId(TextDocument document, string commandKey, int loopStartLine) // REFACTOR !!!
		{
			try // Not sure if this will work correctly, so putting this into a try / catch for safety
			{
				var firstIdRegex = new Regex($@"^\s*#FIRST_ID\s+{commandKey}\s*=\s*(.*)\s*(;.*)?$", RegexOptions.IgnoreCase);

				for (int i = loopStartLine; i <= document.LineCount; i++)
				{
					string processedLineText = CommandParser.GetWholeCommandLineText(document, document.GetLineByNumber(i).Offset);

					if (processedLineText == null)
						continue;

					processedLineText = LineParser.EscapeComments(processedLineText);

					Match match = firstIdRegex.Match(processedLineText);

					if (match.Success)
					{
						string[] numbers = match.Groups[1].Value.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
						int finalResult = 0;

						char[] mathSymbols = match.Groups[1].Value.Where(c => c == '+' || c == '-').ToArray();

						for (int j = 0; j < numbers.Length; j++)
						{
							char equationType = '+';

							if (j > 0)
								equationType = mathSymbols[j - 1];

							string number = numbers[j];

							if (int.TryParse(number.Trim(), out int value))
							{
								switch (equationType)
								{
									case '+': finalResult += value; break;
									case '-': finalResult -= value; break;
								}
							}
							else
							{
								var defineRegex = new Regex($@"^\s*#DEFINE\s+{number.Trim()}\s+(\d+)\s*(;.*)?$", RegexOptions.IgnoreCase);

								foreach (DocumentLine line in document.Lines)
								{
									string lineText = CommandParser.GetWholeCommandLineText(document, line.Offset);

									if (lineText == null)
										continue;

									lineText = LineParser.EscapeComments(lineText);

									Match defineMatch = defineRegex.Match(lineText);

									if (defineMatch.Success && int.TryParse(defineMatch.Groups[1].Value.Trim(), out int defineValue))
									{
										switch (equationType)
										{
											case '+': finalResult += defineValue; break;
											case '-': finalResult -= defineValue; break;
										}
									}
								}
							}
						}

						return finalResult == 0 ? 1 : finalResult;
					}
				}
			}
			catch { }

			return 1;
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
