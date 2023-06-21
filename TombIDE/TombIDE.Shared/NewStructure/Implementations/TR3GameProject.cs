using System;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR3GameProject : CoreStructuredProjectBase
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TR3;

		public override string DataFileExtension => ".tr2";
		public override string EngineExecutableFileName => "tomb3.exe";

		public override bool SupportsPlugins => false;

		public TR3GameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TR3GameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath)
		{ }
	}
}
