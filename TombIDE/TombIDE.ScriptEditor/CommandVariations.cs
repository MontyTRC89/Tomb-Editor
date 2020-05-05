using ICSharpCode.AvalonEdit.Document;

namespace TombIDE.ScriptEditor
{
	internal class CommandVariations
	{
		public static string GetCorrectLevelCommand(TextDocument document, int lineNumber)
		{
			string section = GetCurrentSectionName(document, lineNumber);

			if (section == null)
				return "LevelLevel";

			switch (section.ToLower())
			{
				case "psxextensions":
					return "LevelPSX";

				case "pcextensions":
					return "LevelPC";

				case "title":
					return "LevelLevel";

				case "level":
					return "LevelLevel";
			}

			return null;
		}

		public static string GetCorrectCutCommand(TextDocument document, int lineNumber)
		{
			string section = GetCurrentSectionName(document, lineNumber);

			if (section == null)
				return null;

			switch (section.ToLower())
			{
				case "psxextensions":
					return "CutPSX";

				case "pcextensions":
					return "CutPC";
			}

			return null;
		}

		public static string GetCorrectFMVCommand(TextDocument document, int lineNumber)
		{
			string section = GetCurrentSectionName(document, lineNumber);

			if (section == null)
				return null;

			switch (section.ToLower())
			{
				case "psxextensions":
					return "FMVPSX";

				case "pcextensions":
					return "FMVPC";
			}

			return null;
		}

		public static string GetCurrentSectionName(TextDocument document, int lineNumber)
		{
			for (int i = lineNumber - 1; i > 0; i--)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string currentLineText = document.GetText(currentLine.Offset, currentLine.Length);

				if (currentLineText.Trim().StartsWith("["))
					return currentLineText.Split('[')[1].Split(']')[0].Trim();
			}

			return null;
		}
	}
}
