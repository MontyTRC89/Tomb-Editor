using ICSharpCode.AvalonEdit.Document;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptLib.Core.Services.Implementations;

public sealed class WhitespaceService : IWhitespaceService
{
	public void ConvertTabsToSpaces(TextDocument document, IEnumerable<DocumentLine>? lines = null, int tabSize = 4)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		lines ??= document.Lines;

		document.BeginUpdate();

		try
		{
			foreach (DocumentLine line in lines)
			{
				string lineText = document.GetText(line);

				if (!lineText.Contains('\t'))
					continue;

				string newText = ConvertTabsToSpacesInText(lineText, tabSize);
				document.Replace(line.Offset, line.Length, newText);
			}
		}
		finally
		{
			document.EndUpdate();
		}
	}

	public void ConvertSpacesToTabs(TextDocument document, IEnumerable<DocumentLine>? lines = null, int tabSize = 4)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		lines ??= document.Lines;

		document.BeginUpdate();

		try
		{
			foreach (DocumentLine line in lines)
			{
				string lineText = document.GetText(line);
				string newText = ConvertSpacesToTabsInText(lineText, tabSize);

				if (newText != lineText)
					document.Replace(line.Offset, line.Length, newText);
			}
		}
		finally
		{
			document.EndUpdate();
		}
	}

	public void TrimTrailingWhitespace(TextDocument document, IEnumerable<DocumentLine>? lines = null)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		lines ??= document.Lines;

		document.BeginUpdate();

		try
		{
			foreach (DocumentLine line in lines)
			{
				string lineText = document.GetText(line);
				string trimmedText = TrimTrailingWhitespaceFromText(lineText);

				if (trimmedText != lineText)
					document.Replace(line.Offset, line.Length, trimmedText);
			}
		}
		finally
		{
			document.EndUpdate();
		}
	}

	private static string ConvertTabsToSpacesInText(string text, int tabSize)
	{
		var builder = new StringBuilder();
		int column = 0;

		foreach (char c in text)
		{
			if (c == '\t')
			{
				int spacesToAdd = tabSize - column % tabSize;
				builder.Append(' ', spacesToAdd);
				column += spacesToAdd;
			}
			else
			{
				builder.Append(c);
				column++;
			}
		}

		return builder.ToString();
	}

	private static string ConvertSpacesToTabsInText(string text, int tabSize)
	{
		var builder = new StringBuilder();
		int spaceCount = 0;
		int column = 0;

		foreach (char c in text)
		{
			if (c == ' ')
			{
				spaceCount++;
				column++;

				if (spaceCount == tabSize || column % tabSize == 0 && spaceCount > 0)
				{
					builder.Append('\t');
					spaceCount = 0;
				}
			}
			else
			{
				// Output any remaining spaces that didn't form a complete tab
				if (spaceCount > 0)
				{
					builder.Append(' ', spaceCount);
					spaceCount = 0;
				}

				builder.Append(c);
				column++;

				if (c == '\t')
					// Adjust column for tab character
					column = (column / tabSize + 1) * tabSize;
			}
		}

		// Output any trailing spaces
		if (spaceCount > 0)
			builder.Append(' ', spaceCount);

		return builder.ToString();
	}

	private static string TrimTrailingWhitespaceFromText(string text)
		=> Regex.Replace(text, @"\s+$", "");
}
