using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;
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

		private static List<string> _alreadyVisitedDefineKeys = new List<string>();
		private static DataTable _cachedDataTable;

		private static int GetFirstId(TextDocument document, string commandKey, int loopStartLine)
		{
			_alreadyVisitedDefineKeys.Clear();
			_cachedDataTable = GetMnemonicConstantsDataTable();

			var firstIdRegex = new Regex($@"^\s*#FIRST_ID\s+{Regex.Escape(commandKey)}\s*=\s*(.*)\s*(;.*)?$", RegexOptions.IgnoreCase);

			try // Not sure if this will work correctly, so putting this into a try / catch for safety
			{
				for (int i = loopStartLine; i <= document.LineCount; i++)
				{
					string lineText = CommandParser.GetWholeCommandLineText(document, document.GetLineByNumber(i).Offset);

					if (lineText == null)
						continue;

					lineText = LineParser.EscapeCommentsAndNewLines(lineText);
					Match match = firstIdRegex.Match(lineText);

					if (match.Success)
					{
						int result = 0;

						foreach (string variable in match.Groups[1].Value.Split('+').Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim()))
						{
							if (int.TryParse(variable, out int value))
								result += value;
							else if (variable.StartsWith("-"))
								result -= GetDefineSum(document, variable.TrimStart('-').TrimStart());
							else
								result += GetDefineSum(document, variable);
						}

						return result < 1 ? 1 : result;
					}
				}
			}
			catch { }

			return 1;
		}

		private static int GetDefineSum(TextDocument document, string defineKey)
		{
			if (_alreadyVisitedDefineKeys.Contains(defineKey)) // Stop infinite loop
				return 0;

			_alreadyVisitedDefineKeys.Add(defineKey);

			var defineRegex = new Regex($@"^\s*#DEFINE\s+{Regex.Escape(defineKey)}\s+(.*)\s*(;.*)?$", RegexOptions.IgnoreCase);

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = CommandParser.GetWholeCommandLineText(document, line.Offset);

				if (lineText == null)
					continue;

				lineText = LineParser.EscapeCommentsAndNewLines(lineText);
				Match match = defineRegex.Match(lineText);

				if (match.Success)
				{
					int result = 0;

					foreach (string variable in match.Groups[1].Value.Split('+').Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim()))
					{
						if (int.TryParse(variable, out int value))
							result += value;
						else
						{
							DataRow row = _cachedDataTable.Select($"flag = '{variable.TrimStart('-').TrimStart()}'")?.FirstOrDefault();

							if (row != null && int.TryParse(row[0].ToString(), out int rowValue))
							{
								if (variable.StartsWith("-"))
									result -= rowValue;
								else
									result += rowValue;
							}
							else if (variable.StartsWith("-"))
								result -= GetDefineSum(document, variable.TrimStart('-').TrimStart());
							else
								result += GetDefineSum(document, variable);
						}
					}

					return result < 1 ? 1 : result;
				}
			}

			return 0;
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

		// TODO: MOVE THIS ELSEWHERE !!!

		private static DataTable GetMnemonicConstantsDataTable()
		{
			string xmlPath = Path.Combine(DefaultPaths.ReferencesDirectory, "MnemonicConstants.xml");

			using (var reader = XmlReader.Create(xmlPath))
			{
				var dataSet = new DataSet();
				dataSet.ReadXml(reader);

				DataTable dataTable = dataSet.Tables[0];

				AddPluginMnemonics(dataTable);

				return dataTable;
			}
		}

		private static void AddPluginMnemonics(DataTable dataTable)
		{
			DataTable pluginMnemonicTable = GetPluginMnemonicTable();

			foreach (DataRow row in pluginMnemonicTable.Rows)
				dataTable.Rows.Add(row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), row.ItemArray[2].ToString());
		}

		private static DataTable GetPluginMnemonicTable()
		{
			var dataTable = new DataTable();

			dataTable.Columns.Add("decimal", typeof(string));
			dataTable.Columns.Add("hex", typeof(string));
			dataTable.Columns.Add("flag", typeof(string));

			foreach (PluginConstant mnemonic in MnemonicData.PluginConstants)
			{
				DataRow row = dataTable.NewRow();
				row["decimal"] = mnemonic.DecimalValue;
				row["hex"] = mnemonic.HexValue;
				row["flag"] = mnemonic.FlagName;

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}
	}
}
