using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public record struct FirstIdDirective(string CommandName, int FirstId);

public class LevelSection : Section
{
	public IReadOnlyList<FirstIdDirective> FirstIdDirectives { get; set; }

	public LevelSection(string[] documentLines, int lineIndex, out int lineCount)
		: base(documentLines, lineIndex, out lineCount)
	{
		var firstIdDirectives = new List<FirstIdDirective>();
		lineIndex++;

		while (lineIndex < documentLines.Length)
		{
			string line = documentLines[lineIndex];
			lineIndex++;

			if (LineParser.IsEmptyOrComments(line))
				continue;

			if (LineParser.IsSectionHeaderLine(line))
				break;

			if (LineParser.IsValidFirstIdLine(line))
			{
				string value = line.Remove(0, "#first_id ".Length).Trim();
				string commandName = value[..value.IndexOf('=')].Trim();
				string firstId = value[(value.IndexOf('=') + 1)..].Trim();

				firstIdDirectives.Add(new FirstIdDirective(commandName, int.Parse(firstId)));
			}
		}

		FirstIdDirectives = firstIdDirectives;
	}
}
