using System;
using System.IO;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class Tomb1MainGameProject : GameProjectBase
	{
		public const string MainScriptFileName = "Tomb1Main_gameflow.json5";
		public const string LanguageFileName = MainScriptFileName; // Same file as main script file

		public override TRVersion.Game GameVersion => TRVersion.Game.TR1;

		public override string DataFileExtension => ".phd";
		public override string EngineExecutableFileName => "tomb1main.exe";
		public override string MainScriptFilePath => Path.Combine(GetScriptRootDirectory(), MainScriptFileName);

		public override bool SupportsCustomScriptPaths => false;
		public override bool SupportsPlugins => false;

		public Tomb1MainGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public Tomb1MainGameProject(string name, string directoryPath, string levelsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath)
		{ }

		public override string GetDefaultGameLanguageFilePath()
		{
			string defaultLanguageFilePath = Path.Combine(GetScriptRootDirectory(), LanguageFileName);

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

			return true;
		}

		public override string GetScriptRootDirectory()
			=> Path.Combine(GetEngineRootDirectoryPath(), "cfg"); // Hardcoded path

		public override void SetScriptRootDirectory(string newDirectoryPath)
			=> throw new NotSupportedException("Current project type does not allow changing Script directories.");
	}
}
