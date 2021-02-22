using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

		public static void DeleteFiles(string[] files)
		{
			foreach (string file in files)
				if (File.Exists(file))
					File.Delete(file);
		}

		public static void DisposeItems(IEnumerable<IDisposable> items)
			=> DisposeItems(items.ToArray());

		public static void DisposeItems(params IDisposable[] items)
		{
			for (int i = 0; i < items.Length; i++)
				items[i].Dispose();
		}
	}
}
