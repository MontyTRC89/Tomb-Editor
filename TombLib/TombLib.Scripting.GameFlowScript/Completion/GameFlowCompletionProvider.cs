using TombLib.Scripting.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

public sealed class GameFlowCompletionProvider : ITextCompletionProvider
{
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		_ = context;

		var items = new List<TextCompletionItem>();

		AddItems(items, GameFlowDefinitionsProvider.Sections, ": ");
		AddItems(items, GameFlowDefinitionsProvider.SpecialProperties, ": ");
		AddItems(items, GameFlowDefinitionsProvider.Properties, ": ");
		AddItems(items, GameFlowDefinitionsProvider.Constants, string.Empty);

		return items;
	}

	private static void AddItems(List<TextCompletionItem> items, IReadOnlyList<string> values, string suffix)
	{
		foreach (string value in values)
			items.Add(new TextCompletionItem(value, value + suffix));
	}
}
