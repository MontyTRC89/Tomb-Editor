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
				case TRVersion.Game.TR4:
				case TRVersion.Game.TRNG: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB4");
				default: return null;
			}
		}

		public static string GetDefaultTemplatesPath(TRVersion.Game gameVersion)
		{
			switch (gameVersion)
			{
				case TRVersion.Game.TR4:
				case TRVersion.Game.TRNG: return Path.Combine(DefaultPaths.ProgramDirectory, "TIDE", "Templates", "TOMB4", "Defaults");
				default: return null;
			}
		}
	}
}
