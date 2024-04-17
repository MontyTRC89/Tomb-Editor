using System;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR4GameProject : CoreStructuredProjectBase
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TR4;

		public override string DataFileExtension => ".tr4";
		public override string EngineExecutableFileName => "tomb4.exe";

		public override bool SupportsPlugins => false;

		public TR4GameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TR4GameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath, string pluginsDirectoryPath = null)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath, pluginsDirectoryPath)
		{ }

		public override Version GetCurrentEngineVersion()
			=> new(1, 0);
	}
}
