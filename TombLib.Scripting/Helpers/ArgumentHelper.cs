using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Scripting.Helpers
{
	public static class ArgumentHelper
	{
		public static int GetArgumentIndexAtOffset(TextDocument document, int offset)
		{
			string wholeLineText = CommandHelper.GetWholeCommandLineText(document, offset);

			if (wholeLineText == null)
				return -1;

			wholeLineText = MergeMultipleFlags(wholeLineText);

			if (wholeLineText == null)
				return -1;

			DocumentLine commandStartLine = CommandHelper.GetCommandStartLine(document, offset);
			int wholeLineSubstringOffset = offset - commandStartLine.Offset;

			string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffset);

			int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

			int totalArgumentCount = wholeLineText.Split(',').Length;

			return totalArgumentCount - argumentCountAfterOffset;
		}

		// TODO: Refactor !!!

		public static string GetArgumentFromIndex(TextDocument document, int offset, int index)
		{
			string wholeLineText = CommandHelper.GetWholeCommandLineText(document, offset);

			if (wholeLineText == null)
				return null;

			wholeLineText = MergeMultipleFlags(wholeLineText);

			if (wholeLineText == null)
				return null;

			return wholeLineText.Split(',')[index];
		}

		private static string MergeMultipleFlags(string wholeLineText)
		{
			string cachedArgument = string.Empty;

			string[] arguments = LineHelper.RemoveComments(wholeLineText).Split(',');

			List<string> newArgumentList = new List<string>();

			for (int i = 0; i < arguments.Length; i++)
			{
				string argument = arguments[i];

				if (!argument.Contains("_"))
				{
					newArgumentList.Add(argument);
					continue;
				}

				string flagPrefix;

				if (i == 0)
				{
					if (!argument.Contains("="))
						return null;

					flagPrefix = argument.Split('=').Last().Split('_')[0].Trim();
				}
				else
					flagPrefix = argument.Split('_')[0].Trim();

				if (flagPrefix.Equals(cachedArgument.Split('_')[0].Trim(), StringComparison.OrdinalIgnoreCase))
				{
					if (newArgumentList.Count > 0)
						newArgumentList.RemoveAt(newArgumentList.Count - 1);

					cachedArgument = cachedArgument + "_" + argument;

					newArgumentList.Add(cachedArgument);
				}
				else
				{
					newArgumentList.Add(argument);
					cachedArgument = argument;
				}
			}

			return string.Join(",", newArgumentList.ToArray());
		}
	}
}
