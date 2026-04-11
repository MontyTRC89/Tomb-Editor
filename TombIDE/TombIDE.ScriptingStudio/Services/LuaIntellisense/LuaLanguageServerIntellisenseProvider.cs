using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal sealed class LuaLanguageServerIntellisenseProvider : ILuaIntellisenseProvider
	{
		private readonly string _workspaceRootDirectoryPath;
		private readonly LuaLanguageServerClient _client;
		private readonly LuaIntellisenseDocumentManager _documents = new();
		private readonly SemaphoreSlim _startLock = new(1, 1);

		private bool _isDisposed;
		private bool _startupSucceeded;

		public bool IsAvailable => !_isDisposed && _client is not null;
		public event Action<string, IReadOnlyList<TextEditorDiagnostic>> DiagnosticsUpdated;

		public LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, string serverExecutablePath)
		{
			_workspaceRootDirectoryPath = workspaceRootDirectoryPath is null
				? throw new ArgumentNullException(nameof(workspaceRootDirectoryPath))
				: LuaLanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);

			if (!string.IsNullOrWhiteSpace(serverExecutablePath))
			{
				_client = new LuaLanguageServerClient(_workspaceRootDirectoryPath, serverExecutablePath, BuildLuaSettings);
				_client.DiagnosticsPublished += HandleDiagnosticsPublished;
			}
		}

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
		{
			if (_isDisposed)
				return Array.Empty<TextEditorDiagnostic>();

			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
				return Array.Empty<TextEditorDiagnostic>();

			return _documents.GetDiagnostics(normalizedFilePath);
		}

		public void OpenDocument(string filePath, string content)
			=> _ = TrySynchronizeDocumentAsync(filePath, content, CancellationToken.None);

		public void UpdateDocument(string filePath, string content)
			=> _ = TrySynchronizeDocumentAsync(filePath, content, CancellationToken.None);

		public void CloseDocument(string filePath)
		{
			if (_isDisposed || _client is null || !LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
				return;

			if (!_documents.TryClose(normalizedFilePath, out LuaDocumentSnapshot document))
				return;

			_ = CloseDocumentAsync(document, CancellationToken.None);
		}

		public async Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
				|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
			{
				return Array.Empty<LuaCompletionItem>();
			}

			JsonElement response = await _client.SendRequestAsync("textDocument/completion",
				new
				{
					textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
					position = new { line, character = column },
					context = new { triggerKind = 1 }
				}, cancellationToken).ConfigureAwait(false);

			return LuaLanguageServerResponseParser.ParseCompletionItems(response);
		}

		public async Task<LuaHoverInfo> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
				|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
			{
				return null;
			}

			JsonElement response = await _client.SendRequestAsync("textDocument/hover",
				new
				{
					textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
					position = new { line, character = column }
				}, cancellationToken).ConfigureAwait(false);

			return LuaLanguageServerResponseParser.ParseHoverInfo(response);
		}

		public async Task<LuaDefinitionLocation> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
				|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
			{
				Debug.WriteLine($"[LuaLS] Definition request aborted: document sync failed for '{filePath}'.");
				return null;
			}

			JsonElement response = await _client.SendRequestAsync("textDocument/definition",
				new
				{
					textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
					position = new { line, character = column }
				}, cancellationToken).ConfigureAwait(false);

			LuaDefinitionLocation result = LuaLanguageServerResponseParser.ParseDefinitionLocation(response);
			Debug.WriteLine($"[LuaLS] Definition at ({line},{column}): {(result is not null ? $"'{result.FilePath}' L{result.LineNumber}" : "null")}.");
			return result;
		}

		public async Task<LuaSignatureInfo> GetSignatureHelpAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
				|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
			{
				return null;
			}

			JsonElement response = await _client.SendRequestAsync("textDocument/signatureHelp",
				new
				{
					textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
					position = new { line, character = column }
				}, cancellationToken).ConfigureAwait(false);

			return LuaLanguageServerResponseParser.ParseSignatureHelp(response);
		}

		private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
		{
			if (_client is null)
				return false;

			if (_startupSucceeded && _client.IsReady)
				return true;

			await _startLock.WaitAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				if (_startupSucceeded && _client.IsReady)
					return true;

				if (_startupSucceeded && !_client.IsReady)
				{
					Debug.WriteLine("[LuaLS] Server connection dropped, restarting and reopening tracked documents.");
					_documents.PrepareForRestart();
				}

				_startupSucceeded = await _client.StartAsync(cancellationToken).ConfigureAwait(false);

				Debug.WriteLine($"[LuaLS] Server startup result: {_startupSucceeded}.");
				return _startupSucceeded;
			}
			finally
			{
				_startLock.Release();
			}
		}

		private Task<bool> TrySynchronizeDocumentAsync(string filePath, string content, CancellationToken cancellationToken)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
				return Task.FromResult(false);

			return SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken);
		}

		private async Task<bool> SynchronizeDocumentAsync(string filePath, string content, CancellationToken cancellationToken)
		{
			if (_isDisposed || string.IsNullOrWhiteSpace(filePath) || _client is null)
				return false;

			if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
				return false;

			LuaDocumentSynchronizationRequest request = _documents.Synchronize(filePath, content);

			if (request is null)
				return true;

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
				await _client.SendNotificationAsync("textDocument/didChange",
					new
					{
						textDocument = new
						{
							uri = request.Document.Uri,
							version = request.Document.Version
						},
						contentChanges = new[]
						{
							new { text = request.Document.Content }
						}
					}, cancellationToken).ConfigureAwait(false);
			}

			return true;
		}

		private async Task CloseDocumentAsync(LuaDocumentSnapshot document, CancellationToken cancellationToken)
		{
			if (_client is null)
				return;

			try
			{
				if (!await EnsureStartedAsync(cancellationToken).ConfigureAwait(false))
					return;

				await _client.SendNotificationAsync("textDocument/didClose",
					new { textDocument = new { uri = document.Uri } }, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				// Ignore best-effort close failures.
			}
		}

		private object BuildLuaSettings()
			=> LuaLanguageServerSettingsFactory.Create(_workspaceRootDirectoryPath);

		private void HandleDiagnosticsPublished(JsonElement parameters)
		{
			if (!LuaLanguageServerPathHelper.TryGetFilePath(parameters, out string filePath))
			{
				Debug.WriteLine("[LuaLS] Diagnostics rejected: unable to resolve file path.");
				return;
			}

			LuaDocumentSnapshot document = _documents.GetDocumentSnapshot(filePath);

			if (!LuaLanguageServerDiagnosticsParser.TryParse(parameters, filePath,
				document?.Content, document?.Version ?? 0, out LuaPublishedDiagnostics publishedDiagnostics))
			{
				Debug.WriteLine($"[LuaLS] Diagnostics rejected for '{filePath}'.");
				return;
			}

			if (!_documents.TryStoreDiagnostics(publishedDiagnostics))
			{
				Debug.WriteLine($"[LuaLS] Diagnostics ignored as stale for '{publishedDiagnostics.FilePath}'.");
				return;
			}

			Debug.WriteLine($"[LuaLS] Publishing {publishedDiagnostics.Diagnostics.Count} diagnostics for '{publishedDiagnostics.FilePath}'.");
			DiagnosticsUpdated?.Invoke(publishedDiagnostics.FilePath, publishedDiagnostics.Diagnostics);
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;
			if (_client is not null)
				_client.DiagnosticsPublished -= HandleDiagnosticsPublished;
			_startLock.Dispose();
			_client?.Dispose();
		}
	}
}