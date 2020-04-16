using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using TombIDE.ScriptEditor.Objects;
using TombIDE.ScriptEditor.Resources;
using TombIDE.ScriptEditor.Resources.Syntaxes;

namespace TombIDE.ScriptEditor
{
	internal class ErrorChecking
	{
		public static List<ErrorLine> GetErrorLines(string documentText)
		{
			TextDocument document = new TextDocument(documentText);

			bool commandSectionCheckRequired = false;

			for (int i = 1; i < document.LineCount; i++)
			{
				DocumentLine line = document.GetLineByNumber(i);
				string lineText = document.GetText(line.Offset, line.Length);

				if (lineText.Trim().StartsWith("["))
				{
					commandSectionCheckRequired = true;
					break;
				}
			}

			List<ErrorLine> errorLines = new List<ErrorLine>();

			string currentSection = string.Empty;

			for (int i = 1; i <= document.LineCount; i++)
			{
				DocumentLine line = document.GetLineByNumber(i);
				string lineText = document.GetText(line.Offset, line.Length);

				if (string.IsNullOrWhiteSpace(lineText) || lineText.Trim().StartsWith(";"))
					continue;

				if (lineText.Trim().StartsWith("["))
				{
					if (!IsValidSection(lineText))
					{
						errorLines.Add(new ErrorLine(i, "Error:\nInvalid section. Please check its spelling."));
						continue;
					}

					if (!IsHeaderWellFormatted(lineText))
					{
						errorLines.Add(new ErrorLine(i, "Error:\nInvalid section header formatting."));
						continue;
					}

					currentSection = lineText.Split('[')[1].Split(']')[0];
				}
				else
				{
					if (currentSection.ToLower() == "strings" || currentSection.ToLower() == "psxstrings" || currentSection.ToLower() == "pcstrings")
						continue;

					if (currentSection.ToLower() == "extrang")
					{
						Regex rgx = new Regex(@"^\d*:.*?");

						if (!rgx.IsMatch(lineText))
							errorLines.Add(new ErrorLine(i, "Error:\nNG string must start with an index."));

						continue;
					}

					if (!IsCommandValid(lineText, document, i))
					{
						errorLines.Add(new ErrorLine(i, "Error:\nInvalid command. Please check its spelling."));
						continue;
					}

					if (commandSectionCheckRequired && !IsCommandInCorrectSection(lineText, document, i))
					{
						errorLines.Add(new ErrorLine(i, "Error:\nCommand is placed in the wrong section. Please check the command syntax."));
						continue;
					}

					if (!IsArgumentCountValid(lineText, document, i))
					{
						errorLines.Add(new ErrorLine(i, "Error:\nInvalid argument count. Please check the command syntax."));
						continue;
					}
				}
			}

			return errorLines;
		}

		private static bool IsHeaderWellFormatted(string line)
		{
			Regex rgx = new Regex(@"^\s*?\[.*\]\s*?(;.*)?$");
			return rgx.IsMatch(line);
		}

		private static bool IsValidSection(string line)
		{
			string section = line.Split('[')[1].Split(']')[0];

			foreach (string entry in KeyWords.Sections)
			{
				if (section.ToLower() == entry.ToLower())
					return true;
			}

			return false;
		}

		private static bool IsCommandValid(string lineText, TextDocument document, int lineNumber)
		{
			if (lineText.Trim().StartsWith("#"))
			{
				if (lineText.Trim().StartsWith("#include", StringComparison.OrdinalIgnoreCase)
				|| lineText.Trim().StartsWith("#define", StringComparison.OrdinalIgnoreCase)
				|| lineText.Trim().StartsWith("#first_id", StringComparison.OrdinalIgnoreCase))
					return true;
			}
			else
			{
				string command = string.Empty;

				if (lineText.Contains("="))
					command = lineText.Split('=')[0].Trim();
				else if (lineText.Trim().StartsWith("#"))
					command = lineText.Split(' ')[0].Trim();

				if (command.ToLower() == "level")
					command = CommandVariations.GetCorrectLevelCommand(document, lineNumber);
				else if (command.ToLower() == "cut")
					command = CommandVariations.GetCorrectCutCommand(document, lineNumber);
				else if (command.ToLower() == "fmv")
					command = CommandVariations.GetCorrectFMVCommand(document, lineNumber);

				if (command == null)
					return true; // IsCommandInCorrectSection() will catch it

				// Get resources from OldCommandSyntaxes.resx
				ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
				ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				// Get resources from NewCommandSyntaxes.resx
				ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
				ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				List<DictionaryEntry> entries = new List<DictionaryEntry>();
				entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
				entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

				foreach (DictionaryEntry entry in entries)
				{
					if (command.ToLower() == entry.Key.ToString().ToLower())
						return true;
				}
			}

			return false;
		}

		private static bool IsCommandInCorrectSection(string lineText, TextDocument document, int lineNumber)
		{
			string command = string.Empty;

			if (lineText.Contains("="))
				command = lineText.Split('=')[0].Trim();
			else if (lineText.Trim().StartsWith("#"))
				command = lineText.Split(' ')[0].Trim();

			if (command.ToLower() == "level")
				command = CommandVariations.GetCorrectLevelCommand(document, lineNumber);
			else if (command.ToLower() == "cut")
				command = CommandVariations.GetCorrectCutCommand(document, lineNumber);
			else if (command.ToLower() == "fmv")
				command = CommandVariations.GetCorrectFMVCommand(document, lineNumber);

			if (command == null)
				return false;

			// Get resources from OldCommandSyntaxes.resx
			ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
			ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Get resources from NewCommandSyntaxes.resx
			ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
			ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			List<DictionaryEntry> entries = new List<DictionaryEntry>();
			entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
			entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

			string correctSection = string.Empty;

			foreach (DictionaryEntry entry in entries)
			{
				if (command.ToLower() == entry.Key.ToString().ToLower())
				{
					correctSection = entry.Value.ToString().Split('[')[1].Split(']')[0].Trim();
					break;
				}
			}

			if (correctSection.ToLower() == "any")
				return true;

			for (int i = lineNumber - 1; i > 0; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.Trim().StartsWith("["))
				{
					if (correctSection.ToLower() == "level")
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

		private static bool IsArgumentCountValid(string lineText, TextDocument document, int lineNumber)
		{
			if (!lineText.Contains("="))
				return true;

			string command = lineText.Split('=')[0].Trim();
			int argumentCount = lineText.Split('=')[1].Split(',').Length;

			if (argumentCount == 1)
			{
				if (string.IsNullOrEmpty(lineText.Split('=')[1].Trim()))
					argumentCount = 0;
			}

			if (command.ToLower() == "level")
				command = CommandVariations.GetCorrectLevelCommand(document, lineNumber);
			else if (command.ToLower() == "cut")
				command = CommandVariations.GetCorrectCutCommand(document, lineNumber);
			else if (command.ToLower() == "fmv")
				command = CommandVariations.GetCorrectFMVCommand(document, lineNumber);

			if (command == null)
				return true;

			// Get resources from OldCommandSyntaxes.resx
			ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
			ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Get resources from NewCommandSyntaxes.resx
			ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
			ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			List<DictionaryEntry> entries = new List<DictionaryEntry>();
			entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
			entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

			foreach (DictionaryEntry entry in entries)
			{
				if (command.ToLower() == entry.Key.ToString().ToLower())
				{
					int correctArgumentCount = entry.Value.ToString().Split(']')[1].Split(',').Length;

					if (entry.Value.ToString().ToLower().Contains("array"))
						return argumentCount >= correctArgumentCount;
					else
						return argumentCount == correctArgumentCount;
				}
			}

			return false;
		}
	}
}
