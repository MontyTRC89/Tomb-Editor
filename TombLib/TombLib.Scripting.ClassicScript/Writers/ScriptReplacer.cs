using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.ClassicScript.Writers
{
	public class ScriptReplacer
	{
		public static void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		{
			var regex = new Regex(Patterns.NameCommand, RegexOptions.IgnoreCase);

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
				string cleanString = LineParser.RemoveComments(LineParser.RemoveNGStringIndex(lineText)).Trim();
				return cleanString == oldName
					? lineText.Replace(oldName, newName)
					: null;
			});
	}
}
