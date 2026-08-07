using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.ClassicScript.Writers;

public class ScriptReplacer
{
	private static readonly Regex NameCommandRegex = new Regex(@"^\s*\bName\s*=\s*", RegexOptions.IgnoreCase);

	private readonly IClassicScriptLineService _lineService;

	public ScriptReplacer(IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
			textEditor,
			NameCommandRegex,
			(lineText, regex) => regex.Replace(_lineService.RemoveComments(lineText), string.Empty).Trim(),
			oldName,
			newName);

	public void RenameLanguageString(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(textEditor, lineText =>
		{
			string cleanString = _lineService.RemoveComments(_lineService.RemoveNGStringIndex(lineText)).Trim();
			return cleanString == oldName
				? lineText.Replace(oldName, newName)
				: null;
		});
}
