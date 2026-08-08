using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.ClassicScript.Writers;

/// <summary>
/// Performs script-wide renames inside an open ClassicScript editor.
/// </summary>
public sealed class ScriptReplacer
{
	private static readonly Regex NameCommandRegex = new Regex(@"^\s*\bName\s*=\s*", RegexOptions.IgnoreCase);

	private readonly IClassicScriptLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ScriptReplacer"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to clean lines before matching.</param>
	public ScriptReplacer(IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <summary>
	/// Renames a level script reference in the editor.
	/// </summary>
	/// <param name="textEditor">The editor to update.</param>
	/// <param name="oldName">The current level script name.</param>
	/// <param name="newName">The new level script name.</param>
	public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
		=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
			textEditor,
			NameCommandRegex,
			(lineText, regex) => regex.Replace(_lineService.RemoveComments(lineText), string.Empty).Trim(),
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
			string cleanString = _lineService.RemoveComments(_lineService.RemoveNGStringIndex(lineText)).Trim();
			return cleanString == oldName
				? lineText.Replace(oldName, newName)
				: null;
		});
}
