using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class LineParser
	{
		/// <summary>
		/// Correct formatting:<br/>
		/// #include "My Directory/My File.txt"
		/// </summary>
		public static bool IsValidIncludeLine(string lineText)
			=> lineText.StartsWith("#include ", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(lineText, "\".*\"");

		public static MatchCollection GetComments(string lineText)
			=> Regex.Matches(lineText, @"\s*;.*$", RegexOptions.Multiline);

		public static string RemoveComments(string lineText)
			=> Regex.Replace(lineText, @"\s*;.*$", string.Empty, RegexOptions.Multiline);

		/// <summary>
		/// Replaces comments with whitespace to maintain the original string length.
		/// </summary>
		public static string EscapeComments(string lineText)
		{
			MatchCollection comments = GetComments(lineText);

			foreach (Match match in comments)
				lineText = Regex.Replace(lineText, Regex.Escape(match.Value), new string(' ', match.Length));

			return lineText;
		}

		public static string RemoveNGStringIndex(string lineText)
			=> Regex.Replace(lineText, @"^\d+:\s*", string.Empty, RegexOptions.Multiline);

		public static bool IsEmptyOrComments(string lineText)
			=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(";");

		public static bool IsSectionHeaderLine(string lineText)
			=> Regex.IsMatch(lineText, @"^\[\b.*\b\]\s*(;.*)?$");

		/// <summary>
		/// Input: "[Options] ; Options section"<br />
		/// Output: "Options"
		/// </summary>
		public static string GetSectionHeaderText(string sectionHeaderLine)
			=> Regex.Match(sectionHeaderLine, @"^\[(\b.*\b)\]\s*(;.*)?$").Groups[1].Value;

		/// <summary>
		/// Checks if the line is in the [Strings], [PCStrings] or [PSXStrings] section.
		/// </summary>
		public static bool IsLineInStandardStringSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = DocumentParser.GetSectionName(document, line.Offset);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("strings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("pcstrings", StringComparison.OrdinalIgnoreCase)
				|| lineSectionName.Equals("psxstrings", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsLineInExtraNGSection(TextDocument document, DocumentLine line)
		{
			string lineSectionName = DocumentParser.GetSectionName(document, line.Offset);

			if (string.IsNullOrEmpty(lineSectionName))
				return false;

			return lineSectionName.Equals("extrang", StringComparison.OrdinalIgnoreCase);
		}
	}
}
