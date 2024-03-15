using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public record struct DefineDirective(string Name, string Value, bool IsPluginDefine = false);

public class ScriptFile
{
	public Section PCExtensionsSection { get; set; }
	public Section PSXExtensionsSection { get; set; }
	public Section LanguageSection { get; set; }
	public Section OptionsSection { get; set; }
	public LevelSection TitleSection { get; set; }

	public IReadOnlyList<LevelSection> LevelSections { get; set; }
	public IReadOnlyList<DefineDirective> DefineDirectives { get; set; }

	public ScriptFile(string filePath)
	{
		string[] lines = File.ReadAllLines(filePath);
		lines = MergeIncludeFiles(lines, Path.GetDirectoryName(filePath));

		var sections = new List<Section>();
		int lineIndex = 0;

		while (lineIndex < lines.Length)
		{
			string line = lines[lineIndex];

			if (LineParser.IsEmptyOrComments(line))
			{
				lineIndex++;
				continue;
			}

			if (LineParser.IsSectionHeaderLine(line))
			{
				bool isLevelSection =
					LineParser.GetSectionHeaderText(line).Equals("Level", StringComparison.OrdinalIgnoreCase) ||
					LineParser.GetSectionHeaderText(line).Equals("Title", StringComparison.OrdinalIgnoreCase);

				Section section = isLevelSection
					? new LevelSection(lines, lineIndex, out int sectionLineCount)
					: new Section(lines, lineIndex, out sectionLineCount);

				sections.Add(section);
				lineIndex += sectionLineCount;
			}
		}

		lineIndex = 0;

		// Read #DEFINE directives
		var defines = new List<DefineDirective>();

		while (lineIndex < lines.Length)
		{
			string line = lines[lineIndex];

			if (LineParser.IsEmptyOrComments(line))
			{
				lineIndex++;
				continue;
			}

			if (LineParser.IsValidDefineLine(line))
			{
				string value = line.Remove(0, "#define ".Length).Trim();
				string name = value[..value.IndexOf(' ')].Trim();
				string defineValue = value.Split(' ').Last().Trim();

				bool isPluginDefine = name.StartsWith("@plugin", StringComparison.OrdinalIgnoreCase);

				defines.Add(new DefineDirective(name, defineValue, isPluginDefine));
			}

			lineIndex++;
		}

		PCExtensionsSection = sections.Find(s => s.Name.Equals("PCExtensions", StringComparison.OrdinalIgnoreCase));
		PSXExtensionsSection = sections.Find(s => s.Name.Equals("PSXExtensions", StringComparison.OrdinalIgnoreCase));
		LanguageSection = sections.Find(s => s.Name.Equals("Language", StringComparison.OrdinalIgnoreCase));
		OptionsSection = sections.Find(s => s.Name.Equals("Options", StringComparison.OrdinalIgnoreCase));
		TitleSection = (LevelSection)sections.Find(s => s.Name.Equals("Title", StringComparison.OrdinalIgnoreCase));
		LevelSections = sections.FindAll(s => s.Name.StartsWith("Level", StringComparison.OrdinalIgnoreCase)).Cast<LevelSection>().ToList();

		DefineDirectives = defines;
	}

	private static string[] MergeIncludeFiles(string[] documentLines, string fileDirectory)
	{
		var newLines = new List<string>();

		foreach (string line in documentLines)
		{
			if (LineParser.IsValidIncludeLine(line))
			{
				string includeFilePath = Path.Combine(fileDirectory, line.Split('"')[1]);
				string[] includeFileLines = File.ReadAllLines(includeFilePath);
				string[] mergedIncludeFileLines = MergeIncludeFiles(includeFileLines, Path.GetDirectoryName(includeFilePath));

				newLines.AddRange(mergedIncludeFileLines);
			}
			else
				newLines.Add(line);
		}

		return newLines.ToArray();
	}
}
