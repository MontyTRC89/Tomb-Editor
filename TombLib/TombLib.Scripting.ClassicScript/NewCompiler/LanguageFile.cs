using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public record struct ExtraNGString(int Index, string Text);

public class LanguageFile
{
	public IReadOnlyList<string> PCStrings { get; set; }
	public IReadOnlyList<string> PSXStrings { get; set; }
	public IReadOnlyList<ExtraNGString> ExtraNGStrings { get; set; }

	public LanguageFile(string filePath)
	{
		var pcStrings = new List<string>();
		var psxStrings = new List<string>();
		var extraNGStrings = new List<ExtraNGString>();

		string[] lines = File.ReadAllLines(filePath);
		bool isPC = false;
		bool isPSX = false;
		bool isExtraNG = false;

		foreach (string line in lines)
		{
			if (line.StartsWith("[PCStrings]", StringComparison.OrdinalIgnoreCase))
			{
				isPC = true;
				isPSX = false;
				isExtraNG = false;
			}
			else if (line.StartsWith("[PSXStrings]", StringComparison.OrdinalIgnoreCase))
			{
				isPC = false;
				isPSX = true;
				isExtraNG = false;
			}
			else if (line.StartsWith("[ExtraNG]", StringComparison.OrdinalIgnoreCase))
			{
				isPC = false;
				isPSX = false;
				isExtraNG = true;
			}
			else if (isPC)
				pcStrings.Add(line);
			else if (isPSX)
				psxStrings.Add(line);
			else if (isExtraNG)
			{
				int index = int.Parse(line.Split(':').First());
				string text = line.Remove(0, line.IndexOf(':') + 1).TrimStart();
				extraNGStrings.Add(new ExtraNGString(index, text));
			}
		}

		PCStrings = pcStrings;
		PSXStrings = psxStrings;
		ExtraNGStrings = extraNGStrings;
	}
}
