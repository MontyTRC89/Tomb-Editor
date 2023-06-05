using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	/// <summary>
	/// Base class for all game project types.
	/// </summary>
	public abstract class GameProjectBase : IGameProject
	{
		#region Abstract region

		public abstract TRVersion.Game GameVersion { get; }

		public abstract string DataFileExtension { get; }
		public abstract string EngineExecutableFileName { get; }

		public abstract string GetDefaultGameLanguageFilePath();
		public abstract void SetScriptRootDirectory(string newDirectoryPath);

		#endregion Abstract region

		public Version TargetTrprojVersion { get; set; } = new(1, 0);

		public string Name { get; protected set; }
		public string DirectoryPath { get; protected set; }

		public string MapsDirectoryPath { get; set; }
		public string ScriptDirectoryPath { get; protected set; }
		public string PluginsDirectoryPath { get; set; }

		public string MainScriptFilePath { get; protected set; }
		public string DefaultGameLanguageName { get; set; } = "English";

		public List<string> ExternalMapFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new[] { "English" }.ToList();

		public GameProjectBase(TrprojFile trproj, Version targetTrprojVersion)
		{
			TargetTrprojVersion = targetTrprojVersion;

			Name = trproj.ProjectName;
			DirectoryPath = Path.GetDirectoryName(trproj.FilePath);

			MapsDirectoryPath = trproj.MapsDirectoryPath;
			ScriptDirectoryPath = trproj.ScriptDirectoryPath;
			PluginsDirectoryPath = trproj.PluginsDirectoryPath;

			DefaultGameLanguageName = trproj.DefaultGameLanguageName;

			ExternalMapFilePaths = trproj.ExternalMapFilePaths;
			GameLanguageNames = trproj.GameLanguageNames;
		}

		public virtual string GetTrprojFilePath()
			=> Path.Combine(DirectoryPath, Path.GetFileNameWithoutExtension(GetEngineExecutableFilePath()));

		public virtual string GetLauncherFilePath()
		{
			string launcherFilePath = Directory.EnumerateFiles(DirectoryPath)
				.Where(filePath => FileVersionInfo.GetVersionInfo(filePath).OriginalFilename == "launch.exe")
				.FirstOrDefault();

			if (string.IsNullOrEmpty(launcherFilePath))
			{
				launcherFilePath = GetEngineExecutableFilePath(); // Potentially a legacy project

				if (string.IsNullOrEmpty(launcherFilePath))
					throw new FileNotFoundException("Couldn't find a valid game launching executable."); // Very unlikely to happen
			}

			return launcherFilePath;
		}

		public virtual string GetEngineRootDirectoryPath()
		{
			string engineDirectoryPath = Path.Combine(DirectoryPath, "Engine");

			return Directory.Exists(engineDirectoryPath)
				? engineDirectoryPath // Modern project
				: DirectoryPath; // Legacy project
		}

		public virtual string GetEngineExecutableFilePath()
		{
			string engineExecutableFilePath = Path.Combine(GetEngineRootDirectoryPath(), EngineExecutableFileName);

			return File.Exists(engineExecutableFilePath)
				? engineExecutableFilePath
				: throw new FileNotFoundException("The engine executable file could not be found.");
		}

		public virtual FileInfo[] GetAllValidTrmapFiles()
		{
			var result = new List<FileInfo>();

			result.AddRange(
				from filePath in ExternalMapFilePaths
				where File.Exists(filePath)
				select new FileInfo(filePath)
			);

			var mapsDirectoryInfo = new DirectoryInfo(MapsDirectoryPath);

			foreach (DirectoryInfo mapDirectoryInfo in mapsDirectoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
			{
				FileInfo[] trmapFiles = mapDirectoryInfo.GetFiles("*.trmap", SearchOption.TopDirectoryOnly);

				if (trmapFiles.Length is 0 or > 1)
					continue;
				else
					result.Add(trmapFiles[0]);
			}

			return result.ToArray();
		}

		public virtual MapProject[] GetAllValidMapProjects()
		{
			var result = new List<MapProject>();

			foreach (FileInfo trmapFile in GetAllValidTrmapFiles())
			{
				try { result.Add(MapProject.FromTrmap(trmapFile.FullName)); }
				catch { }
			}

			return result.ToArray();
		}

		public virtual void Rename(string newName, bool renameDirectory)
		{
			if (renameDirectory)
			{
				string newProjectPath = Path.Combine(Path.GetDirectoryName(DirectoryPath), newName);

				if (Directory.Exists(DirectoryPath + "_TEMP")) // The "_TEMP" suffix exists only when the directory name just changed letter cases
					Directory.Move(DirectoryPath + "_TEMP", newProjectPath);
				else
					Directory.Move(DirectoryPath, newProjectPath);

				if (ScriptDirectoryPath.StartsWith(DirectoryPath))
					ScriptDirectoryPath = Path.Combine(newProjectPath, ScriptDirectoryPath.Remove(0, DirectoryPath.Length + 1));

				if (MapsDirectoryPath.StartsWith(DirectoryPath))
					MapsDirectoryPath = Path.Combine(newProjectPath, MapsDirectoryPath.Remove(0, DirectoryPath.Length + 1));

				for (int i = 0; i < ExternalMapFilePaths.Count; i++)
				{
					if (ExternalMapFilePaths[i].StartsWith(DirectoryPath))
						ExternalMapFilePaths[i] = Path.Combine(newProjectPath, ExternalMapFilePaths[i].Remove(0, DirectoryPath.Length + 1));
				}

				DirectoryPath = newProjectPath;
			}

			Name = newName;
		}

		public virtual bool IsValid(out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!Directory.Exists(DirectoryPath))
			{
				errorMessage = "Project directory doesn't exist.";
				return false;
			}

			if (Path.GetFileName(DirectoryPath).Equals("Engine", StringComparison.OrdinalIgnoreCase))
			{
				errorMessage = "Directory name cannot be \"Engine\"."; // LOL you ain't tricking me
				return false;
			}

			if (!Directory.Exists(ScriptDirectoryPath))
			{
				errorMessage = "The project's Script directory is missing.";
				return false;
			}

			if (!Directory.Exists(MapsDirectoryPath))
			{
				errorMessage = "The project's Maps directory is missing.";
				return false;
			}

			try
			{
				GetEngineExecutableFilePath();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}

			return true;
		}

		public virtual void Save()
		{
			if (TargetTrprojVersion == new Version(1, 0))
			{
				// We save the project as a LEGACY .trproj file, since we don't want to enforce new structure yet
				// We simply want to get ready for people to easily migrate in the future while keeping backwards compatibility

				var trproj = new LegacyTrprojFile
				{
					Name = Name,
					GameVersion = GameVersion,
					LevelsPath = MapsDirectoryPath,
					ScriptPath = ScriptDirectoryPath,
					LaunchFilePath = GetLauncherFilePath()
				};

				foreach (MapProject mapProject in GetAllValidMapProjects())
				{
					mapProject.Save();

					trproj.Levels.Add(new LegacyProjectLevel
					{
						Name = mapProject.Name,
						FolderPath = mapProject.DirectoryPath,
						SpecificFile = mapProject.TargetPrj2FileName
					});
				}

				trproj.WriteToFile(GetTrprojFilePath());
			}
			else if (TargetTrprojVersion == new Version(2, 0))
			{
				var trproj = new TrprojFile
				{
					ProjectName = Name,
					TargetGameVersion = GameVersion,

					MapsDirectoryPath = MapsDirectoryPath,
					ScriptDirectoryPath = ScriptDirectoryPath,
					PluginsDirectoryPath = PluginsDirectoryPath,

					DefaultGameLanguageName = DefaultGameLanguageName,

					ExternalMapFilePaths = ExternalMapFilePaths,
					GameLanguageNames = GameLanguageNames
				};

				trproj.WriteToFile(GetTrprojFilePath());
			}
		}

		public static IGameProject FromTrproj(string trprojFilePath)
		{
			TrprojFile trproj = TrprojFile.FromFile(trprojFilePath, out Version targetTrprojVersion);

			return trproj.TargetGameVersion switch
			{
				TRVersion.Game.TR1 => new Tomb1MainGameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR2 => new TR2GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR3 => new TR3GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR4 => new TR4GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TRNG => new TRNGGameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TombEngine => new TENGameProject(trproj, targetTrprojVersion),
				_ => throw new NotSupportedException("The specified .trproj file is for an unsupported game version.")
			};
		}
	}
}
