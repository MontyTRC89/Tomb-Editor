using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		public abstract void Save();

		#endregion Abstract region

		public string Name { get; protected set; }
		public string DirectoryPath { get; protected set; }

		public string MapsDirectoryPath { get; set; }
		public string ScriptRootDirectoryPath { get; protected set; }

		public string MainScriptFilePath { get; protected set; }
		public string DefaultGameLanguageName { get; set; } = "English";

		public List<string> ExternalMapFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new[] { "English" }.ToList();

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

		public virtual string GetEngineDirectoryPath()
		{
			string engineDirectoryPath = Path.Combine(DirectoryPath, "Engine");

			return Directory.Exists(engineDirectoryPath)
				? engineDirectoryPath // Modern project
				: DirectoryPath; // Legacy project
		}

		public virtual string GetEngineExecutableFilePath()
		{
			string engineExecutableFilePath = Path.Combine(GetEngineDirectoryPath(), EngineExecutableFileName);

			return File.Exists(engineExecutableFilePath)
				? engineExecutableFilePath
				: throw new FileNotFoundException("The engine executable file could not be found.");
		}

		public virtual string[] GetAllValidMapFilePaths()
		{
			var result = new List<string>();

			string[] validExtermalMapFilePaths = ExternalMapFilePaths.Where(filePath => File.Exists(filePath)).ToArray();
			result.AddRange(validExtermalMapFilePaths);

			var mapsDirectoryInfo = new DirectoryInfo(MapsDirectoryPath);

			foreach (DirectoryInfo mapDirectoryInfo in mapsDirectoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
			{
				FileInfo[] trmapFiles = mapDirectoryInfo.GetFiles("*.trmap", SearchOption.TopDirectoryOnly);

				if (trmapFiles.Length is 0 or > 1)
					continue;
				else
					result.Add(trmapFiles[0].FullName);
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

				if (ScriptRootDirectoryPath.StartsWith(DirectoryPath))
					ScriptRootDirectoryPath = Path.Combine(newProjectPath, ScriptRootDirectoryPath.Remove(0, DirectoryPath.Length + 1));

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

			if (!Directory.Exists(ScriptRootDirectoryPath))
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
	}
}
