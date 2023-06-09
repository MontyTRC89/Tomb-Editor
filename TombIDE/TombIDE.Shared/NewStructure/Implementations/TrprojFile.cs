using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public class TrprojFile : ITrproj
	{
		[XmlIgnore]
		public string FilePath { get; protected set; }

		public Version Version => new(2, 0);

		public string ProjectName { get; set; }
		public TRVersion.Game TargetGameVersion { get; set; }

		public string LevelsDirectoryPath { get; set; }
		public string ScriptDirectoryPath { get; set; }

		public string DefaultGameLanguageName { get; set; }

		public string PluginsDirectoryPath { get; set; }

		public List<string> ExternalLevelFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new();

		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelsDirectoryPath = Path.GetRelativePath(projectPath, LevelsDirectoryPath);
			ScriptDirectoryPath = Path.GetRelativePath(projectPath, ScriptDirectoryPath);
			PluginsDirectoryPath = Path.GetRelativePath(projectPath, PluginsDirectoryPath);

			ExternalLevelFilePaths = ExternalLevelFilePaths.Select(path => Path.GetRelativePath(projectPath, path)).ToList();
		}

		public void DecodeProjectPaths(string trprojFilePath)
		{
			FilePath = trprojFilePath;

			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelsDirectoryPath = Path.GetFullPath(LevelsDirectoryPath, projectPath);
			ScriptDirectoryPath = Path.GetFullPath(ScriptDirectoryPath, projectPath);
			PluginsDirectoryPath = Path.GetFullPath(PluginsDirectoryPath, projectPath);

			ExternalLevelFilePaths = ExternalLevelFilePaths.Select(path => Path.GetFullPath(path, projectPath)).ToList();
		}

		public void WriteToFile(string filePath)
		{
			try
			{
				EncodeProjectPaths(filePath);
				XmlUtils.WriteXmlFile(filePath, this);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to save the project file.", ex);
			}
			finally
			{
				DecodeProjectPaths(filePath);
			}
		}

		public static TrprojFile FromFile(string filePath, out Version version)
		{
			try
			{
				TrprojFile trprojFile = XmlUtils.ReadXmlFile<TrprojFile>(filePath);
				trprojFile.DecodeProjectPaths(filePath);

				version = trprojFile.Version;
				return trprojFile;
			}
			catch
			{
				try
				{
					LegacyTrprojFile legacyTrproj = XmlUtils.ReadXmlFile<LegacyTrprojFile>(filePath);
					legacyTrproj.DecodeProjectPaths(filePath);

					version = legacyTrproj.Version;
					return FromLegacy(legacyTrproj);
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to load the project file.", ex);
				}
			}
		}

		public static TrprojFile FromLegacy(LegacyTrprojFile legacyTrproj)
		{
			string trprojDirectory = Path.GetDirectoryName(legacyTrproj.FilePath);

			var trproj = new TrprojFile
			{
				FilePath = legacyTrproj.FilePath,

				ProjectName = legacyTrproj.Name,
				TargetGameVersion = legacyTrproj.GameVersion,

				LevelsDirectoryPath = legacyTrproj.LevelsPath,
				ScriptDirectoryPath = legacyTrproj.ScriptPath,
				PluginsDirectoryPath = Path.Combine(trprojDirectory, "Plugins"),

				GameLanguageNames = new List<string> { "English" },
				DefaultGameLanguageName = "English"
			};

			foreach (LegacyProjectLevel level in legacyTrproj.Levels)
			{
				try
				{
					var trlvl = new TrlvlFile
					{
						LevelName = level.Name,
						TargetPrj2FileName = level.SpecificFile
					};

					if (trlvl.TargetPrj2FileName == "$(LatestFile)")
						trlvl.TargetPrj2FileName = null; // New method of specifying the latest file

					string trlvlFilePath = Path.Combine(level.FolderPath, "project.trlvl");
					trlvl.WriteToFile(trlvlFilePath);

					if (!trlvlFilePath.StartsWith(trprojDirectory))
						trproj.ExternalLevelFilePaths.Add(trlvlFilePath);
				}
				catch { } // Skip
			}

			return trproj;
		}
	}
}
