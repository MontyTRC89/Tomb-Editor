#nullable enable

using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal sealed partial class LuaLanguageServerIntellisenseProvider : ILuaIntellisenseProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
	private const int HardStartupFailureThreshold = 3;

	private readonly string _workspaceApiDirectoryPath;
	private readonly string _workspaceRootDirectoryPath;
	private readonly ILuaLanguageServerClient? _client;
	private readonly LuaIntellisenseDocumentManager _documents = new();
	private readonly object _documentOperationSyncRoot = new();
	private readonly ConcurrentDictionary<string, CancellationTokenSource> _semanticTokenRequests = new(StringComparer.OrdinalIgnoreCase);
	private readonly SemaphoreSlim _startLock = new(1, 1);
	private LuaWorkspaceFileWatcher? _workspaceFileWatcher;
	private Task _queuedDocumentOperation = Task.CompletedTask;

	private bool _startupSucceeded;
	private int _consecutiveStartupFailures;
	private bool _permanentStartupFailureReported;
	private bool _transientStartupFailureReported;
	private volatile bool _isDisposed;

	public bool IsAvailable => !_isDisposed && _client is not null
		&& _consecutiveStartupFailures < HardStartupFailureThreshold;

	public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated;
	public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated;
	public event Action<LuaLanguageServerStartupFailure>? StartupFailed;

	public LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, string? serverExecutablePath)
		: this(workspaceRootDirectoryPath, CreateClient(workspaceRootDirectoryPath, serverExecutablePath))
	{ }

	internal LuaLanguageServerIntellisenseProvider(string workspaceRootDirectoryPath, ILuaLanguageServerClient? client)
	{
		_workspaceRootDirectoryPath = LuaLanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);
		_workspaceApiDirectoryPath = Path.Combine(_workspaceRootDirectoryPath, ".API");
		_client = client;

		if (_client is not null)
		{
			_client.DiagnosticsPublished += HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested += HandleSemanticTokensRefreshRequested;
		}
	}

	private static ILuaLanguageServerClient? CreateClient(string workspaceRootDirectoryPath, string? serverExecutablePath)
	{
		if (string.IsNullOrWhiteSpace(serverExecutablePath))
			return null;

		string normalizedRoot = LuaLanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath);
		return new LuaLanguageServerClient(normalizedRoot, serverExecutablePath,
			() => LuaLanguageServerSettingsFactory.Create(normalizedRoot));
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
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, acquireOpenReference: true,
			refreshSemanticTokens: true, CancellationToken.None), "Document open");

	public void UpdateDocument(string filePath, string content)
		=> ObserveBackgroundTask(TrySynchronizeDocumentAsync(filePath, content, acquireOpenReference: false,
			refreshSemanticTokens: true, CancellationToken.None), "Document change");

	public void CloseDocument(string filePath)
	{
		if (_isDisposed || _client is null || !LuaLanguageServerPathHelper.TryNormalizeLocalPath(filePath, out string normalizedFilePath))
			return;

		ObserveBackgroundTask(CloseDocumentAsync(normalizedFilePath, CancellationToken.None), "Document close");
	}

	private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
	{
		if (_client is null || _consecutiveStartupFailures >= HardStartupFailureThreshold)
			return false;

		if (_startupSucceeded && _client.IsReady)
		{
			EnsureWorkspaceFileWatcherStarted();
			return true;
		}

		await _startLock.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			IReadOnlyList<LuaDocumentSnapshot> documentsToReopen = [];

			if (_consecutiveStartupFailures >= HardStartupFailureThreshold)
				return false;

			if (_startupSucceeded && _client.IsReady)
			{
				EnsureWorkspaceFileWatcherStarted();
				return true;
			}

			if (_startupSucceeded && !_client.IsReady)
			{
				Log.Info("Lua language server connection dropped, restarting and reopening tracked documents.");
				documentsToReopen = _documents.PrepareForRestart();
			}

			_startupSucceeded = await _client.StartAsync(cancellationToken).ConfigureAwait(false);

			if (_startupSucceeded && documentsToReopen.Count > 0)
			{
				_startupSucceeded = await ReopenTrackedDocumentsAsync(documentsToReopen, cancellationToken).ConfigureAwait(false);

				if (!_startupSucceeded)
					Log.Warn("Failed to replay tracked documents after Lua language server restart.");
			}

			if (_startupSucceeded)
			{
				_consecutiveStartupFailures = 0;
				_transientStartupFailureReported = false;
				_permanentStartupFailureReported = false;
				EnsureWorkspaceFileWatcherStarted();
			}
			else
			{
				_consecutiveStartupFailures++;
				bool isPermanentFailure = _consecutiveStartupFailures >= HardStartupFailureThreshold;

				if (isPermanentFailure)
				{
					Log.Error("Lua language server failed to start {Count} times consecutively for workspace '{Workspace}'; IntelliSense is now disabled until the editor is restarted.",
						_consecutiveStartupFailures, _workspaceRootDirectoryPath);
				}
				else
				{
					Log.Warn("Failed to start the Lua language server for workspace '{Workspace}' (attempt {Attempt}/{Threshold}).",
						_workspaceRootDirectoryPath, _consecutiveStartupFailures, HardStartupFailureThreshold);
				}

				ReportStartupFailure(isPermanentFailure);
			}

			return _startupSucceeded;
		}
		finally
		{
			_startLock.Release();
		}
	}

	private void EnsureWorkspaceFileWatcherStarted()
	{
		if (_workspaceFileWatcher is not null || _client is null || string.IsNullOrEmpty(_workspaceRootDirectoryPath))
			return;

		var watcher = new LuaWorkspaceFileWatcher(_workspaceRootDirectoryPath, DispatchWorkspaceFileChangesAsync);

		if (!watcher.Start())
			return;

		_workspaceFileWatcher = watcher;
	}

	private async Task DispatchWorkspaceFileChangesAsync(FileChangeBatch batch, CancellationToken cancellationToken)
	{
		if (_client is null || _isDisposed || batch.Count == 0)
			return;

		var changes = new List<object>(batch.Count);
		bool shouldRefreshConfiguration = false;

		foreach ((string path, FileChangeKind kind) in batch.Entries)
		{
			if (!LuaLanguageServerPathHelper.TryNormalizeLocalPath(path, out string normalizedPath))
				continue;

			shouldRefreshConfiguration |= IsWorkspaceConfigurationPath(normalizedPath);

			changes.Add(new
			{
				uri = LuaLanguageServerPathHelper.CreateFileUri(normalizedPath),
				type = (int)kind
			});
		}

		if (changes.Count == 0)
			return;

		try
		{
			if (shouldRefreshConfiguration)
			{
				await _client.SendNotificationAsync("workspace/didChangeConfiguration",
					new { settings = LuaLanguageServerSettingsFactory.Create(_workspaceRootDirectoryPath) },
					cancellationToken).ConfigureAwait(false);
			}

			await _client.SendNotificationAsync("workspace/didChangeWatchedFiles",
				new { changes }, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to forward workspace file changes to the Lua language server.");
		}
	}

	private bool IsWorkspaceConfigurationPath(string normalizedPath)
	{
		if (string.Equals(normalizedPath, _workspaceApiDirectoryPath, StringComparison.OrdinalIgnoreCase))
			return true;

		string apiDirectoryPrefix = _workspaceApiDirectoryPath + Path.DirectorySeparatorChar;

		if (normalizedPath.StartsWith(apiDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
			return true;

		string fileName = Path.GetFileName(normalizedPath);
		return string.Equals(fileName, ".luarc.json", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(fileName, ".luarc.jsonc", StringComparison.OrdinalIgnoreCase);
	}

	private void ReportStartupFailure(bool isPermanentFailure)
	{
		if (isPermanentFailure)
		{
			if (_permanentStartupFailureReported)
				return;

			_permanentStartupFailureReported = true;
		}
		else
		{
			if (_transientStartupFailureReported)
				return;

			_transientStartupFailureReported = true;
		}

		LuaLanguageServerStartupFailure failure = isPermanentFailure
			? new LuaLanguageServerStartupFailure(
				"The bundled Lua language server failed to start repeatedly and Lua IntelliSense is now disabled until TombIDE is restarted. See the log for technical details.",
				true)
			: new LuaLanguageServerStartupFailure(
				"The bundled Lua language server failed to start. Lua IntelliSense will remain unavailable until TombIDE can start the server successfully. TombIDE will retry automatically when Lua IntelliSense is requested again.",
				false);

		try
		{
			StartupFailed?.Invoke(failure);
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Lua IntelliSense startup-failure notification handler threw.");
		}
	}

	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;

		if (_client is not null)
		{
			_client.DiagnosticsPublished -= HandleDiagnosticsPublished;
			_client.SemanticTokensRefreshRequested -= HandleSemanticTokensRefreshRequested;
		}

		if (_workspaceFileWatcher is not null)
		{
			try
			{
				_workspaceFileWatcher.Dispose();
			}
			catch (Exception exception)
			{
				Log.Debug(exception, "Failed to dispose the Lua workspace file watcher.");
			}
		}

		CancelAllSemanticTokenRequests();
		_startLock.Dispose();
		_client?.Dispose();
	}
}
