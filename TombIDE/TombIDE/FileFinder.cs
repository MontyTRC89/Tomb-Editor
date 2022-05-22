using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TombIDE.Shared;

namespace TombIDE
{
	internal static class FileFinder
	{
		/// <exception cref="FileNotFoundException" />
		public static string GetLauncherPathFromProject(Project project)
		{
			if (project.EnginePath.ToLower() == project.ProjectPath.ToLower()) // Old project format
			{
				List<string> validGameExeFiles = GetValidGameExeFiles(project.EnginePath);

				if (validGameExeFiles.Count == 1)
					return validGameExeFiles[0];
				else
					throw new FileNotFoundException("Detected more than one game .exe file in the /Engine/ directory.\n" +
						"Couldn't distinguish the engine type.");
			}
			else // New project format
			{
				List<string> launcherFiles = GetLauncherExecutablesFromDirectory(project.ProjectPath);

				if (launcherFiles.Count == 1)
					return launcherFiles[0];
				else if (launcherFiles.Count > 1)
					throw new FileNotFoundException("Selected project contains more than one launcher executable.\n" +
						"Please check if the project is correctly installed.");
				else
					throw new FileNotFoundException("Selected project doesn't contain any launcher executable.\n" +
						"Please check if the project is correctly installed.");
			}
		}

		/// <summary>
		/// "selectedFilePath" should either be a game .exe file (tomb4.exe, PCTomb5.exe, ...) or a launcher executable (launch.exe, ...).
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		public static string GetLauncherPathFromSelectedFilePath(string selectedFilePath)
		{
			if (FileVersionInfo.GetVersionInfo(selectedFilePath).OriginalFilename == "launch.exe") // If the selected file is a launcher executable
				return selectedFilePath;
			else if (Path.GetFileName(selectedFilePath).Equals("Tomb1Main.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("Tomb2.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("tomb3.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("tomb4.exe", StringComparison.OrdinalIgnoreCase))
			{
				string selectedFileDirectoryName = Path.GetFileName(Path.GetDirectoryName(selectedFilePath));

				if (selectedFileDirectoryName.ToLower() == "engine") // New project format
				{
					string parentDirectory = Path.GetDirectoryName(Path.GetDirectoryName(selectedFilePath));
					List<string> launcherFiles = GetLauncherExecutablesFromDirectory(parentDirectory);

					if (launcherFiles.Count == 1)
						return launcherFiles[0];
					else if (launcherFiles.Count > 1)
						throw new FileNotFoundException("Selected project contains more than one launcher executable.\n" +
							"Please check if the project is correctly installed.");
					else
						throw new FileNotFoundException("Selected project doesn't contain any launcher executable.\n" +
							"Please check if the project is correctly installed.");
				}
				else // Old project format
					return selectedFilePath;
			}
			else
				throw new FileNotFoundException("Invalid game .exe file.");
		}

		/// <summary>
		/// "selectedFilePath" should either be a game .exe file (tomb4.exe, PCTomb5.exe, ...) or a launcher executable (launch.exe, ...).
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		public static string GetGameExePathFromSelectedFilePath(string selectedFilePath)
		{
			if (FileVersionInfo.GetVersionInfo(selectedFilePath).OriginalFilename == "launch.exe") // If the selected file is a launcher executable
			{
				string engineDirectory = Path.Combine(Path.GetDirectoryName(selectedFilePath), "Engine");

				if (Directory.Exists(engineDirectory))
				{
					List<string> validGameExeFiles = GetValidGameExeFiles(engineDirectory);

					if (validGameExeFiles.Count == 1)
						return validGameExeFiles[0];
					else
						throw new FileNotFoundException("Detected more than one game .exe file in the /Engine/ directory.\n" +
							"Couldn't distinguish the engine type.");
				}
				else
					throw new FileNotFoundException("Invalid game .exe file.");
			}
			else if (Path.GetFileName(selectedFilePath).Equals("Tomb1Main.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("Tomb2.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("tomb3.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(selectedFilePath).Equals("tomb4.exe", StringComparison.OrdinalIgnoreCase))
				return selectedFilePath;
			else
				throw new FileNotFoundException("Invalid game .exe file.");
		}

		private static List<string> GetLauncherExecutablesFromDirectory(string directoryPath)
		{
			string[] exeFiles = Directory.GetFiles(directoryPath, "*.exe", SearchOption.TopDirectoryOnly);
			List<string> validExeFiles = new List<string>();

			foreach (string file in exeFiles)
			{
				if (FileVersionInfo.GetVersionInfo(file).OriginalFilename == "launch.exe")
					validExeFiles.Add(file);
			}

			return validExeFiles;
		}

		private static List<string> GetValidGameExeFiles(string engineDirectory)
		{
			List<string> validGameExeFiles = new List<string>();

			foreach (string file in Directory.GetFiles(engineDirectory, "*.exe", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(file).Equals("Tomb1Main.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("Tomb2.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb3.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb4.exe", StringComparison.OrdinalIgnoreCase))
					validGameExeFiles.Add(file);
			}

			return validGameExeFiles;
		}
	}
}
