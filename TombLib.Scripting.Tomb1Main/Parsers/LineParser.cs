using System.Text.RegularExpressions;

namespace TombLib.Scripting.Tomb1Main.Parsers
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
	}
}
