using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Text;

namespace TombLib.Scripting.ClassicScript.Writers
{
	public class LanguageStringWriter
	{
		private readonly IClassicScriptCommandService _commandService;

		public LanguageStringWriter(IClassicScriptCommandService commandService)
		{
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		}

		public void WriteNewLevelNameString(TextEditorBase textEditor, string levelName)
		{
			if (!AssignStockLevelNameStringSlot(textEditor, levelName))
				WriteNewNGString(textEditor, levelName);
		}

		public bool WriteNewNGString(TextEditorBase textEditor, string ngString)
		{
			ITextSnapshot source = new TextDocumentSnapshot(textEditor.Document);

			if (!IsNGStringAlreadyDefined(textEditor.Document, ngString))
			{
				int? extrangSectionStartLineNumber = _commandService.FindDocumentLineOfSection(source, "ExtraNG");

				if (extrangSectionStartLineNumber is null)
					return false;

				ITextLine extrangSectionStartLine = source.GetLineByNumber(extrangSectionStartLineNumber.Value);

				for (int i = textEditor.Document.LineCount; i >= extrangSectionStartLine.LineNumber; i--)
				{
					var line = textEditor.Document.GetLineByNumber(i);
					string lineText = textEditor.Document.GetText(line.Offset, line.Length);

					if (Regex.IsMatch(lineText, @"^\d+:"))
					{
						textEditor.CaretOffset = line.EndOffset;
						int prevNumber = int.Parse(Regex.Replace(lineText, @"^(\d+):.*$", "$1"));

						textEditor.TextArea.PerformTextInput($"{Environment.NewLine}{prevNumber + 1}: {ngString}");

						textEditor.ScrollToLine(i + 1);
						return true;
					}
					else if (i == extrangSectionStartLine.LineNumber)
					{
						textEditor.CaretOffset = line.EndOffset;
						textEditor.TextArea.PerformTextInput($"{Environment.NewLine}0: {ngString}");

						textEditor.ScrollToLine(i + 1);
						return true;
					}
				}
			}

			return false;
		}

		private bool IsNGStringAlreadyDefined(TextDocument document, string ngString)
		{
			var source = new TextDocumentSnapshot(document);
			int? extrangSectionStartLineNumber = _commandService.FindDocumentLineOfSection(source, "ExtraNG");

			if (extrangSectionStartLineNumber is null)
				return true;

			for (int i = extrangSectionStartLineNumber.Value + 1; i < document.LineCount; i++)
			{
				var line = document.GetLineByNumber(i);
				string lineText = document.GetText(line.Offset, line.Length);

				if (Regex.IsMatch(lineText, $@"^\d+:\s*{ngString}\s*(;.*)?$"))
					return true;
			}

			return false;
		}

		private static bool AssignStockLevelNameStringSlot(TextEditorBase textEditor, string levelName)
			=> TextEditorLineOperations.TryReplaceFirstMatchingLine(
				textEditor,
				lineText => Regex.IsMatch(lineText, @"EMPTY\sSTRING\sSLOT\s\d+") ? levelName : null,
				scrollToLine: false);
	}
}
