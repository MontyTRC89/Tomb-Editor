using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombIDE.ProjectMaster.Services.Plugins.Deployment;

/// <summary>
/// TRNG-specific implementation of plugin deployment.
/// </summary>
public sealed class TRNGPluginDeploymentService : IPluginDeploymentService
{
	public void DeployPlugins(IGameProject project)
	{
		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
			return;

		// Copy all plugin DLL files to the engine directory
		foreach (FileInfo dllFile in pluginsDirectory.GetFiles("plugin_*.dll", SearchOption.AllDirectories))
		{
			string destPath = Path.Combine(project.GetEngineRootDirectoryPath(), dllFile.Name);
			dllFile.CopyTo(destPath, true);
		}
	}

	public void HandleScriptReferences(IGameProject project)
	{
		// Delete all .script files from the internal /NGC/ folder
		foreach (string file in Directory.GetFiles(DefaultPaths.InternalNGCDirectory, "plugin_*.script", SearchOption.TopDirectoryOnly))
			File.Delete(file);

		var pluginsDirectory = new DirectoryInfo(project.PluginsDirectoryPath);

		if (!pluginsDirectory.Exists)
			return;

		// Copy all plugin .script files to the internal NGC directory
		foreach (FileInfo scriptFile in pluginsDirectory.GetFiles("plugin_*.script", SearchOption.AllDirectories))
		{
			string destPath = Path.Combine(DefaultPaths.InternalNGCDirectory, scriptFile.Name);
			scriptFile.CopyTo(destPath, true);
		}

		// Refresh mnemonic data
		MnemonicData.SetupConstants(DefaultPaths.InternalNGCDirectory);
	}
}
