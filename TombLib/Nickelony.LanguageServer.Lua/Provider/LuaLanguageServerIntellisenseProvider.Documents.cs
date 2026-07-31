namespace Nickelony.LanguageServer.Lua;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	private Task QueueLatestDocumentUpdateAsync(string filePath, string content)
	{
		return _documentScheduler.QueueLatestUpdateAsync(filePath,
			token => SynchronizeDocumentAsync(filePath, content,
				acquireOpenReference: false,
				acquireRequestReference: false,
				refreshSemanticTokens: true,
				token));
	}

	private async Task<bool> SynchronizeDocumentAsync(string filePath, string content,
		bool acquireOpenReference, bool acquireRequestReference, bool refreshSemanticTokens, CancellationToken cancellationToken)
	{
		if (_isDisposed || string.IsNullOrWhiteSpace(filePath) || _client is null)
			return false;

		try
		{
			DocumentSynchronizationResult synchronizationResult = await _documentScheduler.EnqueuePerDocumentAsync(
				filePath,
				token => SynchronizeDocumentCoreAsync(filePath, content, acquireOpenReference, acquireRequestReference, token),
				cancellationToken).ConfigureAwait(false);

			if (!synchronizationResult.Success)
				return false;

			if (refreshSemanticTokens && synchronizationResult.Document is { } synchronizedDocument)
				await RefreshSemanticTokensAsync(synchronizedDocument, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			return false;
		}
		catch (IOException)
		{
			InvalidateDocumentSynchronization(filePath);
			return false;
		}
		catch (ObjectDisposedException)
		{
			if (!_isDisposed)
				InvalidateDocumentSynchronization(filePath);

			return false;
		}
	}

	private void InvalidateDocumentSynchronization(string filePath)
	{
		MarkStartupTransportUnavailable();

		_documents.InvalidateServerSynchronization(filePath);

		CancelQueuedDocumentUpdate(filePath);
		CancelSemanticTokenRequest(filePath);
	}

	public void RenameDocument(string oldFilePath, string newFilePath, string content)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(oldFilePath, out string normalizedOldFilePath)
			|| !LanguageServerPathHelper.TryNormalizeLocalPath(newFilePath, out string normalizedNewFilePath)
			|| string.Equals(normalizedOldFilePath, normalizedNewFilePath, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		CancelQueuedDocumentUpdate(normalizedOldFilePath);
		CancelQueuedDocumentUpdate(normalizedNewFilePath);

		ObserveBackgroundTask(
			RenameDocumentAsync(normalizedOldFilePath, normalizedNewFilePath, content, CancellationToken.None),
			"Document rename");
	}

	private async Task<bool> RenameDocumentAsync(string oldFilePath, string newFilePath, string content, CancellationToken cancellationToken)
	{
		if (_isDisposed || _client is null)
			return false;

		try
		{
			DocumentRenameRequest? request = await _documentScheduler.EnqueueExclusivePerDocumentAsync(
				oldFilePath,
				newFilePath,
				token => RenameDocumentCoreAsync(oldFilePath, newFilePath, content, token),
				cancellationToken).ConfigureAwait(false);

			if (request is not { } renameRequest)
				return false;

			string filePath = renameRequest.RenamedDocument.FilePath;
			RaiseDiagnosticsUpdated(filePath, _documents.GetDiagnostics(filePath));
			RaiseSemanticTokensUpdated(filePath, _documents.GetSemanticTokens(filePath));

			return true;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (IOException)
		{
			InvalidateDocumentSynchronization(newFilePath);
			return false;
		}
		catch (ObjectDisposedException)
		{
			if (!_isDisposed)
				InvalidateDocumentSynchronization(newFilePath);

			return false;
		}
	}

	private async Task<DocumentSynchronizationResult> SynchronizeDocumentCoreAsync(
		string filePath,
		string content,
		bool acquireOpenReference,
		bool acquireRequestReference,
		CancellationToken cancellationToken)
	{
		bool shouldTrackLocallyWhileUnavailable = acquireOpenReference || _documents.GetDocumentSnapshot(filePath) is not null;

		if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
		{
			if (shouldTrackLocallyWhileUnavailable)
				_documents.Synchronize(filePath, content, acquireOpenReference, acquireRequestReference: false);

			return new DocumentSynchronizationResult(false, null);
		}

		DocumentSynchronizationRequest? request = _documents.Synchronize(filePath, content, acquireOpenReference, acquireRequestReference);

		if (request is not { } pendingRequest)
			return new DocumentSynchronizationResult(true, null);

		await SendDocumentSynchronizationNotificationAsync(pendingRequest, cancellationToken).ConfigureAwait(false);
		return new DocumentSynchronizationResult(true, pendingRequest.Document);
	}

	private async Task<DocumentRenameRequest?> RenameDocumentCoreAsync(
		string oldFilePath,
		string newFilePath,
		string content,
		CancellationToken cancellationToken)
	{
		if (_client is null)
			return null;

		DocumentRenameRequest? request = _documents.Rename(oldFilePath, newFilePath, content);

		if (request is not { } renameRequest)
			return null;

		CancelSemanticTokenRequest(oldFilePath);
		CancelSemanticTokenRequest(newFilePath);

		if (!renameRequest.ReopenServerDocument)
			return renameRequest;

		if (!GetStartupSucceeded() || !_client.IsReady)
		{
			_documents.InvalidateServerSynchronization(newFilePath);
			return renameRequest;
		}

		if (renameRequest.PreviousDocument is not null)
		{
			await _client.SendNotificationAsync("textDocument/didClose",
				new DidCloseTextDocumentParams(new TextDocumentIdentifier(renameRequest.PreviousDocument.Uri)),
				cancellationToken).ConfigureAwait(false);
		}

		await SendDocumentSynchronizationNotificationAsync(
			new DocumentSynchronizationRequest(DocumentSynchronizationKind.Open, renameRequest.RenamedDocument),
			cancellationToken).ConfigureAwait(false);

		return renameRequest;
	}

	private async Task<bool> ReopenTrackedDocumentsAsync(IReadOnlyList<DocumentSnapshot> documents, CancellationToken cancellationToken)
	{
		for (int i = 0; i < documents.Count; i++)
		{
			DocumentSnapshot document = documents[i];
			DocumentSynchronizationRequest? request = _documents.Synchronize(document.FilePath, document.Content);

			if (request is not { } pendingRequest)
				continue;

			try
			{
				await SendDocumentSynchronizationNotificationAsync(pendingRequest, cancellationToken).ConfigureAwait(false);
				await RefreshSemanticTokensAsync(pendingRequest.Document, cancellationToken).ConfigureAwait(false);
			}
			catch (IOException)
			{
				return false;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
		}

		return true;
	}

	private async Task SendDocumentSynchronizationNotificationAsync(DocumentSynchronizationRequest request, CancellationToken cancellationToken)
	{
		if (_client is null)
			return;

		if (request.Kind == DocumentSynchronizationKind.Open)
		{
			await _client.SendNotificationAsync("textDocument/didOpen",
				new DidOpenTextDocumentParams(
					new DidOpenTextDocumentPayload(
						request.Document.Uri,
						"lua",
						request.Document.Version,
						request.Document.Content)),
				cancellationToken).ConfigureAwait(false);
		}
		else if (request.Kind == DocumentSynchronizationKind.Change)
		{
			TextDocumentContentChangePayload contentChange = _client.TextDocumentSyncKind switch
			{
				TextDocumentSyncKind.Incremental when request.ChangeRange is { } changeRange => new(
					changeRange.Text,
					new ProtocolRangePayload(
						new ProtocolNullablePosition(changeRange.StartLine, changeRange.StartCharacter),
						new ProtocolNullablePosition(changeRange.EndLine, changeRange.EndCharacter))),
				TextDocumentSyncKind.Full => new(request.Document.Content),
				TextDocumentSyncKind.Incremental => new(request.Document.Content),
				_ => throw new InvalidOperationException(
					"The Lua language server does not support document changes required by the Lua IntelliSense provider.")
			};

			await _client.SendNotificationAsync("textDocument/didChange",
				new DidChangeTextDocumentParams(
					new VersionedTextDocumentIdentifierPayload(request.Document.Uri, request.Document.Version),
					[contentChange]),
				cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task CloseDocumentAsync(string filePath, CancellationToken cancellationToken)
	{
		if (_client is null)
			return;

		try
		{
			await _documentScheduler.EnqueuePerDocumentAsync(
				filePath,
				async token =>
				{
					if (!_documents.TryClose(filePath, out DocumentSnapshot? document))
						return false;

					// The document just dropped its last open reference; cancel any in-flight
					// semantic token request for it now that no editor will display the result.
					CancelSemanticTokenRequest(filePath);

					// Only forward the close notification if the server is already running.
					// Starting the server just to send didClose would be wasteful and can race with disposal.
					if (document is null || !GetStartupSucceeded() || !_client.IsReady)
						return false;

					await _client.SendNotificationAsync("textDocument/didClose",
						new DidCloseTextDocumentParams(new TextDocumentIdentifier(document.Uri)), token).ConfigureAwait(false);

					return true;
				},
				cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || _isDisposed)
		{ }
		catch (IOException exception)
		{
			_logger.LogDebug(exception, "Lua best-effort document close failed due to a transport error for '{FilePath}'.", filePath);
		}
		catch (ObjectDisposedException exception)
		{
			_logger.LogDebug(exception, "Lua best-effort document close raced with disposal for '{FilePath}'.", filePath);
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "Lua best-effort document close failed unexpectedly for '{FilePath}'.", filePath);
		}
	}

	private async Task ReleaseRequestDocumentAsync(string filePath, CancellationToken cancellationToken)
	{
		if (_client is null)
			return;

		try
		{
			await _documentScheduler.EnqueuePerDocumentAsync(
				filePath,
				async _ =>
				{
					// Caller cancellation must not skip request-reference cleanup, otherwise a canceled
					// IntelliSense request can leave a request-only tracked document pinned indefinitely.
					_documents.ReleaseRequest(filePath);
					IReadOnlyList<DocumentSnapshot> documentsToClose = _documents.TrimRequestOnlyDocuments(MaxTrackedRequestOnlyDocuments);

					for (int i = 0; i < documentsToClose.Count; i++)
					{
						DocumentSnapshot document = documentsToClose[i];
						CancelSemanticTokenRequest(document.FilePath);

						if (!GetStartupSucceeded() || !_client.IsReady)
							continue;

						await _client.SendNotificationAsync("textDocument/didClose",
							new DidCloseTextDocumentParams(new TextDocumentIdentifier(document.Uri)), CancellationToken.None).ConfigureAwait(false);
					}

					return documentsToClose.Count > 0;
				},
				CancellationToken.None).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || _isDisposed)
		{ }
		catch (IOException exception)
		{
			_logger.LogDebug(exception, "Lua best-effort request-document release failed due to a transport error for '{FilePath}'.", filePath);
		}
		catch (ObjectDisposedException exception)
		{
			_logger.LogDebug(exception, "Lua best-effort request-document release raced with disposal for '{FilePath}'.", filePath);
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "Lua best-effort request-document release failed unexpectedly for '{FilePath}'.", filePath);
		}
	}

	private void HandleDiagnosticsPublished(PublishDiagnosticsParams parameters)
	{
		if (_isDisposed)
			return;

		if (!LanguageServerPathHelper.TryGetFilePath(parameters.Uri, out string filePath))
		{
			_logger.LogDebug("Lua diagnostics could not be matched to a local file path.");
			return;
		}

		DocumentSnapshot? document = _documents.GetDocumentSnapshot(filePath);

		// Diagnostics for documents we have never opened are ignored: there is no editor to render them on,
		// and reading the file from disk on the LSP read loop just to discard the result is wasteful.
		if (document is null)
			return;

		if (!LuaLanguageServerDiagnosticsParser.TryParse(parameters, filePath,
			document.Content, document.Version, out LuaPublishedDiagnostics? publishedDiagnostics))
		{
			_logger.LogDebug("Lua diagnostics payload could not be parsed for '{FilePath}'.", filePath);
			return;
		}

		if (_isDisposed)
			return;

		if (!_documents.TryStoreDiagnostics(publishedDiagnostics, document.Version))
			return;

		RaiseDiagnosticsUpdated(publishedDiagnostics.FilePath, publishedDiagnostics.Diagnostics);
	}

	private void ObserveBackgroundTask(Task task, string operation)
	{
		_ = ObserveAsync(task, operation);

		async Task ObserveAsync(Task observedTask, string observedOperation)
		{
			try
			{
				await observedTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{ }
			catch (IOException exception)
			{
				_logger.LogDebug(exception, "Lua language server background operation '{Operation}' failed with a transport error.", observedOperation);
			}
			catch (ObjectDisposedException)
			{ }
			catch (Exception exception)
			{
				_logger.LogWarning(exception, "Lua language server background operation '{Operation}' failed.", observedOperation);
			}
		}
	}

	private void CancelQueuedDocumentUpdate(string filePath)
		=> _documentScheduler.CancelQueuedUpdate(filePath);

	private void CancelAllQueuedDocumentUpdates()
		=> _documentScheduler.CancelAllQueuedUpdates();
}
