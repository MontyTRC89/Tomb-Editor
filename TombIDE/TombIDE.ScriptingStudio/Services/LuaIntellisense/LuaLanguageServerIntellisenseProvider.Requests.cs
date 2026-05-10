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
	/// <summary>
	/// Requests completion items for the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="triggerCharacter">The optional trigger character that caused completion.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The completion items returned by LuaLS.</returns>
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

	/// <summary>
	/// Requests hover information for the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The hover payload, or <see langword="null"/> when none exists.</returns>
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

	/// <summary>
	/// Requests a definition location for the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The definition location, or <see langword="null"/> when none exists.</returns>
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

	/// <summary>
	/// Requests signature-help information for the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The signature-help payload, or <see langword="null"/> when none exists.</returns>
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

	private async Task<JsonElement> SendBoundedRequestAsync(ILuaLanguageServerClient client, string method, object parameters, CancellationToken cancellationToken)
	{
		long transportGeneration = client.TransportGeneration;
		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutCts.CancelAfter(_requestTimeout);

		try
		{
			JsonElement response = await client.SendRequestAsync(method, parameters, timeoutCts.Token).ConfigureAwait(false);
			ResetRequestTimeoutTracking(transportGeneration);
			return response;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			RecordRequestTimeout(client, method, transportGeneration);
			return default;
		}
	}

	private void RecordRequestTimeout(ILuaLanguageServerClient client, string method, long transportGeneration)
	{
		int timeoutCount;
		bool shouldMarkTransportUnhealthy = false;

		lock (_requestTimeoutSyncRoot)
		{
			if (_timedOutRequestGeneration != transportGeneration)
			{
				_timedOutRequestGeneration = transportGeneration;
				_consecutiveRequestTimeouts = 0;
				_restartRequestedGeneration = -1;
			}

			timeoutCount = ++_consecutiveRequestTimeouts;

			if (timeoutCount >= _requestTimeoutRestartThreshold && _restartRequestedGeneration != transportGeneration)
			{
				_restartRequestedGeneration = transportGeneration;
				shouldMarkTransportUnhealthy = true;
			}
		}

		if (shouldMarkTransportUnhealthy)
		{
			Log.Warn("Lua language server request '{Method}' timed out after {Timeout}s {Count} times on transport generation {Generation}; the transport will restart on the next IntelliSense request.",
				method,
				_requestTimeout.TotalSeconds,
				timeoutCount,
				transportGeneration);

			client.MarkTransportUnhealthy();
			return;
		}

		Log.Debug("Lua language server request '{Method}' timed out after {Timeout}s (consecutive {Count}/{Threshold}, generation {Generation}).",
			method,
			_requestTimeout.TotalSeconds,
			timeoutCount,
			_requestTimeoutRestartThreshold,
			transportGeneration);
	}

	private void ResetRequestTimeoutTracking(long transportGeneration)
	{
		lock (_requestTimeoutSyncRoot)
		{
			_timedOutRequestGeneration = transportGeneration;
			_consecutiveRequestTimeouts = 0;
			_restartRequestedGeneration = -1;
		}
	}
}
