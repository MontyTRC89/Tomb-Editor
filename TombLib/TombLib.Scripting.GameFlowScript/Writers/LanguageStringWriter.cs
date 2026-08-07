using System.Text.RegularExpressions;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.GameFlowScript.Writers;

public static class LanguageStringWriter
{
	public static void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		=> TextEditorLineOperations.TryAssignStockLevelNameStringSlot(textEditor, levelName);
}
