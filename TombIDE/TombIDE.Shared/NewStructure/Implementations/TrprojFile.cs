using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	[XmlRoot("GameProject")]
	public class TrprojFile : ITrproj
	{
		[XmlIgnore]
		public string FilePath { get; protected set; }

		[XmlAttribute]
		public string FileFormatVersion { get; set; } = "2.0";

		[XmlAttribute]
		public TRVersion.Game TargetGameVersion { get; set; }

		public string Name { get; set; }

		public string LevelsDirectory { get; set; }
		public string ScriptingDirectory { get; set; }
		public string PluginsDirectory { get; set; }

		[XmlIgnore] // Ignore for now
		public string DefaultLanguage { get; set; } = "English";

		[XmlArrayItem(typeof(string), ElementName = "TrlvlFile")]
		public List<string> ExternalLevels { get; set; } = new();

		[XmlIgnore, XmlArrayItem(typeof(string), ElementName = "Language")] // Ignore for now
		public List<string> Languages { get; set; } = new() { "English" };

		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelsDirectory = Path.GetRelativePath(projectPath, LevelsDirectory);
			ScriptingDirectory = Path.GetRelativePath(projectPath, ScriptingDirectory);

			if (PluginsDirectory is not null)
				PluginsDirectory = Path.GetRelativePath(projectPath, PluginsDirectory);

			ExternalLevels = ExternalLevels.Select(path => Path.GetRelativePath(projectPath, path)).ToList();
		}

		public void DecodeProjectPaths(string trprojFilePath)
		{
			FilePath = trprojFilePath;

			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LevelsDirectory = Path.GetFullPath(LevelsDirectory, projectPath);
			ScriptingDirectory = Path.GetFullPath(ScriptingDirectory, projectPath);

			if (PluginsDirectory is not null)
				PluginsDirectory = Path.GetFullPath(PluginsDirectory, projectPath);

			ExternalLevels = ExternalLevels.Select(path => Path.GetFullPath(path, projectPath)).ToList();
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

				version = new Version(trprojFile.FileFormatVersion);

				if (version != new Version(2, 0))
					throw new Exception("Failed to load the project file. Unsupported version.");

				return trprojFile;
			}
			catch
			{
				try
				{
					LegacyTrprojFile legacyTrproj = XmlUtils.ReadXmlFile<LegacyTrprojFile>(filePath);
					legacyTrproj.DecodeProjectPaths(filePath);

					version = new Version(legacyTrproj.FileFormatVersion);

					if (version != new Version(1, 0))
						throw new Exception("Failed to load the project file. Unsupported version.");

					return FromLegacy(legacyTrproj);
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to load the project file. Unsupported version or corrupted data.", ex);
				}
			}
		}

		public static TrprojFile FromLegacy(LegacyTrprojFile legacyTrproj)
		{
			string trprojDirectory = Path.GetDirectoryName(legacyTrproj.FilePath);

			var trproj = new TrprojFile
			{
				FilePath = legacyTrproj.FilePath,

				Name = legacyTrproj.Name,
				TargetGameVersion = legacyTrproj.GameVersion,

				LevelsDirectory = legacyTrproj.LevelsPath,
				ScriptingDirectory = legacyTrproj.ScriptPath,

				Languages = new List<string> { "English" },
				DefaultLanguage = "English"
			};

			if (trproj.TargetGameVersion is TRVersion.Game.TRNG)
				trproj.PluginsDirectory = Path.Combine(trprojDirectory, "Plugins");

			int i = 0;

			foreach (LegacyProjectLevel level in legacyTrproj.Levels)
			{
				try
				{
					var trlvl = new TrlvlFile
					{
						Name = level.Name,
						StartupFile = level.SpecificFile,
						Order = i
					};

					if (trlvl.StartupFile == "$(LatestFile)")
						trlvl.StartupFile = null; // New method of specifying the latest file

					string trlvlFilePath = Path.Combine(level.FolderPath, "project.trlvl");
					trlvl.WriteToFile(trlvlFilePath);

					if (!trlvlFilePath.StartsWith(trprojDirectory))
						trproj.ExternalLevels.Add(trlvlFilePath);
				}
				catch { } // Skip

				i++;
			}

			return trproj;
		}
	}
}
