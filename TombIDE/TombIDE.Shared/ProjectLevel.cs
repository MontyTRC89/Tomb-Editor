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
			try
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

				if (fileNameWithoutExtension.EndsWith(".backup", StringComparison.OrdinalIgnoreCase))
					return true;

				// 05-06-23
				if (DateTime.TryParse(fileNameWithoutExtension[..7], out _) || DateTime.TryParse(fileNameWithoutExtension[^7..], out _))
					return true;

				// 05-06-2023 || 2023-06-05
				if (DateTime.TryParse(fileNameWithoutExtension[..9], out _) || DateTime.TryParse(fileNameWithoutExtension[^9..], out _))
					return true;
			}
			catch { }

			return false;
		}
	}
}
