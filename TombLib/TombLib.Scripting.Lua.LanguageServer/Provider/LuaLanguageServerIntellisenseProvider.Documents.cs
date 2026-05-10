namespace TombLib.Scripting.Lua.LanguageServer;

public sealed partial class LuaLanguageServerIntellisenseProvider
{
	private Task<TResult> EnqueueDocumentOperationAsync<TResult>(
		Func<CancellationToken, Task<TResult>> operation,
		CancellationToken cancellationToken)
	{
		var completionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

		lock (_documentOperationSyncRoot)
		{
			Task previousOperation = _queuedDocumentOperation;
			_queuedDocumentOperation = RunQueuedDocumentOperationAsync(previousOperation, operation, completionSource, cancellationToken);
		}

		return completionSource.Task;
	}

	private static async Task RunQueuedDocumentOperationAsync<TResult>(
		Task previousOperation,
		Func<CancellationToken, Task<TResult>> operation,
		TaskCompletionSource<TResult> completionSource,
		CancellationToken cancellationToken)
	{
		try
		{
			await WaitForQueuedDocumentOperationAsync(previousOperation).ConfigureAwait(false);
			TResult result = await operation(cancellationToken).ConfigureAwait(false);
			completionSource.TrySetResult(result);
		}
		catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
		{
			completionSource.TrySetCanceled(cancellationToken);
		}
		catch (Exception exception)
		{
			completionSource.TrySetException(exception);
		}
	}

	private static async Task WaitForQueuedDocumentOperationAsync(Task previousOperation)
	{
		try
		{
			await previousOperation.ConfigureAwait(false);
		}
		catch
		{ }
	}

	private async Task<bool> SynchronizeDocumentAsync(string filePath, string content,
		bool acquireOpenReference, bool refreshSemanticTokens, CancellationToken cancellationToken)
	{
		if (_isDisposed || string.IsNullOrWhiteSpace(filePath) || _client is null)
			return false;

		try
		{
			LuaDocumentSynchronizationResult synchronizationResult = await EnqueueDocumentOperationAsync(
				token => SynchronizeDocumentCoreAsync(filePath, content, acquireOpenReference, token),
				cancellationToken).ConfigureAwait(false);

			if (!synchronizationResult.Success)
				return false;

			if (refreshSemanticTokens && synchronizationResult.Document is { } synchronizedDocument)
				await RefreshSemanticTokensAsync(synchronizedDocument, cancellationToken).ConfigureAwait(false);

			return true;
		}
		catch (OperationCanceledException)
		{
			throw;
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
		_startupSucceeded = false;
		_documents.InvalidateServerSynchronization(filePath);
		CancelSemanticTokenRequest(filePath);
	}

	public void RenameDocument(string oldFilePath, string newFilePath, string content)
	{
		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(oldFilePath, out string normalizedOldFilePath)
			|| !LuaLanguageServerPathHelper.TryNormalizeLocalPath(newFilePath, out string normalizedNewFilePath)
			|| string.Equals(normalizedOldFilePath, normalizedNewFilePath, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

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
			LuaDocumentRenameRequest? request = await EnqueueDocumentOperationAsync(
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

	private async Task<LuaDocumentSynchronizationResult> SynchronizeDocumentCoreAsync(
		string filePath,
		string content,
		bool acquireOpenReference,
		CancellationToken cancellationToken)
	{
		if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
			return new LuaDocumentSynchronizationResult(false, null);

		LuaDocumentSynchronizationRequest? request = _documents.Synchronize(filePath, content, acquireOpenReference);

		if (request is not { } pendingRequest)
			return new LuaDocumentSynchronizationResult(true, null);

		await SendDocumentSynchronizationNotificationAsync(pendingRequest, cancellationToken).ConfigureAwait(false);
		return new LuaDocumentSynchronizationResult(true, pendingRequest.Document);
	}

	private async Task<LuaDocumentRenameRequest?> RenameDocumentCoreAsync(
		string oldFilePath,
		string newFilePath,
		string content,
		CancellationToken cancellationToken)
	{
		if (_client is null)
			return null;

		LuaDocumentRenameRequest? request = _documents.Rename(oldFilePath, newFilePath, content);

		if (request is not { } renameRequest)
			return null;

		CancelSemanticTokenRequest(oldFilePath);
		CancelSemanticTokenRequest(newFilePath);

		if (!renameRequest.ReopenServerDocument)
			return renameRequest;

		if (!_startupSucceeded || !_client.IsReady)
		{
			_documents.InvalidateServerSynchronization(newFilePath);
			return renameRequest;
		}

		if (renameRequest.PreviousDocument is not null)
		{
			await _client.SendNotificationAsync("textDocument/didClose",
				new LuaDidCloseTextDocumentParams(new LuaTextDocumentIdentifier(renameRequest.PreviousDocument.Uri)),
				cancellationToken).ConfigureAwait(false);
		}

		await SendDocumentSynchronizationNotificationAsync(
			new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Open, renameRequest.RenamedDocument),
			cancellationToken).ConfigureAwait(false);

		return renameRequest;
	}

	private async Task<bool> ReopenTrackedDocumentsAsync(IReadOnlyList<LuaDocumentSnapshot> documents, CancellationToken cancellationToken)
	{
		for (int i = 0; i < documents.Count; i++)
		{
			LuaDocumentSnapshot document = documents[i];
			LuaDocumentSynchronizationRequest? request = _documents.Synchronize(document.FilePath, document.Content);

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

	private async Task SendDocumentSynchronizationNotificationAsync(LuaDocumentSynchronizationRequest request, CancellationToken cancellationToken)
	{
		if (_client is null)
			return;

		if (request.Kind == LuaDocumentSynchronizationKind.Open)
		{
			await _client.SendNotificationAsync("textDocument/didOpen",
				new LuaDidOpenTextDocumentParams(
					new LuaDidOpenTextDocumentPayload(
						request.Document.Uri,
						"lua",
						request.Document.Version,
						request.Document.Content)),
				cancellationToken).ConfigureAwait(false);
		}
		else if (request.Kind == LuaDocumentSynchronizationKind.Change)
		{
			LuaTextDocumentContentChangePayload contentChange = _client.TextDocumentSyncKind switch
			{
				LuaTextDocumentSyncKind.Incremental when request.ChangeRange is { } changeRange => new(
					changeRange.Text,
					new LuaProtocolRangePayload(
						new LuaProtocolNullablePosition(changeRange.StartLine, changeRange.StartCharacter),
						new LuaProtocolNullablePosition(changeRange.EndLine, changeRange.EndCharacter))),
				LuaTextDocumentSyncKind.Full => new(request.Document.Content),
				LuaTextDocumentSyncKind.Incremental => new(request.Document.Content),
				_ => throw new InvalidOperationException(
					"The Lua language server does not support document changes required by the Lua IntelliSense provider.")
			};

			await _client.SendNotificationAsync("textDocument/didChange",
				new LuaDidChangeTextDocumentParams(
					new LuaVersionedTextDocumentIdentifierPayload(request.Document.Uri, request.Document.Version),
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
			await EnqueueDocumentOperationAsync(
				async token =>
				{
					if (!_documents.TryClose(filePath, out LuaDocumentSnapshot? document))
						return false;

					// The document just dropped its last open reference; cancel any in-flight
					// semantic-token request for it now that no editor will display the result.
					CancelSemanticTokenRequest(filePath);

					// Only forward the close notification if the server is already running.
					// Starting the server just to send didClose would be wasteful and can race with disposal.
					if (document is null || !_startupSucceeded || !_client.IsReady)
						return false;

					await _client.SendNotificationAsync("textDocument/didClose",
						new LuaDidCloseTextDocumentParams(new LuaTextDocumentIdentifier(document.Uri)), token).ConfigureAwait(false);

					return true;
				},
				cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			// Ignore best-effort close failures.
		}
	}

	private void HandleDiagnosticsPublished(LuaPublishDiagnosticsParams parameters)
	{
		if (!LuaLanguageServerPathHelper.TryGetFilePath(parameters.Uri, out string filePath))
		{
			Log.Debug("Lua diagnostics could not be matched to a local file path.");
			return;
		}

		LuaDocumentSnapshot? document = _documents.GetDocumentSnapshot(filePath);

		// Diagnostics for documents we have never opened are ignored: there is no editor to render them on,
		// and reading the file from disk on the LSP read loop just to discard the result is wasteful.
		if (document is null)
			return;

		if (!LuaLanguageServerDiagnosticsParser.TryParse(parameters, filePath,
			document.Content, document.Version, out LuaPublishedDiagnostics? publishedDiagnostics))
		{
			Log.Debug("Lua diagnostics payload could not be parsed for '{FilePath}'.", filePath);
			return;
		}

		if (!_documents.TryStoreDiagnostics(publishedDiagnostics))
			return;

		RaiseDiagnosticsUpdated(publishedDiagnostics.FilePath, publishedDiagnostics.Diagnostics);
	}

	private static void ObserveBackgroundTask(Task task, string operation)
	{
		_ = ObserveAsync(task, operation);

		static async Task ObserveAsync(Task observedTask, string observedOperation)
		{
			try
			{
				await observedTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{ }
			catch (IOException exception)
			{
				Log.Debug(exception, "Lua language server background operation '{Operation}' failed with a transport error.", observedOperation);
			}
			catch (ObjectDisposedException)
			{ }
			catch (Exception exception)
			{
				Log.Warn(exception, "Lua language server background operation '{Operation}' failed.", observedOperation);
			}
		}
	}
}
