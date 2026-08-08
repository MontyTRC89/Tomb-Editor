using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers;

/// <summary>
/// Performs script-wide renames inside an open GameFlow editor.
/// </summary>
public sealed class ScriptReplacer
{
	private static readonly Regex LevelPropertyRegex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

	/// <summary>
	/// Renames a level script reference in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current level script name.</param>
	/// <param name="newName">The new level script name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
			textEditor,
			LevelPropertyRegex,
			(lineText, regex) => regex.Replace(LineCommentHelper.RemoveLineComment(lineText, "//"), string.Empty).Trim(),
			oldName,
			newName);

	/// <summary>
	/// Renames a language string in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current language string name.</param>
	/// <param name="newName">The new language string name.</param>
	public void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(textEditor, lineText =>
		{
			string trimmedLineText = lineText.Trim();
			return trimmedLineText == oldName
				? lineText.Replace(oldName, newName)
				: null;
		});
}
