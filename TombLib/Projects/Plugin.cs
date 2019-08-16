using System.Collections.Generic;
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
		public List<string> ReferenceFilePaths { get; set; } = new List<string>();

		public static Plugin InstallPluginFolder(string pluginFolderPath)
		{
			string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			List<string> referenceFilePaths = new List<string>();

			foreach (string file in Directory.GetFiles(pluginFolderPath, "*.script"))
			{
				string destPath = Path.Combine(programPath, "NGC", Path.GetFileName(file));
				File.Copy(file, destPath, true);
				referenceFilePaths.Add(destPath);
			}

			string pluginName = Path.GetFileName(pluginFolderPath);

			if (Directory.GetFiles(pluginFolderPath, "*.btn").Length > 0)
			{
				string btnFilePath = Directory.GetFiles(pluginFolderPath, "*.btn").First();
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

			string dllFilePath = Directory.GetFiles(pluginFolderPath, "plugin_*.dll").First();

			return new Plugin
			{
				Name = pluginName,
				InternalDllPath = dllFilePath,
				ReferenceFilePaths = referenceFilePaths
			};
		}
	}
}
