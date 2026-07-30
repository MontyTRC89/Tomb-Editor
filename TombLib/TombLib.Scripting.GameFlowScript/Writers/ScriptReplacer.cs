using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers
{
	public static class ScriptReplacer
	{
		private static readonly Regex _levelPropertyRegex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

		public static void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
			=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
				textEditor,
				_levelPropertyRegex,
				(lineText, regex) => regex.Replace(LineCommentHelper.RemoveLineComment(lineText, "//"), string.Empty).Trim(),
				oldName,
				newName);

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
