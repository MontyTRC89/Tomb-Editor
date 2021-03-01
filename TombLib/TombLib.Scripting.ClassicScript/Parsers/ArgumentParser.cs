using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class ArgumentParser
	{
		public static int GetArgumentIndexAtOffset(TextDocument document, int offset)
		{
			string wholeLineText = CommandParser.GetWholeCommandLineText(document, offset);

			if (string.IsNullOrEmpty(wholeLineText))
				return -1;

			wholeLineText = LineParser.EscapeComments(wholeLineText);

			if (string.IsNullOrWhiteSpace(wholeLineText) || !wholeLineText.Contains("="))
				return -1;

			wholeLineText = MergeMultipleFlags(wholeLineText, document, offset);

			if (string.IsNullOrEmpty(wholeLineText))
				return -1;

			int totalArgumentCount = wholeLineText.Split(',').Length;

			DocumentLine commandStartLine = CommandParser.GetCommandStartLine(document, offset);
			int wholeLineSubstringOffset = offset - commandStartLine.Offset;

			if (wholeLineSubstringOffset > wholeLineText.Length) // Useless?
				return totalArgumentCount - 1;

			string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffset);

			int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

			return totalArgumentCount - argumentCountAfterOffset;
		}

		// TODO: Refactor !!!

		public static string GetArgumentFromIndex(TextDocument document, int offset, int index)
		{
			string wholeLineText = CommandParser.GetWholeCommandLineText(document, offset);

			if (wholeLineText == null)
				return null;

			wholeLineText = MergeMultipleFlags(wholeLineText, document, offset);

			if (wholeLineText == null)
				return null;

			return wholeLineText.Split(',')[index];
		}

		private static string MergeMultipleFlags(string wholeLineText, TextDocument document, int offset)
		{
			string cachedArgument = string.Empty;

			string commandSyntax = CommandParser.GetCommandSyntax(document, offset);

			string command = wholeLineText.Split('=')[0];
			string[] arguments = LineParser.EscapeComments(wholeLineText).Split('=')[1]
				.Replace('>', ' ').Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ').Split(',');

			var newArgumentList = new List<string>();

			for (int i = 0; i < arguments.Length; i++)
			{
				string argument = arguments[i];
				string argumentSyntax = string.Empty;

				if (!string.IsNullOrEmpty(commandSyntax) && i < commandSyntax.Split(',').Length)
					argumentSyntax = commandSyntax.Split(',')[i];

				if (!argument.Contains("_"))
				{
					newArgumentList.Add(argument);
					cachedArgument = argument;
					continue;
				}

				string flagPrefix = argument.Split('_')[0].Trim();

				if (flagPrefix.Equals(cachedArgument.Split('_')[0].Trim(), StringComparison.OrdinalIgnoreCase))
				{
					if (argumentSyntax.Contains(flagPrefix + "_"))
					{
						newArgumentList.Add(argument);
						cachedArgument = argument;
						continue;
					}

					if (newArgumentList.Count > 0)
						newArgumentList.RemoveAt(newArgumentList.Count - 1);

					cachedArgument = cachedArgument + "." + argument;

					newArgumentList.Add(cachedArgument);
				}
				else
				{
					newArgumentList.Add(argument);
					cachedArgument = argument;
				}
			}

			return command + "=" + string.Join(",", newArgumentList.ToArray());
		}

		public static string GetFirstLetterOfCurrentArgument(TextDocument document, int offset)
		{
			try // TODO: Possibly get rid of this try / catch
			{
				int currentArgumentIndex = GetArgumentIndexAtOffset(document, offset);

				if (currentArgumentIndex == -1)
					return null;

				string syntax = CommandParser.GetCommandSyntax(document, offset);

				if (string.IsNullOrEmpty(syntax))
					return null;

				string[] syntaxArguments = syntax.Split(',');

				if (syntaxArguments.Length < currentArgumentIndex)
					return null;

				string currentSyntaxArgument = syntaxArguments[currentArgumentIndex];

				if (!currentSyntaxArgument.Contains("_"))
					return null;

				string flagPrefix = currentSyntaxArgument.Split('_')[0].Split('(')[1];

				return flagPrefix[0].ToString();
			}
			catch
			{
				return null;
			}
		}

		public static string GetFirstLetterOfLastFlag(TextDocument document, int offset)
		{
			int currentArgumentIndex = GetArgumentIndexAtOffset(document, offset);

			if (currentArgumentIndex == -1 || currentArgumentIndex == 0)
				return null;

			string prevArgument = GetArgumentFromIndex(document, offset, currentArgumentIndex - 1).Trim();

			if (!prevArgument.Contains("_"))
				return null;

			if (prevArgument.Contains("="))
				prevArgument.Split('=').Last().Trim();

			return prevArgument[0].ToString();
		}
	}
}
