using Nickelony.LanguageServer.Abstractions.Completion;

namespace TombLib.Scripting.Completion;

/// <summary>
/// Provides shared completion items for a document snapshot and caret position.
/// </summary>
public interface ITextCompletionProvider
{
	/// <summary>
	/// Gets completion items for the supplied request context.
	/// </summary>
	IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context);
}
