using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR4GameProject : CoreStructuredProjectBase
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TR4;

		public override string DataFileExtension => ".tr4";
		public override string EngineExecutableFileName => "tomb4.exe";
	}
}
