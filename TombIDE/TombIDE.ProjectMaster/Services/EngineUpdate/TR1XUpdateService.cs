using DarkUI.Forms;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.EngineUpdate;

public sealed class TR1XUpdateService : IEngineUpdateService
{
	private readonly IFileExtractionService _fileExtractionService;

	public TR1XUpdateService(IFileExtractionService fileExtractionService)
		=> _fileExtractionService = fileExtractionService ?? throw new ArgumentNullException(nameof(fileExtractionService));

	public bool CanAutoUpdate(Version currentVersion)
	{
		// 4.8 and 4.15 had breaking changes
		if (currentVersion.Major <= 4 && currentVersion.Minor <= 7)
			return false;

		if (currentVersion.Major <= 4 && currentVersion.Minor <= 14)
			return false;

		return true;
	}

	public string? GetAutoUpdateBlockReason(Version currentVersion)
	{
		if (currentVersion.Major <= 2)
			return "Cannot Auto-Update engine. Current version is too old.";

		if (currentVersion.Major <= 4 && currentVersion.Minor <= 7)
			return "Cannot Auto-Update engine. TR1X 4.8 introduced breaking changes, which require manual migration.";

		if (currentVersion.Major <= 4 && currentVersion.Minor <= 14)
			return "Cannot Auto-Update engine. TR1X 4.15 introduced breaking changes, which require manual migration.";

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

		DialogResult result = MessageBox.Show(owner,
			"This update will replace the following directories and files:\n\n" +

			"- Engine/shaders/\n" +
			"- Engine/TR1X.exe\n" +
			"- Engine/TR1X_ConfigTool.exe\n\n" +

			"If any of these directories / files are important to you, please update the engine manually or create a copy of these files before performing this update.\n\n" +

			"Are you sure you want to continue?\n" +
			"This action cannot be reverted.",
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
			string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TR1.zip");
			using var engineArchive = new ZipArchive(File.OpenRead(enginePresetPath));

			var shaders = engineArchive.Entries.Where(entry => entry.FullName.StartsWith("Engine/shaders")).ToList();
			_fileExtractionService.ExtractEntries(shaders, project);

			var executables = engineArchive.Entries.Where(entry => entry.FullName.EndsWith(".exe")).ToList();
			_fileExtractionService.ExtractEntries(executables, project);

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
}
