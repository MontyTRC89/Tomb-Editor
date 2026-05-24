using TombLib.Scripting.Completion;
using TombLib.Scripting.Core.Lua;

namespace TombLib.LanguageServer.Lua;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	private const int CompletionTriggerKindInvoked = 1;
	private const int CompletionTriggerKindTriggerCharacter = 2;

	public async Task<IReadOnlyList<TextCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
		int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
	{
		return await SendPositionRequestAsync<CompletionResponse?, IReadOnlyList<TextCompletionItem>>(
			filePath, content, line, column, "textDocument/completion",
			(textDocument, position) => new CompletionParams(textDocument, position, BuildCompletionContext(triggerCharacter)),
			response =>
			{
				IReadOnlyList<CompletionItemPayload> itemPayloads = response?.Items ?? [];

				if (itemPayloads.Count == 0)
					return [];

				Func<TextCompletionItem, CompletionItemPayload, int, Func<CancellationToken, Task<TextCompletionItem>>?>? resolveFactory =
					_client is not null && _client.SupportsCompletionResolve
						? (unresolvedItem, itemPayload, itemIndex) =>
							cancellationToken => ResolveCompletionItemAsync(unresolvedItem, itemPayload, itemIndex, cancellationToken)
						: null;

				return LuaLanguageServerResponseParser.ParseCompletionItems(itemPayloads, resolveFactory);
			},
			timeoutValue: null,
			defaultValue: [],
			cancellationToken).ConfigureAwait(false);
	}

	private static CompletionContextPayload BuildCompletionContext(char? triggerCharacter)
	{
		return triggerCharacter is null
			? new CompletionContextPayload(TriggerKind: CompletionTriggerKindInvoked)
			: new CompletionContextPayload(TriggerKind: CompletionTriggerKindTriggerCharacter, triggerCharacter.ToString());
	}

	private async Task<TextCompletionItem> ResolveCompletionItemAsync(TextCompletionItem unresolvedItem, CompletionItemPayload itemPayload, int itemIndex, CancellationToken cancellationToken)
	{
		ILanguageServerClient? client = _client;

		if (client is null)
			return unresolvedItem;

		if (!client.SupportsCompletionResolve)
			return unresolvedItem;

		try
		{
			CompletionItemPayload? resolvedItem = await SendBoundedRequestAsync<CompletionItemPayload?>(client, "completionItem/resolve", itemPayload,
				timeoutValue: null, cancellationToken).ConfigureAwait(false);

			if (resolvedItem is not null)
			{
				TextCompletionItem? parsedItem = LuaLanguageServerResponseParser.ParseCompletionItem(resolvedItem, itemIndex);

				if (parsedItem is not null)
					return unresolvedItem.WithResolvedContent(parsedItem);
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to resolve Lua completion item '{Label}'; falling back to the unresolved item.", unresolvedItem.Label);
		}

		return unresolvedItem;
	}
}
