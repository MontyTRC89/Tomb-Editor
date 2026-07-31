using Nickelony.LanguageServer.Core.Editing;
using Nickelony.LanguageServer.Core.Hover;
using Nickelony.LanguageServer.Core.Navigation;
using Nickelony.LanguageServer.Core.Signatures;

namespace Nickelony.LanguageServer.Lua;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	public async Task<IReadOnlyList<TextReferenceLocation>> GetReferencesAsync(TextReferenceRequest request, CancellationToken cancellationToken = default)
	{
		return await GetReferencesAsync(
			request.FilePath,
			request.DocumentText,
			request.Line,
			request.Column,
			cancellationToken).ConfigureAwait(false);
	}

	public Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<HoverResponse?, TextHoverInfo?>(
			filePath, content, line, column, "textDocument/hover",
			static (textDocument, position) => new TextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseHoverInfo,
			timeoutValue: null,
			defaultValue: null,
			cancellationToken);
	}

	public Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<DefinitionResponse, TextDefinitionLocation?>(
			filePath, content, line, column, "textDocument/definition",
			static (textDocument, position) => new TextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseDefinitionLocation,
			timeoutValue: default!,
			defaultValue: null,
			cancellationToken);
	}

	/// <summary>
	/// Resolves reference locations for a symbol at a position within a document.
	/// </summary>
	/// <param name="filePath">The local file path of the document.</param>
	/// <param name="content">The current document content.</param>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="column">The zero-based column index.</param>
	/// <param name="cancellationToken">A token that can cancel the request.</param>
	/// <returns>The resolved reference locations, or an empty list when none are available.</returns>
	public async Task<IReadOnlyList<TextReferenceLocation>> GetReferencesAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		ILanguageServerClient? client = _client;

		if (client is null)
			return [];

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		try
		{
			if (!await SynchronizeDocumentAsync(normalizedFilePath, content,
				acquireOpenReference: false, acquireRequestReference: true, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
			{
				return [];
			}

			if (!client.SupportsReferences)
				return [];

			var textDocument = new TextDocumentIdentifier(LanguageServerPathHelper.CreateFileUri(normalizedFilePath));
			var position = new ProtocolPosition(line, column);

			ReferenceResponse[]? response = await SendBoundedRequestAsync<ReferenceResponse[]?>(client, "textDocument/references",
				new ReferenceParams(textDocument, position, new ReferenceContextPayload(IncludeDeclaration: true)),
				timeoutValue: null,
				cancellationToken).ConfigureAwait(false);

			return LuaLanguageServerResponseParser.ParseReferenceLocations(response);
		}
		finally
		{
			await ReleaseRequestDocumentAsync(normalizedFilePath, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task<TextWorkspaceEdit?> RenameSymbolAsync(TextRenameRequest request, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(request.NewName))
			return null;

		ILanguageServerClient? client = _client;

		if (client is null)
			return null;

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(request.FilePath, out string normalizedFilePath))
			return null;

		try
		{
			if (!await SynchronizeDocumentAsync(normalizedFilePath, request.DocumentText,
				acquireOpenReference: false, acquireRequestReference: true, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
			{
				return null;
			}

			if (!client.SupportsRename)
				return null;

			var textDocument = new TextDocumentIdentifier(LanguageServerPathHelper.CreateFileUri(normalizedFilePath));
			var position = new ProtocolPosition(request.Line, request.Column);

			WorkspaceEditResponse? response = await SendBoundedRequestAsync<WorkspaceEditResponse?>(client, "textDocument/rename",
				new RenameParams(textDocument, position, request.NewName),
				timeoutValue: null,
				cancellationToken).ConfigureAwait(false);

			return LuaLanguageServerResponseParser.ParseWorkspaceEdit(response, _logger);
		}
		finally
		{
			await ReleaseRequestDocumentAsync(normalizedFilePath, cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task<TextWorkspaceEdit?> FormatDocumentAsync(TextFormatRequest request, CancellationToken cancellationToken = default)
	{
		ILanguageServerClient? client = _client;

		if (client is null)
			return null;

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(request.FilePath, out string normalizedFilePath))
			return null;

		try
		{
			if (!await SynchronizeDocumentAsync(normalizedFilePath, request.DocumentText,
				acquireOpenReference: false, acquireRequestReference: true, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
			{
				return null;
			}

			if (!client.SupportsFormatting)
				return null;

			TextEditPayload[]? response = await SendBoundedRequestAsync<TextEditPayload[]?>(client, "textDocument/formatting",
				new DocumentFormattingParams(
					new TextDocumentIdentifier(LanguageServerPathHelper.CreateFileUri(normalizedFilePath)),
					new FormattingOptionsPayload(request.Options.TabSize, request.Options.InsertSpaces)),
				timeoutValue: null,
				cancellationToken).ConfigureAwait(false);

			IReadOnlyList<TextEdit> textEdits = LuaLanguageServerResponseParser.ParseDocumentFormattingEdits(response);

			return textEdits.Count == 0
				? null
				: new TextWorkspaceEdit([
					new TextDocumentEdit(request.FilePath, textEdits)
				]);
		}
		finally
		{
			await ReleaseRequestDocumentAsync(normalizedFilePath, cancellationToken).ConfigureAwait(false);
		}
	}

	public Task<TextSignatureHelpInfo?> GetSignatureHelpAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		return SendPositionRequestAsync<SignatureHelpResponse?, TextSignatureHelpInfo?>(
			filePath, content, line, column, "textDocument/signatureHelp",
			static (textDocument, position) => new TextDocumentPositionParams(textDocument, position),
			LuaLanguageServerResponseParser.ParseSignatureHelp,
			timeoutValue: null,
			defaultValue: null,
			cancellationToken);
	}

	/// <summary>
	/// Synchronizes the requested document, executes a position-based language-server request,
	/// and releases the temporary request tracking afterwards.
	/// </summary>
	private async Task<TResult> SendPositionRequestAsync<TResponse, TResult>(
		string filePath, string content, int line, int column,
		string method,
		Func<TextDocumentIdentifier, ProtocolPosition, object> buildParameters,
		Func<TResponse, TResult> parseResponse,
		TResponse timeoutValue,
		TResult defaultValue,
		CancellationToken cancellationToken)
	{
		ILanguageServerClient? client = _client;

		if (client is null)
			return defaultValue;

		if (!LanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return defaultValue;

		try
		{
			// Synchronize the document without triggering post-edit semantic token refresh.
			if (
				// Request-driven sync paths (completion / hover / definition / signature) intentionally
				// skip the semantic token refresh: typing a single identifier character can otherwise turn
				// into didChange + completion + semanticTokens/full per keystroke, which is the dominant
				// performance regression observed during normal editing. UpdateDocument (TextChangedDelayed)
				// remains the single owner of post-edit semantic token refresh.
				!await SynchronizeDocumentAsync(normalizedFilePath, content,
					acquireOpenReference: false, acquireRequestReference: true, refreshSemanticTokens: false, cancellationToken).ConfigureAwait(false))
			{
				return defaultValue;
			}

			// Dispatch the request against the normalized document URI.
			var textDocument = new TextDocumentIdentifier(LanguageServerPathHelper.CreateFileUri(normalizedFilePath));
			var position = new ProtocolPosition(line, column);

			TResponse response = await SendBoundedRequestAsync(client, method,
				buildParameters(textDocument, position), timeoutValue, cancellationToken).ConfigureAwait(false);

			if (response is null)
				return defaultValue;

			return parseResponse(response);
		}
		finally
		{
			await ReleaseRequestDocumentAsync(normalizedFilePath, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Sends a language-server request with timeout tracking and a single retry when the active transport changes or becomes unavailable.
	/// </summary>
	private async Task<TResponse> SendBoundedRequestAsync<TResponse>(
		ILanguageServerClient client,
		string method,
		object parameters,
		TResponse timeoutValue,
		CancellationToken cancellationToken)
	{
		long transportGeneration = client.TransportGeneration;

		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
		timeoutCts.CancelAfter(_requestTimeout);

		try
		{
			// Primary attempt on the current transport generation.
			TResponse response = await client.SendRequestAsync<TResponse>(method, parameters, timeoutCts.Token).ConfigureAwait(false);
			ResetRequestTimeoutTracking(transportGeneration);
			return response;
		}
		catch (OperationCanceledException) when (_isDisposed || _disposeCts.IsCancellationRequested)
		{
			return timeoutValue;
		}
		catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
		{
			RecordRequestTimeout(client, method, transportGeneration);
			return timeoutValue;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' was canceled before the provider timeout elapsed on generation {Generation}; returning the fallback value without counting a timeout or forcing a restart.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);

			return timeoutValue;
		}
		catch (LanguageServerTransportChangedException) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' crossed a transport restart boundary on generation {Generation}; retrying once.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);
		}
		catch (LanguageServerTransportUnavailableException exception) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug(exception,
				"Lua language server request '{Method}' for workspace '{Workspace}' failed after transport generation {Generation} became unavailable; retrying once.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);
		}

		// If the transport changed underneath the request, ensure the client is healthy before retrying once.
		if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
			return timeoutValue;

		transportGeneration = client.TransportGeneration;

		using var retryTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
		retryTimeoutCts.CancelAfter(_requestTimeout);

		try
		{
			// Retry once on the refreshed transport generation.
			TResponse response = await client.SendRequestAsync<TResponse>(method, parameters, retryTimeoutCts.Token).ConfigureAwait(false);
			ResetRequestTimeoutTracking(transportGeneration);
			return response;
		}
		catch (OperationCanceledException) when (_isDisposed || _disposeCts.IsCancellationRequested)
		{
			return timeoutValue;
		}
		catch (OperationCanceledException) when (retryTimeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
		{
			RecordRequestTimeout(client, method, transportGeneration);
			return timeoutValue;
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' was canceled before the provider timeout elapsed during the retry on generation {Generation}; returning the fallback value without counting a timeout or forcing a restart.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);

			return timeoutValue;
		}
		catch (LanguageServerTransportChangedException) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' crossed a second transport restart boundary on generation {Generation}; returning the fallback value.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);

			return timeoutValue;
		}
		catch (LanguageServerTransportUnavailableException exception) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogDebug(exception,
				"Lua language server request '{Method}' for workspace '{Workspace}' failed again after transport generation {Generation} became unavailable; returning the fallback value.",
				method,
				_workspaceRootDirectoryPath,
				transportGeneration);

			return timeoutValue;
		}
	}

	private void RecordRequestTimeout(ILanguageServerClient client, string method, long transportGeneration)
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
			if (client.TryMarkTransportUnhealthy(transportGeneration))
			{
				_logger.LogWarning("Lua language server request '{Method}' for workspace '{Workspace}' timed out after {Timeout}s {Count} consecutive times on transport generation {Generation} (threshold {Threshold}); marking that transport unhealthy so the next IntelliSense request restarts it.",
					method,
					_workspaceRootDirectoryPath,
					_requestTimeout.TotalSeconds,
					timeoutCount,
					transportGeneration,
					_requestTimeoutRestartThreshold);
			}
			else
			{
				_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' timed out on superseded transport generation {Generation}; leaving the active transport unchanged.",
					method,
					_workspaceRootDirectoryPath,
					transportGeneration);
			}

			return;
		}

		_logger.LogDebug("Lua language server request '{Method}' for workspace '{Workspace}' timed out after {Timeout}s (consecutive {Count}/{Threshold}, generation {Generation}).",
			method,
			_workspaceRootDirectoryPath,
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
