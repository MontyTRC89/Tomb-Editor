using System.IO;
using System.Linq;

namespace TombIDE.Shared.SharedClasses
{
	public class PathHelper
	{
		public static string RemoveIllegalPathSymbols(string fileName)
		{
			return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}

		/// <exception cref="FileNotFoundException" />
		public static string GetScriptFilePath(Project targetProject)
		{
			string scriptFilePath = string.Empty;

			// Find the script file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(targetProject.ScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileNameWithoutExtension(file).ToLower() == "script")
				{
					scriptFilePath = file;
					break;
				}
			}

			if (string.IsNullOrEmpty(scriptFilePath))
				throw new FileNotFoundException("Couldn't find the SCRIPT.TXT file.");

			return scriptFilePath;
		}

		/// <exception cref="FileNotFoundException" />
		public static string GetLanguageFilePath(Project targetProject, GameLanguage language)
		{
			string languageFilePath = string.Empty;

			// Find the language file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(targetProject.ScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileNameWithoutExtension(file).ToLower() == language.ToString().ToLower())
				{
					languageFilePath = file;
					break;
				}
			}

			if (string.IsNullOrEmpty(languageFilePath))
				throw new FileNotFoundException("Couldn't find the " + language.ToString().ToUpper() + ".TXT file.");

			return languageFilePath;
		}
	}
}
