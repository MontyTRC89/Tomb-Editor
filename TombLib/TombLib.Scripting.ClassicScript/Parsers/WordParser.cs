using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.ClassicScript.Enums;

namespace TombLib.Scripting.ClassicScript.Parsers
{
	public static class WordParser
	{
		public static string GetWordFromOffset(TextDocument document, int offset)
		{
			if (offset > document.TextLength)
				return null;

			DocumentLine line = document.GetLineByOffset(offset);

			int wordStart = -1;
			int wordEnd = -1;

			if (offset == document.TextLength)
			{
				wordEnd = offset;
				offset--;
			}
			else
			{
				for (int i = offset; i <= line.EndOffset; i++)
				{
					if (i == line.EndOffset)
					{
						wordEnd = i;
						break;
					}

					char c = document.GetCharAt(i);

					if (c == ',' || c == '=' || c == ']' || c == '+' || c == '\n')
					{
						wordEnd = i;
						break;
					}
				}
			}

			for (int i = offset; i >= line.Offset; i--)
			{
				char c = document.GetCharAt(i);

				if (c == ',' || c == '=' || c == '[' || c == '+')
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
			if (offset > document.TextLength)
				return WordType.Unknown;
			else if (offset == document.TextLength)
				offset--;

			DocumentLine line = document.GetLineByOffset(offset);

			for (int i = offset; i <= line.EndOffset; i++)
			{
				if (i == line.EndOffset)
				{
					for (int j = offset; j > line.Offset; j--)
					{
						char ch = document.GetCharAt(j);

						if (ch == '_')
							return WordType.MnemonicConstant;
						else if (ch == '$')
							return WordType.Hexadecimal;
						else if (ch == ',' || ch == '=' || ch == '+')
							return WordType.Unknown;
					}

					break;
				}

				char c = document.GetCharAt(i);

				if (c == ']')
				{
					for (int j = offset; j >= line.Offset; j--)
						if (document.GetCharAt(j) == '[')
							return WordType.Header;
				}
				else if (c == '=')
					return WordType.Command;
				else if (c == '_')
					return WordType.MnemonicConstant;
				else if (c == '$')
					return WordType.Hexadecimal;
				else if (c == ',' || c == '+' || c == '\n')
				{
					for (int j = offset; j > line.Offset; j--)
					{
						char ch = document.GetCharAt(j);

						if (ch == '_')
							return WordType.MnemonicConstant;
						else if (ch == '$')
							return WordType.Hexadecimal;
						else if (ch == ',' || ch == '=' || ch == '+')
							return WordType.Unknown;
					}
				}
			}

			return WordType.Unknown;
		}
	}
}
