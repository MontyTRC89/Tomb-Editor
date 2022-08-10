using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.GameFlowScript.Resources;

namespace TombLib.Scripting.GameFlowScript.Writers
{
	public static class ScriptReplacer
	{
		public static void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		{
			foreach (DocumentLine line in textEditor.Document.Lines)
			{
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);
				var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

				if (regex.IsMatch(lineText))
				{
					string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim();

					if (scriptLevelName == oldName)
					{
						lineText = lineText.Replace(oldName, newName);

						textEditor.ReplaceLine(line, lineText, true);
						textEditor.ScrollToLine(line.LineNumber);

						break;
					}
				}
			}
		}

		public static void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
		{
			foreach (DocumentLine line in textEditor.Document.Lines)
			{
				string lineText = textEditor.Document.GetText(line.Offset, line.Length).Trim();

				if (lineText == oldName)
				{
					lineText = lineText.Replace(oldName, newName);

					textEditor.ReplaceLine(line, lineText, true);
					textEditor.ScrollToLine(line.LineNumber);

					break;
				}
			}
		}
	}
}
