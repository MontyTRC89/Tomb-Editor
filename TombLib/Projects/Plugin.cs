using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TombLib.Projects
{
	public class Plugin
	{
		public string Name { get; set; }
		public string InternalDllPath { get; set; }

		public static Plugin InstallPluginFolder(string pluginFolderPath)
		{
			string pluginName = Path.GetFileName(pluginFolderPath);
			string dllFilePath = Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First();

			string scriptFilePath = Path.Combine(pluginFolderPath, Path.GetFileNameWithoutExtension(dllFilePath) + ".script");
			string btnFilePath = Path.Combine(pluginFolderPath, Path.GetFileNameWithoutExtension(dllFilePath) + ".btn");

			string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (File.Exists(scriptFilePath))
				File.Copy(scriptFilePath, Path.Combine(programPath, "NGC", Path.GetFileName(scriptFilePath)), true);

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
