using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public class Command : Block
{
	public IReadOnlyList<string> Arguments { get; set; }

	public Command(string[] documentLines, int lineIndex, out int lineCount) : base(lineIndex)
	{
		string line = documentLines[lineIndex];
		Name = line.Split('=')[0];

		lineCount = GetCommandLineCount(documentLines, lineIndex);
		var arguments = new List<string>();

		for (int i = 0; i < lineCount; i++)
		{
			line = documentLines[lineIndex + i];
			line = LineParser.RemoveComments(line).Trim();

			if (i == 0)
			{
				arguments.AddRange(line
					.Remove(0, line.IndexOf('=') + 1)
					.Replace('>', ' ')
					.Split(',')
					.Where(arg => !string.IsNullOrWhiteSpace(arg))
					.Select(arg => arg.Trim()));
			}
			else
			{
				arguments.AddRange(line
					.Replace('>', ' ')
					.Split(',')
					.Where(arg => !string.IsNullOrWhiteSpace(arg))
					.Select(arg => arg.Trim()));
			}
		}

		Arguments = arguments;
		LineCount = lineCount;
	}

	private static int GetCommandLineCount(string[] documentLines, int lineIndex)
	{
		int lineCount = 1;
		string line = documentLines[lineIndex];
		line = LineParser.RemoveComments(line).Trim();

		while (line.EndsWith(">"))
		{
			lineCount++;
			line = documentLines[lineIndex + lineCount - 1];
			line = LineParser.RemoveComments(line).Trim();
		}

		return lineCount;
	}
}
