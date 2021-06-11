using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Enums;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class WordParser
	{
		public static string GetWordFromOffset(TextDocument document, int offset)
		{
			DocumentLine line = document.GetLineByOffset(offset);
			int lineEndOffset = line.Offset + line.Length;

			int wordStart = -1;
			int wordEnd = -1;

			for (int i = offset; i < lineEndOffset; i++)
			{
				char c = document.GetCharAt(i);

				if (c == ',' || c == '=' || c == ']' || i == lineEndOffset)
				{
					wordEnd = i;
					break;
				}
			}

			for (int i = offset; i >= line.Offset; i--)
			{
				char c = document.GetCharAt(i);

				if (c == ',' || c == '=' || c == '[')
				{
					wordStart = i + 1;
					break;
				}
				else if (i == line.Offset)
				{
					wordStart = i;
					break;
				}
			}

			if (wordStart >= 0 && wordEnd >= 0 && wordStart < wordEnd)
				return document.GetText(wordStart, wordEnd - wordStart).Trim();

			return null;
		}

		public static WordType GetWordTypeFromOffset(TextDocument document, int offset)
		{
			DocumentLine line = document.GetLineByOffset(offset);
			int lineEndOffset = line.Offset + line.Length;

			for (int i = offset; i < lineEndOffset; i++)
				switch (document.GetCharAt(i))
				{
					case ']':
						for (int j = offset; j >= line.Offset; j--)
							if (document.GetCharAt(j) == '[')
								return WordType.Header;
						return WordType.Unknown;

					case '=':
						return WordType.Command;

					case ',':
						for (int j = offset; j > line.Offset; j--)
							if (document.GetCharAt(j) == '_')
								return WordType.MnemonicConstant;
							else if (document.GetCharAt(j) == ',' || document.GetCharAt(j) == '=')
								return WordType.Unknown;
						return WordType.Unknown;
				}

			return WordType.Unknown;
		}
	}
}
