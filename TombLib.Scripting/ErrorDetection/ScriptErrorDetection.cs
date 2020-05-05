using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;

namespace TombLib.Scripting.ErrorDetection
{
	public static class ScriptErrorDetection
	{
		#region Public methods

		public static List<ErrorLine> DetectErrorLines(TextDocument document)
		{
			List<ErrorLine> errorLines = new List<ErrorLine>();

			bool commandSectionCheckRequired = DocumentContainsSections(document);

			foreach (DocumentLine processedLine in document.Lines)
			{
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);

				if (LineHelper.IsEmptyLine(processedLineText))
					continue;

				ErrorLine error = FindErrorsInLine(document, processedLine, processedLineText, commandSectionCheckRequired);

				if (error != null)
					errorLines.Add(error);
			}

			return errorLines;
		}

		#endregion Public methods

		#region Error line finding

		private static ErrorLine FindErrorsInLine(TextDocument document, DocumentLine line, string lineText, bool commandSectionCheckRequired)
		{
			if (LineHelper.IsSectionHeaderLine(lineText))
				return FindErrorsInSectionHeaderLine(line, lineText);
			else
			{
				if (commandSectionCheckRequired && LineHelper.IsLineInStandardStringSection(document, line))
					return null;
				else if (commandSectionCheckRequired && LineHelper.IsLineInNGStringSection(document, line))
					return FindErrorsInNGStringLine(line, lineText);
				else
					return FindErrorsInCommandLine(document, line, lineText, commandSectionCheckRequired);
			}
		}

		private static ErrorLine FindErrorsInSectionHeaderLine(DocumentLine line, string lineText)
		{
			if (!IsValidSectionName(lineText))
				return new ErrorLine("Invalid section name. Please check its spelling.",
					line.LineNumber, LineHelper.RemoveComments(lineText));

			return null;
		}

		private static ErrorLine FindErrorsInNGStringLine(DocumentLine line, string lineText)
		{
			if (!IsNGStringLineWellFormatted(lineText))
				return new ErrorLine("NG string must start with an index.\n\nExample:\n0: First String\n1: Second String",
					line.LineNumber, LineHelper.RemoveComments(lineText));

			return null;
		}

		private static ErrorLine FindErrorsInCommandLine(TextDocument document, DocumentLine line, string lineText, bool commandSectionCheckRequired)
		{
			string commandKey = CommandHelper.GetCommandKey(document, line.Offset);

			if (!IsValidCommandKey(commandKey))
			{
				string errorSegmentText = Regex.Match(lineText, "^.*=").Value.TrimEnd();

				if (errorSegmentText.Length == 0 && commandKey != null)
					return null;

				if (commandKey == null)
					errorSegmentText = lineText.TrimEnd();

				return new ErrorLine("Invalid command. Please check its spelling.",
					line.LineNumber, errorSegmentText);
			}

			if (commandSectionCheckRequired && !IsCommandLineInCorrectSection(document, line.LineNumber, commandKey))
				return new ErrorLine("Command is placed in the wrong section. Please check the command syntax.",
					line.LineNumber, LineHelper.RemoveComments(lineText));

			if (!IsArgumentCountValid(document, line.Offset))
			{
				string errorSegmentText = Regex.Match(lineText, @"=\s*?(\b.*)").Groups[1].Value;
				errorSegmentText = LineHelper.RemoveComments(errorSegmentText);

				if (errorSegmentText.Length == 0)
					errorSegmentText = lineText.TrimEnd();

				return new ErrorLine("Invalid argument count. Please check the command syntax.",
					line.LineNumber, errorSegmentText);
			}

			return null;
		}

		#endregion Error line finding

		#region Error detection methods

		private static bool IsValidSectionName(string sectionHeaderLineText)
		{
			string section = sectionHeaderLineText.Split('[')[1].Split(']')[0];

			foreach (string entry in ScriptKeyWords.Sections)
				if (section.Equals(entry, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		private static bool IsNGStringLineWellFormatted(string lineText)
		{ return Regex.IsMatch(lineText, @"^\d*:.*?"); }

		private static bool IsValidCommandKey(string commandKey)
		{
			if (commandKey == null)
				return false;

			foreach (DictionaryEntry entry in CommandHelper.GetCommandSyntaxResources())
				if (commandKey.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		private static bool IsCommandLineInCorrectSection(TextDocument document, int lineNumber, string command)
		{
			string correctSection = string.Empty;

			foreach (DictionaryEntry entry in CommandHelper.GetCommandSyntaxResources())
				if (command.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					correctSection = entry.Value.ToString().Split('[')[1].Split(']')[0].Trim();
					break;
				}

			if (correctSection.Equals("any", StringComparison.OrdinalIgnoreCase))
				return true;

			for (int i = lineNumber - 1; i > 0; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.StartsWith("["))
				{
					if (correctSection.Equals("level", StringComparison.OrdinalIgnoreCase))
					{
						if (Regex.IsMatch(currentLineText, @"\[(level|title)\]", RegexOptions.IgnoreCase))
							return true;
						else
							return false;
					}
					else if (Regex.IsMatch(currentLineText, @"\[" + correctSection + @"\]", RegexOptions.IgnoreCase))
						return true;
					else
						return false;
				}
			}

			return false;
		}

		private static bool IsArgumentCountValid(TextDocument document, int lineOffset)
		{
			string lineText = CommandHelper.GetWholeCommandLine(document, lineOffset);

			if (lineText == null)
				return false;

			if (lineText.StartsWith("#"))
				return true;

			if (!lineText.Contains("="))
				return false;

			string command = CommandHelper.GetCommandKey(document, lineOffset);

			if (command == null)
				return false;

			int argumentCount = LineHelper.RemoveComments(lineText).Split('=')[1].Split(',').Length;

			if (argumentCount == 1 && string.IsNullOrEmpty(lineText.Split('=')[1].Trim()))
				argumentCount = 0;

			foreach (DictionaryEntry entry in CommandHelper.GetCommandSyntaxResources())
				if (command.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					int correctArgumentCount = entry.Value.ToString().Split(']')[1].Split(',').Length;

					if (entry.Value.ToString().ToUpper().Contains("ARRAY"))
						return argumentCount >= correctArgumentCount - 1; // TODO: Check if -1 is correct
					else
						return argumentCount == correctArgumentCount;
				}

			return false;
		}

		#endregion Error detection methods

		#region Boolean region

		private static bool DocumentContainsSections(TextDocument document)
		{
			foreach (DocumentLine processedLine in document.Lines)
			{
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);

				if (LineHelper.IsSectionHeaderLine(processedLineText))
					return true;
			}

			return false;
		}

		#endregion Boolean region
	}
}
