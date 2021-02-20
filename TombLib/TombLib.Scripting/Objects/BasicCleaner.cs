using System;

namespace TombLib.Scripting.Objects
{
	public class BasicCleaner
	{
		public string TrimEndingWhitespace(string editorContent)
		{
			string[] lines = TrimEndingWhitespaceOnLines(editorContent.Replace("\r", string.Empty).Split('\n'));
			return string.Join(Environment.NewLine, lines);
		}

		public static string[] TrimEndingWhitespaceOnLines(string[] lines)
		{
			string[] trimmedText = new string[lines.Length];

			for (int i = 0; i < trimmedText.Length; i++)
				trimmedText[i] = lines[i].TrimEnd();

			return trimmedText;
		}
	}
}
