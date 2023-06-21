using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public sealed record GameProjectDTO
	{
		public string ProjectName { get; init; }
		public string RootDirectoryPath { get; init; }

		public TRVersion.Game GameVersion { get; init; }

		public string EngineExecutableFilePath { get; init; }
		public string LauncherExecutableFilePath { get; init; }

		public string LevelsDirectoryPath { get; init; }
		public string ScriptDirectoryPath { get; init; }
		public string PluginsDirectoryPath { get; init; }
	}
}
