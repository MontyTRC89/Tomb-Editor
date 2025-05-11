using ICSharpCode.AvalonEdit.Document;
using ScriptLib.Core;
using ScriptLib.Core.Extensions;
using ScriptLib.IniScript.Structs;
using Shared.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	private static readonly JsonHintLoader CustHints = JsonHintLoader.Load("ScriptLib.IniScript.Resources.CustHints.json");
	private static readonly JsonHintLoader ParamHints = JsonHintLoader.Load("ScriptLib.IniScript.Resources.ParamHints.json");
	private static readonly JsonHintLoader NewCommandHints = JsonHintLoader.Load("ScriptLib.IniScript.Resources.NewCommandHints.json");
	private static readonly JsonHintLoader OldCommandHints = JsonHintLoader.Load("ScriptLib.IniScript.Resources.OldCommandHints.json");

	#region public

	/// <summary>
	/// Gets the syntax description for the command at the specified offset in the document.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>The syntax description for the command if found; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This method handles both regular commands and subcommands (commands with special behavior based on their first parameter).<br />
	/// It first checks if the offset points to a valid subcommand, and if so, retrieves its specific syntax.<br />
	/// If not, or if no specific syntax is found, it falls back to the standard command syntax.
	/// </remarks>
	public static string? GetSyntaxForCommandAtOffset(TextDocument document, int offset)
	{
		string? fullCommandText = GetFullCommandTextFromOffset(document, offset, out int commandStartOffset);

		if (fullCommandText is null || commandStartOffset == -1)
			return null;

		if (IsOffsetAtValidSubcommand(document, offset, out string? commandName, out string? firstParam))
		{
			string? syntax = GetSubcommandSyntax(commandName, firstParam);

			if (syntax is not null)
				return syntax;
		}

		if (commandName is null)
		{
			string commandPart = fullCommandText.Split('=')[0].Trim(); // `=` is guaranteed to be present here
			commandName = GetCorrectCommandVariation(document, offset, commandPart);
		}

		return OldCommandHints.GetHint(commandName, StringComparison.OrdinalIgnoreCase)
			?? NewCommandHints.GetHint(commandName, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Gets the full command text from the specified offset in the document.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>The full command text if found; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This is a convenience method that calls <see cref="GetFullCommandTextFromOffset(TextDocument, int, out int)"/>
	/// and discards the command start offset information.
	/// </remarks>
	public static string? GetFullCommandTextFromOffset(TextDocument document, int offset)
		=> GetFullCommandTextFromOffset(document, offset, out _);

	/// <summary>
	/// Gets the full command text from the specified offset in the document and returns the start offset of the command.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <param name="commandStartOffset">When this method returns, contains the offset where the command starts if found; otherwise, -1.</param>
	/// <returns>The full command text if found; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This method handles both single-line and multi-line commands. For multi-line commands,
	/// it combines all the relevant lines into a single string.
	/// </remarks>
	public static string? GetFullCommandTextFromOffset(TextDocument document, int offset, out int commandStartOffset)
	{
		commandStartOffset = -1;
		DocumentLine? commandStartLine = GetCommandStartLineFromOffset(document, offset, out bool isMultiline);

		if (commandStartLine is null)
			return null;

		commandStartOffset = commandStartLine.Offset;
		string lineText = document.GetText(commandStartLine);

		if (!isMultiline)
			return lineText;

		DocumentLine[] linesToMerge = GetLinesToMerge(document, commandStartLine);
		return MergeLines(document, linesToMerge);
	}

	/// <summary>
	/// Gets the line where a command starts based on the specified offset.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <param name="isMultiline">When this method returns, indicates whether the command is a multi-line command.</param>
	/// <returns>The <see cref="DocumentLine"/> where the command starts if found; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This method identifies the start line of a command based on specific syntax rules:<br />
	/// - For single-line commands, it returns the current line if it contains an equals sign.<br />
	/// - For multi-line commands, it searches backward to find the beginning of the command.<br />
	/// - It ignores empty lines and comments.
	/// </remarks>
	public static DocumentLine? GetCommandStartLineFromOffset(TextDocument document, int offset, out bool isMultiline)
	{
		isMultiline = false;
		DocumentLine? lineAtOffset = document.TryGetLineByOffset(offset);

		if (lineAtOffset is null)
			return null;

		string lineText = document.GetText(lineAtOffset);

		if (IsEmptyOrComments(lineText, out string? trimmedLine))
			return null;

		string trimmedLineWithoutComments = RemoveComments(trimmedLine);

		if (trimmedLineWithoutComments.StartsWith('['))
			return null; // This is a section header line

		bool isCommandStartLine = trimmedLineWithoutComments.Contains('=');

		if (isCommandStartLine)
		{
			isMultiline = trimmedLineWithoutComments.EndsWith('>');
			return lineAtOffset;
		}

		DocumentLine? commandStartLine = FindMultilineCommandStartLine(document, lineAtOffset);

		if (commandStartLine is null)
			return null; // This is not a command line

		isMultiline = true;
		return commandStartLine;
	}

	/// <summary>
	/// Gets the command name from the specified offset in the document.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>The command name if found; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This method determines the command name by finding the text before the equals sign
	/// in the command line. It also handles command variations based on the current section context.
	/// </remarks>
	public static string? GetCommandNameFromOffset(TextDocument document, int offset)
	{
		DocumentLine? commandStartLine = GetCommandStartLineFromOffset(document, offset, out _);

		if (commandStartLine is null)
			return null;

		string commandStartLineText = document.GetText(commandStartLine);
		// No need to remove comments here, the line is guaranteed to have a `=` in the correct location
		string commandKey = commandStartLineText.Split('=')[0].Trim();

		return GetCorrectCommandVariation(document, commandStartLine.Offset, commandKey);
	}

	/// <summary>
	/// Gets the full file path for an include directive at the specified offset.
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>The full path to the included file if valid; otherwise, <see langword="null"/>.</returns>
	/// <remarks>
	/// This method resolves include paths relative to the directory of the current document.
	/// It validates that the line at the specified offset contains a proper include directive.
	/// </remarks>
	public static string? GetFullIncludePath(TextDocument document, int offset)
	{
		string? rootPath = Path.GetDirectoryName(document.FileName);

		if (rootPath is null)
			return null;

		DocumentLine lineAtOffset = document.GetLineByOffset(offset);
		string lineText = document.GetText(lineAtOffset);

		return IsValidIncludeLine(lineText, out string? fileName)
			? Path.Combine(rootPath, fileName)
			: null;
	}

	/// <summary>
	/// Determines if the offset is positioned at a valid subcommand (a command whose syntax changes based on its first parameter).
	/// </summary>
	/// <param name="document">The text document being analyzed.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <param name="commandName">When this method returns, contains the normalized command name if at a valid subcommand; otherwise, <see langword="null"/>.</param>
	/// <param name="firstParam">When this method returns, contains the first parameter of the command if at a valid subcommand; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the offset is positioned at a valid subcommand; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// A subcommand is considered valid when it contains at least two parameters (separated by a comma).
	/// The command name will be processed through <see cref="GetCorrectCommandVariation"/> to match the appropriate syntax definition.
	/// </remarks>
	public static bool IsOffsetAtValidSubcommand(TextDocument document, int offset,
		[NotNullWhen(true)] out string? commandName, [NotNullWhen(true)] out string? firstParam)
	{
		commandName = null;
		firstParam = null;

		string? fullCommandText = GetFullCommandTextFromOffset(document, offset, out int commandStartOffset);

		if (fullCommandText is null)
			return false;

		int equalsIndex = fullCommandText.IndexOf('=');
		string commandPart = fullCommandText[..equalsIndex]);
		string valuePart = fullCommandText[(equalsIndex + 1)..];
		int commaIndex = valuePart.IndexOf(',');

		if (commaIndex == -1)
			return false; // Second argument needs to be at least started, otherwise the command is not a subcommand

		commandName = GetCorrectCommandVariation(document, offset, commandPart.Trim());
		firstParam = valuePart[..commaIndex].Trim();

		return true;
	}

	#endregion public

	#region private

	private static string? GetSubcommandSyntax(string commandName, string firstArg)
	{
		JsonHintLoader? resourceSet = commandName.ToUpper() switch
		{
			"CUSTOMIZE" => CustHints,
			"PARAMETER" => ParamHints,
			_ => null
		};

		if (resourceSet is null)
			return null; // No syntax found for this command

		string? custParamSyntax = FindCustParamSyntaxByKey(resourceSet, firstArg);

		if (custParamSyntax is not null)
			return custParamSyntax;

		return NewCommandHints.GetHint(commandName, StringComparison.OrdinalIgnoreCase);
	}

	private static string? FindCustParamSyntaxByKey(JsonHintLoader resourceSet, string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return null;

		// Search in the given ResourceSet
		string? result = resourceSet.GetHint(key, StringComparison.OrdinalIgnoreCase);

		if (!string.IsNullOrEmpty(result))
			return result;

		// Search in PluginMnemonics
		foreach (PluginConstant pluginMnemonic in MnemonicData.PluginConstants)
		{
			if (!pluginMnemonic.FlagName.EqualsIgnoreCase(key))
				continue;

			string description = pluginMnemonic.Description;
			int syntaxIndex = description.IndexOfIgnoreCase("syntax:");

			if (syntaxIndex < 0)
				continue;

			// Skip "syntax:"
			string syntaxPart = description[(syntaxIndex + 7)..];

			// Remove carriage returns and get the first line
			syntaxPart = syntaxPart.Replace("\r", string.Empty);
			int lineBreakIndex = syntaxPart.IndexOf('\n');

			if (lineBreakIndex >= 0)
				syntaxPart = syntaxPart[..lineBreakIndex];

			return syntaxPart.Trim();
		}

		return null;
	}

	private static string GetCorrectCommandVariation(TextDocument document, int offset, string command)
	{
		string? sectionName = GetSectionNameAtOffset(document, offset);

		return command.ToUpper() switch
		{
			"LEVEL" => GetCommandVariationForSection(sectionName, "Level", "Level"),
			"CUT" => GetCommandVariationForSection(sectionName, "Cut", string.Empty),
			"FMV" => GetCommandVariationForSection(sectionName, "FMV", "Level"),
			_ => command
		};
	}

	private static string GetCommandVariationForSection(string? sectionName, string baseCommand, string defaultSuffix)
	{
		if (string.IsNullOrEmpty(sectionName))
			return baseCommand + defaultSuffix;

		return sectionName.ToUpper() switch
		{
			"PCEXTENSIONS" => baseCommand + "PC",
			"PSXEXTENSIONS" => baseCommand + "PSX",
			_ => baseCommand + defaultSuffix
		};
	}

	private static DocumentLine? FindMultilineCommandStartLine(TextDocument document, DocumentLine searchStartingLine)
	{
		for (int i = searchStartingLine.LineNumber - 1; i > 0; i--)
		{
			DocumentLine line = document.GetLineByNumber(i);
			string lineText = document.GetText(line);
			lineText = RemoveComments(lineText, trimTrailingWhitespace: true);

			bool endsWithNextLineChar = lineText.EndsWith('>');
			bool isCommandStartLine = lineText.Contains('=');

			if (endsWithNextLineChar && isCommandStartLine)
				return line;

			if (!endsWithNextLineChar)
				break; // End of multiline command
		}

		return null;
	}

	private static DocumentLine[] GetLinesToMerge(TextDocument document, DocumentLine startingLine)
	{
		var linesToMerge = new List<DocumentLine> { startingLine };

		for (int i = startingLine.LineNumber + 1; i < document.LineCount; i++)
		{
			DocumentLine line = document.GetLineByNumber(i);
			linesToMerge.Add(line);

			string lineText = document.GetText(line);

			if (IsEmptyOrComments(lineText, out string? trimmedLine))
				continue;

			string trimmedLineWithoutComments = RemoveComments(trimmedLine, trimTrailingWhitespace: true);

			if (!trimmedLineWithoutComments.EndsWith('>'))
				break; // End of multiline command
		}

		return [.. linesToMerge];
	}

	private static string MergeLines(TextDocument document, IEnumerable<DocumentLine> lines)
	{
		var builder = new StringBuilder();

		foreach (DocumentLine line in lines)
		{
			string lineText = document.GetText(line);
			lineText = EscapeCommentsAndNewLines(lineText);
			builder.Append(lineText);
		}

		return builder.ToString();
	}

	#endregion private
}
