using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;
using TombLib.Scripting.Resources.Syntaxes;

namespace TombLib.Scripting.Helpers
{
	public enum SubcommandType
	{
		Cust,
		Param
	}

	public static class CommandHelper
	{
		#region Public methods

		public static string GetCommandSyntax(TextDocument document, int offset)
		{
			string wholeCommandLine = GetWholeCommandLine(document, offset);

			if (wholeCommandLine == null)
				return null;

			if (Regex.IsMatch(wholeCommandLine, @"Customize\s*?=.*?,", RegexOptions.IgnoreCase)) // "Customize  =  CUST_CMD,"
				return GetSubcommandSyntax(wholeCommandLine, SubcommandType.Cust);
			else if (Regex.IsMatch(wholeCommandLine, @"Parameters\s*?=.*?,", RegexOptions.IgnoreCase)) // "Parameters  =  PARAM_CMD,"
				return GetSubcommandSyntax(wholeCommandLine, SubcommandType.Param);
			else
			{
				string commandKey = GetCommandKey(document, offset);

				foreach (DictionaryEntry entry in GetCommandSyntaxResources())
					if (commandKey.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
						return entry.Value.ToString();
			}

			return null;
		}

		public static string GetWholeCommandLine(TextDocument document, int offset)
		{
			int commandStartLineNumber = GetCommandStartLineNumber(document, offset);

			if (commandStartLineNumber == 0)
				return null;

			DocumentLine commandStartLine = document.GetLineByNumber(commandStartLineNumber);
			string commandStartLineText = document.GetText(commandStartLine.Offset, commandStartLine.Length);

			if (!Regex.IsMatch(commandStartLineText, ScriptPatterns.NextLineKey)) // If commandStartLineText is not multiline
				return commandStartLineText;
			else
			{
				List<string> linesToMerge = GetLinesToMerge(document, commandStartLine);

				if (linesToMerge == null)
					return null;

				return MergeLines(linesToMerge);
			}
		}

		public static int GetCommandStartLineNumber(TextDocument document, int offset)
		{
			DocumentLine offsetLine = document.GetLineByOffset(offset);
			string offsetLineText = document.GetText(offsetLine.Offset, offsetLine.Length);

			if (offsetLineText.Contains("=") || offsetLineText.Trim().StartsWith("#"))
				return offsetLine.LineNumber;
			else if (LineHelper.IsSectionHeaderLine(offsetLineText))
				return 0;
			else
				return FindCommandStartLineNumber(document, offsetLine);
		}

		public static string GetSectionNameFromLine(TextDocument document, int lineNumber)
		{
			for (int i = lineNumber; i > 0; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.Trim().StartsWith("["))
					return currentLineText.Split('[')[1].Split(']')[0].Trim();
			}

			return null;
		}

		public static string GetCommandKey(TextDocument document, int offset)
		{
			int commandStartLineNumber = GetCommandStartLineNumber(document, offset);

			if (commandStartLineNumber == 0)
				return null;

			DocumentLine commandStartLine = document.GetLineByNumber(commandStartLineNumber);
			string commandStartLineText = document.GetText(commandStartLine.Offset, commandStartLine.Length);

			if (commandStartLineText.Contains("="))
				return GetCorrectCommandVariation(document, commandStartLineNumber, commandStartLineText.Split('=')[0].Trim());
			else if (commandStartLineText.Trim().StartsWith("#"))
				return commandStartLineText.Split(' ')[0].Trim();

			return null;
		}

		public static List<DictionaryEntry> GetCommandSyntaxResources()
		{
			List<DictionaryEntry> entries = new List<DictionaryEntry>();

			// Get resources from OldCommandSyntaxes.resx
			ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
			ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Get resources from NewCommandSyntaxes.resx
			ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
			ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
			entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

			return entries;
		}

		#endregion Public methods

		#region Subcommands

		private static string GetSubcommandSyntax(string wholeCommandLine, SubcommandType subcommandType)
		{
			ResourceManager syntaxResource = null;

			switch (subcommandType)
			{
				case SubcommandType.Cust:
					syntaxResource = new ResourceManager(typeof(CustSyntaxes));
					break;

				case SubcommandType.Param:
					syntaxResource = new ResourceManager(typeof(ParamSyntaxes));
					break;
			}

			ResourceSet resourceSet = syntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			string key = wholeCommandLine.Split('=')[1].Split(',')[0].Trim();

			return FindCustParamSyntaxByKey(resourceSet, key);
		}

		private static string FindCustParamSyntaxByKey(ResourceSet resourceSet, string key)
		{
			// Search in the given ResourceSet
			foreach (DictionaryEntry entry in resourceSet)
				if (entry.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase))
					return entry.Value.ToString();

			// Search in PluginMnemonics
			foreach (PluginMnemonic pluginMnemonic in ScriptKeyWords.PluginMnemonics)
				if (pluginMnemonic.FlagName.Equals(key, StringComparison.OrdinalIgnoreCase))
					return Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Trim();

			return null;
		}

		#endregion Subcommands

		#region Command variations

		private static string GetCorrectCommandVariation(TextDocument document, int lineNumber, string command)
		{
			if (command.Equals("level", StringComparison.OrdinalIgnoreCase))
				return GetCorrectLevelCommand(document, lineNumber);
			else if (command.Equals("cut", StringComparison.OrdinalIgnoreCase))
				return GetCorrectCutCommand(document, lineNumber);
			else if (command.Equals("fmv", StringComparison.OrdinalIgnoreCase))
				return GetCorrectFMVCommand(document, lineNumber);
			else
				return command;
		}

		private static string GetCorrectLevelCommand(TextDocument document, int lineNumber)
		{
			string section = GetSectionNameFromLine(document, lineNumber);

			if (section == null)
				return "LevelLevel";

			switch (section.ToLower())
			{
				case "pcextensions":
					return "LevelPC";

				case "psxextensions":
					return "LevelPSX";

				default:
					return "LevelLevel";
			}
		}

		private static string GetCorrectCutCommand(TextDocument document, int lineNumber)
		{
			string section = GetSectionNameFromLine(document, lineNumber);

			if (section == null)
				return null;

			switch (section.ToLower())
			{
				case "pcextensions":
					return "CutPC";

				case "psxextensions":
					return "CutPSX";

				default:
					return "CutPC";
			}
		}

		private static string GetCorrectFMVCommand(TextDocument document, int lineNumber)
		{
			string section = GetSectionNameFromLine(document, lineNumber);

			if (section == null)
				return "FMVLevel";

			switch (section.ToLower())
			{
				case "pcextensions":
					return "FMVPC";

				case "psxextensions":
					return "FMVPSX";

				default:
					return "FMVLevel";
			}
		}

		#endregion Command variations

		#region Other methods

		public static int FindCommandStartLineNumber(TextDocument document, DocumentLine searchStartingLine)
		{
			DocumentLine previousLine;
			string previousLineText;

			int i = searchStartingLine.LineNumber - 1;

			do
			{
				if (i <= 0)
					return 0;

				previousLine = document.GetLineByNumber(i);
				previousLineText = document.GetText(previousLine.Offset, previousLine.Length);

				if (Regex.IsMatch(previousLineText, ScriptPatterns.NextLineKey) && previousLineText.Contains("="))
					return i;

				i--;
			}
			while (Regex.IsMatch(previousLineText, ScriptPatterns.NextLineKey));

			return 0;
		}

		private static List<string> GetLinesToMerge(TextDocument document, DocumentLine startingLine)
		{
			List<string> linesToMerge = new List<string>();

			string startingLineText = document.GetText(startingLine.Offset, startingLine.Length);
			linesToMerge.Add(startingLineText);

			DocumentLine nextLine;
			string nextLineText;

			int i = startingLine.LineNumber + 1;

			do
			{
				if (i > document.LineCount)
					return null;

				nextLine = document.GetLineByNumber(i);
				nextLineText = document.GetText(nextLine.Offset, nextLine.Length);

				linesToMerge.Add(nextLineText);
				i++;
			}
			while (Regex.IsMatch(nextLineText, ScriptPatterns.NextLineKey));

			return linesToMerge;
		}

		private static string MergeLines(List<string> lines)
		{
			StringBuilder builder = new StringBuilder();

			foreach (string line in lines)
				builder.Append(line + Environment.NewLine);

			return builder.ToString();
		}

		#endregion Other methods
	}
}
