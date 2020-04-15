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
			if (Path.GetFileName(project.ProjectPath).ToLower() == "engine")
				return false; // LOL you ain't trickin' me

			return IsExeFileValid(project) && IsScriptFileValid(project);
		}

		public static bool IsExeFileValid(Project project)
		{
			foreach (string file in Directory.GetFiles(project.EnginePath, "*.exe", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(file).ToLower() == "tomb4.exe" || Path.GetFileName(file).ToLower() == "pctomb5.exe")
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
