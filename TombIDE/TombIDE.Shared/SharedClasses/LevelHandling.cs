using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombIDE.Shared.SharedClasses
{
	public static class LevelHandling
	{
		public static void UpdatePrj2GameSettings(string prj2FilePath, ProjectLevel destLevel, Project destProject)
		{
			Level level = Prj2Loader.LoadFromPrj2(prj2FilePath, null);

			string exeFilePath = Path.Combine(destProject.EnginePath, destProject.GetExeFileName());

			string dataFileName = destLevel.DataFileName + destProject.GetLevelFileExtension();
			string dataFilePath = Path.Combine(destProject.EnginePath, "data", dataFileName);

			level.Settings.LevelFilePath = prj2FilePath;

			level.Settings.GameDirectory = level.Settings.MakeRelative(destProject.EnginePath, VariableType.LevelDirectory);
			level.Settings.GameExecutableFilePath = level.Settings.MakeRelative(exeFilePath, VariableType.LevelDirectory);
			level.Settings.GameLevelFilePath = level.Settings.MakeRelative(dataFilePath, VariableType.LevelDirectory);
			level.Settings.GameVersion = destProject.GameVersion;

			Prj2Writer.SaveToPrj2(prj2FilePath, level);
		}

		public static List<string> GetValidPrj2FilesFromDirectory(string directoryPath)
		{
			List<string> validPrj2Files = new List<string>();

			foreach (string file in Directory.GetFiles(directoryPath, "*.prj2", SearchOption.AllDirectories))
			{
				if (!ProjectLevel.IsBackupFile(Path.GetFileName(file)))
					validPrj2Files.Add(file);
			}

			return validPrj2Files;
		}

		public static List<string> GenerateScriptLines(ProjectLevel level, TRVersion.Game gameVersion, int ambientSoundID, bool horizon = false)
		{
			if (gameVersion == TRVersion.Game.TR1)
			{
				return new List<string>
				{
					"\n		{",
					$"			\"title\": \"{level.Name}\"",
					$"			\"file\": \"data\\\\{level.DataFileName}.phd\"",
					$"			\"type\": \"normal\"",
					$"			\"music\": {ambientSoundID}",
					"		},"
				};
			}
			else if (gameVersion == TRVersion.Game.TR2 || gameVersion == TRVersion.Game.TR3)
			{
				return new List<string>
				{
					"\nLEVEL: " + level.Name,
					"",
					"	GAME: data\\" + level.DataFileName.ToLower() + ".tr2",
					"	TRACK: " + ambientSoundID,
					"	LOAD_PIC: " + "pix\\" + (gameVersion == TRVersion.Game.TR2 ? "mansion.pcx" : "house.bmp"),
					"",
					"END:"
				};
			}
			else if (gameVersion == TRVersion.Game.TR4 || gameVersion == TRVersion.Game.TRNG)
			{
				return new List<string>
				{
					"\n[Level]",
					"Name= " + level.Name,
					"Level= DATA\\" + level.DataFileName.ToUpper() + ", " + ambientSoundID,
					"LoadCamera= 0, 0, 0, 0, 0, 0, 0",
					"Horizon= " + (horizon? "ENABLED" : "DISABLED")
				};
			}

			return new List<string>();
		}

		public static string RemoveIllegalNameSymbols(string levelName)
		{
			char[] illegalNameChars = { ';', '[', ']', '=', ',', '.', '!' };
			return illegalNameChars.Aggregate(levelName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}
	}
}
