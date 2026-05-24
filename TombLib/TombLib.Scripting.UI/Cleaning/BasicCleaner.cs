using System;

namespace TombLib.Scripting.UI.Cleaning
{
	public static class BasicCleaner
	{
		public static string TrimTrailingWhitespace(string input)
		{
			string[] lines = TrimTrailingWhitespaceOnLines(input.Replace("\r", string.Empty).Split('\n'));
			return string.Join(Environment.NewLine, lines);
		}

		public static string[] TrimTrailingWhitespaceOnLines(string[] lines)
		{
			string[] trimmedText = new string[lines.Length];

			for (int i = 0; i < trimmedText.Length; i++)
				trimmedText[i] = lines[i].TrimEnd();

			return trimmedText;
		}

		public static string TrimEndingWhitespace(string input)
			=> TrimTrailingWhitespace(input);

		public static string[] TrimEndingWhitespaceOnLines(string[] lines)
			=> TrimTrailingWhitespaceOnLines(lines);
	}
}
