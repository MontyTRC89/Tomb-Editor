using System.Text.RegularExpressions;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.TRX.Writers;

public class ScriptReplacer
{
	private static readonly Regex levelPropertyRegex = new(Patterns.LevelProperty, RegexOptions.IgnoreCase);

	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
			textEditor,
			levelPropertyRegex,
			(lineText, regex) => regex.Replace(LineCommentHelper.RemoveLineComment(lineText, "//"), string.Empty).Trim().Trim(',').Trim('"'),
			oldName,
			newName);
}
