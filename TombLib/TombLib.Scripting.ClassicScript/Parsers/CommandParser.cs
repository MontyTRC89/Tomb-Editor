using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.ClassicScript.Syntaxes;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class CommandParser
	{
		#region Public methods

		public static string GetCommandSyntax(TextDocument document, int offset)
		{
			string wholeCommandLineText = GetWholeCommandLineText(document, offset);

			if (string.IsNullOrEmpty(wholeCommandLineText))
				return null;

			if (Regex.IsMatch(wholeCommandLineText, Patterns.CustomizeCommandWithFirstArg, RegexOptions.IgnoreCase))
				return GetSubcommandSyntax(wholeCommandLineText, SubcommandType.Cust);
			else if (Regex.IsMatch(wholeCommandLineText, Patterns.ParametersCommandWithFirstArg, RegexOptions.IgnoreCase))
				return GetSubcommandSyntax(wholeCommandLineText, SubcommandType.Param);
			else
			{
				string commandKey = GetCommandKey(document, offset);

				if (string.IsNullOrEmpty(commandKey))
					return null;

				foreach (DictionaryEntry entry in GetCommandSyntaxResources())
					if (commandKey.Equals(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase))
						return entry.Value.ToString();
			}

			return null;
		}

		public static string GetWholeCommandLineText(TextDocument document, int offset)
		{
			DocumentLine commandStartLine = GetCommandStartLine(document, offset);

			if (commandStartLine == null)
				return null;

			string commandStartLineText = document.GetText(commandStartLine.Offset, commandStartLine.Length);

			if (!Regex.IsMatch(commandStartLineText, Patterns.NextLineKey)) // If commandStartLineText is not multiline
				return commandStartLineText;
			else
			{
				IEnumerable<DocumentLine> linesToMerge = GetLinesToMerge(document, commandStartLine);

				if (linesToMerge == null)
					return null;

				return MergeLines(document, linesToMerge);
			}
		}

		public static DocumentLine GetCommandStartLine(TextDocument document, int offset)
		{
			DocumentLine offsetLine = document.GetLineByOffset(offset);
			string offsetLineText = LineParser.EscapeComments(document.GetText(offsetLine.Offset, offsetLine.Length));

			if (offsetLineText.Contains("=") || offsetLineText.Trim().StartsWith("#"))
				return offsetLine;
			else if (LineParser.IsSectionHeaderLine(offsetLineText))
				return null;
			else
				return FindCommandStartLine(document, offsetLine);
		}

		public static string GetCommandKey(TextDocument document, int offset)
		{
			DocumentLine commandStartLine = GetCommandStartLine(document, offset);

			if (commandStartLine == null)
				return null;

			string commandStartLineText = document.GetText(commandStartLine.Offset, commandStartLine.Length);
			commandStartLineText = LineParser.EscapeComments(commandStartLineText);

			if (commandStartLineText.Contains("="))
				return GetCorrectCommandVariation(document, commandStartLine.Offset, commandStartLineText.Split('=')[0].Trim());
			else if (commandStartLineText.Trim().StartsWith("#"))
				return commandStartLineText.Split(' ')[0].Trim();

			return null;
		}

		public static List<DictionaryEntry> GetCommandSyntaxResources()
		{
			var entries = new List<DictionaryEntry>();

			// Get resources from OldCommandSyntaxes.resx
			var oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
			ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Get resources from NewCommandSyntaxes.resx
			var newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
			ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
			entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

			return entries;
		}

		public static string GetFullIncludePath(TextDocument document, int offset)
		{
			DocumentLine caretLine = document.GetLineByOffset(offset);
			string caretLineText = document.GetText(caretLine.Offset, caretLine.Length);

			if (LineParser.IsValidIncludeLine(caretLineText))
			{
				string rootPath = Path.GetDirectoryName(document.FileName);
				string pathPart = caretLineText.Split('"')[1];

				return Path.Combine(rootPath, pathPart);
			}

			return null;
		}

		#endregion Public methods

		#region Subcommands

		private static string GetSubcommandSyntax(string wholeCommandLineText, SubcommandType subcommandType)
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

			string key = wholeCommandLineText.Split('=')[1].Split(',')[0].Trim();

			string custParamSyntax = FindCustParamSyntaxByKey(resourceSet, key);

			if (string.IsNullOrEmpty(custParamSyntax))
				foreach (DictionaryEntry entry in GetCommandSyntaxResources())
					switch (subcommandType)
					{
						case SubcommandType.Cust:
							if (entry.Key.ToString().Equals("Customize", StringComparison.OrdinalIgnoreCase))
								return entry.Value.ToString();
							break;

						case SubcommandType.Param:
							if (entry.Key.ToString().Equals("Parameters", StringComparison.OrdinalIgnoreCase))
								return entry.Value.ToString();
							break;
					}

			return custParamSyntax;
		}

		private static string FindCustParamSyntaxByKey(ResourceSet resourceSet, string key)
		{
			if (string.IsNullOrWhiteSpace(key))
				return null;

			// Search in the given ResourceSet
			foreach (DictionaryEntry entry in resourceSet)
				if (entry.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase))
					return entry.Value.ToString();

			// Search in PluginMnemonics
			foreach (PluginConstant pluginMnemonic in MnemonicData.PluginConstants)
				if (pluginMnemonic.FlagName.Equals(key, StringComparison.OrdinalIgnoreCase))
					return Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Trim();

			return null;
		}

		#endregion Subcommands

		#region Command variations

		private static string GetCorrectCommandVariation(TextDocument document, int offset, string command)
		{
			string sectionName = DocumentParser.GetCurrentSectionName(document, offset);

			if (command.Equals("level", StringComparison.OrdinalIgnoreCase))
				return GetCorrectLevelCommandForSection(sectionName);
			else if (command.Equals("cut", StringComparison.OrdinalIgnoreCase))
				return GetCorrectCutCommandForSection(sectionName);
			else if (command.Equals("fmv", StringComparison.OrdinalIgnoreCase))
				return GetCorrectFMVCommandForSection(sectionName);
			else
				return command;
		}

		private static string GetCorrectLevelCommandForSection(string sectionName)
		{
			if (string.IsNullOrEmpty(sectionName))
				return "LevelLevel";

			switch (sectionName.ToUpper())
			{
				case "PCEXTENSIONS":
					return "LevelPC";

				case "PSXEXTENSIONS":
					return "LevelPSX";

				default:
					return "LevelLevel";
			}
		}

		private static string GetCorrectCutCommandForSection(string sectionName)
		{
			if (string.IsNullOrEmpty(sectionName))
				return null;

			switch (sectionName.ToUpper())
			{
				case "PCEXTENSIONS":
					return "CutPC";

				case "PSXEXTENSIONS":
					return "CutPSX";

				default:
					return null;
			}
		}

		private static string GetCorrectFMVCommandForSection(string sectionName)
		{
			if (string.IsNullOrEmpty(sectionName))
				return "FMVLevel";

			switch (sectionName.ToUpper())
			{
				case "PCEXTENSIONS":
					return "FMVPC";

				case "PSXEXTENSIONS":
					return "FMVPSX";

				default:
					return "FMVLevel";
			}
		}

		#endregion Command variations

		#region Other methods

		private static DocumentLine FindCommandStartLine(TextDocument document, DocumentLine searchStartingLine)
		{
			DocumentLine previousLine;
			string previousLineText;

			int i = searchStartingLine.LineNumber - 1;

			do
			{
				if (i <= 0)
					return null;

				previousLine = document.GetLineByNumber(i);
				previousLineText = LineParser.EscapeComments(document.GetText(previousLine.Offset, previousLine.Length));

				if (Regex.IsMatch(previousLineText, Patterns.NextLineKey) && previousLineText.Contains("="))
					return previousLine;

				i--;
			}
			while (Regex.IsMatch(previousLineText, Patterns.NextLineKey));

			return null;
		}

		private static IEnumerable<DocumentLine> GetLinesToMerge(TextDocument document, DocumentLine startingLine)
		{
			yield return startingLine;

			DocumentLine nextLine;
			string nextLineText;

			int i = startingLine.LineNumber + 1;

			do
			{
				if (i > document.LineCount)
					yield break;

				nextLine = document.GetLineByNumber(i);
				nextLineText = document.GetText(nextLine.Offset, nextLine.Length);

				yield return nextLine;

				i++;
			}
			while (Regex.IsMatch(nextLineText, Patterns.NextLineKey));
		}

		private static string MergeLines(TextDocument document, IEnumerable<DocumentLine> lines)
		{
			var builder = new StringBuilder();

			foreach (DocumentLine line in lines)
				builder.Append(document.GetText(line.Offset, line.Length) + Environment.NewLine);

			return builder.ToString();
		}

		#endregion Other methods
	}
}
