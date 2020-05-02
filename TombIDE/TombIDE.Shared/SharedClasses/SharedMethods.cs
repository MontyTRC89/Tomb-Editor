using System.Diagnostics;
using System.IO;

namespace TombIDE.Shared.SharedClasses
{
	public static class SharedMethods
	{
		public static void OpenInExplorer(string fileOrDirectoryPath)
		{
			string directoryPath;

			if (File.GetAttributes(fileOrDirectoryPath).HasFlag(FileAttributes.Directory))
				directoryPath = fileOrDirectoryPath;
			else
				directoryPath = Path.GetDirectoryName(fileOrDirectoryPath);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "explorer.exe",
				Arguments = directoryPath
			};

			Process.Start(startInfo);
		}
	}
}
