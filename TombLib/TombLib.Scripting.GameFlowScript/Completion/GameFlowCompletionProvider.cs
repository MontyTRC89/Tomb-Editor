using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowCompletionProvider : ITextCompletionProvider
{
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		var items = new List<TextCompletionItem>();

		AddItems(items, GameFlowDefinitionCatalog.Sections, ": ");
		AddItems(items, GameFlowDefinitionCatalog.SpecialProperties, ": ");
		AddItems(items, GameFlowDefinitionCatalog.Properties, ": ");
		AddItems(items, GameFlowDefinitionCatalog.Constants, string.Empty);

		return TextCompletionFilter.FilterByCurrentWord(items, context);
	}

	private static void AddItems(List<TextCompletionItem> items, IReadOnlyList<string> values, string suffix)
	{
		foreach (string value in values)
			items.Add(new TextCompletionItem(value, value + suffix));
	}
}
