using System.Diagnostics.CodeAnalysis;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	/// <summary>
	/// Determines whether a line of text is empty or contains only a comment.
	/// </summary>
	/// <param name="lineText">The text line to analyze.</param>
	/// <returns><see langword="true" /> if the line is <see langword="null" />, empty, consists only of whitespace, or starts with a comment character (;).</returns>
	public static bool IsEmptyOrComments(string lineText)
		=> string.IsNullOrWhiteSpace(lineText) || lineText.TrimStart().StartsWith(";");

	/// <summary>
	/// Determines whether a line of text is empty or contains only a comment,
	/// and provides the leading-whitespace-trimmed version of the line.
	/// </summary>
	/// <param name="lineText">The text line to analyze.</param>
	/// <param name="trimmedLine">When this method returns, contains the trimmed line without leading and trailing whitespace,
	/// or <see langword="null" /> if the line is null or empty.</param>
	/// <returns><see langword="true" /> if the line is <see langword="null" />, empty, consists only of whitespace, or starts with a comment character (;).</returns>
	public static bool IsEmptyOrComments(string lineText, [NotNullWhen(false)] out string? trimmedLine)
	{
		trimmedLine = null;

		if (string.IsNullOrWhiteSpace(lineText))
			return true;

		trimmedLine = lineText.Trim();
		return trimmedLine.StartsWith(";");
	}

	/// <summary>
	/// Removes all comments from a line of text by truncating the line at the first comment marker.
	/// </summary>
	/// <param name="lineText">The text line to process.</param>
	/// <param name="trimTrailingWhitespace">If <see langword="true" />, trailing whitespace will be removed from the line.</param>
	/// <returns>The line with comments removed, or the original line if no comments are found.</returns>
	public static string RemoveComments(string lineText, bool trimTrailingWhitespace = false)
	{
		if (string.IsNullOrWhiteSpace(lineText))
			return lineText;

		int commentIndex = lineText.IndexOf(';');

		if (commentIndex < 0)
			return lineText;

		string result = lineText[..commentIndex];

		if (trimTrailingWhitespace)
			result = result.TrimEnd();

		return result;
	}

	/// <summary>
	/// Replaces all comment characters with spaces while preserving the original length and structure of the text.
	/// Comments begin with a semicolon and end at a newline character.
	/// </summary>
	/// <param name="lineText">The text to process.</param>
	/// <returns>The text with comments replaced with spaces, or the original text if no comments are found.</returns>
	public static string EscapeComments(string lineText)
	{
		if (string.IsNullOrWhiteSpace(lineText))
			return lineText;

		int commentIndex = lineText.IndexOf(';');

		if (commentIndex < 0)
			return lineText;

		Span<char> chars = lineText.ToCharArray();
		bool inComment = false;

		for (int i = commentIndex; i < chars.Length; i++)
		{
			if (chars[i] == ';')
				inComment = true;

			if (inComment)
				chars[i] = ' ';

			if (chars[i] is '\n' or '\r')
				inComment = false;
		}

		return new string(chars);
	}

	/// <summary>
	/// Replaces all comments, greater-than symbols, and newline characters with spaces.
	/// Maintains the original string length while neutralizing special characters.
	/// </summary>
	/// <param name="lineText">The text to process.</param>
	/// <returns>The text with comments, greater-than symbols, and newlines replaced with spaces.</returns>
	public static string EscapeCommentsAndNewLines(string lineText)
	{
		if (string.IsNullOrWhiteSpace(lineText))
			return lineText;

		Span<char> chars = lineText.ToCharArray();
		bool inComment = false;

		for (int i = 0; i < chars.Length; i++)
		{
			if (chars[i] == ';')
				inComment = true;

			if (inComment || chars[i] is '>')
				chars[i] = ' ';

			if (chars[i] is '\n' or '\r')
			{
				inComment = false;
				chars[i] = ' ';
			}
		}

		return new string(chars);
	}

	/// <summary>
	/// Determines whether a line of text represents a valid include directive.
	/// </summary>
	/// <remarks>
	/// Correct formatting example:<br />
	/// <c>#include "My Directory/My File.txt"</c>
	/// </remarks>
	/// <param name="lineText">The text line to analyze.</param>
	/// <param name="fileName">When this method returns, contains the extracted file name if the line
	/// is a valid include directive, or <see langword="null"/> if the line is not valid.</param>
	/// <returns><see langword="true"/> if the line is a valid include directive; otherwise, <see langword="false"/>.</returns>
	public static bool IsValidIncludeLine(string lineText, [NotNullWhen(true)] out string? fileName)
	{
		fileName = null;

		if (IsEmptyOrComments(lineText, out string? trimmedLine))
			return false;

		// Check if line starts with #include (case insensitive)
		if (!trimmedLine.StartsWith("#include ", StringComparison.OrdinalIgnoreCase))
			return false;

		string commentsRemovedLine = RemoveComments(trimmedLine);

		// Check if there's a quoted string
		int firstQuote = commentsRemovedLine.IndexOf('"');

		if (firstQuote < 0)
			return false;

		// Check if there's a closing quote after the first one
		int lastQuote = commentsRemovedLine.IndexOf('"', firstQuote + 1);

		if (lastQuote <= firstQuote)
			return false;

		// Extract the file name between the quotes
		string fileNameCandidate = commentsRemovedLine[(firstQuote + 1)..lastQuote].Trim();

		// Check if the file name is not empty
		if (string.IsNullOrWhiteSpace(fileNameCandidate))
			return false;

		fileName = fileNameCandidate;
		return true;
	}

	/// <summary>
	/// Determines whether a line of text represents a valid section header in INI format.
	/// </summary>
	/// <remarks>
	/// Correct formatting example:<br />
	/// <c>[SectionName]</c>
	/// </remarks>
	/// <param name="lineText">The text line to analyze.</param>
	/// <param name="sectionName">When this method returns, contains the section name if the line
	/// is a valid section header, or <see langword="null"/> if the line is not valid.</param>
	/// <returns><see langword="true"/> if the line is a valid section header; otherwise, <see langword="false"/>.</returns>
	public static bool IsSectionHeaderLine(string lineText, [NotNullWhen(true)] out string? sectionName)
	{
		sectionName = null;

		if (IsEmptyOrComments(lineText, out string? trimmedLine))
			return false;

		// Must start with '['
		if (!trimmedLine.StartsWith('['))
			return false;

		string commentsRemovedLine = RemoveComments(trimmedLine, trimTrailingWhitespace: true);

		// Must have at least 3 characters: [X]
		if (commentsRemovedLine.Length < 3)
			return false;

		// Find the closing bracket
		int closingBracketIndex = commentsRemovedLine.IndexOf(']');

		if (closingBracketIndex <= 1) // Must have at least one character between brackets
			return false;

		// Check what's between brackets (must be non-empty)
		string headerContent = trimmedLine[1..closingBracketIndex];

		if (string.IsNullOrWhiteSpace(headerContent))
			return false;

		// Check what comes after the closing bracket
		if (closingBracketIndex < trimmedLine.Length - 1)
		{
			string afterBracket = trimmedLine[(closingBracketIndex + 1)..];

			// Must be empty or only whitespace
			if (!string.IsNullOrWhiteSpace(afterBracket))
				return false;
		}

		sectionName = headerContent.Trim();
		return true;
	}

	/// <summary>
	/// Gets the value of a string in NG string format.
	/// </summary>
	/// <remarks>
	/// For example, transforms:<br />
	/// <c>"123: Hello World ; This is a string"</c><br /><br />
	/// into<br /><br />
	/// <c>"Hello World"</c>.
	/// </remarks>
	/// <param name="lineText">The text line to process.</param>
	/// <returns>The line with the numeric index removed if present, or the original line otherwise.</returns>
	public static string GetNGStringValue(string lineText)
	{
		if (IsEmptyOrComments(lineText, out string? trimmedLine))
			return lineText;

		string commentsRemovedLine = RemoveComments(trimmedLine, trimTrailingWhitespace: true);

		// Check if the line contains a colon
		int colonIndex = commentsRemovedLine.IndexOf(':');

		if (colonIndex < 0)
			return commentsRemovedLine;

		string indexPart = commentsRemovedLine[..colonIndex].Trim();

		// Check if the index part is empty or is not a valid number
		if (string.IsNullOrEmpty(indexPart) || !int.TryParse(indexPart, out _))
			return commentsRemovedLine;

		return commentsRemovedLine[(colonIndex + 1)..].TrimStart();
	}
}
