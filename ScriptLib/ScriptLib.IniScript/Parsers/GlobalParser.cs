using ICSharpCode.AvalonEdit.Document;
using NCalc;
using Shared.Extensions;
using System.Data;
using System.Text.RegularExpressions;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	public static int GetNextFreeIndex(TextDocument document, int offset)
	{
		string? commandKey = GetCommandNameFromOffset(document, offset);

		if (string.IsNullOrEmpty(commandKey))
			return -1;

		return GetNextFreeIndex(document, offset, commandKey);
	}

	public static int GetNextFreeIndex(TextDocument document, int offset, string commandKey)
	{
		if (commandKey.EqualsAny(StringComparison.OrdinalIgnoreCase,
			"AddEffect", "ColorRGB", "GlobalTrigger", "Image", "ItemGroup", "MultiEnvCondition",
			"Organizer", "Parameters", "TestPosition", "TriggerGroup", "Plugin"))
		{
			int loopStartLine = 1;

			if (DocumentContainsSections(document))
			{
				DocumentLine? sectionStartLine = GetFirstLineOfSectionAtOffset(document, offset);

				if (sectionStartLine is null)
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

	private static Stack<string> _alreadyVisitedVariables = new();

	private static int GetFirstId(TextDocument document, string commandKey, int loopStartLine)
	{
		_alreadyVisitedVariables.Clear();

		int result = 0;
		var firstIdRegex = new Regex($@"^\s*#FIRST_ID\s+{Regex.Escape(commandKey)}\s*=\s*(.*)\s*(;.*)?$", RegexOptions.IgnoreCase);

		try // Not sure if this will work correctly, so putting this into a try / catch for safety
		{
			for (int i = loopStartLine; i <= document.LineCount; i++)
			{
				string? lineText = GetFullCommandTextFromOffset(document, document.GetLineByNumber(i).Offset);

				if (lineText == null)
					continue;

				lineText = EscapeCommentsAndNewLines(lineText);
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

						DataRow row = MnemonicData.MnemonicConstantsDataTable.Select($"flag = '{variable}'")?.FirstOrDefault();

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
			string? lineText = GetFullCommandTextFromOffset(document, line.Offset);

			if (lineText is null)
				continue;

			lineText = EscapeCommentsAndNewLines(lineText);
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

					DataRow row = MnemonicData.MnemonicConstantsDataTable.Select($"flag = '{subVariable}'")?.FirstOrDefault();

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
			processedLineText = EscapeComments(processedLineText);

			string? command = GetCommandNameFromOffset(document, processedLine.Offset);

			if (string.IsNullOrEmpty(command) || !processedLineText.Contains("="))
				continue;

			if (command.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(processedLineText.Split('=')[1].Split(',')[0].Trim(), out int takenIndex))
					yield return takenIndex;
			}
		}
	}
}
