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

		public static void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			if (!Directory.Exists(targetPath))
				Directory.CreateDirectory(targetPath);

			// Create directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

			// Copy files
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
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
			if(items != null)
				for (int i = 0; i < items.Length; i++)
					items[i].Dispose();
		}
	}
}
