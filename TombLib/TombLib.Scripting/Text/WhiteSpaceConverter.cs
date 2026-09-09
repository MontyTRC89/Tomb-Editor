using System;
using System.Text;

namespace TombLib.Scripting.Text;

/// <summary>
/// Converts between space and tab indentation in text.
/// Conversion operates on indentation only and preserves the original line-ending
/// convention and all non-indentation content.
/// </summary>
public static class WhiteSpaceConverter
{
	/// <summary>
	/// Converts leading space indentation to tabs using the specified tab size.
	/// Only leading whitespace is converted; existing tabs, partial groups of spaces
	/// that do not reach a tab stop, and all non-indentation content are preserved.
	/// </summary>
	/// <param name="input">The text to convert.</param>
	/// <param name="tabSize">The number of spaces per tab stop.</param>
	/// <returns>The converted text.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tabSize"/> is less than or equal to zero.</exception>
	public static string ConvertSpacesToTabs(string input, int tabSize)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tabSize);
		return TransformLines(input, line => ConvertLineIndentationToTabs(line, tabSize));
	}

	/// <summary>
	/// Converts every tab in the text to the number of spaces that reach the next tab stop.
	/// </summary>
	/// <param name="input">The text to convert.</param>
	/// <param name="tabSize">The number of spaces per tab stop.</param>
	/// <returns>The converted text.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="tabSize"/> is less than or equal to zero.</exception>
	public static string ConvertTabsToSpaces(string input, int tabSize)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tabSize);
		return TransformLines(input, line => ExpandTabs(line, tabSize));
	}

	private static string TransformLines(string input, Func<string, string> lineTransform)
	{
		if (string.IsNullOrEmpty(input))
			return string.Empty;

		var builder = new StringBuilder(input.Length);
		int lineStart = 0;

		while (lineStart < input.Length)
		{
			// Find the next line terminator without treating a standalone CR as content.
			int lineEndingOffset = input.AsSpan(lineStart).IndexOfAny('\r', '\n');
			int lineEndingIndex = lineEndingOffset < 0 ? -1 : lineStart + lineEndingOffset;

			if (lineEndingIndex < 0)
			{
				builder.Append(lineTransform(input[lineStart..]));
				break;
			}

			builder.Append(lineTransform(input[lineStart..lineEndingIndex]));

			// Treat CRLF as one line ending while preserving standalone CR and LF.
			int lineEndingLength = input[lineEndingIndex] == '\r'
				&& lineEndingIndex + 1 < input.Length
				&& input[lineEndingIndex + 1] == '\n'
				? 2
				: 1;

			builder.Append(input, lineEndingIndex, lineEndingLength);
			lineStart = lineEndingIndex + lineEndingLength;
		}

		return builder.ToString();
	}

	private static string ConvertLineIndentationToTabs(string line, int tabSize)
	{
		int indentLength = 0;

		while (indentLength < line.Length && (line[indentLength] == ' ' || line[indentLength] == '\t'))
			indentLength++;

		if (indentLength == 0)
			return line;

		var builder = new StringBuilder(line.Length);
		int column = 0;

		for (int i = 0; i < indentLength;)
		{
			if (line[i] == '\t')
			{
				builder.Append('\t');

				// An existing tab advances to the next tab stop rather than by tabSize columns.
				column = ((column / tabSize) + 1) * tabSize;
				i++;

				continue;
			}

			int runStart = i;

			while (i < indentLength && line[i] == ' ')
				i++;

			int spaceCount = i - runStart;

			// Replace groups of spaces that reach a tab stop with a single tab.
			// Remaining spaces that would not reach a tab stop stay as spaces.
			while (spaceCount > 0)
			{
				int spacesToNextStop = tabSize - (column % tabSize);

				if (spaceCount < spacesToNextStop)
					break;

				builder.Append('\t');
				column += spacesToNextStop;
				spaceCount -= spacesToNextStop;
			}

			if (spaceCount > 0)
			{
				builder.Append(' ', spaceCount);
				column += spaceCount;
			}
		}

		builder.Append(line, indentLength, line.Length - indentLength);
		return builder.ToString();
	}

	private static string ExpandTabs(string line, int tabSize)
	{
		if (!line.Contains('\t'))
			return line;

		var builder = new StringBuilder(line.Length);
		int column = 0;

		foreach (char c in line)
		{
			if (c == '\t')
			{
				// Expand each tab only as far as the next tab stop for this line.
				int spaces = tabSize - (column % tabSize);
				builder.Append(' ', spaces);
				column += spaces;
			}
			else
			{
				builder.Append(c);
				column++;
			}
		}

		return builder.ToString();
	}
}
