using System.IO;
using System.IO.Compression;
using System.Linq;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.Services.Plugins.Initialization;

/// <summary>
/// TRNG-specific implementation of plugin initialization.
/// </summary>
public sealed class TRNGPluginInitializationService : IPluginInitializationService
{
	public void InitializePluginDirectory(IGameProject project)
	{
		string pluginsPath = project.PluginsDirectoryPath;
		var pluginsDirectory = new DirectoryInfo(pluginsPath);

		if (!pluginsDirectory.Exists || !pluginsDirectory.EnumerateFiles("*", SearchOption.AllDirectories).Any())
		{
			if (!pluginsDirectory.Exists)
			{
				pluginsDirectory.Create();
				pluginsDirectory = new DirectoryInfo(pluginsDirectory.FullName);
			}

			// TODO: Remove .parc support in future versions
			string parcPath = Path.Combine(project.GetEngineRootDirectoryPath(), "plugins.parc");

			if (File.Exists(parcPath))
				CopyPluginsFromPARCToProject(project, parcPath, pluginsDirectory);

			if (Directory.Exists(DefaultPaths.TRNGPluginsDirectory)) // Priority
				CopyPluginsFromTIDEToProject(project, pluginsDirectory);
		}
	}

	private static void CopyPluginsFromTIDEToProject(IGameProject project, DirectoryInfo pluginsDirectory)
	{
		string[] internalPluginDirs = Directory.GetDirectories(DefaultPaths.TRNGPluginsDirectory, "*", SearchOption.TopDirectoryOnly);

		foreach (string pluginFile in Directory.GetFiles(project.GetEngineRootDirectoryPath(), "plugin_*.dll", SearchOption.TopDirectoryOnly))
		{
			string? tidePluginDir = internalPluginDirs.FirstOrDefault(x =>
				Path.GetFileName(x).Equals(Path.GetFileNameWithoutExtension(pluginFile)));

			if (tidePluginDir is not null)
				SharedMethods.CopyFilesRecursively(tidePluginDir, Path.Combine(pluginsDirectory.FullName, Path.GetFileName(tidePluginDir)));
		}
	}

	private static void CopyPluginsFromPARCToProject(IGameProject project, string parcPath, DirectoryInfo pluginsDirectory)
	{
		using ZipArchive parc = ZipFile.OpenRead(parcPath);

		foreach (string pluginFile in Directory.GetFiles(project.GetEngineRootDirectoryPath(), "plugin_*.dll", SearchOption.TopDirectoryOnly))
		{
			foreach (ZipArchiveEntry entry in parc.Entries)
			{
				if (entry.FullName.Split('\\')[0].Equals(Path.GetFileNameWithoutExtension(pluginFile), System.StringComparison.OrdinalIgnoreCase))
				{
					string destPath = Path.Combine(pluginsDirectory.FullName, entry.FullName);
					string? dirName = Path.GetDirectoryName(destPath);

					if (dirName is not null && !Directory.Exists(dirName))
						Directory.CreateDirectory(dirName);

					entry.ExtractToFile(destPath, true);
				}
			}
		}
	}
}
