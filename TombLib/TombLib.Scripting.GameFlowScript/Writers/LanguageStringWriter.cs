using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers
{
	public static class LanguageStringWriter
	{
		public static void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		{
			AssignStockLevelNameStringSlot(textEditor, levelName);
		}

		private static bool AssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
			=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
				textEditor,
				lineText => Regex.IsMatch(lineText, @"EMPTY\sSTRING\sSLOT\s\d+") ? levelName : null,
				scrollToLine: false);
	}
}
