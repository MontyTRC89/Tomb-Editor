using System;
using System.Collections.Generic;

namespace TombLib.Scripting.CodeCleaners
{
	public class BasicCleaner
	{
		public string TrimEndingWhitespace(string editorContent)
		{
			string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');

			List<string> trimmedText = new List<string>();

			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return string.Join(Environment.NewLine, trimmedText);
		}

		public string[] TrimEndingWhitespaceOnLines(string[] lines)
		{
			List<string> trimmedText = new List<string>();

			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return trimmedText.ToArray();
		}
	}
}
