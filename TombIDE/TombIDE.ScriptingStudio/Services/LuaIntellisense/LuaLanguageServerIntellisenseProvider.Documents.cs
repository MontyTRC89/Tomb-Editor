#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerIntellisenseProvider
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

	private static async Task RunQueuedDocumentOperationAsync<TResult>(Task previousOperation,
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

	private Task<bool> TrySynchronizeDocumentAsync(string filePath, string content,
		bool acquireOpenReference, bool refreshSemanticTokens, CancellationToken cancellationToken)
	{
		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return Task.FromResult(false);

		return SynchronizeDocumentAsync(normalizedFilePath, content, acquireOpenReference, refreshSemanticTokens, cancellationToken);
	}

	private async Task<bool> SynchronizeDocumentAsync(string filePath, string content,
		bool acquireOpenReference, bool refreshSemanticTokens, CancellationToken cancellationToken)
	{
		if (_isDisposed || string.IsNullOrWhiteSpace(filePath) || _client is null)
			return false;

		try
		{
			(bool success, LuaDocumentSnapshot? document) = await EnqueueDocumentOperationAsync(
				token => SynchronizeDocumentCoreAsync(filePath, content, acquireOpenReference, token),
				cancellationToken).ConfigureAwait(false);

			if (!success)
				return false;

			if (refreshSemanticTokens && document is not null)
				await RefreshSemanticTokensAsync(document, cancellationToken).ConfigureAwait(false);

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

	private async Task<(bool Success, LuaDocumentSnapshot? Document)> SynchronizeDocumentCoreAsync(
		string filePath,
		string content,
		bool acquireOpenReference,
		CancellationToken cancellationToken)
	{
		if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
			return (false, null);

		LuaDocumentSynchronizationRequest? request = _documents.Synchronize(filePath, content, acquireOpenReference);

		if (request is not { } pendingRequest)
			return (true, null);

		await SendDocumentSynchronizationNotificationAsync(pendingRequest, cancellationToken).ConfigureAwait(false);
		return (true, pendingRequest.Document);
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
				new
				{
					textDocument = new
					{
						uri = request.Document.Uri,
						languageId = "lua",
						version = request.Document.Version,
						text = request.Document.Content
					}
				}, cancellationToken).ConfigureAwait(false);
		}
		else if (request.Kind == LuaDocumentSynchronizationKind.Change)
		{
			object contentChange = _client.TextDocumentSyncKind switch
			{
				LuaTextDocumentSyncKind.Incremental when request.ChangeRange is { } changeRange => new
				{
					range = new
					{
						start = new { line = changeRange.StartLine, character = changeRange.StartCharacter },
						end = new { line = changeRange.EndLine, character = changeRange.EndCharacter }
					},
					text = changeRange.Text
				},
				LuaTextDocumentSyncKind.Full => new { text = request.Document.Content },
				LuaTextDocumentSyncKind.Incremental => new { text = request.Document.Content },
				_ => throw new InvalidOperationException(
					"The Lua language server does not support document changes required by TombIDE.")
			};

			await _client.SendNotificationAsync("textDocument/didChange",
				new
				{
					textDocument = new
					{
						uri = request.Document.Uri,
						version = request.Document.Version
					},
					contentChanges = new[] { contentChange }
				}, cancellationToken).ConfigureAwait(false);
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
					if (!_startupSucceeded || !_client.IsReady)
						return false;

					await _client.SendNotificationAsync("textDocument/didClose",
						new { textDocument = new { uri = document.Uri } }, token).ConfigureAwait(false);

					return true;
				},
				cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			// Ignore best-effort close failures.
		}
	}

	private void HandleDiagnosticsPublished(JsonElement parameters)
	{
		if (!LuaLanguageServerPathHelper.TryGetFilePath(parameters, out string filePath))
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

		DiagnosticsUpdated?.Invoke(publishedDiagnostics.FilePath, publishedDiagnostics.Diagnostics);
	}

	private static void ObserveBackgroundTask(Task task, string operation)
		=> _ = ObserveBackgroundTaskAsync(task, operation);

	private static async Task ObserveBackgroundTaskAsync(Task task, string operation)
	{
		try
		{
			await task.ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{ }
		catch (IOException exception)
		{
			Log.Debug(exception, "Lua language server background operation '{Operation}' failed with a transport error.", operation);
		}
		catch (ObjectDisposedException)
		{ }
		catch (Exception exception)
		{
			Log.Warn(exception, "Lua language server background operation '{Operation}' failed.", operation);
		}
	}
}
