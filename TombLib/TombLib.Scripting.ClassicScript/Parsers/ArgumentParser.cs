using ICSharpCode.AvalonEdit.Document;

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

			int totalArgumentCount = wholeLineText.Split(',').Length;

			DocumentLine commandStartLine = CommandParser.GetCommandStartLine(document, offset);
			int wholeLineSubstringOffset = offset - commandStartLine.Offset;

			if (wholeLineSubstringOffset > wholeLineText.Length) // Useless?
				return totalArgumentCount - 1;

			string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffset);

			int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

			return totalArgumentCount - argumentCountAfterOffset;
		}

		public static string GetArgumentFromIndex(TextDocument document, int offset, int index)
		{
			string wholeLineText = CommandParser.GetWholeCommandLineText(document, offset);

			if (wholeLineText == null)
				return null;

			return wholeLineText.Split(',')[index];
		}

		public static string GetFlagPrefixOfCurrentArgument(TextDocument document, int offset)
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

				if (!currentSyntaxArgument.Contains("_") || !currentSyntaxArgument.Contains("."))
					return null;

				return currentSyntaxArgument.Split('.')[0].Split('(')[1];
			}
			catch
			{
				return null;
			}
		}
	}
}
