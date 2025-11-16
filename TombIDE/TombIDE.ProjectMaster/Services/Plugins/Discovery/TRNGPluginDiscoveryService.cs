using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ProjectMaster.Services.Plugins.Metadata;
using TombIDE.ProjectMaster.Services.Plugins.Models;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Plugins.Discovery;

/// <summary>
/// TRNG-specific implementation of plugin discovery.
/// </summary>
public sealed class TRNGPluginDiscoveryService : IPluginDiscoveryService
{
	private readonly IPluginMetadataService _metadataService;

	public TRNGPluginDiscoveryService(IPluginMetadataService metadataService)
		=> _metadataService = metadataService ?? throw new System.ArgumentNullException(nameof(metadataService));

	public IEnumerable<PluginInfo> DiscoverPlugins(IGameProject project)
	{
		var plugins = new List<PluginInfo>();
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
			return plugins;

		foreach (DirectoryInfo subDirectory in pluginsDirectory.GetDirectories())
		{
			FileInfo? dllFile = subDirectory.GetFiles("plugin_*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();

			if (dllFile is null)
				continue;

			var pluginInfo = _metadataService.ReadPluginMetadata(subDirectory.FullName, dllFile.Name);
			plugins.Add(pluginInfo);
		}

		return plugins;
	}

	public PluginInfo? GetPluginInfo(string pluginDirectoryPath)
	{
		var directory = new DirectoryInfo(pluginDirectoryPath);

		if (!directory.Exists)
			return null;

		FileInfo? dllFile = directory.GetFiles("plugin_*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();

		if (dllFile is null)
			return null;

		return _metadataService.ReadPluginMetadata(pluginDirectoryPath, dllFile.Name);
	}
}
