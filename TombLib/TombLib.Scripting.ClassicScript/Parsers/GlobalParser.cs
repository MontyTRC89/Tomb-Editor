using ICSharpCode.AvalonEdit.Document;
using NCalc;
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

		private static Stack<string> _alreadyVisitedVariables = new Stack<string>();
		private static DataTable _cachedDataTable;

		private static int GetFirstId(TextDocument document, string commandKey, int loopStartLine)
		{
			_alreadyVisitedVariables.Clear();
			_cachedDataTable = GetMnemonicConstantsDataTable();

			int result = 0;
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
						string expressionString = match.Groups[1].Value;

						IEnumerable<string> variables = expressionString.Split(new[] { '+', '-', '*', '/' })
							.Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim()).Where(x => !string.IsNullOrWhiteSpace(x));

						foreach (string variable in variables)
						{
							if (int.TryParse(variable, out int _))
								continue;

							DataRow row = _cachedDataTable.Select($"flag = '{variable}'")?.FirstOrDefault();

							if (row != null && int.TryParse(row[0].ToString(), out int rowValue))
								expressionString = expressionString.Replace(variable, rowValue.ToString());
							else
								expressionString = expressionString.Replace(variable, GetVariableValue(document, variable).ToString());
						}

						var expression = new Expression(expressionString);

						if (expression.HasErrors())
							result = 0;
						else if (expressionString.Contains('/'))
							result = expression.HasErrors() ? 0 : (int)Math.Floor((double)expression.Evaluate());
						else
							result = expression.HasErrors() ? 0 : (int)expression.Evaluate();
					}
				}
			}
			catch { }

			return result < 1 ? 1 : result;
		}

		private static int GetVariableValue(TextDocument document, string variable)
		{
			if (_alreadyVisitedVariables.Contains(variable)) // Stop infinite loop
				return 0;

			_alreadyVisitedVariables.Push(variable);

			int result = 0;
			var defineRegex = new Regex($@"^\s*#DEFINE\s+{Regex.Escape(variable)}\s+(.*)\s*(;.*)?$", RegexOptions.IgnoreCase);

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = CommandParser.GetWholeCommandLineText(document, line.Offset);

				if (lineText == null)
					continue;

				lineText = LineParser.EscapeCommentsAndNewLines(lineText);
				Match match = defineRegex.Match(lineText);

				if (match.Success)
				{
					string expressionString = match.Groups[1].Value;

					IEnumerable<string> variables = expressionString.Split(new[] { '+', '-', '*', '/' })
						.Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim()).Where(x => !string.IsNullOrWhiteSpace(x));

					foreach (string subVariable in variables)
					{
						if (int.TryParse(subVariable, out int _))
							continue;

						DataRow row = _cachedDataTable.Select($"flag = '{subVariable}'")?.FirstOrDefault();

						if (row != null && int.TryParse(row[0].ToString(), out int rowValue))
							expressionString = expressionString.Replace(subVariable, rowValue.ToString());
						else
							expressionString = expressionString.Replace(subVariable, GetVariableValue(document, subVariable).ToString());
					}

					var expression = new Expression(expressionString);

					if (expression.HasErrors())
						result = 0;
					else if (expressionString.Contains('/'))
						result = expression.HasErrors() ? 0 : (int)Math.Floor((double)expression.Evaluate());
					else
						result = expression.HasErrors() ? 0 : (int)expression.Evaluate();
				}
			}

			_alreadyVisitedVariables.Pop();
			return result;
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
