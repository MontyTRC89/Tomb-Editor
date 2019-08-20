using System;
using System.Collections.Generic;

namespace TombIDE.Shared.Scripting
{
	internal class SyntaxTidying
	{
		private static Configuration _config;

		public static string ReindentScript(string editorContent)
		{
			_config = Configuration.Load();

			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);

			return TrimScript(editorContent);
		}

		public static string TrimScript(string editorContent)
		{
			// Get all lines and create a list to store them
			string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');
			List<string> trimmedText = new List<string>();

			// Trim whitespace at the end of every line and add it to the list
			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return string.Join(Environment.NewLine, trimmedText);
		}

		public static string[] TrimLines(string[] lines)
		{
			List<string> trimmedText = new List<string>();

			// Trim whitespace at the end of every line and add it to the list
			foreach (string line in lines)
				trimmedText.Add(line.TrimEnd());

			return trimmedText.ToArray();
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
