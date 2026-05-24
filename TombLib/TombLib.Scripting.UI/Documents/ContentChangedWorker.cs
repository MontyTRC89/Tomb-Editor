using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TombLib.Scripting.UI.Documents
{
	public class ContentChangedWorker : IDisposable
	{
		#region Properties

		private volatile string _filePath = string.Empty;
		public string FilePath
		{
			get => _filePath;
			set
			{
				if (string.IsNullOrEmpty(value) || !value.Equals(_filePath, StringComparison.OrdinalIgnoreCase))
				{
					DeleteBackupFile();

					lock (_syncRoot)
					{
						_stateVersion++;
						_latestRequestId++;
						_hasPendingRequest = false;
					}
				}

				_filePath = value;
			}
		}

		private volatile bool _createBackupFiles;
		public bool CreateBackupFiles
		{
			get => _createBackupFiles;
			set
			{
				if (value == false)
					DeleteBackupFile();

				_createBackupFiles = value;
			}
		}

		public bool IsBusy => _isBusy;

		#endregion Properties

		#region Events

		public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

		#endregion Events

		#region Fields

		private readonly Dispatcher _dispatcher;
		private readonly object _syncRoot = new();

		private Task? _processingTask;
		private volatile bool _isBusy;
		private bool _hasPendingRequest;
		private int _latestRequestId;
		private int _stateVersion;
		private string _pendingEditorContent = string.Empty;
		private string _persistedContent = string.Empty;

		#endregion Fields

		#region Construction

		public ContentChangedWorker() : this(string.Empty)
		{ }
		public ContentChangedWorker(string filePath) : this(filePath, true)
		{ }
		public ContentChangedWorker(string filePath, bool createBackupFiles)
		{
			_dispatcher = Dispatcher.CurrentDispatcher;
			FilePath = filePath;
			CreateBackupFiles = createBackupFiles;
		}

		#endregion Construction

		#region Disposal

		public void Dispose()
			=> DeleteBackupFile();

		#endregion Disposal

		#region Public methods

		public void RunAsync(string editorContent)
		{
			if (string.IsNullOrEmpty(FilePath))
				return;

			lock (_syncRoot)
			{
				_pendingEditorContent = editorContent ?? string.Empty;
				_hasPendingRequest = true;
				_latestRequestId++;
				_isBusy = true;

				if (_processingTask is null || _processingTask.IsCompleted)
					_processingTask = ProcessLatestRequestAsync();
			}
		}

		public void SetPersistedContent(string persistedContent)
		{
			int stateVersion;
			int requestId;
			string filePath = FilePath;

			lock (_syncRoot)
			{
				_persistedContent = persistedContent ?? string.Empty;
				_pendingEditorContent = _persistedContent;
				_hasPendingRequest = false;
				stateVersion = ++_stateVersion;
				requestId = ++_latestRequestId;
				_isBusy = false;
			}

			_ = SynchronizeBackupStateAsync(filePath, _persistedContent, false, stateVersion, requestId);
		}

		public bool HasChanges(string editorContent)
		{
			lock (_syncRoot)
				return !string.Equals(editorContent ?? string.Empty, _persistedContent, StringComparison.Ordinal);
		}

		public void CreateBackupFile(string editorContent)
			=> _ = SynchronizeBackupStateAsync(FilePath, editorContent ?? string.Empty, true, CaptureStateVersion(), CaptureRequestId());

		public void DeleteBackupFile()
			=> DeleteBackupFileByOriginalPath(FilePath);

		#endregion Public methods

		#region Private methods

		private async Task ProcessLatestRequestAsync()
		{
			while (true)
			{
				string editorContent;
				string persistedContent;
				string filePath;
				int stateVersion;
				int requestId;

				lock (_syncRoot)
				{
					if (!_hasPendingRequest)
					{
						_isBusy = false;
						return;
					}

					editorContent = _pendingEditorContent;
					persistedContent = _persistedContent;
					filePath = _filePath;
					stateVersion = _stateVersion;
					requestId = _latestRequestId;
					_hasPendingRequest = false;
				}

				bool isChanged = !string.Equals(editorContent, persistedContent, StringComparison.Ordinal);
				Exception? error = null;

				try
				{
					await SynchronizeBackupStateAsync(filePath, editorContent, isChanged, stateVersion, requestId).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					error = ex;
					isChanged = true;
				}

				if (!IsLatestRequest(requestId, stateVersion))
					continue;

				await _dispatcher.InvokeAsync(() =>
				{
					if (!IsLatestRequest(requestId, stateVersion))
						return;

					lock (_syncRoot)
						_isBusy = false;

					RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(isChanged, error, false));
				});

				return;
			}
		}

		private async Task SynchronizeBackupStateAsync(string originalFilePath, string editorContent, bool isChanged, int stateVersion, int requestId)
		{
			if (string.IsNullOrWhiteSpace(originalFilePath))
				return;

			string backupFilePath = GetBackupFilePath(originalFilePath);

			if (!CreateBackupFiles || !isChanged)
			{
				DeleteBackupFileByBackupPath(backupFilePath);
				return;
			}

			await File.WriteAllTextAsync(backupFilePath, editorContent ?? string.Empty).ConfigureAwait(false);

			if (!IsLatestRequest(requestId, stateVersion) || !string.Equals(originalFilePath, FilePath, StringComparison.OrdinalIgnoreCase))
				DeleteBackupFileByBackupPath(backupFilePath);
		}

		private static string GetBackupFilePath(string originalFilePath)
			=> originalFilePath + ".backup";

		private void DeleteBackupFileByOriginalPath(string originalFilePath)
		{
			if (string.IsNullOrWhiteSpace(originalFilePath))
				return;

			DeleteBackupFileByBackupPath(GetBackupFilePath(originalFilePath));
		}

		private void DeleteBackupFileByBackupPath(string backupFilePath)
		{
			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath);
		}

		private bool IsLatestRequest(int requestId, int stateVersion)
		{
			lock (_syncRoot)
				return requestId == _latestRequestId && stateVersion == _stateVersion && !_hasPendingRequest;
		}

		private int CaptureStateVersion()
		{
			lock (_syncRoot)
				return _stateVersion;
		}

		private int CaptureRequestId()
		{
			lock (_syncRoot)
				return _latestRequestId;
		}

		#endregion Private methods
	}
}
