using System;
using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Lua.Utils
{
	internal static class LuaEditorInteractionRules
	{
		public static bool CanRequestHover(TextDocument? document, int offset, bool isCompletionWindowOpen, bool isSignatureHelpOpen)
		{
			if (isCompletionWindowOpen || isSignatureHelpOpen)
				return false;

			return !IsInsideCommentOrString(document, offset);
		}

		public static bool IsValidAutocompleteContext(TextDocument? document, int offset, char? triggerCharacter)
		{
			if (offset <= 0 || document is null || document.TextLength == 0)
				return false;

			if (IsInsideCommentOrString(document, offset))
				return false;

			if (triggerCharacter is '.' || triggerCharacter is ':')
				return true;

			char typedCharacter = document.GetCharAt(offset - 1);

			if (!IsIdentifierCharacter(typedCharacter))
				return false;

			if (offset >= 2 && document.GetCharAt(offset - 2) == '.')
				return false;

			return true;
		}

		public static bool IsValidManualCompletionContext(TextDocument? document, int offset)
		{
			if (document is null)
				return false;

			if (document.TextLength == 0)
				return true;

			return !IsInsideCommentOrString(document, offset);
		}

		public static bool TryGetDefinitionStartOffset(TextDocument? document, int offset, out int definitionOffset)
		{
			definitionOffset = 0;

			if (document is null || document.TextLength == 0)
				return false;

			int safeOffset = Math.Max(0, Math.Min(offset, document.TextLength));

			if (IsInsideCommentOrString(document, safeOffset))
				return false;

			if (!TryGetDefinitionWordBounds(document, safeOffset, out definitionOffset, out _))
				return false;

			return true;
		}

		public static bool IsInsideCommentOrString(TextDocument? document, int offset)
		{
			if (document is null || document.TextLength == 0)
				return false;

			int safeOffset = Math.Max(0, Math.Min(offset, document.TextLength));
			DocumentLine currentLine = document.GetLineByOffset(safeOffset);
			int lineStart = currentLine.Offset;
			int inspectedLength = Math.Max(0, Math.Min(safeOffset, currentLine.EndOffset) - lineStart);
			string lineText = document.GetText(lineStart, inspectedLength);

			return LuaLineParser.IsInsideCommentOrString(lineText);
		}

		private static bool TryGetDefinitionWordBounds(TextDocument document, int offset, out int wordStart, out int wordEnd)
		{
			wordStart = 0;
			wordEnd = 0;

			if (document.TextLength == 0)
				return false;

			int safeOffset = Math.Max(0, Math.Min(offset, document.TextLength));
			int probeOffset = safeOffset;

			if (probeOffset >= document.TextLength)
				probeOffset = document.TextLength - 1;

			if (probeOffset > 0
				&& !IsIdentifierCharacter(document.GetCharAt(probeOffset))
				&& IsIdentifierCharacter(document.GetCharAt(probeOffset - 1)))
			{
				probeOffset--;
			}

			if (!IsIdentifierCharacter(document.GetCharAt(probeOffset)))
				return false;

			wordStart = probeOffset;
			wordEnd = probeOffset + 1;

			while (wordStart > 0 && IsIdentifierCharacter(document.GetCharAt(wordStart - 1)))
				wordStart--;

			while (wordEnd < document.TextLength && IsIdentifierCharacter(document.GetCharAt(wordEnd)))
				wordEnd++;

			return wordEnd > wordStart;
		}

		private static bool IsIdentifierCharacter(char character)
			=> char.IsLetterOrDigit(character) || character == '_';
	}
}