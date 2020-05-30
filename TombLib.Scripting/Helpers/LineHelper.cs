﻿using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.Helpers
{
	public static class LineHelper
	{
		public static MatchCollection GetComments(string lineText)
		{
			return Regex.Matches(lineText, @"\s*;.*$", RegexOptions.Multiline);
		}

		public static string RemoveComments(string lineText) =>
			Regex.Replace(lineText, @"\s*;.*$", string.Empty, RegexOptions.Multiline);

		public static string EscapeComments(string lineText)
		{
			MatchCollection comments = GetComments(lineText);

			foreach (Match match in comments)
				lineText = Regex.Replace(lineText, Regex.Escape(match.Value), new string(' ', match.Length));

			return lineText;
		}

		public static bool IsEmptyLine(string lineText) =>
			string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(";");

		public static bool IsSectionHeaderLine(string lineText) =>
			Regex.IsMatch(lineText, @"^\[\b.*\b\]\s*(;.*)?$");

		public static bool IsLineInStandardStringSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = DocumentHelper.GetSectionName(document, line.Offset);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("strings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("pcstrings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("psxstrings", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsLineInNGStringSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = DocumentHelper.GetSectionName(document, line.Offset);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("extrang", StringComparison.OrdinalIgnoreCase);
		}
	}
}
