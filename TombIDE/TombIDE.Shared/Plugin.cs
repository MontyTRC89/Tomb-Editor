using System.IO;
using System.Linq;
using System.Text;

namespace TombIDE.Shared
{
	public class Plugin
	{
		public string Name { get; set; }
		public string InternalDllPath { get; set; } = string.Empty;

		public static Plugin InstallPluginFolder(string pluginFolderPath)
		{
			string dllFilePath = Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First();

			// Rename the plugin directory to the DLL file name for better consistency
			string newPluginFolderPath = Path.Combine(Path.GetDirectoryName(pluginFolderPath), Path.GetFileNameWithoutExtension(dllFilePath));

			// Check if the name isn't the correct one already
			if (pluginFolderPath != newPluginFolderPath)
			{
				// Rename the directory if it isn't correct
				Directory.Move(pluginFolderPath, newPluginFolderPath);

				pluginFolderPath = newPluginFolderPath;
				dllFilePath = Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First();
			}

			string pluginName = Path.GetFileName(pluginFolderPath);

			string btnFilePath = Path.Combine(pluginFolderPath, Path.GetFileNameWithoutExtension(dllFilePath) + ".btn");

			if (File.Exists(btnFilePath))
			{
				string[] btnFileContent = File.ReadAllLines(btnFilePath, Encoding.GetEncoding(1252));

				foreach (string line in btnFileContent)
				{
					if (line.StartsWith("NAME#"))
					{
						pluginName = line.Replace("NAME#", string.Empty).Trim();
						break;
					}
				}
			}

			return new Plugin
			{
				Name = pluginName,
				InternalDllPath = dllFilePath
			};
		}
	}
}
