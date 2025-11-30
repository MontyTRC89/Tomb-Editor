using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using TombIDE.ProjectMaster.Services.Plugins.Metadata;
using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Installation;

public sealed class TRNGPluginInstallationService : IPluginInstallationService
{
	private const string PluginDllPattern = "plugin_*.dll";
	private const string PluginDllRegexPattern = @"plugin_.*\.dll";

	private readonly IPluginMetadataService _metadataService;

	public TRNGPluginInstallationService(IPluginMetadataService metadataService)
		=> _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));

	public PluginInfo InstallPlugin(IGameProject project, PluginInstallationSource source) => source.Type switch
	{
		PluginInstallationSourceType.Archive => InstallFromArchive(project, source.Path),
		PluginInstallationSourceType.Folder => InstallFromFolder(project, source.Path),
		_ => throw new NotSupportedException($"Installation source type '{source.Type}' is not supported.")
	};

	public void RemovePlugin(IGameProject project, PluginInfo plugin)
	{
		if (plugin?.DllFile is null)
			throw new ArgumentNullException(nameof(plugin));

		// Delete DLL file from engine directory
		string engineDllFilePath = Path.Combine(project.GetEngineRootDirectoryPath(), plugin.DllFile.Name);

		if (File.Exists(engineDllFilePath))
			FileSystem.DeleteFile(engineDllFilePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

		// Delete plugin directory
		if (Directory.Exists(plugin.DirectoryPath))
			FileSystem.DeleteDirectory(plugin.DirectoryPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
	}

	private PluginInfo InstallFromArchive(IGameProject project, string archivePath)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
		{
			pluginsDirectory.Create();
			pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
		}

		using var fileStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
		using var archive = new ZipArchive(fileStream);

		IReadOnlyList<ZipArchiveEntry> dllFileEntries = archive.Entries
			.Where(entry => Regex.IsMatch(entry.Name, PluginDllRegexPattern, RegexOptions.IgnoreCase))
			.ToList();

		if (dllFileEntries.Count == 0)
			throw new ArgumentException("Selected archive doesn't contain a valid plugin DLL file.");
		else if (dllFileEntries.Count > 1)
			throw new ArgumentException("Selected archive contains more than 1 valid plugin .dll file.");

		ZipArchiveEntry dllFileEntry = dllFileEntries[0];

		string dllFileName = dllFileEntry.Name;
		string dllSubPath = dllFileEntry.FullName[..^dllFileName.Length];

		string unzipDirectoryPath = Path.Combine(pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

		if (!Directory.Exists(unzipDirectoryPath))
			Directory.CreateDirectory(unzipDirectoryPath);

		IEnumerable<ZipArchiveEntry> entriesToExtract = archive.Entries.Where(entry =>
			entry.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

		foreach (ZipArchiveEntry entry in entriesToExtract)
		{
			string unzipFilePath = Path.Combine(unzipDirectoryPath, entry.FullName[dllSubPath.Length..]);

			if (string.IsNullOrWhiteSpace(Path.GetExtension(unzipFilePath)))
			{
				if (!Directory.Exists(unzipFilePath))
					Directory.CreateDirectory(unzipFilePath);

				continue;
			}

			string? fileSubDirectory = Path.GetDirectoryName(unzipFilePath);

			if (fileSubDirectory is not null && !Directory.Exists(fileSubDirectory))
				Directory.CreateDirectory(fileSubDirectory);

			entry.ExtractToFile(unzipFilePath, true);
		}

		return _metadataService.ReadPluginMetadata(unzipDirectoryPath, dllFileName);
	}

	private PluginInfo InstallFromFolder(IGameProject project, string folderPath)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
		{
			pluginsDirectory.Create();
			pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
		}

		var selectedDir = new DirectoryInfo(folderPath);

		FileInfo[] dllFiles = selectedDir.GetFiles(PluginDllPattern, System.IO.SearchOption.TopDirectoryOnly);

		if (dllFiles.Length == 0)
			throw new ArgumentException("Selected folder doesn't contain a valid plugin DLL file.");
		else if (dllFiles.Length > 1)
			throw new ArgumentException("Selected folder contains more than 1 valid plugin DLL file.");

		FileInfo dllFile = dllFiles[0];

		string dllFileName = dllFile.Name;
		string dllSubPath = dllFile.FullName[..^dllFileName.Length];

		string copyDirectoryPath = Path.Combine(pluginsDirectory.FullName, Path.GetFileNameWithoutExtension(dllFileName));

		if (!Directory.Exists(copyDirectoryPath))
			Directory.CreateDirectory(copyDirectoryPath);

		IEnumerable<FileInfo> filesToCopy = selectedDir.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly)
			.Where(file => file.FullName.StartsWith(dllSubPath, StringComparison.OrdinalIgnoreCase));

		foreach (FileInfo file in filesToCopy)
		{
			string destFilePath = Path.Combine(copyDirectoryPath, file.FullName[dllSubPath.Length..]);
			file.CopyTo(destFilePath, true);
		}

		return _metadataService.ReadPluginMetadata(copyDirectoryPath, dllFileName);
	}
}
