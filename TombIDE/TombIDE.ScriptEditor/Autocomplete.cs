using ICSharpCode.AvalonEdit.CodeCompletion;
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
using TombIDE.Shared.Scripting;

namespace TombIDE.ScriptEditor
{
	internal class Autocomplete
	{
		public static List<ICompletionData> GetNewLineAutocompleteList()
		{
			List<ICompletionData> data = new List<ICompletionData>();

			foreach (string keyword in KeyWords.OldCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in KeyWords.NewCommands)
				data.Add(new CompletionData(keyword + "="));

			foreach (string keyword in KeyWords.Sections)
				data.Add(new CompletionData("[" + keyword + "]"));

			data.Add(new CompletionData("#INCLUDE "));
			data.Add(new CompletionData("#DEFINE "));
			data.Add(new CompletionData("#FIRST_ID "));

			return data;
		}

		public static List<ICompletionData> GetCommandAutocompleteList(string editorText, int caretOffset)
		{
			TextDocument document = new TextDocument(editorText);
			List<ICompletionData> completionData = new List<ICompletionData>();

			DocumentLine currentLine = document.GetLineByOffset(caretOffset);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

			if (!currentLineText.Contains("="))
				return null;

			string command = currentLineText.Split('=')[0].Trim();

			// Get resources from OldCommandSyntaxes.resx
			ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
			ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Get resources from NewCommandSyntaxes.resx
			ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
			ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			List<DictionaryEntry> entries = new List<DictionaryEntry>();
			entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
			entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

			string syntax = string.Empty;

			foreach (DictionaryEntry entry in entries)
			{
				if (command.ToLower() == entry.Key.ToString().ToLower())
				{
					syntax = entry.Value.ToString();
					break;
				}
			}

			if (string.IsNullOrEmpty(syntax))
				return null;

			Regex regex = new Regex(@"\(.*?_\.*?\)");

			if (!regex.IsMatch(syntax) && (!syntax.ToLower().Contains("enabled") && !syntax.ToLower().Contains("disabled")))
				return null;

			int totalArgumentCount = currentLineText.Split(',').Length - 1;
			string textAfterCaret = document.GetText(caretOffset, currentLine.EndOffset - caretOffset);

			int argumentCountAfterCaret = textAfterCaret.Split(',').Length - 1;
			int currentArgumentIndex = totalArgumentCount - argumentCountAfterCaret;

			string currentArgument = syntax.Split(',')[currentArgumentIndex];

			if (regex.IsMatch(currentArgument))
			{
				string searchedMnemonicsPrefix = currentArgument.Split('(')[1].Split(')')[0].Trim('.');

				foreach (string mnemonicConstant in KeyWords.AllMnemonics)
				{
					if (mnemonicConstant.StartsWith(searchedMnemonicsPrefix, StringComparison.OrdinalIgnoreCase))
						completionData.Add(new CompletionData(mnemonicConstant));
				}
			}
			else if (currentArgument.ToLower().Contains("enabled") || currentArgument.ToLower().Contains("disabled"))
			{
				completionData.Add(new CompletionData("ENABLED"));
				completionData.Add(new CompletionData("DISABLED"));
			}

			return completionData;
		}

		public static List<ICompletionData> GetCustomizeAutocompleteList(string editorText, int caretOffset)
		{
			TextDocument document = new TextDocument(editorText);
			List<ICompletionData> completionData = new List<ICompletionData>();

			DocumentLine currentLine = document.GetLineByOffset(caretOffset);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

			string custKey = currentLineText.Split('=')[1].Split(',')[0].Trim();

			completionData.AddRange(GetNextCUSTKeyWords(document, caretOffset, custKey));
			completionData.AddRange(GetNextPluginCUSTKeyWords(currentLineText, custKey));

			return completionData;
		}

		private static List<ICompletionData> GetNextCUSTKeyWords(TextDocument document, int caretOffset, string custKey)
		{
			List<ICompletionData> data = new List<ICompletionData>();

			// Get resources from CustSyntaxes.resx
			ResourceManager custSyntaxResource = new ResourceManager(typeof(CustSyntaxes));
			ResourceSet custResourceSet = custSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			foreach (DictionaryEntry entry in custResourceSet)
			{
				// If the first argument from the current line matches a PARAM key defined in the ResourceSet
				if (custKey.ToLower() == entry.Key.ToString().ToLower())
				{
					int currentArgumentIndex = ArgumentHelper.GetCurrentArgumentIndex(document, caretOffset);

					string syntax = entry.Value.ToString();
					string[] arguments = syntax.Split(',');

					if (currentArgumentIndex >= arguments.Length)
						return new List<ICompletionData>();

					string currentArgument = syntax.Split(',')[currentArgumentIndex];

					Regex regex = new Regex(@"\(.*?_\.*?\)");
					string mnemonicPrefix = regex.Match(currentArgument).Value.Trim('(').Trim(')').Trim('.');

					if (string.IsNullOrWhiteSpace(mnemonicPrefix))
					{
						if (currentArgument.ToLower().Contains("enabled") || currentArgument.ToLower().Contains("disabled"))
						{
							data.Add(new CompletionData("ENABLED"));
							data.Add(new CompletionData("DISABLED"));

							return data;
						}
						else
							return new List<ICompletionData>();
					}

					for (int k = 0; k < KeyWords.AllMnemonics.Length; k++)
					{
						string nextMnemonic = KeyWords.AllMnemonics[k];

						if (nextMnemonic.StartsWith(mnemonicPrefix, StringComparison.OrdinalIgnoreCase))
							data.Add(new CompletionData(nextMnemonic));
					}

					break;
				}
			}

			return data;
		}

		private static List<ICompletionData> GetNextPluginCUSTKeyWords(string currentLineText, string custKey)
		{
			List<ICompletionData> data = new List<ICompletionData>();

			for (int i = 0; i < KeyWords.PluginMnemonics.Length; i++)
			{
				PluginMnemonic pluginMnemonic = KeyWords.PluginMnemonics[i];

				if (pluginMnemonic.Flag.ToLower() == custKey.ToLower())
				{
					string[] nextKeys = Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Split(',');
					string[] currentLineKeys = currentLineText.Split('=')[1].Split(',');

					for (int j = currentLineKeys.Length - 1; j < nextKeys.Length; j++)
					{
						if (currentLineKeys.Length >= nextKeys.Length)
							break;

						string nextKeyPrefix = string.Empty;

						try { nextKeyPrefix = nextKeys[j].Split('(')[1].Split('.')[0]; }
						catch { continue; }

						if (string.IsNullOrWhiteSpace(nextKeyPrefix))
							continue;

						for (int k = 0; k < KeyWords.AllMnemonics.Length; k++)
						{
							string nextMnemonic = KeyWords.AllMnemonics[k];

							if (nextMnemonic.StartsWith(nextKeyPrefix, StringComparison.OrdinalIgnoreCase))
								data.Add(new CompletionData(nextMnemonic));
						}

						break;
					}
				}
			}

			return data;
		}

		public static List<ICompletionData> GetParametersAutocompleteList(string editorText, int caretOffset)
		{
			TextDocument document = new TextDocument(editorText);
			List<ICompletionData> completionData = new List<ICompletionData>();

			DocumentLine currentLine = document.GetLineByOffset(caretOffset);
			string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

			string paramKey = currentLineText.Split('=')[1].Split(',')[0].Trim();

			completionData.AddRange(GetNextPARAMKeyWords(document, caretOffset, paramKey));
			completionData.AddRange(GetNextPluginPARAMKeyWords(currentLineText, paramKey));

			return completionData;
		}

		private static List<ICompletionData> GetNextPARAMKeyWords(TextDocument document, int caretOffset, string paramKey)
		{
			List<ICompletionData> data = new List<ICompletionData>();

			// Get resources from ParamSyntaxes.resx
			ResourceManager paramSyntaxResource = new ResourceManager(typeof(ParamSyntaxes));
			ResourceSet paramResourceSet = paramSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			foreach (DictionaryEntry entry in paramResourceSet)
			{
				// If the first argument from the current line matches a PARAM key defined in the ResourceSet
				if (paramKey.ToLower() == entry.Key.ToString().ToLower())
				{
					int currentArgumentIndex = ArgumentHelper.GetCurrentArgumentIndex(document, caretOffset);

					string syntax = entry.Value.ToString();
					string[] arguments = syntax.Split(',');

					if (currentArgumentIndex >= arguments.Length)
						return new List<ICompletionData>();

					string currentArgument = syntax.Split(',')[currentArgumentIndex];

					Regex regex = new Regex(@"\(.*?_\.*?\)");
					string mnemonicPrefix = regex.Match(currentArgument).Value.Trim('(').Trim(')').Trim('.');

					if (string.IsNullOrWhiteSpace(mnemonicPrefix))
					{
						if (currentArgument.ToLower().Contains("enabled") || currentArgument.ToLower().Contains("disabled"))
						{
							data.Add(new CompletionData("ENABLED"));
							data.Add(new CompletionData("DISABLED"));

							return data;
						}
						else
							return new List<ICompletionData>();
					}

					for (int k = 0; k < KeyWords.AllMnemonics.Length; k++)
					{
						string nextMnemonic = KeyWords.AllMnemonics[k];

						if (nextMnemonic.StartsWith(mnemonicPrefix, StringComparison.OrdinalIgnoreCase))
							data.Add(new CompletionData(nextMnemonic));
					}

					break;
				}
			}

			return data;
		}

		private static List<ICompletionData> GetNextPluginPARAMKeyWords(string currentLineText, string paramKey)
		{
			List<ICompletionData> data = new List<ICompletionData>();

			for (int i = 0; i < KeyWords.PluginMnemonics.Length; i++)
			{
				PluginMnemonic pluginMnemonic = KeyWords.PluginMnemonics[i];

				if (pluginMnemonic.Flag.ToLower() == paramKey.ToLower())
				{
					string[] nextKeys = Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Split(',');
					string[] currentLineKeys = currentLineText.Split('=')[1].Split(',');

					for (int j = currentLineKeys.Length - 1; j < nextKeys.Length; j++)
					{
						if (currentLineKeys.Length >= nextKeys.Length)
							break;

						string nextKeyPrefix = string.Empty;

						try { nextKeyPrefix = nextKeys[j].Split('(')[1].Split('.')[0]; }
						catch { continue; }

						if (string.IsNullOrWhiteSpace(nextKeyPrefix))
							continue;

						for (int k = 0; k < KeyWords.AllMnemonics.Length; k++)
						{
							string nextMnemonic = KeyWords.AllMnemonics[k];

							if (nextMnemonic.StartsWith(nextKeyPrefix, StringComparison.OrdinalIgnoreCase))
								data.Add(new CompletionData(nextMnemonic));
						}

						break;
					}
				}
			}

			return data;
		}
	}
}
