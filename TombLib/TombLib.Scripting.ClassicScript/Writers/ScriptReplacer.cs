using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombLib.Scripting.ClassicScript.Writers
{
	public class ScriptReplacer
	{
		public static void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		{
			foreach (DocumentLine line in textEditor.Document.Lines)
			{
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);
				var regex = new Regex(Patterns.NameCommand, RegexOptions.IgnoreCase);

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
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);
				string cleanString = LineParser.RemoveComments(LineParser.RemoveNGStringIndex(lineText)).Trim();

				if (cleanString == oldName)
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
