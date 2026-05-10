using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

public sealed partial class LuaLanguageServerIntellisenseProvider
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
		return await SendPositionRequestAsync<LuaCompletionResponse?, IReadOnlyList<LuaCompletionItem>>(
			filePath, content, line, column, "textDocument/completion",
			(textDocument, position) => new LuaCompletionParams(textDocument, position, BuildCompletionContext(triggerCharacter)),
			response =>
			{
				IReadOnlyList<LuaCompletionItemPayload> itemPayloads = response?.Items ?? [];

				if (itemPayloads.Count == 0)
					return [];

				Func<LuaCompletionItem, LuaCompletionItemPayload, int, Func<CancellationToken, Task<LuaCompletionItem>>?>? resolveFactory =
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
		return SendPositionRequestAsync<LuaHoverResponse?, LuaHoverInfo?>(
			filePath, content, line, column, "textDocument/hover",
			static (textDocument, position) => new LuaTextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseHoverInfo,
			timeoutValue: null,
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
		return SendPositionRequestAsync<LuaDefinitionResponse, LuaDefinitionLocation?>(
			filePath, content, line, column, "textDocument/definition",
			static (textDocument, position) => new LuaTextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseDefinitionLocation,
			timeoutValue: default,
			defaultValue: null,
			cancellationToken);
	}

	/// <summary>
	/// Requests all known references for the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The reference locations returned by LuaLS.</returns>
	public async Task<IReadOnlyList<LuaReferenceLocation>> GetReferencesAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null)
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		if (!await SynchronizeDocumentAsync(normalizedFilePath, content,
			acquireOpenReference: false, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
		{
			return [];
		}

		if (!client.SupportsReferences)
			return [];

		var textDocument = new LuaTextDocumentIdentifier(LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath));
		var position = new LuaProtocolPosition(line, column);

		LuaReferenceResponse[]? response = await SendBoundedRequestAsync<LuaReferenceResponse[]?>(client, "textDocument/references",
			new LuaReferenceParams(textDocument, position, new LuaReferenceContextPayload(IncludeDeclaration: true)),
			timeoutValue: null,
			cancellationToken).ConfigureAwait(false);

		return LuaLanguageServerResponseParser.ParseReferenceLocations(response);
	}

	/// <summary>
	/// Requests workspace edits to rename the symbol at the specified document position.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="newName">The requested replacement symbol name.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The workspace edit returned by LuaLS, or <see langword="null"/> when unavailable.</returns>
	public async Task<LuaWorkspaceEdit?> RenameSymbolAsync(string filePath, string content,
		int line, int column, string newName, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(newName))
			return null;

		ILuaLanguageServerClient? client = _client;

		if (client is null)
			return null;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return null;

		if (!await SynchronizeDocumentAsync(normalizedFilePath, content,
			acquireOpenReference: false, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
		{
			return null;
		}

		if (!client.SupportsRename)
			return null;

		var textDocument = new LuaTextDocumentIdentifier(LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath));
		var position = new LuaProtocolPosition(line, column);

		LuaWorkspaceEditResponse? response = await SendBoundedRequestAsync<LuaWorkspaceEditResponse?>(client, "textDocument/rename",
			new LuaRenameParams(textDocument, position, newName),
			timeoutValue: null,
			cancellationToken).ConfigureAwait(false);

		return LuaLanguageServerResponseParser.ParseWorkspaceEdit(response);
	}

	/// <summary>
	/// Requests formatting edits for the specified Lua document.
	/// </summary>
	/// <param name="filePath">The local file path.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="options">The editor formatting preferences to pass to LuaLS.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The text edits returned by LuaLS.</returns>
	public async Task<IReadOnlyList<LuaTextEdit>> FormatDocumentAsync(string filePath, string content,
		LuaFormattingOptions options, CancellationToken cancellationToken = default)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null)
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		if (!await SynchronizeDocumentAsync(normalizedFilePath, content,
			acquireOpenReference: false, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
		{
			return [];
		}

		if (!client.SupportsFormatting)
			return [];

		LuaTextEditPayload[]? response = await SendBoundedRequestAsync<LuaTextEditPayload[]?>(client, "textDocument/formatting",
			new LuaDocumentFormattingParams(
				new LuaTextDocumentIdentifier(LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath)),
				new LuaFormattingOptionsPayload(options.TabSize, options.InsertSpaces)),
			timeoutValue: null,
			cancellationToken).ConfigureAwait(false);

		return LuaLanguageServerResponseParser.ParseDocumentFormattingEdits(response);
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
		return SendPositionRequestAsync<LuaSignatureHelpResponse?, LuaSignatureInfo?>(
			filePath, content, line, column, "textDocument/signatureHelp",
			static (textDocument, position) => new LuaTextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseSignatureHelp,
			timeoutValue: null,
			defaultValue: null,
			cancellationToken);
	}

	private static LuaCompletionContextPayload BuildCompletionContext(char? triggerCharacter)
	{
		return triggerCharacter is null
			? new LuaCompletionContextPayload(TriggerKind: 1)
			: new LuaCompletionContextPayload(TriggerKind: 2, triggerCharacter.ToString());
	}

	private async Task<LuaCompletionItem> ResolveCompletionItemAsync(LuaCompletionItem unresolvedItem, LuaCompletionItemPayload itemPayload, int itemIndex, CancellationToken cancellationToken)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null)
			return unresolvedItem;

		if (!client.SupportsCompletionResolve)
			return unresolvedItem;

		try
		{
			LuaCompletionItemPayload? resolvedItem = await SendBoundedRequestAsync<LuaCompletionItemPayload?>(client, "completionItem/resolve", itemPayload,
				timeoutValue: null, cancellationToken).ConfigureAwait(false);

			if (resolvedItem is not null)
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

	private async Task<TResult> SendPositionRequestAsync<TResponse, TResult>(
		string filePath, string content, int line, int column,
		string method,
		Func<LuaTextDocumentIdentifier, LuaProtocolPosition, object> buildParameters,
		Func<TResponse, TResult> parseResponse,
		TResponse timeoutValue,
		TResult defaultValue,
		CancellationToken cancellationToken)
	{
		ILuaLanguageServerClient? client = _client;

		if (client is null)
			return defaultValue;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return defaultValue;

		if (
			// Request-driven sync paths (completion / hover / definition / signature) intentionally
			// skip the semantic-token refresh: typing a single identifier character can otherwise turn
			// into didChange + completion + semanticTokens/full per keystroke, which is the dominant
			// performance regression observed during normal editing. UpdateDocument (TextChangedDelayed)
			// remains the single owner of post-edit semantic-token refresh.
			!await SynchronizeDocumentAsync(normalizedFilePath, content,
				acquireOpenReference: false, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
		{
			return defaultValue;
		}

		var textDocument = new LuaTextDocumentIdentifier(LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath));
		var position = new LuaProtocolPosition(line, column);

		TResponse response = await SendBoundedRequestAsync(client, method,
			buildParameters(textDocument, position), timeoutValue, cancellationToken).ConfigureAwait(false);

		if (response is null)
			return defaultValue;

		return parseResponse(response);
	}

	private async Task<TResponse> SendBoundedRequestAsync<TResponse>(
		ILuaLanguageServerClient client,
		string method,
		object parameters,
		TResponse timeoutValue,
		CancellationToken cancellationToken)
	{
		long transportGeneration = client.TransportGeneration;

		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutCts.CancelAfter(_requestTimeout);

		try
		{
			TResponse response = await client.SendRequestAsync<TResponse>(method, parameters, timeoutCts.Token).ConfigureAwait(false);
			ResetRequestTimeoutTracking(transportGeneration);
			return response;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			RecordRequestTimeout(client, method, transportGeneration);
			return timeoutValue;
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
