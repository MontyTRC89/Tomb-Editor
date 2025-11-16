using DarkUI.Forms;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

public sealed class TombEngineUpdateService : IEngineUpdateService
{
	private readonly IFileExtractionService _fileExtractionService;

	public TombEngineUpdateService(IFileExtractionService fileExtractionService)
		=> _fileExtractionService = fileExtractionService ?? throw new ArgumentNullException(nameof(fileExtractionService));

	public bool CanAutoUpdate(Version currentVersion)
	{
		// 1.0.9 is the first version that supports auto-updating
		return currentVersion.Major > 1 || currentVersion.Minor > 0 || currentVersion.Build > 8;
	}

	public string? GetAutoUpdateBlockReason(Version currentVersion)
	{
		if (!CanAutoUpdate(currentVersion))
			return "Cannot Auto-Update engine. Current version is too old.";

		return null;
	}

	public bool UpdateEngine(IGameProject project, Version currentVersion, Version latestVersion, IWin32Window owner)
	{
		if (!CanAutoUpdate(currentVersion))
		{
			MessageBox.Show(owner, GetAutoUpdateBlockReason(currentVersion),
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}

		// In 1.6 onwards we need to upgrade settings file.
		var settingsUpdate16 = latestVersion.Major == 1 && latestVersion.Minor >= 6
			&& currentVersion.Major == 1 && currentVersion.Minor <= 6
			&& currentVersion <= latestVersion;

		var message =
			"This update will replace the following directories and files:\n\n" +

			"- Engine/Bin/\n" +
			"- Engine/Shaders/\n" +
			"- Engine/Scripts/Engine/\n" +
			"- Engine/Scripts/SystemStrings.lua" +

			(settingsUpdate16 ? "\n- Engine/Scripts/Settings.lua" : string.Empty) +

			"\n\n" +
			"If any of these directories / files are important to you, please update the engine manually or create a copy of these files before performing this update.\n\n" +

			"Are you sure you want to continue?\n" +
			"This action cannot be reverted.";

		DialogResult result = MessageBox.Show(owner, message,
			"Warning...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

		if (result is not DialogResult.Yes)
			return false;

		string engineDirectoryPath = Path.Combine(project.DirectoryPath, "Engine");

		if (!Directory.Exists(engineDirectoryPath))
		{
			DarkMessageBox.Show(owner, "Couldn't locate \"Engine\" directory. Updating is not supported for your project structure.",
				"Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}

		try
		{
			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TEN.zip");
			string libsPath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TEN External DLLs.zip");
			string resourcesPath = Path.Combine(DefaultPaths.TemplatesDirectory, "Shared", "TEN Resources.zip");

			using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));
			using var libsArchive = new ZipArchive(File.OpenRead(libsPath));
			using var resourcesArchive = new ZipArchive(File.OpenRead(resourcesPath));

			var bin = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Bin")).ToList();
			bin.AddRange(libsArchive.Entries);

			_fileExtractionService.ExtractEntries(bin, project);

			// Delete the "Engine/Shaders/Bin" directory before extracting new shaders
			string compiledShadersPath = Path.Combine(project.DirectoryPath, "Engine/Shaders/Bin");

			if (Directory.Exists(compiledShadersPath))
				Directory.Delete(compiledShadersPath, true);

			var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Shaders")).ToList();
			_fileExtractionService.ExtractEntries(shaders, project);

			// Delete the "Engine/Scripts/Engine" directory before extracting new scripts
			string engineScriptsPath = Path.Combine(project.DirectoryPath, "Engine/Scripts/Engine");

			if (Directory.Exists(engineScriptsPath))
				Directory.Delete(engineScriptsPath, true);

			var scriptsEngine = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/Scripts/Engine")).ToList();
			_fileExtractionService.ExtractEntries(scriptsEngine, project);

			ZipArchiveEntry? systemStrings = engineArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("SystemStrings.lua"));
			systemStrings?.ExtractToFile(Path.Combine(project.DirectoryPath, systemStrings.FullName), true);

			// Version-specific file updates.
			if (settingsUpdate16)
			{
				ZipArchiveEntry? settings = engineArchive.Entries.FirstOrDefault(entry => entry.Name.Equals("Settings.lua"));
				settings?.ExtractToFile(Path.Combine(project.DirectoryPath, settings.FullName), true);
			}

			// Extract resources, but don't overwrite
			_fileExtractionService.ExtractEntries(resourcesArchive.Entries, project, false);

			UpdateTENApi(project, latestVersion, owner);

			DarkMessageBox.Show(owner, "Engine has been updated successfully!", "Done.",
				MessageBoxButtons.OK, MessageBoxIcon.Information);

			return true;
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(owner, "An error occurred while updating the engine:\n\n" + ex.Message,
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return false;
		}
	}

	private void UpdateTENApi(IGameProject project, Version currentEngineVersion, IWin32Window owner)
	{
		try
		{
			TENApiService.InjectTENApi(project, currentEngineVersion);
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(owner, "An error occurred while updating the API:\n\n" + ex.Message,
				"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
