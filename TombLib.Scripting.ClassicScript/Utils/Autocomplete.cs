using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class Autocomplete
	{
		public static List<ICompletionData> GetNewLineAutocompleteList()
		{
			var data = new List<ICompletionData>();

			foreach (string keyword in Keywords.OldCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in Keywords.NewCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in Keywords.Sections)
				data.Add(new CompletionData("[" + keyword + "]"));

			data.Add(new CompletionData("#INCLUDE "));
			data.Add(new CompletionData("#DEFINE "));
			data.Add(new CompletionData("#FIRST_ID "));

			return data;
		}

		public static List<ICompletionData> GetCompletionData(TextDocument document, int caretOffset, int argumentIndex = -1)
		{
			var completionData = new List<ICompletionData>();

			string syntax = CommandParser.GetCommandSyntax(document, caretOffset);

			if (string.IsNullOrEmpty(syntax))
				return null;

			var regex = new Regex(Patterns.CommandPrefixInParenthesis);

			if (!regex.IsMatch(syntax) && !syntax.ToUpper().Contains("ENABLED") && !syntax.ToUpper().Contains("DISABLED"))
				return null;

			string[] arguments = syntax.Split(',');

			if (argumentIndex == -1)
				argumentIndex = ArgumentParser.GetArgumentIndexAtOffset(document, caretOffset);

			if (arguments.Length <= argumentIndex || argumentIndex == -1)
				return null;

			string currentArgument = arguments[argumentIndex];

			if (regex.IsMatch(currentArgument))
			{
				string mnemonicPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.').Trim();

				foreach (string mnemonicConstant in MnemonicData.AllConstantFlags)
					if (mnemonicConstant.StartsWith(mnemonicPrefix, StringComparison.OrdinalIgnoreCase))
						completionData.Add(new CompletionData(mnemonicConstant));
			}
			else if (currentArgument.ToUpper().Contains("ENABLED") || currentArgument.ToUpper().Contains("DISABLED"))
			{
				completionData.Add(new CompletionData("ENABLED"));
				completionData.Add(new CompletionData("DISABLED"));
			}

			return completionData;
		}
	}
}
