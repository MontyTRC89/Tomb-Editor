using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Scripting.ClassicScript.Writers
{
	public class ScriptReplacer
	{
		private static readonly Regex _nameCommandRegex = new Regex(@"^\s*\bName\s*=\s*", RegexOptions.IgnoreCase);

		private readonly IClassicScriptLineService _lineService;

		public ScriptReplacer(IClassicScriptLineService lineService)
		{
			_lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
		}

		public void RenameLevelScript(TextEditorBase textEditor, string oldName, string newName)
			=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
				textEditor,
				_nameCommandRegex,
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
}
