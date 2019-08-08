using System;
using System.Collections.Generic;

namespace TombIDE.Shared.Scripting
{
	internal class SyntaxTidying
	{
		private static Configuration _config;

		public static string Reindent(string editorContent)
		{
			_config = Configuration.Load();

			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);
			return Trim(editorContent);
		}

		public static string Trim(string editorContent)
		{
			// Get all lines and create a list to store them
			string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');
			List<string> trimmedText = new List<string>();

			// Trim whitespace on every line and add it to the list
			for (int i = 0; i < lines.Length; i++)
			{
				string currentLineText = (lines.Length >= i) ? lines[i] : Environment.NewLine;
				trimmedText.Add(currentLineText.Trim());
			}

			return string.Join("\r\n", trimmedText);
		}

		private static string HandleSpacesBeforeEquals(string editorContent)
		{
			if (_config.Tidy_PreEqualSpace)
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
			if (_config.Tidy_PostEqualSpace)
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
			if (_config.Tidy_PreCommaSpace)
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
			if (_config.Tidy_PostCommaSpace)
			{
				editorContent = editorContent.Replace(",", ", ");

				while (editorContent.Contains(",  "))
					editorContent = editorContent.Replace(",  ", ", ");
			}
			else
			{
				while (editorContent.Contains(", "))
					editorContent = editorContent.Replace(", ", ",");
			}

			return editorContent;
		}

		private static string HandleSpaceReduction(string editorContent)
		{
			if (_config.Tidy_ReduceSpaces)
			{
				while (editorContent.Contains("  "))
					editorContent = editorContent.Replace("  ", " ");
			}

			return editorContent;
		}
	}
}
