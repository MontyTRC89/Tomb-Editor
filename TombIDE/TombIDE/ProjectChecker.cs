using System;
using System.IO;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	internal static class ProjectChecker
	{
		public static bool IsProjectNameDuplicate(string projectName)
		{
			foreach (Project project in XmlHandling.GetProjectsFromXml())
			{
				if (project.Name.ToLower() == projectName.ToLower())
					return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if the project is installed correctly.
		/// </summary>
		public static bool IsValidProject(Project project)
		{
			string message; // For Lwmte's outdated VS
			return IsValidProject(project, out message);
		}

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

			if (!IsScriptFileValid(project))
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
				if (Path.GetFileName(file).Equals("Tomb2.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb3.exe", StringComparison.OrdinalIgnoreCase)
				|| Path.GetFileName(file).Equals("tomb4.exe", StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		public static bool IsScriptFileValid(Project project)
		{
			foreach (string file in Directory.GetFiles(project.ScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(file).ToLower() == "script.txt")
					return true;
			}

			return false;
		}
	}
}
