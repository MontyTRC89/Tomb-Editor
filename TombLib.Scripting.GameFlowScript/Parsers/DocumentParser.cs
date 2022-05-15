using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Enums;
using TombLib.Scripting.GameFlowScript.Resources;

namespace TombLib.Scripting.GameFlowScript.Parsers
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
					string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim();

					if (scriptLevelName == levelName)
						return true;
				}
			}

			return false;
		}

		public static DocumentLine FindDocumentLineOfObject(TextDocument document, string objectName, ObjectType type)
		{
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				switch (type)
				{
					case ObjectType.Section:
						if (lineText.StartsWith(objectName))
							return line;
						break;

					case ObjectType.Level:
						if (Regex.Replace(lineText, Patterns.LevelProperty, string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
							return line;
						break;
				}
			}

			return null;
		}
	}
}
