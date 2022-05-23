using System;
using System.IO;
using System.Linq;

namespace TombIDE.Shared.SharedClasses
{
	public static class PathHelper
	{
		public static string RemoveIllegalPathSymbols(string fileName)
			=> Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

		/// <exception cref="FileNotFoundException" />
		public static string GetScriptFilePath(string scriptRootDirectoryPath, bool isTR1 = false)
		{
			string targetFile = isTR1 ? "Tomb1Main_gameflow.json5" : "Script.txt";
			string targetExtension = isTR1 ? "*.json5" : "*.txt";

			foreach (string file in Directory.GetFiles(scriptRootDirectoryPath, targetExtension, SearchOption.TopDirectoryOnly))
				if (Path.GetFileName(file).Equals(targetFile, StringComparison.OrdinalIgnoreCase))
					return file;

			throw new FileNotFoundException("Script file is missing.");
		}

		/// <exception cref="FileNotFoundException" />
		public static string GetLanguageFilePath(string scriptRootDirectoryPath, GameLanguage language)
		{
			foreach (string file in Directory.GetFiles(scriptRootDirectoryPath, "*.txt", SearchOption.TopDirectoryOnly))
				if (Path.GetFileName(file).Equals($"{language}.txt", StringComparison.OrdinalIgnoreCase))
					return file;

			throw new FileNotFoundException($"{language}.txt is missing.");
		}
	}
}
