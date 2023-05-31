using System;
using System.IO;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	/// <summary>
	/// A game project base that follows the Core Design project structure, which includes a <b>Script.txt</b> and a <b>{LANGUAGE}.txt</b> file.
	/// <para>Used for TR2, TR3, TR4 and TRNG projects.</para>
	/// </summary>
	public abstract class CoreStructuredProjectBase : GameProjectBase
	{
		public const string MainScriptFileName = "Script.txt";

		public abstract override TRVersion.Game GameVersion { get; }

		public abstract override string DataFileExtension { get; }
		public abstract override string EngineExecutableFileName { get; }

		public CoreStructuredProjectBase()
			=> MainScriptFilePath = Path.Combine(ScriptRootDirectoryPath, MainScriptFileName);

		public override string GetDefaultGameLanguageFilePath()
		{
			string defaultLanguageFilePath = Path.Combine(ScriptRootDirectoryPath, $"{DefaultGameLanguageName}.txt");

			return File.Exists(defaultLanguageFilePath)
				? defaultLanguageFilePath
				: throw new FileNotFoundException("The default game language file could not be found.\n" +
					$"Required file not found: {DefaultGameLanguageName}.txt");
		}

		public override void SetScriptRootDirectory(string newDirectoryPath)
		{
			string mainScriptFilePath = Path.Combine(newDirectoryPath, MainScriptFileName);

			if (!File.Exists(mainScriptFilePath))
				throw new FileNotFoundException($"A main {MainScriptFileName} file could not be found in the given directory.");

			string defaultLanguageFilePath = Path.Combine(newDirectoryPath, $"{DefaultGameLanguageName}.txt");

			if (!File.Exists(defaultLanguageFilePath))
				throw new FileNotFoundException("A matching game language file could not be found in the given directory.\n" +
					$"Required file not found: {DefaultGameLanguageName}.txt\n" +
					$"Try changing your default game language to one that matches the chosen directory and try again.");

			MainScriptFilePath = mainScriptFilePath;
			ScriptRootDirectoryPath = newDirectoryPath;
		}

		public override bool IsValid(out string errorMessage)
		{
			if (!base.IsValid(out errorMessage))
				return false;

			if (!File.Exists(MainScriptFilePath))
			{
				errorMessage = $"The project does not have a valid {MainScriptFileName} file.";
				return false;
			}

			try
			{
				GetDefaultGameLanguageFilePath();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}

			return true;
		}

		public override void Save() => throw new NotImplementedException();
	}
}
