using ICSharpCode.AvalonEdit.Document;
using ScriptLib.Core.Extensions;
using Shared.Extensions;

namespace ScriptLib.IniScript.Parsers;

public static partial class IniScriptParser
{
	//public static DocumentLine FindDocumentLineOfObject(TextDocument document, string objectName, ObjectType type)
	//{
	//	foreach (DocumentLine line in document.Lines)
	//	{
	//		string lineText = document.GetText(line.Offset, line.Length);

	//		switch (type)
	//		{
	//			case ObjectType.Section:
	//				if (lineText.TrimStart().StartsWith(objectName))
	//					return line;
	//				break;

	//			case ObjectType.Level:
	//				if (Regex.Replace(lineText, Patterns.NameCommand, string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
	//					return line;
	//				break;

	//			case ObjectType.Include:
	//				if (Regex.Replace(lineText, Patterns.IncludeCommand, string.Empty, RegexOptions.IgnoreCase).TrimStart('"').StartsWith(objectName))
	//					return line;
	//				break;

	//			case ObjectType.Define:
	//				if (Regex.Replace(lineText, Patterns.DefineCommand, string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
	//					return line;
	//				break;
	//		}
	//	}

	//	return null;
	//}

	#region public

	/// <summary>
	/// Determines whether the document contains any section headers.
	/// </summary>
	/// <param name="document">The document to check for sections.</param>
	/// <returns>
	/// <see langword="true"/> if the document contains at least one section header;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public static bool DocumentContainsSections(TextDocument document)
	{
		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line);

			if (IsSectionHeaderLine(lineText, out _))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Counts the number of sections in the document.
	/// </summary>
	/// <param name="document">The document to count sections in.</param>
	/// <returns>The total number of section headers found in the document.</returns>
	public static int GetSectionsCount(TextDocument document)
	{
		int sectionsCount = 0;

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line);

			if (IsSectionHeaderLine(lineText, out _))
				sectionsCount++;
		}

		return sectionsCount;
	}

	/// <summary>
	/// Gets the name of the section that contains the specified offset.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The name of the section containing the specified offset, or <see langword="null"/>
	/// if no section contains the offset or the offset is invalid.
	/// </returns>
	public static string? GetSectionNameAtOffset(TextDocument document, int offset)
	{
		DocumentLine? lineAtOffset = document.TryGetLineByOffset(offset);

		if (lineAtOffset is null)
			return null;

		for (int i = lineAtOffset.LineNumber; i >= 1; i--)
		{
			DocumentLine currentLine = document.GetLineByNumber(i);
			string currentLineText = document.GetText(currentLine);

			if (IsSectionHeaderLine(currentLineText, out string? sectionName))
				return sectionName;
		}

		return null;
	}

	/// <summary>
	/// Gets the first line of the section that contains the specified offset.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The <see cref="DocumentLine"/> representing the section header line containing the offset,
	/// or <see langword="null"/> if no section contains the offset or the offset is invalid.
	/// </returns>
	public static DocumentLine? GetFirstLineOfSectionAtOffset(TextDocument document, int offset)
	{
		DocumentLine? startLine = document.TryGetLineByOffset(offset);

		if (startLine is null)
			return null;

		for (int i = startLine.LineNumber; i >= 1; i--)
		{
			DocumentLine currentLine = document.GetLineByNumber(i);
			string currentLineText = document.GetText(currentLine);

			if (IsSectionHeaderLine(currentLineText, out _))
				return currentLine;
		}

		return null;
	}

	/// <summary>
	/// Gets the last non-empty line of the section that contains the specified offset.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// The <see cref="DocumentLine"/> representing the last non-empty line of the section containing the offset,
	/// or <see langword="null"/> if no section contains the offset, the section is empty, or the offset is invalid.
	/// </returns>
	public static DocumentLine? GetLastLineOfSectionAtOffset(TextDocument document, int offset)
	{
		DocumentLine? lineAtOffset = document.TryGetLineByOffset(offset);

		if (lineAtOffset is null)
			return null;

		// Find the section end (either the next section header or end of document)
		int endLineNumber = FindSectionEndLineNumber(document, lineAtOffset.LineNumber);

		// Find the last non-empty line in the section
		return FindLastNonEmptyLineInRange(document, 1, endLineNumber);
	}

	/// <summary>
	/// Finds the line containing the header for the specified section name.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="sectionName">The name of the section to find, with or without brackets.</param>
	/// <returns>
	/// The <see cref="DocumentLine"/> representing the section header line with the specified name,
	/// or <see langword="null"/> if no section with that name exists or the section name is invalid.
	/// </returns>
	public static DocumentLine? FindHeaderLineOfSection(TextDocument document, string sectionName)
	{
		if (sectionName.StartsWith('['))
			sectionName = sectionName.TrimStart('[').TrimEnd(']');

		if (string.IsNullOrWhiteSpace(sectionName))
			return null;

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line);

			if (IsSectionHeaderLine(lineText, out string? currentSectionName)
				&& currentSectionName.EqualsIgnoreCase(sectionName))
			{
				return line;
			}
		}

		return null;
	}

	/// <summary>
	/// Determines whether the specified offset is located in a standard string section.
	/// </summary>
	/// <param name="document">The document to check.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// <see langword="true"/> if the line is in a standard string section ("Strings", "PCStrings", or "PSXStrings");
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public static bool IsOffsetInStandardStringSection(TextDocument document, int offset)
	{
		string? sectionName = GetSectionNameAtOffset(document, offset);

		return sectionName is not null
			&& sectionName.EqualsAny(StringComparison.OrdinalIgnoreCase, "Strings", "PCStrings", "PSXStrings");
	}

	/// <summary>
	/// Determines whether the specified offset is located in the "ExtraNG" section.
	/// </summary>
	/// <param name="document">The document to check.</param>
	/// <param name="offset">The character offset within the document.</param>
	/// <returns>
	/// <see langword="true"/> if the line is in the "ExtraNG" section;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public static bool IsOffsetInExtraNGSection(TextDocument document, int offset)
	{
		string? sectionName = GetSectionNameAtOffset(document, offset);

		return sectionName is not null
			&& sectionName.Equals("ExtraNG", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Determines whether a level with the specified name is defined in the document.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="levelName">The name of the level to look for.</param>
	/// <returns>
	/// <see langword="true" /> if a level with the specified name is defined in the document;
	/// otherwise, <see langword="false" />.
	/// </returns>
	/// <remarks>
	/// A level is considered defined when a "Name=" command is found with the specified level name.
	/// </remarks>
	public static bool IsLevelScriptDefined(TextDocument document, string levelName)
	{
		if (string.IsNullOrWhiteSpace(levelName))
			return false;

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line);

			if (IsEmptyOrComments(lineText, out string? trimmedLine))
				continue;

			string spacesRemovedLine = trimmedLine.Replace(" ", "");

			// Check if line starts with Name= (case insensitive)
			if (!spacesRemovedLine.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
				continue;

			string commentsRemovedLine = RemoveComments(lineText, trimTrailingWhitespace: true);
			int equalSignIndex = commentsRemovedLine.IndexOf('=');
			string namePart = commentsRemovedLine[(equalSignIndex + 1)..].TrimStart();

			if (string.IsNullOrEmpty(namePart))
				continue;

			if (levelName == namePart)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Determines whether a plugin with the specified name is defined in the Options section of the document.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="pluginName">The name of the plugin to look for.</param>
	/// <returns>
	/// <see langword="true" /> if a plugin with the specified name is defined in the document's Options section;
	/// otherwise, <see langword="false" />.
	/// </returns>
	/// <remarks>
	/// The method searches for "Plugin" commands in the [Options] section with the format "Plugin= 0, PluginName".
	/// If the Options section doesn't exist, the method returns false.
	/// </remarks>
	public static bool IsPluginDefined(TextDocument document, string pluginName)
	{
		if (string.IsNullOrWhiteSpace(pluginName))
			return false;

		DocumentLine? optionsSectionLine = FindHeaderLineOfSection(document, "Options");

		if (optionsSectionLine is null)
			return false;

		// Find the last line of the Options section
		int optionsSectionStart = optionsSectionLine.LineNumber;
		int optionsSectionEnd = FindSectionEndLineNumber(document, optionsSectionStart);

		// Scan through the Options section looking for Plugin commands
		for (int i = optionsSectionStart; i <= optionsSectionEnd; i++)
		{
			DocumentLine line = document.GetLineByNumber(i);
			string lineText = document.GetText(line);

			if (IsEmptyOrComments(lineText, out _))
				continue;

			string? commandKey = GetCommandNameFromOffset(document, line.Offset);

			if (commandKey is null || !commandKey.EqualsIgnoreCase("Plugin"))
				continue;

			string? fullCommandText = GetFullCommandTextFromOffset(document, line.Offset);

			if (string.IsNullOrEmpty(fullCommandText))
				continue;

			// Extract plugin name from command text (expected format: `Plugin= 0, PluginName`)
			int commaIndex = fullCommandText.IndexOf(',');

			if (commaIndex >= 0 && commaIndex < fullCommandText.Length - 1)
			{
				string definedName = fullCommandText[(commaIndex + 1)..].Trim();

				if (definedName.EqualsIgnoreCase(pluginName))
					return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determines whether a level name is defined as a language string in the document.
	/// </summary>
	/// <param name="document">The document to search in.</param>
	/// <param name="levelName">The level name to look for in language strings.</param>
	/// <returns>
	/// <see langword="true" /> if a language string with the exact level name is found in the document;
	/// otherwise, <see langword="false" />.
	/// </returns>
	public static bool IsLevelLanguageStringDefined(TextDocument document, string levelName)
	{
		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line);
			string stringValue = GetNGStringValue(lineText);

			if (stringValue == levelName)
				return true;
		}

		return false;
	}

	#endregion public

	#region private

	private static int FindSectionEndLineNumber(TextDocument document, int startLineNumber)
	{
		for (int i = startLineNumber + 1; i <= document.LineCount; i++)
		{
			DocumentLine currentLine = document.GetLineByNumber(i);
			string lineText = document.GetText(currentLine);

			if (IsSectionHeaderLine(lineText, out _))
				return i - 1;
		}

		return document.LineCount; // No next section found, use end of document
	}

	private static DocumentLine? FindLastNonEmptyLineInRange(TextDocument document, int startLineNumber, int endLineNumber)
	{
		for (int i = endLineNumber; i >= startLineNumber; i--)
		{
			DocumentLine line = document.GetLineByNumber(i);
			string lineText = document.GetText(line);

			if (!IsEmptyOrComments(lineText))
				return line;
		}

		return null; // No non-empty lines found in this section
	}

	#endregion private
}
