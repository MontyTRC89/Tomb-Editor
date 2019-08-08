using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TombLib.LevelData;

namespace TombLib.Projects
{
	public class Project
	{
		/// <summary>
		/// Displayed project name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Game engine version. (TR4, TRNG, TR5, ...)
		/// </summary>
		public GameVersion GameVersion { get; set; }

		/// <summary>
		/// The .trproj folder path.
		/// <para>Note: The .trproj file should always be in the exact same folder as the engine's .exe file.</para>
		/// </summary>
		[XmlIgnore]
		public string ProjectPath { get; set; }

		/// <summary>
		/// The path where the project's SCRIPT.TXT and {LANGUAGE}.TXT files are being stored.
		/// </summary>
		public string ScriptPath { get; set; }

		/// <summary>
		/// The path where all the project's newly created / imported levels are being stored.
		/// </summary>
		public string LevelsPath { get; set; }

		/// <summary>
		/// A list of the project's levels.
		/// </summary>
		public List<ProjectLevel> Levels { get; set; } = new List<ProjectLevel>();

		/// <summary>
		/// Creates and returns an exact copy of the current project without overwriting the original data.
		/// </summary>
		public Project Clone()
		{
			Project projectCopy = new Project
			{
				Name = Name,
				GameVersion = GameVersion,
				ProjectPath = ProjectPath,
				ScriptPath = ScriptPath,
				LevelsPath = LevelsPath,
			};

			foreach (ProjectLevel level in Levels)
				projectCopy.Levels.Add(level.Clone());

			return projectCopy;
		}

		/// <summary>
		/// Renames the project (and it's directory if renameDirectory is true).
		/// </summary>
		public void Rename(string newName, bool renameDirectory = true)
		{
			if (renameDirectory)
			{
				// Rename project (Game) directory
				string newProjectPath = Path.Combine(Path.GetDirectoryName(ProjectPath), newName);
				Directory.Move(ProjectPath, newProjectPath);

				// Change ScriptPath / LevelsPath values of the project if they were inside the ProjectPath folder
				if (ScriptPath.StartsWith(ProjectPath))
					ScriptPath = Path.Combine(newProjectPath, ScriptPath.Remove(0, ProjectPath.Length + 1));

				if (LevelsPath.StartsWith(ProjectPath))
					LevelsPath = Path.Combine(newProjectPath, LevelsPath.Remove(0, ProjectPath.Length + 1));

				ProjectPath = newProjectPath;
			}

			Name = newName;
		}

		/// <summary>
		/// Replaces "$(ProjectDirectory)" with the project's .trproj folder path.
		/// <para>This is used to make projects inside TombIDE easier to read for the software.</para>
		/// </summary>
		public void DecodeProjectPaths(string trprojFilePath)
		{
			ProjectPath = Path.GetDirectoryName(trprojFilePath);

			if (ScriptPath.StartsWith("$(ProjectDirectory)"))
				ScriptPath = ScriptPath.Replace("$(ProjectDirectory)", ProjectPath);

			if (LevelsPath.StartsWith("$(ProjectDirectory)"))
				LevelsPath = LevelsPath.Replace("$(ProjectDirectory)", ProjectPath);

			foreach (ProjectLevel level in Levels)
			{
				if (level.FolderPath.StartsWith("$(ProjectDirectory)"))
					level.FolderPath = level.FolderPath.Replace("$(ProjectDirectory)", ProjectPath);
			}
		}

		/// <summary>
		/// Replaces the project's .trproj folder path with "$(ProjectDirectory)".
		/// <para>This is used before saving .trproj files to avoid having "hardcoded" paths.</para>
		/// </summary>
		public void EncodeProjectPaths()
		{
			if (ScriptPath.StartsWith(ProjectPath))
				ScriptPath = ScriptPath.Replace(ProjectPath, "$(ProjectDirectory)");

			if (LevelsPath.StartsWith(ProjectPath))
				LevelsPath = LevelsPath.Replace(ProjectPath, "$(ProjectDirectory)");

			foreach (ProjectLevel level in Levels)
			{
				if (level.FolderPath.StartsWith(ProjectPath))
					level.FolderPath = level.FolderPath.Replace(ProjectPath, "$(ProjectDirectory)");
			}
		}

		/// <summary>
		/// .trproj file name = game's .exe file name. (tomb4, PCTomb5, ...)
		/// <para>Returns null if the paths haven't been decoded yet.</para>
		/// <para>To decode paths use DecodeProjectPaths(string trprojFilePath)</para>
		/// </summary>
		public string GetTRPROJFilePath()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(ProjectPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				if (GameVersion == GameVersion.TR4 || GameVersion == GameVersion.TRNG)
				{
					if (fileInfo.Name.ToLower().Contains("tomb4.exe"))
						return Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.FullName) + ".trproj");
				}
				else if (GameVersion == GameVersion.TR5 || GameVersion == GameVersion.TR5Main)
				{
					if (fileInfo.Name.ToLower().Contains("pctomb5.exe"))
						return Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.FullName) + ".trproj");
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the .exe file name depending on the project's GameVersion. (tomb4.exe, PCTomb5.exe, ...)
		/// </summary>
		public string GetExeFileName()
		{
			switch (GameVersion)
			{
				case GameVersion.TR4:
					return "tomb4.exe";

				case GameVersion.TRNG:
					return "tomb4.exe";

				case GameVersion.TR5:
					return "PCTomb5.exe";

				case GameVersion.TR5Main:
					return "PCTomb5.exe";

				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the level data file extension depending on the project's GameVersion. (.tr4, .trc, ...)
		/// </summary>
		public string GetLevelFileExtension()
		{
			switch (GameVersion)
			{
				case GameVersion.TR4:
					return ".tr4";

				case GameVersion.TRNG:
					return ".tr4";

				case GameVersion.TR5:
					return ".trc";

				case GameVersion.TR5Main:
					return ".trc";

				default:
					return null;
			}
		}

		/// <summary>
		/// Checks if the project is installed correctly.
		/// </summary>
		public bool IsValidProject()
		{
			try
			{
				return IsExeFileValid() && IsScriptFileValid();
			}
			catch
			{
				return false;
			}
		}

		public bool IsExeFileValid()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(ProjectPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				if (fileInfo.Name.ToLower() == "tomb4.exe" || fileInfo.Name.ToLower() == "pctomb5.exe")
					return true;
			}

			return false;
		}

		public bool IsScriptFileValid()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(ScriptPath);

			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				if (fileInfo.Name.ToLower() == "script.txt")
					return true;
			}

			return false;
		}
	}
}
