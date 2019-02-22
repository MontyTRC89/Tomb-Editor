using System;
using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxTidying
	{
		public static List<string> ReindentLines(string editorContent)
		{
			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);
			return TrimLines(editorContent);
		}

		private static string HandleSpacesBeforeEquals(string editorContent)
		{
			if (Properties.Settings.Default.PreEqualSpace)
			{
				editorContent = editorContent.Replace("=", " =");

				while (editorContent.Contains("  ="))
					editorContent = editorContent.Replace("  =", " =");
			}
			else
			{
				while (editorContent.Contains(" ="))
					editorContent = editorContent.Replace(" =", "=");
			}

			return editorContent;
		}

		private static string HandleSpacesAfterEquals(string editorContent)
		{
			if (Properties.Settings.Default.PostEqualSpace)
			{
				editorContent = editorContent.Replace("=", "= ");

				while (editorContent.Contains("=  "))
					editorContent = editorContent.Replace("=  ", "= ");
			}
			else
			{
				while (editorContent.Contains("= "))
					editorContent = editorContent.Replace("= ", "=");
			}

			return editorContent;
		}

		private static string HandleSpacesBeforeCommas(string editorContent)
		{
			if (Properties.Settings.Default.PreCommaSpace)
			{
				editorContent = editorContent.Replace(",", " ,");

				while (editorContent.Contains("  ,"))
					editorContent = editorContent.Replace("  ,", " ,");
			}
			else
			{
				while (editorContent.Contains(" ,"))
					editorContent = editorContent.Replace(" ,", ",");
			}

			return editorContent;
		}

		private static string HandleSpacesAfterCommas(string editorContent)
		{
			if (Properties.Settings.Default.PostCommaSpace)
			{
				editorContent = editorContent.Replace(",", ", ");

				while (editorContent.Contains(",  "))
				{
					editorContent = editorContent.Replace(",  ", ", ");
				}
			}
			else
			{
				while (editorContent.Contains(", "))
				{
					editorContent = editorContent.Replace(", ", ",");
				}
			}

			return editorContent;
		}

		private static string HandleSpaceReduction(string editorContent)
		{
			if (Properties.Settings.Default.ReduceSpaces)
			{
				while (editorContent.Contains("  "))
				{
					editorContent = editorContent.Replace("  ", " ");
				}
			}

			return editorContent;
		}

		public static List<string> TrimLines(string editorContent)
		{
			// Get all lines and create a list to store them
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
	}
}
