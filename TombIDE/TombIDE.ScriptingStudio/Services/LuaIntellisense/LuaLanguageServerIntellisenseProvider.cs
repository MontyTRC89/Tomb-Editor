#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed class LuaLanguageServerIntellisenseProvider : ILuaIntellisenseProvider
{
	private readonly string _workspaceRootDirectoryPath;
	private readonly LuaLanguageServerClient? _client;
	private readonly LuaIntellisenseDocumentManager _documents = new();
	private readonly SemaphoreSlim _startLock = new(1, 1);

	private bool _isDisposed;
	private bool _startupSucceeded;

	public bool IsAvailable => !_isDisposed && _client is not null;

	public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;

	public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;

	public LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, string? serverExecutablePath)
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
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetDiagnostics(normalizedFilePath);
	}

	public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
	{
		if (_isDisposed)
			return [];

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return [];

		return _documents.GetSemanticTokens(normalizedFilePath);
	}

	public void OpenDocument(string filePath, string content)
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, CancellationToken.None), "Document open");

	public void UpdateDocument(string filePath, string content)
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, CancellationToken.None), "Document change");

	public void CloseDocument(string filePath)
	{
		if (_isDisposed || _client is null || !LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		if (!_documents.TryClose(normalizedFilePath, out LuaDocumentSnapshot? document))
			return;

		ObserveBackgroundTask(CloseDocumentAsync(document, CancellationToken.None), "Document close");
	}

	public async Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
		int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
	{
		LuaLanguageServerClient? client = _client;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
			|| client is null
			|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
		{
			return [];
		}

		JsonElement response = await client.SendRequestAsync("textDocument/completion",
			new
			{
				textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
				position = new { line, character = column },
				context = BuildCompletionContext(triggerCharacter)
			}, cancellationToken).ConfigureAwait(false);

		IReadOnlyList<JsonElement> itemElements = LuaLanguageServerResponseParser.ExtractCompletionItems(response);

		if (itemElements.Count == 0)
			return [];

		return LuaLanguageServerResponseParser.ParseCompletionItems(itemElements, BuildCompletionItemResolveCallback);
	}

	private static object BuildCompletionContext(char? triggerCharacter)
		=> triggerCharacter is null
			? new { triggerKind = 1 }
			: new { triggerKind = 2, triggerCharacter = triggerCharacter.ToString() };

	private Func<CancellationToken, Task<LuaCompletionItem>>? BuildCompletionItemResolveCallback(JsonElement itemElement, int itemIndex)
	{
		if (_client is null || !_client.SupportsCompletionResolve || !LuaLanguageServerResponseParser.CompletionItemNeedsResolve(itemElement))
			return null;

		return cancellationToken => ResolveCompletionItemAsync(itemElement, itemIndex, cancellationToken);
	}

	private async Task<LuaCompletionItem> ResolveCompletionItemAsync(JsonElement itemElement, int itemIndex, CancellationToken cancellationToken)
	{
		LuaCompletionItem unresolvedItem = LuaLanguageServerResponseParser.ParseCompletionItem(itemElement, itemIndex)
			?? throw new InvalidOperationException("Completion item payload is missing a label.");

		if (_client is null || !_client.SupportsCompletionResolve)
			return unresolvedItem;

		try
		{
			LuaLanguageServerClient? client = _client;

			if (client is null)
				return unresolvedItem;

			JsonElement resolvedItem = await client.SendRequestAsync("completionItem/resolve", itemElement, cancellationToken).ConfigureAwait(false);

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
		catch
		{
			// Ignore
		}

		return unresolvedItem;
	}

	public async Task<LuaHoverInfo?> GetHoverAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		LuaLanguageServerClient? client = _client;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
			|| client is null
			|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
		{
			return null;
		}

		JsonElement response = await client.SendRequestAsync("textDocument/hover",
			new
			{
				textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
				position = new { line, character = column }
			}, cancellationToken).ConfigureAwait(false);

		return LuaLanguageServerResponseParser.ParseHoverInfo(response);
	}

	public async Task<LuaDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		LuaLanguageServerClient? client = _client;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
			|| client is null
			|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
		{
			Debug.WriteLine($"[LuaLS] Definition request skipped because document sync failed for '{filePath}'.");
			return null;
		}

		JsonElement response = await client.SendRequestAsync("textDocument/definition",
			new
			{
				textDocument = new { uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedFilePath) },
				position = new { line, character = column }
			}, cancellationToken).ConfigureAwait(false);

		return LuaLanguageServerResponseParser.ParseDefinitionLocation(response);
	}

	public async Task<LuaSignatureInfo?> GetSignatureHelpAsync(string filePath, string content,
		int line, int column, CancellationToken cancellationToken = default)
	{
		LuaLanguageServerClient? client = _client;

		if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath)
			|| client is null
			|| !await SynchronizeDocumentAsync(normalizedFilePath, content, cancellationToken).ConfigureAwait(false))
		{
			return null;
		}

		JsonElement response = await client.SendRequestAsync("textDocument/signatureHelp",
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

			if (!_startupSucceeded)
				Debug.WriteLine("[LuaLS] Failed to start the Lua language server.");

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

		LuaDocumentSynchronizationRequest? request = _documents.Synchronize(filePath, content);

		if (request is null)
			return true;

		try
		{
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

			await RefreshSemanticTokensAsync(request.Document, cancellationToken).ConfigureAwait(false);
			return true;
		}
		catch (OperationCanceledException)
		{
			throw;
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

	private async Task RefreshSemanticTokensAsync(LuaDocumentSnapshot document, CancellationToken cancellationToken)
	{
		if (_client is null || _client.SemanticTokenTypes.Count == 0)
			return;

		try
		{
			JsonElement response = await _client.SendRequestAsync("textDocument/semanticTokens/range",
				new
				{
					textDocument = new { uri = document.Uri },
					range = BuildDocumentRange(document.Content)
				}, cancellationToken).ConfigureAwait(false);

			IReadOnlyList<LuaSemanticToken> semanticTokens = LuaLanguageServerSemanticTokensParser.Parse(
				response,
				document,
				_client.SemanticTokenTypes,
				_client.SemanticTokenModifiers);

			if (!_documents.TryStoreSemanticTokens(document.FilePath, document.Version, semanticTokens))
				return;

			SemanticTokensUpdated?.Invoke(document.FilePath, semanticTokens);
		}
		catch
		{
			// Ignore semantic token failures and fall back to TextMate syntax highlighting.
		}
	}

	private static object BuildDocumentRange(string? content)
	{
		string normalizedContent = (content ?? string.Empty)
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace('\r', '\n');

		string[] lines = normalizedContent.Split('\n');
		int endLine = Math.Max(0, lines.Length - 1);
		int endCharacter = lines.Length == 0 ? 0 : lines[endLine].Length;

		return new
		{
			start = new { line = 0, character = 0 },
			end = new { line = endLine, character = endCharacter }
		};
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
			Debug.WriteLine("[LuaLS] Diagnostics could not be matched to a local file path.");
			return;
		}

		LuaDocumentSnapshot? document = _documents.GetDocumentSnapshot(filePath);

		if (!LuaLanguageServerDiagnosticsParser.TryParse(parameters, filePath,
			document?.Content, document?.Version ?? 0, out LuaPublishedDiagnostics? publishedDiagnostics))
		{
			Debug.WriteLine($"[LuaLS] Diagnostics payload could not be parsed for '{filePath}'.");
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
		catch (IOException)
		{ }
		catch (ObjectDisposedException)
		{ }
		catch (Exception exception)
		{
			Debug.WriteLine($"[LuaLS] {operation} failed: {exception}");
		}
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
