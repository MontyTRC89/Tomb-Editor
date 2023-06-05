using System;
using System.IO;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TENGameProject : GameProjectBase
	{
		public const string MainScriptFileName = "Gameflow.lua";
		public const string LanguageFileName = "Strings.lua";

		public override TRVersion.Game GameVersion => TRVersion.Game.TombEngine;

		public override string DataFileExtension => ".ten";
		public override string EngineExecutableFileName => "TombEngine.exe";

		public TENGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
			=> MainScriptFilePath = Path.Combine(ScriptDirectoryPath, MainScriptFileName);

		public override string GetEngineExecutableFilePath()
		{
			string engineExecutableFilePath;

			string engineDirectoryPath = GetEngineRootDirectoryPath();
			string x64Directory = Path.Combine(engineDirectoryPath, "Bin", "x64");
			string x86Directory = Path.Combine(engineDirectoryPath, "Bin", "x86");

			if (Directory.Exists(x64Directory) || Directory.Exists(x86Directory))
			{
				engineExecutableFilePath = Environment.Is64BitOperatingSystem && Directory.Exists(x64Directory)
					? x64Directory
					: x86Directory;
			}
			else
				engineExecutableFilePath = Path.Combine(engineDirectoryPath, EngineExecutableFileName);

			return File.Exists(engineExecutableFilePath)
				? engineExecutableFilePath
				: throw new FileNotFoundException("The engine executable file could not be found.");
		}

		public override string GetDefaultGameLanguageFilePath()
		{
			string defaultLanguageFilePath = Path.Combine(ScriptDirectoryPath, LanguageFileName);

			return File.Exists(defaultLanguageFilePath)
				? defaultLanguageFilePath
				: throw new FileNotFoundException("The default game language file could not be found.\n" +
					$"Required file not found: {LanguageFileName}");
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

		public override void SetScriptRootDirectory(string newDirectoryPath)
			=> throw new NotSupportedException("Current project type does not allow changing Script directories.");
	}
}
