using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.Helpers
{
	public static class LineHelper
	{
		public static string RemoveComments(string lineText)
		{
			return Regex.Replace(lineText, ";.*$", string.Empty).TrimEnd();
		}

		public static bool IsEmptyLine(string lineText)
		{
			return string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(";");
		}

		public static bool IsSectionHeaderLine(string lineText)
		{
			return Regex.IsMatch(lineText, @"^\[\b.*\b\]\s*?(;.*)?$");
		}

		public static bool IsLineInStandardStringSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = CommandHelper.GetSectionNameFromLine(document, line.LineNumber);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("strings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("pcstrings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("psxstrings", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsLineInNGStringSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = CommandHelper.GetSectionNameFromLine(document, line.LineNumber);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("extrang", StringComparison.OrdinalIgnoreCase);
		}
	}
}
