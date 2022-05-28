namespace TombLib.Scripting.GameFlowScript.Resources
{
	public static class Keywords
	{
		public static readonly string[] SpecialProperties = new string[]
		{
			"DESCRIPTION", // Game description
			"GameStrings", // Name of the strings file (TR2)
			"PCGameStrings", // Name of the PC strings file (TR3)
			"PSXGameStrings" // Name of the PSX strings file (TR3)
		};

		public static readonly string[] Sections = new string[]
		{
			"END", // Section end
			"OPTIONS", // General game options section
			"TITLE", // Title definition
			"FRONTEND", // FMVs which play before the game starts
			"GYM", // Lara's Home level definition
			"LEVEL", // A level definition
			"DEMOLEVEL", // A demo level definition
			"GAME_STRINGS",
			"PC_STRINGS",
			"PSX_STRINGS"
		};

		public static readonly string[] Constants = new string[]
		{
			"MEDI",
			"BIGMEDI",
			"PISTOLS",
			"PISTOLS_AMMO",
			"SHOTGUN",
			"SHOTGUN_AMMO",
			"AUTOPISTOLS",
			"AUTOPISTOLS_AMMO",
			"UZIS",
			"UZI_AMMO",
			"ROCKET",
			"ROCKET_AMMO",
			"HARPOON",
			"HARPOON_AMMO",
			"M16",
			"M16_AMMO",
			"DESERTEAGLE",
			"DESERTEAGLE_AMMO",
			"MP5",
			"MP5_AMMO",
			"GRENADE",
			"GRENADE_AMMO",
			"FLARES",
			"CRYSTAL",
			"KEY1",
			"KEY2",
			"KEY3",
			"KEY4",
			"PUZZLE1",
			"PUZZLE2",
			"PUZZLE3",
			"PUZZLE4",
			"PICKUP1",
			"PICKUP2"
		};

		public static readonly string[] Properties = new string[]
		{
			// OPTIONS
			"FIRSTOPTION",
			"TITLE_REPLACE",
			"ONDEATH_DEMO_MODE",
			"ONDEATH_INGAME",
			"ON_DEMO_INTERRUPT",
			"ON_DEMO_END",

			"DEMOVERSION",
			"TITLE_DISABLED",
			"CHEATMODECHECK_DISABLED",
			"NOINPUT_TIMEOUT",
			"LOADSAVE_DISABLED",
			"SCREENSIZING_DISABLED",
			"LOCKOUT_OPTIONRING",
			"DOZY_CHEAT_ENABLED",
			"SELECT_ANY_LEVEL",
			"ENABLE_CHEAT_CODE",
			"GYM_DISABLED",

			"LANGUAGE",

			"NOINPUT_TIME",
			"PCNOINPUT_TIME",
			"PSXNOINPUT_TIME",
			"SINGLELEVEL",
			"CYPHER_CODE",
			"SECRET_TRACK",

			// TITLE
			"PCFILE", // PC title metadata file
			"PSXFILE", // PSX title metadata file

			// FMV
			"FMV_START",
			"FMV_END",
			"PSXFMV",
			"PCFMV",
			"FMV",

			// CUT
			"CUTANGLE", // Starting angle of the cutscene (???)
			"CUT", // The in-game cutscene which should be playing

			// INV
			"PSXSTARTINV", // An inventory item which Lara should start the level with (PSX only)
			"STARTINV", // An inventory item which Lara should start the level with

			// LEVEL
			"GAME", // Target level file name (.tr2)
			"DEMO", // Target demo level file name (.tr2)
			"SECRETS", // Secrets count
			"LOAD_PIC", // .raw (TR2) / .bmp (TR3) file
			"TRACK", // The audio track which should be playing during the level
			"STARTANIM", // Defines an animation which Lara should start the level with
			"BONUS", // A bonus item you get for all secrets. (TR2 only)
			"SUNSET", // The level gets darker after some time (TR2 only)
			"NOFLOOR", // The level has an endless abyss
			"DEADLY_WATER",
			"REMOVE_WEAPONS", // Removes Lara's weapons
			"REMOVE_AMMO", // Removes Lara's ammo
			"KILLTOCOMPLETE", // Requires Lara to kill all enemies in order to complete the level
			"COMPLETE", // Set level finished
			"GAMECOMPLETE", // Set game finished

			// ITEMS
			"KEY1",
			"KEY2",
			"KEY3",
			"KEY4",

			"PUZZLE1",
			"PUZZLE2",
			"PUZZLE3",
			"PUZZLE4",

			"PICKUP1",
			"PICKUP2",

			"SECRET1",
			"SECRET2",
			"SECRET3",
			"SECRET4",

			"SPECIAL1",
			"SPECIAL2"
		};
	}
}
