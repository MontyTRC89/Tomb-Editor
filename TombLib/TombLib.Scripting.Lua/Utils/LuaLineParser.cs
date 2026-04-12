using System.Collections.Generic;
using System.Text;

namespace TombLib.Scripting.Lua.Utils
{
	internal static class LuaLineParser
	{
		public static bool IsInsideCommentOrString(string lineText)
		{
			if (string.IsNullOrEmpty(lineText))
				return false;

			bool isInsideSingleQuotedString = false;
			bool isInsideDoubleQuotedString = false;
			bool isEscaped = false;

			for (int i = 0; i < lineText.Length; i++)
			{
				char currentChar = lineText[i];

				if (!isInsideSingleQuotedString && !isInsideDoubleQuotedString && IsLineCommentStart(lineText, i))
					return true;

				if (isEscaped)
				{
					isEscaped = false;
					continue;
				}

				if ((isInsideSingleQuotedString || isInsideDoubleQuotedString) && currentChar == '\\' && i + 1 < lineText.Length)
				{
					isEscaped = true;
					continue;
				}

				if (!isInsideDoubleQuotedString && currentChar == '\'')
					isInsideSingleQuotedString = !isInsideSingleQuotedString;
				else if (!isInsideSingleQuotedString && currentChar == '"')
					isInsideDoubleQuotedString = !isInsideDoubleQuotedString;
			}

			return isInsideSingleQuotedString || isInsideDoubleQuotedString;
		}

		public static string StripLineComment(string lineText)
		{
			if (string.IsNullOrEmpty(lineText))
				return string.Empty;

			var builder = new StringBuilder(lineText.Length);
			bool isInsideSingleQuotedString = false;
			bool isInsideDoubleQuotedString = false;

			for (int i = 0; i < lineText.Length; i++)
			{
				char currentChar = lineText[i];

				if (!isInsideSingleQuotedString && !isInsideDoubleQuotedString && IsLineCommentStart(lineText, i))
					break;

				if ((isInsideSingleQuotedString || isInsideDoubleQuotedString) && currentChar == '\\' && i + 1 < lineText.Length)
				{
					builder.Append(currentChar);
					builder.Append(lineText[i + 1]);
					i++;
					continue;
				}

				if (!isInsideDoubleQuotedString && currentChar == '\'')
					isInsideSingleQuotedString = !isInsideSingleQuotedString;
				else if (!isInsideSingleQuotedString && currentChar == '"')
					isInsideDoubleQuotedString = !isInsideDoubleQuotedString;

				builder.Append(currentChar);
			}

			return builder.ToString();
		}

		public static IEnumerable<char> EnumerateStructuralCharacters(string lineText)
		{
			if (string.IsNullOrEmpty(lineText))
				yield break;

			bool isInsideSingleQuotedString = false;
			bool isInsideDoubleQuotedString = false;

			for (int i = 0; i < lineText.Length; i++)
			{
				char currentChar = lineText[i];

				if (!isInsideSingleQuotedString && !isInsideDoubleQuotedString && IsLineCommentStart(lineText, i))
					yield break;

				if ((isInsideSingleQuotedString || isInsideDoubleQuotedString) && currentChar == '\\' && i + 1 < lineText.Length)
				{
					i++;
					continue;
				}

				if (!isInsideDoubleQuotedString && currentChar == '\'')
				{
					isInsideSingleQuotedString = !isInsideSingleQuotedString;
					continue;
				}

				if (!isInsideSingleQuotedString && currentChar == '"')
				{
					isInsideDoubleQuotedString = !isInsideDoubleQuotedString;
					continue;
				}

				if (!isInsideSingleQuotedString && !isInsideDoubleQuotedString)
					yield return currentChar;
			}
		}

		private static bool IsLineCommentStart(string lineText, int index)
			=> lineText[index] == '-' && index + 1 < lineText.Length && lineText[index + 1] == '-';
	}
}