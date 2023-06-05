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

		public string MapsDirectoryPath { get; set; }
		public string ScriptDirectoryPath { get; set; }

		public string DefaultGameLanguageName { get; set; }

		public string PluginsDirectoryPath { get; set; }

		public List<string> ExternalMapFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new();

		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			MapsDirectoryPath = Path.GetRelativePath(projectPath, MapsDirectoryPath);
			ScriptDirectoryPath = Path.GetRelativePath(projectPath, ScriptDirectoryPath);
			PluginsDirectoryPath = Path.GetRelativePath(projectPath, PluginsDirectoryPath);

			ExternalMapFilePaths = ExternalMapFilePaths.Select(path => Path.GetRelativePath(projectPath, path)).ToList();
		}

		public void DecodeProjectPaths(string trprojFilePath)
		{
			FilePath = trprojFilePath;

			string projectPath = Path.GetDirectoryName(trprojFilePath);

			MapsDirectoryPath = Path.GetFullPath(MapsDirectoryPath, projectPath);
			ScriptDirectoryPath = Path.GetFullPath(ScriptDirectoryPath, projectPath);
			PluginsDirectoryPath = Path.GetFullPath(PluginsDirectoryPath, projectPath);

			ExternalMapFilePaths = ExternalMapFilePaths.Select(path => Path.GetFullPath(path, projectPath)).ToList();
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

		private static TrprojFile FromLegacy(LegacyTrprojFile legacyTrproj)
		{
			string trprojDirectory = Path.GetDirectoryName(legacyTrproj.FilePath);

			var trproj = new TrprojFile
			{
				FilePath = legacyTrproj.FilePath,

				ProjectName = legacyTrproj.Name,
				TargetGameVersion = legacyTrproj.GameVersion,

				MapsDirectoryPath = legacyTrproj.LevelsPath,
				ScriptDirectoryPath = legacyTrproj.ScriptPath,
				PluginsDirectoryPath = Path.Combine(trprojDirectory, "Plugins"),

				GameLanguageNames = new List<string> { "English" },
				DefaultGameLanguageName = "English"
			};

			foreach (LegacyProjectLevel level in legacyTrproj.Levels)
			{
				try
				{
					var trmap = new TrmapFile
					{
						MapName = level.Name,
						TargetPrj2FileName = level.SpecificFile
					};

					if (trmap.TargetPrj2FileName == "$(LatestFile)")
						trmap.TargetPrj2FileName = null; // New method of specifying the latest file

					string trmapFilePath = Path.Combine(level.FolderPath, "project.trmap");
					trmap.WriteToFile(trmapFilePath);

					if (!trmapFilePath.StartsWith(trprojDirectory))
						trproj.ExternalMapFilePaths.Add(trmapFilePath);
				}
				catch { } // Skip
			}

			return trproj;
		}
	}
}
