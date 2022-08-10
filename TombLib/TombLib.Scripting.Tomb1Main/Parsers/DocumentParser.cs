using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Parsers
{
	public static class DocumentParser
	{
		public static bool IsLevelScriptDefined(TextDocument document, string levelName)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);
				var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

				if (regex.IsMatch(lineText))
				{
					string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim().TrimEnd(',').Trim('"');

					if (scriptLevelName == levelName)
						return true;
				}
			}

			return false;
		}

		public static DocumentLine FindDocumentLineOfLevel(TextDocument document, string levelName)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				if (Regex.Replace(lineText, Patterns.LevelProperty, string.Empty, RegexOptions.IgnoreCase).Trim().TrimEnd(',').Trim('"').StartsWith(levelName))
					return line;
			}

			return null;
		}
	}
}
