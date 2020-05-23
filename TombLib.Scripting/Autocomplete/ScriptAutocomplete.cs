﻿using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;

namespace TombLib.Scripting.Autocomplete
{
	public static class ScriptAutocomplete
	{
		public static List<ICompletionData> GetNewLineAutocompleteList()
		{
			List<ICompletionData> data = new List<ICompletionData>();

			foreach (string keyword in ScriptKeywords.OldCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in ScriptKeywords.NewCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in ScriptKeywords.Sections)
				data.Add(new CompletionData("[" + keyword + "]"));

			data.Add(new CompletionData("#INCLUDE "));
			data.Add(new CompletionData("#DEFINE "));
			data.Add(new CompletionData("#FIRST_ID "));

			return data;
		}

		public static List<ICompletionData> GetCompletionData(TextDocument document, int caretOffset, int argumentIndex = -1)
		{
			List<ICompletionData> completionData = new List<ICompletionData>();

			string syntax = CommandHelper.GetCommandSyntax(document, caretOffset);

			if (string.IsNullOrEmpty(syntax))
				return null;

			Regex regex = new Regex(@"\(.*?_\.*?\)"); // (CMD_...)

			if (!regex.IsMatch(syntax) && !syntax.ToUpper().Contains("ENABLED") && !syntax.ToUpper().Contains("DISABLED"))
				return null;

			string[] arguments = syntax.Split(',');

			if (argumentIndex == -1)
				argumentIndex = ArgumentHelper.GetArgumentIndexAtOffset(document, caretOffset);

			if (arguments.Length <= argumentIndex || argumentIndex == -1)
				return null;

			string currentArgument = arguments[argumentIndex];

			if (regex.IsMatch(currentArgument))
			{
				string mnemonicPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.').Trim();

				foreach (string mnemonicConstant in ScriptKeywords.AllMnemonics)
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
