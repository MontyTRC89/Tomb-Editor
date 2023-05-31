using System;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public sealed class TRNGGameProject : TR4GameProject
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TRNG;

		public string PluginsDirectoryPath { get; private set; }

		public override void Save() => throw new NotImplementedException();
	}
}
