using System.Drawing;
using System.IO;

namespace TombIDE.ProjectMaster.Services.Plugins.Models;

/// <summary>
/// Represents information about a plugin.
/// </summary>
public sealed class PluginInfo
{
	/// <summary>
	/// The full path to the plugin DLL file.
	/// </summary>
	public string DllFilePath { get; }

	/// <summary>
	/// The DLL file name (e.g., "plugin_example.dll").
	/// </summary>
	public string DllFileName { get; }

	/// <summary>
	/// The directory containing the plugin files.
	/// </summary>
	public string DirectoryPath { get; }

	/// <summary>
	/// The display name of the plugin.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The description of the plugin, if available.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// The logo/image for the plugin, if available.
	/// </summary>
	public Image? Logo { get; set; }

	/// <summary>
	/// The FileInfo object for the DLL file.
	/// </summary>
	public FileInfo? DllFile { get; }

	public PluginInfo(string dllFilePath)
	{
		DllFilePath = dllFilePath;
		DllFileName = Path.GetFileName(dllFilePath);
		DirectoryPath = Path.GetDirectoryName(dllFilePath) ?? string.Empty;
		Name = Path.GetFileNameWithoutExtension(dllFilePath);

		var dllFile = new FileInfo(dllFilePath);

		if (dllFile.Exists)
			DllFile = dllFile;
	}
}
