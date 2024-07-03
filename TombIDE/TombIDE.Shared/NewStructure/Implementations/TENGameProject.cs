using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
		public override string MainScriptFilePath => Path.Combine(GetScriptRootDirectory(), MainScriptFileName);

		public override bool SupportsCustomScriptPaths => false;
		public override bool SupportsPlugins => false;

		public TENGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TENGameProject(string name, string directoryPath, string levelsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath)
		{ }

		public override string GetEngineExecutableFilePath()
		{
			string engineExecutableFilePath;

			string engineDirectoryPath = GetEngineRootDirectoryPath();
			string x64Directory = Path.Combine(engineDirectoryPath, "Bin", "x64");
			string x86Directory = Path.Combine(engineDirectoryPath, "Bin", "x86");

			if (Directory.Exists(x64Directory) || Directory.Exists(x86Directory))
			{
				string engineExecutableDirectory = Environment.Is64BitOperatingSystem && Directory.Exists(x64Directory)
					? x64Directory
					: x86Directory;

				engineExecutableFilePath = Path.Combine(engineExecutableDirectory, EngineExecutableFileName);
			}
			else
				engineExecutableFilePath = Path.Combine(engineDirectoryPath, EngineExecutableFileName);

			return File.Exists(engineExecutableFilePath)
				? engineExecutableFilePath
				: throw new FileNotFoundException("The engine executable file could not be found.");
		}

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

		public override string GetScriptRootDirectory()
			=> Path.Combine(GetEngineRootDirectoryPath(), "Scripts"); // Hardcoded path

		public override void SetScriptRootDirectory(string newDirectoryPath)
			=> throw new NotSupportedException("Current project type does not allow changing Script directories.");

		public override Version GetCurrentEngineVersion()
		{
			try
			{
				string engineExecutablePath = GetEngineExecutableFilePath();
				string versionInfo = FileVersionInfo.GetVersionInfo(engineExecutablePath).FileVersion;
				return new Version(versionInfo);
			}
			catch
			{
				return null;
			}
		}

		public override Version GetLatestEngineVersion()
		{
			string tempFileName = Path.GetTempFileName();

			try
			{
				string enginePresetPath = Path.Combine(DefaultPaths.PresetsDirectory, "TEN.zip");

				using ZipArchive archive = ZipFile.OpenRead(enginePresetPath);
				ZipArchiveEntry entry = archive.Entries.FirstOrDefault(e => e.Name == "TombEngine.exe");

				if (entry == null)
					return null;

				entry.ExtractToFile(tempFileName, true);
				string productVersion = FileVersionInfo.GetVersionInfo(tempFileName).FileVersion;

				return new Version(productVersion);
			}
			catch
			{
				return null;
			}
			finally
			{
				if (File.Exists(tempFileName))
					File.Delete(tempFileName);
			}
		}
	}
}
