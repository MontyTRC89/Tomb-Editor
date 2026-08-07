using System.Text.RegularExpressions;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.TRX.Writers;

/// <summary>
/// Applies script-wide renames to TRX documents.
/// </summary>
public class ScriptReplacer
{
	private static readonly Regex LevelPropertyRegex = TRXLevelNameParser.LevelPropertyRegex;

	/// <summary>
	/// Renames a level script in the given editor by replacing the matching title property value.
	/// </summary>
	/// <param name="textEditor">The editor containing the level script.</param>
	/// <param name="oldName">The current level name.</param>
	/// <param name="newName">The new level name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
			textEditor,
			LevelPropertyRegex,
			(lineText, _) => TRXLevelNameParser.ExtractTitleName(LineCommentHelper.RemoveLineComment(lineText, "//")),
			oldName,
			newName);
}
