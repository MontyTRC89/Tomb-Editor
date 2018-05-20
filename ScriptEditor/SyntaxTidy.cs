using System;
using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxTidy
	{
		public static string ReindentScript(string editorContent)
		{
			editorContent = editorContent.Replace("=", " = ");
			editorContent = editorContent.Replace(",", ", ");

			// Check if there are too many spaces
			if (editorContent.Contains("  "))
			{
				bool ScriptNeedsCleaning = true;

				while (ScriptNeedsCleaning)
				{
					editorContent = editorContent.Replace("  ", " ");

					// Is the script cleaned yet?
					if (!editorContent.Contains("  "))
					{
						ScriptNeedsCleaning = false;
					}
				}
			}

			return TrimWhitespace(editorContent);
		}

		public static string TrimWhitespace(string editorContent)
		{
			// Get every single line and create a list that will store them
			string[] lines = editorContent.Replace("\r", "").Split('\n');
			List<string> trimmedText = new List<string>();

			int i = 0;

			// Trim whitespace on every line and add it to the list
			while (i < lines.Length)
			{
				string currentLineText = (lines.Length >= i) ? lines[i] : Environment.NewLine;
				trimmedText.Add(currentLineText.Trim());
				i++;
			}

			return String.Join(Environment.NewLine, trimmedText.ToArray());
		}
	}
}
