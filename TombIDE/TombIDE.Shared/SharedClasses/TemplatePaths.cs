using System.IO;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses
{
	public static class TemplatePaths
	{
		public static string GetSharedTemplatesPath(TRVersion.Game gameVersion)
		{
			switch (gameVersion)
			{
				case TRVersion.Game.TR1: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB1");
				case TRVersion.Game.TR2: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB2");
				case TRVersion.Game.TR3: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB3");
				case TRVersion.Game.TR4:
				case TRVersion.Game.TRNG: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB4");
				default: return null;
			}
		}

		public static string GetDefaultTemplatesPath(TRVersion.Game gameVersion)
		{
			switch (gameVersion)
			{
				case TRVersion.Game.TR1: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB1", "Defaults");
				case TRVersion.Game.TR2: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB2", "Defaults");
				case TRVersion.Game.TR3: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB3", "Defaults");
				case TRVersion.Game.TR4:
				case TRVersion.Game.TRNG: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB4", "Defaults");
				default: return null;
			}
		}
	}
}
