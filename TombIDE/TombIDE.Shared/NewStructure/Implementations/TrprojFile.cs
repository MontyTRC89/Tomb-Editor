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
		public Version Version => new(2, 0);

		public string ProjectName { get; set; }
		public TRVersion.Game TargetGameVersion { get; set; }

		public string MapsDirectoryPath { get; set; }
		public string ScriptRootDirectoryPath { get; set; }

		public string DefaultGameLanguageName { get; set; }

		public string PluginsDirectoryPath { get; set; }

		public List<string> ExternalMapFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new();

		/// <summary>
		/// A list of legacy map projects that will be converted to the new project structure.
		/// </summary>
		[XmlIgnore]
		public List<ProjectLevel> MapProjectsToConvert { get; set; } = new();

		public void DecodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			MapsDirectoryPath = Path.GetFullPath(MapsDirectoryPath, projectPath);
			ScriptRootDirectoryPath = Path.GetFullPath(ScriptRootDirectoryPath, projectPath);
			PluginsDirectoryPath = Path.GetFullPath(PluginsDirectoryPath, projectPath);

			ExternalMapFilePaths = ExternalMapFilePaths.Select(path => Path.GetFullPath(path, projectPath)).ToList();
		}

		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			MapsDirectoryPath = Path.GetRelativePath(projectPath, MapsDirectoryPath);
			ScriptRootDirectoryPath = Path.GetRelativePath(projectPath, ScriptRootDirectoryPath);
			PluginsDirectoryPath = Path.GetRelativePath(projectPath, PluginsDirectoryPath);

			ExternalMapFilePaths = ExternalMapFilePaths.Select(path => Path.GetRelativePath(projectPath, path)).ToList();
		}

		public static TrprojFile FromFile(string filePath)
		{
			try
			{
				TrprojFile trprojFile = XmlUtils.ReadXmlFile<TrprojFile>(filePath);
				trprojFile.DecodeProjectPaths(filePath);

				return trprojFile;
			}
			catch
			{
				try
				{
					LegacyTrprojFile legacyTrproj = XmlUtils.ReadXmlFile<LegacyTrprojFile>(filePath);
					legacyTrproj.DecodeProjectPaths(filePath);

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
			TrprojFile trproj = new()
			{
				ProjectName = legacyTrproj.Name,
				TargetGameVersion = legacyTrproj.GameVersion,
				MapsDirectoryPath = legacyTrproj.LevelsPath,
				ScriptRootDirectoryPath = legacyTrproj.ScriptPath,
				DefaultGameLanguageName = "English",
				PluginsDirectoryPath = string.Empty
			};

			trproj.MapProjectsToConvert.AddRange(legacyTrproj.Levels);

			return trproj;
		}
	}
}
