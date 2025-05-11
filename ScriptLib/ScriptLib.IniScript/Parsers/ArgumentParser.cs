using ICSharpCode.AvalonEdit.Document;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	/// <summary>
	/// Determines the index of the argument at the specified offset within a command.
	/// </summary>
	/// <param name="document">The text document containing the command.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The zero-based index of the argument at the specified offset, or -1 if not within a valid command.
	/// If the offset is beyond the command text length, returns the index of the last argument.
	/// </returns>
	public static int GetArgumentIndexAtOffset(TextDocument document, int offset)
	{
		string? fullCommandText = GetFullCommandTextFromOffset(document, offset, out int commandStartOffset);

		if (fullCommandText is null)
			return -1;

		int relativeOffset = offset - commandStartOffset;

		// Handle case where offset is beyond text length
		if (relativeOffset >= fullCommandText.Length)
			return fullCommandText.Split(',').Length - 1;

		// Count arguments before cursor
		int argumentIndex = 0;
		int commaPos = -1;

		while ((commaPos = fullCommandText.IndexOf(',', commaPos + 1)) >= 0 && commaPos < relativeOffset)
			argumentIndex++;

		return argumentIndex;
	}

	/// <summary>
	/// Retrieves the value of an argument at the specified index from the command at the given offset.
	/// </summary>
	/// <param name="document">The text document containing the command.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <param name="argumentIndex">The zero-based index of the argument to retrieve.</param>
	/// <returns>
	/// The trimmed string value of the specified argument, or <see langword="null" /> if the command or argument doesn't exist.
	/// </returns>
	public static string? GetArgumentValueFromCommandAtOffset(TextDocument document, int offset, int argumentIndex)
	{
		string? fullCommandText = GetFullCommandTextFromOffset(document, offset);
		return fullCommandText?.Split(',').ElementAtOrDefault(argumentIndex)?.Trim();
	}

	/// <summary>
	/// Retrieves the flag prefix of the argument at the specified offset within a command.
	/// </summary>
	/// <param name="document">The text document containing the command.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The flag prefix string if the argument at the offset is a flagged argument,
	/// or <see langword="null"/> if the argument is not flagged, the command doesn't exist,
	/// or the offset is not within a valid command argument.
	/// </returns>
	/// <remarks>
	/// A flagged argument is one that has syntax in the form "(CUST_...)".
	/// This method extracts the "prefix" portion from such arguments - in this case "CUST".
	/// </remarks>
	public static string? GetFlagPrefixOfArgumentAtOffset(TextDocument document, int offset)
	{
		// Get the index of the current argument
		int currentArgumentIndex = GetArgumentIndexAtOffset(document, offset);

		if (currentArgumentIndex == -1)
			return null;

		// Get the command syntax
		string? syntax = GetSyntaxForCommandAtOffset(document, offset);

		if (string.IsNullOrEmpty(syntax))
			return null;

		// Split the syntax into arguments
		string[] syntaxArguments = syntax.Split(',');

		// Ensure the current argument index is valid
		if (currentArgumentIndex >= syntaxArguments.Length)
			return null;

		string currentSyntaxArgument = syntaxArguments[currentArgumentIndex];

		// Check if this is a flagged argument
		// Must contain a '(' and '.' but not be an array type
		if (!currentSyntaxArgument.Contains('(') || !currentSyntaxArgument.Contains('.')
			|| currentSyntaxArgument.Contains("(*Array*)", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		// Extract the flag prefix by:
		// 1. Split by '.' and take the first part
		// 2. Split that by '(' and take the second part (after the opening parenthesis)
		string[] dotParts = currentSyntaxArgument.Split('.');

		if (dotParts.Length == 0)
			return null;

		string[] bracketParts = dotParts[0].Split('(');

		if (bracketParts.Length < 2)
			return null;

		return bracketParts[1];
	}
}
