using ICSharpCode.AvalonEdit.Document;
using System.Windows.Documents;
using TombLib.Scripting.GameFlowScript.Navigation;
using Nickelony.LanguageServer.Abstractions.Hover;
using TombLib.Scripting.Hover;

namespace TombLib.Scripting.GameFlowScript.Hover;

public sealed class GameFlowHoverProvider : ITextHoverProvider
{
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		var document = new TextDocument(request.DocumentText);
		string? hoveredWord = GetWordFromOffset(document, request.HoveredOffset);

		if (string.IsNullOrWhiteSpace(hoveredWord))
			return null;

		if (Contains(GameFlowDefinitionsProvider.Sections, hoveredWord))
			return new TextHoverInfo($"GameFlow section \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: ObjectType.Section);

		if (Contains(GameFlowDefinitionsProvider.SpecialProperties, hoveredWord))
			return new TextHoverInfo($"GameFlow special property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: "SpecialProperty");

		if (Contains(GameFlowDefinitionsProvider.Properties, hoveredWord))
			return new TextHoverInfo($"GameFlow property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: "Property");

		if (Contains(GameFlowDefinitionsProvider.Constants, hoveredWord))
			return new TextHoverInfo($"GameFlow constant \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: "Constant");

		return null;
	}

	private static bool Contains(IReadOnlyList<string> values, string value)
	{
		for (int index = 0; index < values.Count; index++)
		{
			if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private static string? GetWordFromOffset(TextDocument document, int offset)
	{
		int wordStart = TextUtilities.GetNextCaretPosition(document, offset, LogicalDirection.Backward, CaretPositioningMode.WordBorder);
		int wordEnd = TextUtilities.GetNextCaretPosition(document, offset, LogicalDirection.Forward, CaretPositioningMode.WordBorder);

		if (wordStart < 0 || wordEnd < 0 || wordEnd <= wordStart)
			return null;

		return document.GetText(wordStart, wordEnd - wordStart).Trim();
	}
}
