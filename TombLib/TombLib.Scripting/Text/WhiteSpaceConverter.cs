using System.Text;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.Text;

/// <summary>
/// Converts between space and tab indentation in text.
/// </summary>
public static class WhiteSpaceConverter
{
	private static readonly Regex TrailingDoubleSpacesRegex = new(@" {2,}$");

	/// <summary>
	/// Converts runs of spaces to tabs using the specified tab size.
	/// </summary>
	/// <param name="input">The text to convert.</param>
	/// <param name="tabSize">The number of spaces per tab.</param>
	/// <returns>The converted text.</returns>
	public static string ConvertSpacesToTabs(string input, int tabSize)
	{
		string[] lines = input.Replace("\r", string.Empty).Split('\n');

		var resultBuilder = new StringBuilder();

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			var lineBuilder = new StringBuilder();

			for (int j = 0; j < line.Length; j += tabSize)
			{
				int length = Math.Min(line.Length - j, tabSize);
				Match match = TrailingDoubleSpacesRegex.Match(line, j, length);

				if (match.Success)
				{
					lineBuilder.Append(line.Substring(j, tabSize - match.Length));
					lineBuilder.Append('\t');
				}
				else
					lineBuilder.Append(line.Substring(j, length));
			}

			resultBuilder.Append(lineBuilder.ToString());

			if (i < lines.Length - 1)
				resultBuilder.Append(Environment.NewLine); // Prevents adding an extra line at the end
		}

		return resultBuilder.ToString();
	}

	/// <summary>
	/// Converts tabs to spaces using the specified tab size.
	/// </summary>
	/// <param name="input">The text to convert.</param>
	/// <param name="tabSize">The number of spaces per tab.</param>
	/// <returns>The converted text.</returns>
	public static string ConvertTabsToSpaces(string input, int tabSize)
	{
		string[] lines = input.Replace("\r", string.Empty).Split('\n');

		var resultBuilder = new StringBuilder();

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			string[] lineSegments = line.Split('\t');

			if (lineSegments.Length > 1)
				for (int j = 0; j < lineSegments.Length; j++)
				{
					string segment = lineSegments[j];

					if (j == lineSegments.Length - 1)
						resultBuilder.Append(segment); // Prevents adding extra spaces at the end of the line
					else
						resultBuilder.Append(segment + new string(' ', tabSize - segment.Length % tabSize));
				}
			else
				resultBuilder.Append(line);

			if (i < lines.Length - 1)
				resultBuilder.Append(Environment.NewLine); // Prevents adding an extra line at the end
		}

		return resultBuilder.ToString();
	}
}
