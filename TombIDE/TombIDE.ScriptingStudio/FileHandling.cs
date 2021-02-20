using System.IO;

namespace TombIDE.ScriptEditor
{
	internal class FileHandling
	{
		/// <exception cref="FileNotFoundException" />
		public static string GetScriptFilePath(string projectScriptPath)
		{
			string scriptFilePath = string.Empty;

			// Find the script file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(projectScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
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

		/// <summary>
		/// Valid 'language' arguments: "english", "spanish", "german" etc.
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		public static string GetLanguageFilePath(string projectScriptPath, string language)
		{
			string languageFilePath = string.Empty;

			// Find the language file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(projectScriptPath, "*.txt", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileNameWithoutExtension(file).ToLower() == language.ToLower())
				{
					languageFilePath = file;
					break;
				}
			}

			if (string.IsNullOrEmpty(languageFilePath))
				throw new FileNotFoundException("Couldn't find the " + language.ToUpper() + ".TXT file.");

			return languageFilePath;
		}
	}
}
