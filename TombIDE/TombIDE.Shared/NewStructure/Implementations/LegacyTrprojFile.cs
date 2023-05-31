using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure.Implementations
{
	[XmlRoot("Project")]
	public sealed class LegacyTrprojFile : ITrproj
	{
		/// <summary><c>"$(ProjectDirectory)"</c></summary>
		public const string RelativePathKey = "$(ProjectDirectory)";

		public Version Version => new(1, 0);

		public string Name { get; set; }
		public TRVersion.Game GameVersion { get; set; }

		public string LevelsPath { get; set; }
		public string ScriptPath { get; set; }

		public List<ProjectLevel> Levels { get; set; } = new();

		/// <summary>
		/// Replaces <inheritdoc cref="RelativePathKey" /> with the project's .trproj directory path.
		/// </summary>
		public void DecodeProjectPaths(string trprojFilePath)
		{
			string projectPath = Path.GetDirectoryName(trprojFilePath);

			if (LevelsPath.StartsWith(RelativePathKey))
				LevelsPath = LevelsPath.Replace(RelativePathKey, projectPath);

			if (ScriptPath.StartsWith(RelativePathKey))
				ScriptPath = ScriptPath.Replace(RelativePathKey, projectPath);

			foreach (ProjectLevel level in Levels)
			{
				if (level.FolderPath.StartsWith(RelativePathKey))
					level.FolderPath = level.FolderPath.Replace(RelativePathKey, projectPath);
			}
		}
	}
}
