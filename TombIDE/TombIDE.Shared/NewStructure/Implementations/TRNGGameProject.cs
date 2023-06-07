using System;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public sealed class TRNGGameProject : TR4GameProject
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TRNG;

		public TRNGGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TRNGGameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath, string pluginsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath, pluginsDirectoryPath)
		{ }
	}
}
