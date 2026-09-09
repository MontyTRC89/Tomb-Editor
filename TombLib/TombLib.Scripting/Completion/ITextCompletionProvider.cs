using Nickelony.LanguageServer.Abstractions.Completion;
using System.Collections.Generic;

namespace TombLib.Scripting.Completion;

/// <summary>
/// Provides shared completion items for a document snapshot and caret position.
/// </summary>
public interface ITextCompletionProvider
{
	/// <summary>
	/// Gets completion items for the supplied request context. Implementations must be safe to
	/// call from any thread: the context is an immutable snapshot and no UI state may be touched.
	/// This permits the host to run catalog-driven completion on the thread pool when the work is
	/// CPU-bound.
	/// </summary>
	IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context);
}
