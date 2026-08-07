using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Diagnostics;

namespace TombLib.Scripting.ClassicScript.Diagnostics;

public class ErrorDetector : IErrorDetector, ITextDiagnosticsProvider
{
	private readonly IClassicScriptLineService _lineService;
	private readonly IClassicScriptCommandService _commandService;
	private readonly ClassicScriptSyntaxCatalogService _syntaxCatalogService;
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	public ErrorDetector(
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		ClassicScriptSyntaxCatalogService syntaxCatalogService)
	{
		_lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		_syntaxCatalogService = syntaxCatalogService ?? throw new ArgumentNullException(nameof(syntaxCatalogService));
	}

	#region Public methods

	public IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
		=> DetectErrorLines(new StringTextSnapshot(editorContent));

	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
		=> DetectErrorLines(new StringTextSnapshot(request.DocumentText));

	#endregion Public methods

	#region Error line finding

	private List<TextEditorDiagnostic> DetectErrorLines(ITextSnapshot source)
	{
		var errorLines = new List<TextEditorDiagnostic>();

		bool commandSectionCheckRequired = _commandService.DocumentContainsSections(source);

		foreach (ITextLine processedLine in source.Lines)
		{
			string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);

			if (_lineService.IsEmptyOrComments(processedLineText))
				continue;

			TextEditorDiagnostic? error = FindErrorsInLine(source, processedLine, processedLineText, commandSectionCheckRequired);

			if (error is not null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private TextEditorDiagnostic? FindErrorsInLine(ITextSnapshot source, ITextLine line, string lineText, bool commandSectionCheckRequired)
	{
		if (_lineService.IsSectionHeaderLine(lineText))
			return FindErrorsInSectionHeaderLine(source, line, lineText);
		else
		{
			if (commandSectionCheckRequired && IsLineInStandardStringSection(source, line))
				return null;
			else if (commandSectionCheckRequired && IsLineInExtraNGSection(source, line))
				return FindErrorsInNGStringLine(source, line, lineText);
			else
				return FindErrorsInCommandLine(source, line, lineText, commandSectionCheckRequired);
		}
	}

	private TextEditorDiagnostic? FindErrorsInSectionHeaderLine(ITextSnapshot source, ITextLine line, string lineText)
	{
		if (!IsValidSectionName(lineText))
			return CreateDiagnostic(source, line,
				"Invalid section name. Please check its spelling.", _lineService.RemoveComments(lineText));

		return null;
	}

	private TextEditorDiagnostic FindErrorsInNGStringLine(ITextSnapshot source, ITextLine line, string lineText)
	{
		if (!IsNGStringLineWellFormatted(lineText))
			return CreateDiagnostic(source, line,
				"NG string must start with an index.\n\nExample:\n0: First String\n1: Second String",
				_lineService.RemoveComments(lineText));

		return null;
	}

	private TextEditorDiagnostic? FindErrorsInCommandLine(ITextSnapshot source, ITextLine line, string lineText, bool commandSectionCheckRequired)
	{
		string? commandKey = _commandService.GetCommandKey(source, line.Offset);

		if (!IsValidCommandKey(commandKey))
		{
			string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), "^.*=").Value.TrimEnd();

			if (errorSegmentText.Length == 0 && commandKey is not null)
				return null;

			if (commandKey is null)
				errorSegmentText = lineText.TrimEnd();

			return CreateDiagnostic(source, line,
				"Invalid command. Please check its spelling.", errorSegmentText);
		}

		if (commandSectionCheckRequired && !IsCommandLineInCorrectSection(source, line.LineNumber, commandKey))
			return CreateDiagnostic(source, line,
				"Command is placed in the wrong section. Please check the command syntax.",
				_lineService.RemoveComments(lineText));

		if (ContainsBrokenNextLines(source, line.Offset))
		{
			string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

			if (errorSegmentText.Length == 0)
				errorSegmentText = _lineService.RemoveComments(lineText);

			return CreateDiagnostic(source, line,
				"Misplaced \">\" symbols were found.\nYou can only use these symbols at the end of the line and there can only be one on each line.",
				errorSegmentText);
		}

		if (!IsArgumentCountValid(source, line.Offset))
		{
			string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

			if (errorSegmentText.Length == 0)
				errorSegmentText = _lineService.RemoveComments(lineText);

			return CreateDiagnostic(source, line,
				"Invalid argument count. Please check the command syntax.", errorSegmentText);
		}

		if (ContainsEmptyArguments(source, line.Offset))
		{
			string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

			if (errorSegmentText.Length == 0)
				errorSegmentText = _lineService.RemoveComments(lineText);

			return CreateDiagnostic(source, line, "Empty arguments were found.", errorSegmentText);
		}

		return null;
	}

	private static TextEditorDiagnostic CreateDiagnostic(ITextSnapshot source, ITextLine line, string message, string errorSegmentText)
	{
		string lineText = source.GetText(line.Offset, line.Length);
		string segmentText = string.IsNullOrWhiteSpace(errorSegmentText)
			? lineText.Trim()
			: errorSegmentText;

		int startOffset = line.Offset;
		int endOffset = Math.Max(line.Offset + 1, line.EndOffset);

		if (!string.IsNullOrWhiteSpace(segmentText))
		{
			int matchIndex = lineText.IndexOf(segmentText, StringComparison.Ordinal);

			if (matchIndex >= 0)
			{
				startOffset = line.Offset + matchIndex;
				endOffset = startOffset + segmentText.Length;
			}
		}

		return new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, message, startOffset, endOffset);
	}

	#endregion Error line finding

	#region Error detection methods

	private bool IsValidSectionName(string sectionHeaderLineText)
	{
		string section = sectionHeaderLineText.Split('[')[1].Split(']')[0];

		foreach (string entry in _commandCatalogService.Sections)
			if (section.Equals(entry, StringComparison.OrdinalIgnoreCase))
				return true;

		return false;
	}

	private static bool IsNGStringLineWellFormatted(string lineText)
		=> Regex.IsMatch(lineText, @"^\d*:.*");

	private bool IsValidCommandKey(string? commandKey)
	{
		return _syntaxCatalogService.GetCommandDefinition(commandKey) is not null;
	}

	private bool IsLineInStandardStringSection(ITextSnapshot source, ITextLine line)
	{
		string? sectionName = _commandService.GetCurrentSectionName(source, line.Offset);
		return _lineService.IsStandardStringSectionName(sectionName);
	}

	private bool IsLineInExtraNGSection(ITextSnapshot source, ITextLine line)
	{
		string? sectionName = _commandService.GetCurrentSectionName(source, line.Offset);
		return _lineService.IsExtraNGSectionName(sectionName);
	}

	private bool IsCommandLineInCorrectSection(ITextSnapshot source, int lineNumber, string command)
	{
		ClassicScriptSyntaxDefinition? definition = _syntaxCatalogService.GetCommandDefinition(command);
		string correctSection = definition?.ApplicableSection ?? string.Empty;

		if (string.IsNullOrWhiteSpace(correctSection) || correctSection.Equals("any", StringComparison.OrdinalIgnoreCase))
			return true;

		for (int i = lineNumber - 1; i > 0; i--)
		{
			ITextLine currentLine = source.GetLineByNumber(i);
			string currentLineText = source.GetText(currentLine.Offset, currentLine.Length);

			if (currentLineText.TrimStart().StartsWith("["))
			{
				if (correctSection.Equals("level", StringComparison.OrdinalIgnoreCase))
				{
					if (Regex.IsMatch(currentLineText, @"\[(level|title)\]", RegexOptions.IgnoreCase))
						return true;
					else
						return false;
				}
				else if (Regex.IsMatch(currentLineText, @"\[" + correctSection + @"\]", RegexOptions.IgnoreCase))
					return true;
				else
					return false;
			}
		}

		return false;
	}

	private bool ContainsBrokenNextLines(ITextSnapshot source, int lineOffset)
	{
		int? startingLineNumber = _commandService.GetCommandStartLine(source, lineOffset);

		if (startingLineNumber is null)
			return false;

		string nextLineText;

		int i = startingLineNumber.Value;

		do
		{
			if (i > source.LineCount)
				break;

			ITextLine nextLine = source.GetLineByNumber(i);
			nextLineText = _lineService.EscapeComments(source.GetText(nextLine.Offset, nextLine.Length));

			if ((nextLineText.Contains('>') && !ContinuationHelper.IsValidContinuation(nextLineText, ";", '>')) || nextLineText.Count(c => c == '>') > 1)
				return true;

			i++;
		}
		while (ContinuationHelper.IsValidContinuation(nextLineText, ";", '>'));

		return false;
	}

	private bool IsArgumentCountValid(ITextSnapshot source, int lineOffset)
	{
		string? lineText = _commandService.GetWholeCommandLineText(source, lineOffset);

		if (lineText is null)
			return false;

		if (lineText.TrimStart().StartsWith("#"))
			return true;

		lineText = _lineService.EscapeComments(lineText);

		if (!lineText.Contains('='))
			return false;

		string? command = _commandService.GetCommandKey(source, lineOffset);

		if (string.IsNullOrEmpty(command))
			return false;

		if (command.Equals("Legend", StringComparison.OrdinalIgnoreCase)) // "Legend=" ignores commas
			return true;

		int argumentCount = _lineService.EscapeComments(lineText).Split('=')[1].Split(',').Length;

		if (argumentCount == 1 && string.IsNullOrWhiteSpace(lineText.Split('=')[1]))
			argumentCount = 0;

		ClassicScriptSyntaxDefinition? definition = _syntaxCatalogService.GetCommandDefinition(command);

		if (definition is null)
			return false;

		if (definition.HasArrayArguments)
			return true; // Whatever.

		return argumentCount == definition.ArgumentCount;
	}

	private bool ContainsEmptyArguments(ITextSnapshot source, int lineOffset)
	{
		string? lineText = _commandService.GetWholeCommandLineText(source, lineOffset);

		if (string.IsNullOrEmpty(lineText))
			return true;

		string[] arguments = _lineService.EscapeComments(lineText).Split(',');

		foreach (string argument in arguments)
			if (string.IsNullOrWhiteSpace(argument.Replace('>', ' ')))
				return true;

		return false;
	}

	#endregion Error detection methods
}
