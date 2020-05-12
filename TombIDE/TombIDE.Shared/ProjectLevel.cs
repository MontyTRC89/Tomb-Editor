using System;
using System.IO;
using System.Linq;

namespace TombIDE.Shared
{
	public class ProjectLevel
	{
		public string Name { get; set; }
		public string DataFileName { get; set; }
		public string FolderPath { get; set; }
		public string SpecificFile { get; set; } = "$(LatestFile)";

		public ProjectLevel Clone()
		{
			return new ProjectLevel
			{
				Name = Name,
				FolderPath = FolderPath,
				SpecificFile = SpecificFile
			};
		}

		public void Rename(string newName, bool renameDirectory = true)
		{
			if (renameDirectory)
			{
				string newFolderPath = Path.Combine(Path.GetDirectoryName(FolderPath), newName);

				if (Directory.Exists(FolderPath + "_TEMP")) // The "_TEMP" suffix exists only when the directory name just changed letter cases
					Directory.Move(FolderPath + "_TEMP", newFolderPath);
				else
					Directory.Move(FolderPath, newFolderPath);

				FolderPath = newFolderPath;
			}

			Name = newName;
		}

		public string GetLatestPrj2File()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.prj2", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime))
			{
				if (IsBackupFile(fileInfo.Name))
					continue;

				return fileInfo.Name;
			}

			return null; // This will only happen when the folder is empty or consists of only backup files
		}

		public bool IsValidLevel()
		{
			if (Directory.Exists(FolderPath))
			{
				if (FolderContainsLevelFiles())
					return true;
			}

			return false;
		}

		private bool FolderContainsLevelFiles()
		{
			foreach (string file in Directory.GetFiles(FolderPath, "*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!IsBackupFile(Path.GetFileName(file)))
					return true;
			}

			return false;
		}

		public static bool IsBackupFile(string fileName)
		{
			if (fileName.Length < 9)
				return false;

			DateTime dateTime;

			// 01-01-0001 || 0001-01-01
			if (DateTime.TryParse(fileName.Substring(0, 9), out dateTime))
				return true;

			if (fileName.Length < 7)
				return false;

			// 01-01-01
			if (DateTime.TryParse(fileName.Substring(0, 7), out dateTime))
				return true;

			return false;
		}
	}
}
