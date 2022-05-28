using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;

namespace TombLib.Scripting.GameFlowScript.Parsers
{
	public static class LineParser
	{
		public static MatchCollection GetComments(string lineText)
			=> Regex.Matches(lineText, @"\s*//.*$", RegexOptions.Multiline);

		public static string RemoveComments(string lineText)
			=> Regex.Replace(lineText, @"\s*//.*$", string.Empty, RegexOptions.Multiline);

		/// <summary>
		/// Replaces comments with whitespace to maintain the original string length.
		/// </summary>
		public static string EscapeComments(string lineText)
		{
			foreach (Match match in GetComments(lineText))
				lineText = Regex.Replace(lineText, Regex.Escape(match.Value), new string(' ', match.Length));

			return lineText;
		}

		public static bool IsEmptyOrComments(string lineText)
			=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith("//");

		public static bool IsSectionHeaderLine(string lineText)
			=> Regex.IsMatch(lineText, Patterns.Sections, RegexOptions.IgnoreCase);

		public static string GetSectionHeaderText(string lineText)
		{
			Match match = Regex.Match(lineText, Patterns.Sections, RegexOptions.IgnoreCase);

			if (match.Success)
				return match.Value.Trim().Trim(':');

			return null;
		}
	}
}
