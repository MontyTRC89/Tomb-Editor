using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.UI.Cleaning;

namespace TombLib.Scripting.ClassicScript.Cleaning;

public sealed class ClassicScriptDocumentFormatter : ITextDocumentFormatter
{
	public bool PreEqualSpace { get; set; } = ConfigurationDefaults.Tidy_PreEqualSpace;
	public bool PostEqualSpace { get; set; } = ConfigurationDefaults.Tidy_PostEqualSpace;

	public bool PreCommaSpace { get; set; } = ConfigurationDefaults.Tidy_PreCommaSpace;
	public bool PostCommaSpace { get; set; } = ConfigurationDefaults.Tidy_PostCommaSpace;

	public bool ReduceSpaces { get; set; } = ConfigurationDefaults.Tidy_ReduceSpaces;

	public string FormatDocument(string editorContent, bool trimOnly = false)
	{
		if (trimOnly)
			return editorContent.TrimTrailingWhitespaceOnLines();

		string[] lines = editorContent.Replace("\r", string.Empty).Split('\n');

		for (int i = 0; i < lines.Length; i++)
			lines[i] = FormatLine(lines[i]);

		return string.Join(Environment.NewLine, lines.TrimTrailingWhitespaceOnLines());
	}

	public static string FormatCompilerOutput(string content)
	{
		string[] lines = content.Replace("\r", string.Empty).Split('\n');

		for (int i = 0; i < lines.Length; i++)
			lines[i] = RemoveSpacesBeforeEquals(lines[i]);

		return string.Join(Environment.NewLine, lines.TrimTrailingWhitespaceOnLines());
	}

	private string FormatLine(string line)
	{
		var builder = new System.Text.StringBuilder(line.Length);

		for (int index = 0; index < line.Length; index++)
		{
			char currentChar = line[index];

			if (currentChar is '=' or ',')
			{
				ApplySpacingBeforeToken(builder, currentChar);
				builder.Append(currentChar);
				index = SkipSpacesAfterToken(line, index);
				ApplySpacingAfterToken(builder, currentChar, line, index + 1);
				continue;
			}

			if (ReduceSpaces && currentChar == ' ' && builder.Length > 0 && builder[builder.Length - 1] == ' ')
				continue;

			builder.Append(currentChar);
		}

		return builder.ToString();
	}

	private void ApplySpacingBeforeToken(System.Text.StringBuilder builder, char token)
	{
		while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
			builder.Length--;

		bool needsSpace = token == '=' ? PreEqualSpace : PreCommaSpace;

		if (needsSpace && builder.Length > 0)
			builder.Append(' ');
	}

	private void ApplySpacingAfterToken(System.Text.StringBuilder builder, char token, string line, int nextIndex)
	{
		bool needsSpace = token == '=' ? PostEqualSpace : PostCommaSpace;

		if (!needsSpace || nextIndex >= line.Length)
			return;

		builder.Append(' ');
	}

	private static int SkipSpacesAfterToken(string line, int index)
	{
		while (index + 1 < line.Length && line[index + 1] == ' ')
			index++;

		return index;
	}

	private static string RemoveSpacesBeforeEquals(string line)
	{
		var builder = new System.Text.StringBuilder(line.Length);

		for (int index = 0; index < line.Length; index++)
		{
			char currentChar = line[index];

			if (currentChar == '=')
			{
				while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
					builder.Length--;
			}

			builder.Append(currentChar);
		}

		return builder.ToString();
	}
}
