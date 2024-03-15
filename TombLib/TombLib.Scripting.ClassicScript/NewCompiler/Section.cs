using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public class Section : Block
{
	public IReadOnlyList<Command> Commands { get; set; }

	public Section(string[] documentLines, int lineIndex, out int lineCount) : base(lineIndex)
	{
		Name = LineParser.GetSectionHeaderText(documentLines[lineIndex]);
		var commands = new List<Command>();

		lineIndex++;
		lineCount = 1;

		while (lineIndex < documentLines.Length)
		{
			string line = documentLines[lineIndex];

			if (LineParser.IsEmptyOrComments(line) || line.TrimStart().StartsWith('#'))
			{
				lineIndex++;
				lineCount++;

				continue;
			}

			if (LineParser.IsSectionHeaderLine(line))
				break;

			var command = new Command(documentLines, lineIndex, out int commandLineCount);
			commands.Add(command);

			lineIndex += commandLineCount;
			lineCount += commandLineCount;
		}

		Commands = commands;
		LineCount = lineCount;
	}
}
