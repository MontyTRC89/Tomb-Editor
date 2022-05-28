using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;

namespace TombLib.Scripting.GameFlowScript.Writers
{
	public static class LanguageStringWriter
	{
		public static void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		{
			AssignStockLevelNameStringSlot(textEditor, levelName);
		}

		private static bool AssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
		{
			foreach (DocumentLine line in textEditor.Document.Lines)
			{
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);

				if (Regex.IsMatch(lineText, @"EMPTY\sSTRING\sSLOT\s\d+"))
				{
					textEditor.Select(line.Offset, line.Length);
					textEditor.SelectedText = levelName;

					return true;
				}
			}

			return false;
		}
	}
}
