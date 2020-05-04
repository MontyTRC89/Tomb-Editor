using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.LevelData;

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

		public static string GetConfigsPath()
		{
			return Path.Combine(GetProgramDirectory(), "Configs");
		}

		public static string GetTIDESubFolderPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE");
		}

		public static string GetInternalNGCPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC");
		}

		public static string GetVGEPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE");
		}

		public static string GetVGEScriptPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE", "Script");
		}

		public static string GetReferencesPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "References");
		}

		public static string GetMnemonicDefinitionsPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "References", "Mnemonics");
		}

		public static string GetOCBDefinitionsPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "References", "OCBs");
		}

		public static string GetOLDCommandDefinitionsPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "References", "OLD Commands");
		}

		public static string GetNEWCommandDefinitionsPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "References", "NEW Commands");
		}

		public static string GetEngineTemplatesPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "Engines");
		}

		public static string GetSharedTemplatesPath(TRVersion.Game gameVersion)
		{
			if (gameVersion == TRVersion.Game.TR4 || gameVersion == TRVersion.Game.TRNG)
				return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB4");
			else if (gameVersion == TRVersion.Game.TR5Main)
				return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB5");

			return string.Empty;
		}

		public static string GetDefaultTemplatesPath(TRVersion.Game gameVersion)
		{
			if (gameVersion == TRVersion.Game.TR4 || gameVersion == TRVersion.Game.TRNG)
				return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB4", "Defaults");
			else if (gameVersion == TRVersion.Game.TR5Main)
				return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB5", "Defaults");

			return string.Empty;
		}

		public static string GetTRNGPluginsPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "TRNG Plugins");
		}

		public static string GetProgramDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		[DllImport("shell32.dll")]
		private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

		/// <summary>
		/// Returns either the "System32" path or the "SysWOW64" path.
		/// </summary>
		public static string GetSystemDirectory()
		{
			StringBuilder path = new StringBuilder(260);
			SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);

			return path.ToString();
		}
	}
}
