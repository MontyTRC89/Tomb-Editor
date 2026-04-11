using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public class ErrorDetector : IErrorDetector
	{
		#region Public methods

		public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
			=> DetectErrorLines(new TextDocument(editorContent));

		#endregion Public methods

		#region Error line finding

		private static List<TextEditorDiagnostic> DetectErrorLines(TextDocument document)
		{
			var errorLines = new List<TextEditorDiagnostic>();

			bool commandSectionCheckRequired = DocumentParser.DocumentContainsSections(document);

			foreach (DocumentLine processedLine in document.Lines)
			{
				string processedLineText = document.GetText(processedLine.Offset, processedLine.Length);

				if (LineParser.IsEmptyOrComments(processedLineText))
					continue;

				TextEditorDiagnostic error = FindErrorsInLine(document, processedLine, processedLineText, commandSectionCheckRequired);

				if (error != null)
					errorLines.Add(error);
			}

			return errorLines;
		}

		private static TextEditorDiagnostic FindErrorsInLine(TextDocument document, DocumentLine line, string lineText, bool commandSectionCheckRequired)
		{
			if (LineParser.IsSectionHeaderLine(lineText))
				return FindErrorsInSectionHeaderLine(document, line, lineText);
			else
			{
				if (commandSectionCheckRequired && LineParser.IsLineInStandardStringSection(document, line))
					return null;
				else if (commandSectionCheckRequired && LineParser.IsLineInExtraNGSection(document, line))
					return FindErrorsInNGStringLine(document, line, lineText);
				else
					return FindErrorsInCommandLine(document, line, lineText, commandSectionCheckRequired);
			}
		}

		private static TextEditorDiagnostic FindErrorsInSectionHeaderLine(TextDocument document, DocumentLine line, string lineText)
		{
			if (!IsValidSectionName(lineText))
				return CreateDiagnostic(document, line,
					"Invalid section name. Please check its spelling.", LineParser.RemoveComments(lineText));

			return null;
		}

		private static TextEditorDiagnostic FindErrorsInNGStringLine(TextDocument document, DocumentLine line, string lineText)
		{
			if (!IsNGStringLineWellFormatted(lineText))
				return CreateDiagnostic(document, line,
					"NG string must start with an index.\n\nExample:\n0: First String\n1: Second String",
					LineParser.RemoveComments(lineText));

			return null;
		}

		private static TextEditorDiagnostic FindErrorsInCommandLine(TextDocument document, DocumentLine line, string lineText, bool commandSectionCheckRequired)
		{
			string commandKey = CommandParser.GetCommandKey(document, line.Offset);

			if (!IsValidCommandKey(commandKey))
			{
				string errorSegmentText = Regex.Match(LineParser.RemoveComments(lineText), "^.*=").Value.TrimEnd();

				if (errorSegmentText.Length == 0 && commandKey != null)
					return null;

				if (commandKey == null)
					errorSegmentText = lineText.TrimEnd();

				return CreateDiagnostic(document, line,
					"Invalid command. Please check its spelling.", errorSegmentText);
			}

			if (commandSectionCheckRequired && !IsCommandLineInCorrectSection(document, line.LineNumber, commandKey))
				return CreateDiagnostic(document, line,
					"Command is placed in the wrong section. Please check the command syntax.",
					LineParser.RemoveComments(lineText));

			if (ContainsBrokenNextLines(document, line.Offset))
			{
				string errorSegmentText = Regex.Match(LineParser.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

				if (errorSegmentText.Length == 0)
					errorSegmentText = LineParser.RemoveComments(lineText);

				return CreateDiagnostic(document, line,
					"Misplaced \">\" symbols were found.\nYou can only use these symbols at the end of the line and there can only be one on each line.",
					errorSegmentText);
			}

			if (!IsArgumentCountValid(document, line.Offset))
			{
				string errorSegmentText = Regex.Match(LineParser.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

				if (errorSegmentText.Length == 0)
					errorSegmentText = LineParser.RemoveComments(lineText);

				return CreateDiagnostic(document, line,
					"Invalid argument count. Please check the command syntax.", errorSegmentText);
			}

			if (ContainsEmptyArguments(document, line.Offset))
			{
				string errorSegmentText = Regex.Match(LineParser.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

				if (errorSegmentText.Length == 0)
					errorSegmentText = LineParser.RemoveComments(lineText);

				return CreateDiagnostic(document, line, "Empty arguments were found.", errorSegmentText);
			}

			return null;
		}

		private static TextEditorDiagnostic CreateDiagnostic(TextDocument document, DocumentLine line, string message, string errorSegmentText)
		{
			string lineText = document.GetText(line);
			string segmentText = string.IsNullOrWhiteSpace(errorSegmentText)
				? lineText.Trim()
				: errorSegmentText;

			int startOffset = line.Offset;
			int endOffset = Math.Max(line.Offset + 1, line.EndOffset);

			if (!string.IsNullOrWhiteSpace(segmentText))
			{
				int matchIndex = lineText.IndexOf(segmentText, StringComparison.Ordinal);

				if (matchIndex >= 0)
				{
					startOffset = line.Offset + matchIndex;
					endOffset = startOffset + segmentText.Length;
				}
			}

			return new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, message, startOffset, endOffset);
		}

		#endregion Error line finding

		#region Error detection methods

		private static bool IsValidSectionName(string sectionHeaderLineText)
		{
			string section = sectionHeaderLineText.Split('[')[1].Split(']')[0];

			foreach (string entry in Keywords.Sections)
				if (section.Equals(entry, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		private static bool IsNGStringLineWellFormatted(string lineText)
			=> Regex.IsMatch(lineText, @"^\d*:.*");

		private static bool IsValidCommandKey(string commandKey)
		{
			if (commandKey == null)
				return false;

			foreach (DictionaryEntry entry in CommandParser.GetCommandSyntaxResources())
				if (commandKey.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		private static bool IsCommandLineInCorrectSection(TextDocument document, int lineNumber, string command)
		{
			string correctSection = string.Empty;

			foreach (DictionaryEntry entry in CommandParser.GetCommandSyntaxResources())
				if (command.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					string entryValue = entry.Value.ToString();

					if (entryValue.Contains('['))
						correctSection = entry.Value.ToString().Split('[')[1].Split(']')[0].Trim();

					break;
				}

			if (string.IsNullOrWhiteSpace(correctSection) || correctSection.Equals("any", StringComparison.OrdinalIgnoreCase))
				return true;

			for (int i = lineNumber - 1; i > 0; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.TrimStart().StartsWith("["))
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

		private static bool ContainsBrokenNextLines(TextDocument document, int lineOffset)
		{
			DocumentLine startingLine = CommandParser.GetCommandStartLine(document, lineOffset);

			if (startingLine == null)
				return false;

			DocumentLine nextLine;
			string nextLineText;

			int i = startingLine.LineNumber;

			do
			{
				if (i > document.LineCount)
					break;

				nextLine = document.GetLineByNumber(i);
				nextLineText = LineParser.EscapeComments(document.GetText(nextLine.Offset, nextLine.Length));

				if ((nextLineText.Contains('>') && !Regex.IsMatch(nextLineText, Patterns.NextLineKey)) || nextLineText.Count(c => c == '>') > 1)
					return true;

				i++;
			}
			while (Regex.IsMatch(nextLineText, Patterns.NextLineKey));

			return false;
		}

		private static bool IsArgumentCountValid(TextDocument document, int lineOffset)
		{
			string lineText = CommandParser.GetWholeCommandLineText(document, lineOffset);

			if (lineText == null)
				return false;

			if (lineText.TrimStart().StartsWith("#"))
				return true;

			lineText = LineParser.EscapeComments(lineText);

			if (!lineText.Contains('='))
				return false;

			string command = CommandParser.GetCommandKey(document, lineOffset);

			if (string.IsNullOrEmpty(command))
				return false;

			if (command.Equals("Legend", StringComparison.OrdinalIgnoreCase)) // "Legend=" ignores commas
				return true;

			int argumentCount = LineParser.EscapeComments(lineText).Split('=')[1].Split(',').Length;

			if (argumentCount == 1 && string.IsNullOrWhiteSpace(lineText.Split('=')[1]))
				argumentCount = 0;

			foreach (DictionaryEntry entry in CommandParser.GetCommandSyntaxResources())
				if (command.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					int correctArgumentCount = entry.Value.ToString().Split(']')[1].Split(',').Length;

					if (entry.Value.ToString().ToUpper().Contains("ARRAY"))
						return true; // Whatever.
					else
						return argumentCount == correctArgumentCount;
				}

			return false;
		}

		private static bool ContainsEmptyArguments(TextDocument document, int lineOffset)
		{
			string lineText = CommandParser.GetWholeCommandLineText(document, lineOffset);

			if (string.IsNullOrEmpty(lineText))
				return true;

			string[] arguments = LineParser.EscapeComments(lineText).Split(',');

			foreach (string argument in arguments)
				if (string.IsNullOrWhiteSpace(argument.Replace('>', ' ')))
					return true;

			return false;
		}

		#endregion Error detection methods
	}
}
