using System;
using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxTidy
	{
		public static List<string> ReindentLines(string editorContent)
		{
			string firstCondition = string.Empty;
			string secondCondition = string.Empty;

			if (Properties.Settings.Default.PreEqualSpace)
			{
				editorContent = editorContent.Replace("=", " =");
				firstCondition = "  =";
				secondCondition = " =";
			}
			else
			{
				editorContent = editorContent.Replace(" =", "=");
				firstCondition = " =";
				secondCondition = "=";
			}

			editorContent = LoopReplace(editorContent, firstCondition, secondCondition);

			if (Properties.Settings.Default.PostEqualSpace)
			{
				editorContent = editorContent.Replace("=", "= ");
				firstCondition = "=  ";
				secondCondition = "= ";
			}
			else
			{
				editorContent = editorContent.Replace("= ", "=");
				firstCondition = "= ";
				secondCondition = "=";
			}

			editorContent = LoopReplace(editorContent, firstCondition, secondCondition);

			if (Properties.Settings.Default.PreCommaSpace)
			{
				editorContent = editorContent.Replace(",", " ,");
				firstCondition = "  ,";
				secondCondition = " ,";
			}
			else
			{
				editorContent = editorContent.Replace(" ,", ",");
				firstCondition = " ,";
				secondCondition = ",";
			}

			editorContent = LoopReplace(editorContent, firstCondition, secondCondition);

			if (Properties.Settings.Default.PostCommaSpace)
			{
				editorContent = editorContent.Replace(",", ", ");
				firstCondition = ",  ";
				secondCondition = ", ";
			}
			else
			{
				editorContent = editorContent.Replace(", ", ",");
				firstCondition = ", ";
				secondCondition = ",";
			}

			editorContent = LoopReplace(editorContent, firstCondition, secondCondition);

			if (Properties.Settings.Default.ReduceSpaces)
			{
				editorContent = LoopReplace(editorContent, "  ", " ");
			}

			return TrimLines(editorContent);
		}

		public static List<string> TrimLines(string editorContent)
		{
			// Get every single line and create a list that will store them
			string[] lines = editorContent.Replace("\r", "").Split('\n');
			List<string> trimmedText = new List<string>();

			// Trim whitespace on every line and add it to the list
			for (int i = 0; i < lines.Length; i++)
			{
				string currentLineText = (lines.Length >= i) ? lines[i] : Environment.NewLine;
				trimmedText.Add(currentLineText.Trim());
			}

			return trimmedText;
		}

		private static string LoopReplace(string editorContent, string firstCondition, string secondCondition)
		{
			if (editorContent.Contains(firstCondition))
			{
				bool ScriptNeedsCleaning = true;

				while (ScriptNeedsCleaning)
				{
					editorContent = editorContent.Replace(firstCondition, secondCondition);

					// Is the script cleaned yet?
					if (!editorContent.Contains(firstCondition))
					{
						ScriptNeedsCleaning = false;
					}
				}
			}

			return editorContent;
		}
	}
}
