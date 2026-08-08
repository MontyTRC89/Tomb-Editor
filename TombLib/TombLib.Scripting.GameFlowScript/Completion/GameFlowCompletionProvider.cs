using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

/// <summary>
/// Builds completion items from the GameFlow definition catalog.
/// </summary>
public sealed class GameFlowCompletionProvider : ITextCompletionProvider
{
	/// <summary>
	/// Gets the completion items for the given context.
	/// </summary>
	/// <param name="context">The completion context.</param>
	/// <returns>The full set of contextually valid completion items.</returns>
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var items = new List<TextCompletionItem>();

		AddItems(items, GameFlowDefinitionCatalog.Sections, ": ");
		AddItems(items, GameFlowDefinitionCatalog.SpecialProperties, ": ");
		AddItems(items, GameFlowDefinitionCatalog.Properties, ": ");
		AddItems(items, GameFlowDefinitionCatalog.Constants, string.Empty);

		// The provider returns the full contextually-valid candidate set; the session
		// coordinator owns word filtering so candidates are not filtered twice.
		return items;
	}

	private static void AddItems(List<TextCompletionItem> items, IReadOnlyList<string> values, string suffix)
	{
		foreach (string value in values)
			items.Add(new TextCompletionItem(value, value + suffix));
	}
}
