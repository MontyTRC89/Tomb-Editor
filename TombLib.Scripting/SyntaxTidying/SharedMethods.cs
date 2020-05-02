using System;
using System.Collections.Generic;

namespace TombLib.Scripting.SyntaxTidying
{
	internal class SharedTidyingMethods
	{
		public static string TrimEndingWhitespace(string editorContent)
		{
			string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');

			List<string> trimmedText = new List<string>();

			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return string.Join(Environment.NewLine, trimmedText);
		}

		public static string[] TrimEndingWhitespaceOnLines(string[] lines)
		{
			List<string> trimmedText = new List<string>();

			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return trimmedText.ToArray();
		}
	}
}
