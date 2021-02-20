using System;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Parsers;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public class ContentReader
	{
		public static bool NextSectionExists(string[] lines, int lineNumber, out int nextSectionLineNumber)
		{
			for (int i = lineNumber; i < lines.Length; i++)
				if (LineParser.IsSectionHeaderLine(lines[i]))
				{
					nextSectionLineNumber = i;
					return true;
				}

			nextSectionLineNumber = -1;
			return false;
		}

		public static List<string> GetStrings(string[] lines, int sectionStartLineNumber)
		{
			var strings = new List<string>();

			for (int i = sectionStartLineNumber + 1; i < lines.Length; i++)
			{
				if (LineParser.IsSectionHeaderLine(lines[i]))
					break;

				string line = GetParsedLine(lines[i]);

				if (!LineParser.IsEmptyOrComments(line))
					strings.Add(line);
			}

			return strings;
		}

		public static string GetParsedLine(string line)
		{
			line = LineParser.RemoveComments(line);
			line = line.Replace("\\x3B", ";");
			line = line.Replace("\\n", Environment.NewLine);
			line = line.Trim();

			return line;
		}

		public static string GetShortHex(short decimalValue, int size = 4)
		{
			string hexValue = decimalValue.ToString("X");

			int zerosToAdd = size - hexValue.Length;
			hexValue = $"${new string('0', zerosToAdd)}{hexValue}";

			return hexValue;
		}
	}
}
