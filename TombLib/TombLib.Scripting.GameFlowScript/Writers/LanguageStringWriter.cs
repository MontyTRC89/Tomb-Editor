using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers;

/// <summary>
/// Writes GameFlow language string entries into an open editor.
/// </summary>
public static class LanguageStringWriter
{
	/// <summary>
	/// Writes a new level name string for the given level name.
	/// </summary>
	/// <param name="textEditor">The editor to write into.</param>
	/// <param name="levelName">The level name to write.</param>
	public static void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		=> TextEditorLineOperations.TryAssignStockLevelNameStringSlot(textEditor, levelName);
}
