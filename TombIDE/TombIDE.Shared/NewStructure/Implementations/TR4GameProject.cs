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

		public TR4GameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }
	}
}
