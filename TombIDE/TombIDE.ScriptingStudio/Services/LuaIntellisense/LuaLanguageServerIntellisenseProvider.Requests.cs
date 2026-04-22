#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerIntellisenseProvider
{
	public async Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
		int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
	{
		return await SendPositionRequestAsync(
			filePath, content, line, column, "textDocument/completion",
			(textDocument, position) => new
			{
				textDocument,
				position,
				context = BuildCompletionContext(triggerCharacter)
			},
			response =>
			{
				IReadOnlyList<JsonElement> itemElements = LuaLanguageServerResponseParser.ExtractCompletionItems(response);

				return itemElements.Count > 0
					? LuaLanguageServerResponseParser.ParseCompletionItems(itemElements, BuildCompletionItemResolveCallback)
					: [];
			},
			defaultValue: [],
			cancellationToken).ConfigureAwait(false);
	}

	public Task<LuaHoverInfo?> GetHoverAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<LuaHoverInfo?>(
			filePath, content, line, column, "textDocument/hover",
			static (textDocument, position) => new { textDocument, position },
			LuaLanguageServerResponseParser.ParseHoverInfo,
			defaultValue: null,
			cancellationToken);
	}

	public Task<LuaDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<LuaDefinitionLocation?>(
			filePath, content, line, column, "textDocument/definition",
			static (textDocument, position) => new { textDocument, position },
			LuaLanguageServerResponseParser.ParseDefinitionLocation,
			defaultValue: null,
			cancellationToken);
	}

	public Task<LuaSignatureInfo?> GetSignatureHelpAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<LuaSignatureInfo?>(
			filePath, content, line, column, "textDocument/signatureHelp",
			static (textDocument, position) => new { textDocument, position },
			LuaLanguageServerResponseParser.ParseSignatureHelp,
			defaultValue: null,
			cancellationToken);
	}

	private static object BuildCompletionContext(char? triggerCharacter)
	{
		return triggerCharacter is null
			? new { triggerKind = 1 }
			: new { triggerKind = 2, triggerCharacter = triggerCharacter.ToString() };
	}

	private Func<CancellationToken, Task<LuaCompletionItem>>? BuildCompletionItemResolveCallback(LuaCompletionItem unresolvedItem, JsonElement itemElement, int itemIndex)
	{
		if (_client is null || !_client.SupportsCompletionResolve)
			return null;

		return cancellationToken => ResolveCompletionItemAsync(unresolvedItem, itemElement, itemIndex, cancellationToken);
	}

	private async Task<LuaCompletionItem> ResolveCompletionItemAsync(LuaCompletionItem unresolvedItem, JsonElement itemElement, int itemIndex, CancellationToken cancellationToken)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null || !client.SupportsCompletionResolve)
			return unresolvedItem;

		try
		{
			JsonElement resolvedItem = await SendBoundedRequestAsync(client, "completionItem/resolve", itemElement, cancellationToken).ConfigureAwait(false);

			if (resolvedItem.ValueKind == JsonValueKind.Object)
			{
				LuaCompletionItem? parsedItem = LuaLanguageServerResponseParser.ParseCompletionItem(resolvedItem, itemIndex);

				if (parsedItem is not null)
					return parsedItem;
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

	private async Task<TResult> SendPositionRequestAsync<TResult>(
		string filePath, string content, int line, int column,
		string method,
		Func<object, object, object> buildParameters,
		Func<JsonElement, TResult> parseResponse,
		TResult defaultValue,
		CancellationToken cancellationToken)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null
			|| !LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
			// Request-driven sync paths (completion / hover / definition / signature) intentionally
			// skip the semantic-token refresh: typing a single identifier character can otherwise turn
			// into didChange + completion + semanticTokens/full per keystroke, which is the dominant
			// performance regression observed during normal editing. UpdateDocument (TextChangedDelayed)
			// remains the single owner of post-edit semantic-token refresh.
			|| !await SynchronizeDocumentAsync(normalizedFilePath, content,
				acquireOpenReference: false, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
		{
			return defaultValue;
		}

		object textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) };
		object position = new { line, character = column };

		JsonElement response = await SendBoundedRequestAsync(client, method,
			buildParameters(textDocument, position), cancellationToken).ConfigureAwait(false);

		return parseResponse(response);
	}

	private static async Task<JsonElement> SendBoundedRequestAsync(ILuaLanguageServerClient client, string method, object parameters, CancellationToken cancellationToken)
	{
		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutCts.CancelAfter(RequestTimeout);

		try
		{
			return await client.SendRequestAsync(method, parameters, timeoutCts.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Debug("Lua language server request '{Method}' timed out after {Timeout}s.", method, RequestTimeout.TotalSeconds);
			return default;
		}
	}
}
