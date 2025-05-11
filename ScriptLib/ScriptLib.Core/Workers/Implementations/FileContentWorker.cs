using System.IO;
using System.Text;

namespace ScriptLib.Core.Workers.Implementations;

public sealed class FileContentWorker : IFileContentWorker
{
	public string FilePath { get; }
	public bool CreatesBackupFile { get; }

	private readonly FileSystemWatcher _watcher;
	private readonly SemaphoreSlim _cacheLock = new(1, 1);
	private string? _cachedFileContent;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="FileContentWorker"/> class.
	/// </summary>
	/// <param name="filePath">The path to the file to monitor for changes.</param>
	/// <param name="createsBackupFile">Determines whether backup files should be created when content changes.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
	public FileContentWorker(string filePath, bool createsBackupFile)
	{
		FilePath = filePath;
		CreatesBackupFile = createsBackupFile;

		_watcher = new FileSystemWatcher
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
		};

		_watcher.Changed += OnFileChanged;
		_watcher.Deleted += OnFileChanged;
		_watcher.Renamed += OnFileChanged;
		_watcher.Error += OnFileError;

		ConfigureWatcher(filePath);
	}

	public async Task<bool> HasContentChanged(string currentContent, Encoding encoding)
	{
		if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
			return true;

		string fileContent = await ReadFileContentAsync(encoding);
		bool isChanged = currentContent != fileContent;

		if (isChanged && CreatesBackupFile)
			await CreateBackupFileAsync(currentContent, encoding);
		else if (!isChanged && CreatesBackupFile)
			await DeleteBackupFileIfExistsAsync();

		return isChanged;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void ConfigureWatcher(string filePath)
	{
		string? directory = Path.GetDirectoryName(filePath);

		if (!string.IsNullOrEmpty(directory))
		{
			_watcher.Path = directory;
			_watcher.Filter = Path.GetFileName(filePath);
			_watcher.EnableRaisingEvents = true;
		}
	}

	private async void OnFileChanged(object sender, FileSystemEventArgs e) => await InvalidateCacheAsync();
	private async void OnFileError(object sender, ErrorEventArgs e) => await InvalidateCacheAsync();

	private async Task InvalidateCacheAsync()
	{
		await _cacheLock.WaitAsync();

		try
		{
			_cachedFileContent = null;
		}
		finally
		{
			_cacheLock.Release();
		}
	}

	private async Task<string> ReadFileContentAsync(Encoding encoding)
	{
		await _cacheLock.WaitAsync();

		try
		{
			if (_cachedFileContent != null)
				return _cachedFileContent;

			if (!File.Exists(FilePath))
				return string.Empty;

			using var reader = new StreamReader(FilePath, encoding);
			_cachedFileContent = await reader.ReadToEndAsync();
			return _cachedFileContent;
		}
		finally
		{
			_cacheLock.Release();
		}
	}

	private async Task CreateBackupFileAsync(string content, Encoding encoding)
	{
		try
		{
			string backupFilePath = FilePath + ".backup";
			await File.WriteAllTextAsync(backupFilePath, content, encoding);
		}
		catch
		{
			// TODO: Add logging
		}
	}

	private async Task DeleteBackupFileIfExistsAsync()
	{
		try
		{
			string backupFilePath = FilePath + ".backup";

			if (File.Exists(backupFilePath))
				await Task.Run(() => File.Delete(backupFilePath)); // Async to avoid blocking the UI thread
		}
		catch
		{
			// TODO: Add logging
		}
	}

	private void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			_watcher.Changed -= OnFileChanged;
			_watcher.Deleted -= OnFileChanged;
			_watcher.Renamed -= OnFileChanged;
			_watcher.Error -= OnFileError;

			_watcher.Dispose();
			_cacheLock.Dispose();
		}

		_disposed = true;
	}
}
