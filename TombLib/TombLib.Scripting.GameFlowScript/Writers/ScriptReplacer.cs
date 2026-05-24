using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers
{
	public static class ScriptReplacer
	{
		public static void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		{
			var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

			TextEditorLineOperations.TryReplaceFirstMatchingLine(textEditor, lineText =>
			{
				if (!regex.IsMatch(lineText))
					return null;

				string scriptLevelName = regex.Replace(LineParser.RemoveComments(lineText), string.Empty).Trim();
				return scriptLevelName == oldName
					? lineText.Replace(oldName, newName)
					: null;
			});
		}

		public static void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
			=> TextEditorLineOperations.TryReplaceFirstMatchingLine(textEditor, lineText =>
			{
				string trimmedLineText = lineText.Trim();
				return trimmedLineText == oldName
					? trimmedLineText.Replace(oldName, newName)
					: null;
			});
	}
}
