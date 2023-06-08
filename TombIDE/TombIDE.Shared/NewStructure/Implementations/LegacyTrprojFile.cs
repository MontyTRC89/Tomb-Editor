using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	[XmlRoot("Project")]
	public sealed class LegacyTrprojFile : ITrproj
	{
		/// <summary><c>"$(ProjectDirectory)"</c></summary>
		public const string RelativePathKey = "$(ProjectDirectory)";

		[XmlIgnore]
		public string FilePath { get; private set; }

		public Version Version => new(1, 0);

		public string Name { get; set; }
		public TRVersion.Game GameVersion { get; set; }

		public string LevelsPath { get; set; }
		public string ScriptPath { get; set; }

		public string LaunchFilePath { get; set; }

		[XmlArrayItem(typeof(LegacyProjectLevel), ElementName = "ProjectLevel")]
		public List<LegacyProjectLevel> Levels { get; set; } = new();

		public LegacyTrprojFile()
		{ }

		public LegacyTrprojFile(string baseFilePath)
			=> FilePath = baseFilePath;

		/// <summary>
		/// Replaces the project's .trproj directory path with <inheritdoc cref="RelativePathKey" />.
		/// </summary>
		public void EncodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			LaunchFilePath = Path.Combine(RelativePathKey, Path.GetFileName(LaunchFilePath));

			if (ScriptPath.StartsWith(projectPath))
				ScriptPath = ScriptPath.Replace(projectPath, RelativePathKey);

			if (LevelsPath.StartsWith(projectPath))
				LevelsPath = LevelsPath.Replace(projectPath, RelativePathKey);

			foreach (LegacyProjectLevel level in Levels)
			{
				if (level.FolderPath.StartsWith(projectPath))
					level.FolderPath = level.FolderPath.Replace(projectPath, RelativePathKey);
			}
		}

		/// <summary>
		/// Replaces <inheritdoc cref="RelativePathKey" /> with the project's .trproj directory path.
		/// </summary>
		public void DecodeProjectPaths(string trprojFilePath)
		{
			FilePath = trprojFilePath;

			string projectPath = Path.GetDirectoryName(trprojFilePath);

			if (LevelsPath.StartsWith(RelativePathKey))
				LevelsPath = LevelsPath.Replace(RelativePathKey, projectPath);

			if (ScriptPath.StartsWith(RelativePathKey))
				ScriptPath = ScriptPath.Replace(RelativePathKey, projectPath);

			foreach (LegacyProjectLevel level in Levels)
			{
				if (level.FolderPath.StartsWith(RelativePathKey))
					level.FolderPath = level.FolderPath.Replace(RelativePathKey, projectPath);
			}
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
	}
}
