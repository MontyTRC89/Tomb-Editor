using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

/// <summary>
/// Base class for game archive creation services.
/// Provides common functionality for creating distribution archives.
/// </summary>
public abstract class GameArchiveServiceBase : IGameArchiveService
{
	public event EventHandler<ArchiveProgressEventArgs>? ProgressChanged;

	protected void OnProgressChanged(string operation, int filesProcessed, int totalFiles)
	{
		ProgressChanged?.Invoke(this, new ArchiveProgressEventArgs
		{
			CurrentOperation = operation,
			FilesProcessed = filesProcessed,
			TotalFiles = totalFiles
		});
	}

	public abstract bool SupportsGameVersion(IGameProject project);

	public async Task CreateGameArchiveAsync(IGameProject project, string outputPath, string? readmeText = null)
	{
		if (!SupportsGameVersion(project))
			throw new InvalidOperationException($"This service does not support {project.GameVersion}");

		string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

		try
		{
			if (!Directory.Exists(tempDirectory))
				Directory.CreateDirectory(tempDirectory);

			OnProgressChanged("Preparing archive structure...", 0, 100);

			string engineDirectory = project.GetEngineRootDirectoryPath();
			string targetTempEngineDirectory = Path.Combine(tempDirectory, "Engine");

			// Get engine-specific folders and files
			var importantFolders = GetImportantFolders(engineDirectory);
			var importantFiles = GetImportantFiles(engineDirectory);

			int totalItems = importantFolders.Count + importantFiles.Count + (CreateSavesFolder() ? 1 : 0);
			int processedItems = 0;

			// Copy folders
			foreach (string folder in importantFolders)
			{
				if (!Directory.Exists(folder))
					continue;

				string pathPart = folder[engineDirectory.Length..];
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				OnProgressChanged($"Copying folder: {Path.GetFileName(folder)}", processedItems++, totalItems);
				await Task.Run(() => SharedMethods.CopyFilesRecursively(folder, targetPath));
			}

			// Create saves folder if needed
			if (CreateSavesFolder())
			{
				OnProgressChanged("Creating saves folder...", processedItems++, totalItems);
				Directory.CreateDirectory(Path.Combine(targetTempEngineDirectory, "saves"));
			}

			// Copy files
			foreach (string file in importantFiles)
			{
				if (!File.Exists(file))
					continue;

				string pathPart = file[engineDirectory.Length..];
				string targetPath = Path.Combine(targetTempEngineDirectory, pathPart.Trim('\\'));

				string? targetDirectory = Path.GetDirectoryName(targetPath);

				if (targetDirectory is not null && !Directory.Exists(targetDirectory))
					Directory.CreateDirectory(targetDirectory);

				OnProgressChanged($"Copying file: {Path.GetFileName(file)}", processedItems++, totalItems);
				await Task.Run(() => File.Copy(file, targetPath));
			}

			// Copy launcher
			string launchFilePath = project.GetLauncherFilePath();

			if (File.Exists(launchFilePath))
			{
				OnProgressChanged("Copying launcher...", processedItems++, totalItems);
				await Task.Run(() => File.Copy(launchFilePath, Path.Combine(tempDirectory, Path.GetFileName(launchFilePath)), true));
			}

			// Create README if provided
			if (!string.IsNullOrWhiteSpace(readmeText))
			{
				OnProgressChanged("Creating README.txt...", processedItems++, totalItems);
				await File.WriteAllTextAsync(Path.Combine(tempDirectory, "README.txt"), readmeText);
			}

			// Create the archive
			if (File.Exists(outputPath))
				File.Delete(outputPath);

			OnProgressChanged("Creating ZIP archive...", processedItems++, totalItems);
			await Task.Run(() => ZipFile.CreateFromDirectory(tempDirectory, outputPath));

			OnProgressChanged("Archive creation complete!", totalItems, totalItems);
		}
		finally
		{
			// Cleanup temp directory
			if (Directory.Exists(tempDirectory))
			{
				try
				{
					Directory.Delete(tempDirectory, true);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}
		}
	}

	protected abstract IReadOnlyList<string> GetImportantFolders(string engineDirectory);
	protected abstract IReadOnlyList<string> GetImportantFiles(string engineDirectory);

	/// <summary>
	/// Gets the common files that are present in all game versions.
	/// Derived classes should call this and add their specific files.
	/// </summary>
	protected virtual IReadOnlyList<string> GetCommonFiles(string engineDirectory) => new List<string>
	{
		Path.Combine(engineDirectory, "splash.bmp"),
		Path.Combine(engineDirectory, "splash.xml")
	};

	protected virtual bool CreateSavesFolder() => false;
}
