using System.Drawing;
using System.IO;
using System.Linq;
using TombIDE.ProjectMaster.Services.Plugins.Models;

namespace TombIDE.ProjectMaster.Services.Plugins.Metadata;

/// <summary>
/// TRNG-specific implementation of plugin metadata reading.
/// </summary>
public sealed class TRNGPluginMetadataService : IPluginMetadataService
{
	public PluginInfo ReadPluginMetadata(string pluginDirectoryPath, string dllFileName)
	{
		string dllFilePath = Path.Combine(pluginDirectoryPath, dllFileName);
		var pluginInfo = new PluginInfo(dllFilePath);

		// Read plugin name from .btn file
		string btnFilePath = Path.Combine(pluginDirectoryPath, Path.GetFileNameWithoutExtension(dllFileName) + ".btn");

		if (File.Exists(btnFilePath))
		{
			string? nameLine = File.ReadAllLines(btnFilePath)
				.FirstOrDefault(line => line.StartsWith("NAME#"));

			if (nameLine is not null)
				pluginInfo.Name = nameLine.Replace("NAME#", string.Empty).Trim();
		}

		// Read description from .txt file
		string descriptionFilePath = Path.Combine(pluginDirectoryPath, Path.GetFileNameWithoutExtension(dllFileName) + ".txt");

		if (File.Exists(descriptionFilePath))
			pluginInfo.Description = File.ReadAllText(descriptionFilePath);

		// Read logo image
		foreach (string file in Directory.GetFiles(pluginDirectoryPath))
		{
			string extension = Path.GetExtension(file).ToLower();

			if (extension is ".jpg" or ".png" or ".bmp" or ".gif")
			{
				try
				{
					using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
					pluginInfo.Logo = Image.FromStream(stream);

					break;
				}
				catch
				{
					// Ignore errors reading image
				}
			}
		}

		return pluginInfo;
	}
}
