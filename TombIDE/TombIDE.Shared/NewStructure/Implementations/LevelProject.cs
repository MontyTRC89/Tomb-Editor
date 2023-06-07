using System;
using System.IO;
using System.Linq;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public class LevelProject : ILevelProject
	{
		private string _targetPrj2FileName = null;

		public string TargetPrj2FileName
		{
			get => _targetPrj2FileName ?? GetMostRecentlyModifiedPrj2FileName();
			set => _targetPrj2FileName = value;
		}

		public string Name { get; protected set; } = "UNTITLED MAP";
		public string DirectoryPath { get; protected set; } = string.Empty;

		public LevelProject(string name, string directoryPath, string targetPrj2FileName = null)
		{
			Name = name;
			DirectoryPath = directoryPath;
			TargetPrj2FileName = targetPrj2FileName;
		}

		public FileInfo[] GetPrj2Files(bool includeBackups = false)
		{
			FileInfo[] allPrj2Files = new DirectoryInfo(DirectoryPath).GetFiles("*.prj2", SearchOption.TopDirectoryOnly);

			return includeBackups
				? allPrj2Files
				: allPrj2Files.Where(x => !Prj2Helper.IsBackupFile(x.FullName)).ToArray();
		}

		public string GetMostRecentlyModifiedPrj2FileName()
		{
			foreach (FileInfo file in GetPrj2Files().OrderByDescending(file => file.LastWriteTime))
			{
				if (Prj2Helper.IsBackupFile(file.FullName))
					continue;

				return file.Name;
			}

			return null; // This will only happen when the directory is empty or consists of only backup files
		}

		public string GetTrmapFilePath()
		{
			string[] allTrmapFiles = Directory.GetFiles(DirectoryPath, "*.trlev", SearchOption.TopDirectoryOnly);
			return allTrmapFiles.Length == 0 ? Path.Combine(DirectoryPath, "project.trmap") : allTrmapFiles[0];
		}

		public bool IsValid(out string errorMessage)
		{
			if (!Directory.Exists(DirectoryPath))
			{
				errorMessage = "The map project directory does not exist.";
				return false;
			}

			string[] allPrj2Files = Directory.GetFiles(DirectoryPath, "*.prj2", SearchOption.TopDirectoryOnly);

			if (allPrj2Files.Length == 0)
			{
				errorMessage = "The map project directory does not contain any .prj2 files.";
				return false;
			}

			errorMessage = string.Empty;
			return true;
		}

		public bool IsExternal(string relativeToLevelsDirectoryPath)
			=> !DirectoryPath.StartsWith(relativeToLevelsDirectoryPath, StringComparison.OrdinalIgnoreCase);

		public void Rename(string newName, bool renameDirectory = false)
		{
			if (renameDirectory)
			{
				string newFolderPath = Path.Combine(Path.GetDirectoryName(DirectoryPath), newName);
				Directory.Move(DirectoryPath, newFolderPath);

				DirectoryPath = newFolderPath;
			}

			Name = newName;
		}

		public void Save()
		{
			var trmap = new TrlevFile
			{
				MapName = Name,
				TargetPrj2FileName = TargetPrj2FileName
			};

			trmap.WriteToFile(GetTrmapFilePath());
		}

		public static LevelProject FromTrmap(string trmapFilePath)
		{
			TrlevFile trmap = XmlUtils.ReadXmlFile<TrlevFile>(trmapFilePath);
			return new LevelProject(trmap.MapName, Path.GetDirectoryName(trmapFilePath), trmap.TargetPrj2FileName);
		}
	}
}
