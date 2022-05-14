using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Writers
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
					string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim().Trim('"');

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
	}
}
