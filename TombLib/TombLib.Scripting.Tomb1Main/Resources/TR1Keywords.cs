namespace TombLib.Scripting.Tomb1Main.Resources
{
	public static class TR1Keywords
	{
		public static readonly string[] Collections = new string[]
		{
			"item_drops",
		};

		public static readonly string[] Constants = new string[]
		{
			// Game flow commands
			"restart_level",
			"story_so_far",
			"loading_screen",
			"flip_map",
			"mesh_swap",
			"remove_scions",
			"set_cutscene_pos",
			"setup_bacon_lara",
		};

		public static readonly string[] Properties = new string[]
		{
			// Global properties
			"convert_dropped_guns",
			"demo_delay",
			"fog_start",
			"fog_end",
			"enable_killer_pushblocks",
			"enable_tr2_item_drops",
			"enforced_config",
			"main_menu_picture",
			"savegame_fmt_bson",
			"savegame_fmt_legacy",
			"water_color",

			// Level properties
			"lara_type",
			"unobtainable_kills",
			"unobtainable_pickups",
			"unobtainable_secrets",
		};
	}
}
