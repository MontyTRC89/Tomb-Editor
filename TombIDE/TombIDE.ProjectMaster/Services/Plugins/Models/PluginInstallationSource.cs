namespace TombIDE.ProjectMaster.Services.Plugins.Models;

/// <summary>
/// Represents the source type for plugin installation.
/// </summary>
public enum PluginInstallationSourceType
{
	Archive,
	Folder
}

/// <summary>
/// Represents a source from which a plugin can be installed.
/// </summary>
public sealed class PluginInstallationSource
{
	public string Path { get; }
	public PluginInstallationSourceType Type { get; }

	public PluginInstallationSource(string path, PluginInstallationSourceType type)
	{
		Path = path;
		Type = type;
	}
}
