using System;
using System.IO;
using System.Linq;

namespace TombLib.Projects
{
	public class ProjectLevel
	{
		public string Name { get; set; }
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
			DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.prj2", SearchOption.TopDirectoryOnly))
			{
				if (!IsBackupFile(fileInfo.Name))
					return true;
			}

			return false;
		}

		public static bool IsBackupFile(string fileName)
		{
			if (fileName.Length < 9)
				return false;

			// 01-01-0001 || 0001-01-01
			if (DateTime.TryParse(fileName.Substring(0, 9), out _))
				return true;

			if (fileName.Length < 7)
				return false;

			// 01-01-01
			if (DateTime.TryParse(fileName.Substring(0, 7), out _))
				return true;

			return false;
		}
	}
}
