using System;
using System.IO;
using TombIDE.Shared;

namespace TombIDE
{
	internal static class ProjectChecker
	{
		/// <summary>
		/// Checks if the project is installed correctly.
		/// </summary>
		public static bool IsValidProject(Project project)
			=> IsValidProject(project, out _);

		/// <summary>
		/// Checks if the project is installed correctly.
		/// </summary>
		public static bool IsValidProject(Project project, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!Directory.Exists(project.ProjectPath))
			{
				errorMessage = "Project directory doesn't exist.";
				return false;
			}

			if (Path.GetFileName(project.ProjectPath).Equals("engine", StringComparison.OrdinalIgnoreCase))
			{
				errorMessage = "Directory name cannot be \"Engine\"."; // LOL you ain't trickin' me
				return false;
			}

			if (!IsExeFileValid(project))
			{
				errorMessage = "The game's .exe file is either invalid or missing.";
				return false;
			}

			if (!Directory.Exists(project.ScriptPath))
			{
				errorMessage = "The project's /Script/ directory is missing.";
				return false;
			}

			if (!Directory.Exists(project.LevelsPath))
			{
				errorMessage = "The project's /Levels/ directory is missing.";
				return false;
			}

			if (project.GameVersion != TombLib.LevelData.TRVersion.Game.TR1 && project.GameVersion != TombLib.LevelData.TRVersion.Game.TombEngine && !IsScriptFileValid(project))
			{
				errorMessage = "The project does not have a valid SCRIPT.TXT file.";
				return false;
			}

			return true;
		}

		public static bool IsExeFileValid(Project project)
		{
			foreach (string file in Directory.GetFiles(project.EnginePath, "*.exe", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(file).Equals("Tomb1Main.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("Tomb2.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb3.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb4.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("TombEngine.exe", StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		public static bool IsScriptFileValid(Project project) // TODO: T1M and TEN also have to have their script files checked !!!
		{
			foreach (string file in Directory.GetFiles(project.ScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(file).Equals("script.txt", StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}
	}
}
