namespace TombLib.Scripting.Tomb1Main.Resources
{
	public static class Keywords
	{
		public static readonly string[] Collections = new string[]
		{
			"ambient_tracks",
			"cutscenes",
			"demos",
			"fmvs",
			"injections",
			"item_drops", // TR1 only
			"levels",
			"object_ids",
			"sequence",
			"strings"
		};

		public static readonly string[] Values = new string[]
		{
			"true",
			"false",
			"null"
		};

		public static readonly string[] Constants = new string[]
		{
			// Game flow commands
			"exit_game",
			"exit_to_title",
			"level_complete",
			"load_saved_game",
			"noop",
			"play_cutscene",
			"play_demo",
			"play_fmv",
			"play_level",
			"restart_level", // TR1 only
			"select_level",
			"story_so_far", // TR1 only

			// Level property values
			"bonus",
			"current",
			"dummy",
			"gym",
			"normal",

			// Sequence types
			"display_picture",
			"flip_map", // TR1 only
			"give_item",
			"level_stats",
			"loading_screen", // TR1 only
			"loop_game",
			"mesh_swap", // TR1 only
			"play_cutscene",
			"play_music",
			"remove_ammo",
			"remove_medipacks",
			"remove_scions", // TR1 only
			"remove_weapons",
			"set_cutscene_angle",
			"set_cutscene_pos", // TR1 only
			"setup_bacon_lara", // TR1 only
			"total_stats",

			// Unknown - Were removed at some point?
			"fix_pyramid_secret",
			"loop_cine",
			"play_synced_audio",
			"remove_guns",
			"set_cam_angle",
			"set_cam_x",
			"set_cam_z",
			"start_cine",
			"stop_cine",
			"stop_game",
		};

		public static readonly string[] Properties = new string[]
		{
			// Global properties
			"convert_dropped_guns",
			"demo_delay",
			"draw_distance_fade", // TR1 only
			"draw_distance_max", // TR1 only
			"enable_killer_pushblocks",
			"enable_tr2_item_drops",
			"enforced_config",
			"main_menu_picture",
			"savegame_fmt_bson",
			"savegame_fmt_legacy",
			"water_color", // TR1 only

			// Level properties
			"enemy_num",
			"inherit_injections",
			"lara_type", // TR1 only
			"music_track",
			"path",
			"type",
			"unobtainable_kills", // TR1 only
			"unobtainable_pickups", // TR1 only
			"unobtainable_secrets", // TR1 only

			// Sequence properties
			"anchor_room",
			"cutscene_id",
			"display_time",
			"fade_in_time",
			"fade_out_time",
			"fmv_id",
			"mesh_id",
			"object_id",
			"object1_id",
			"object2_id",
			"quantity",
			"value",
			"x",
			"y",
			"z",

			// Other properties
			"game_strings",
			"name",
			"objects",

			// Unknown - Were removed at some point?
			"audio_id",
			"demo",
			"enable_game_modes",
			"enable_save_crystals",
			"force_disable_game_modes",
			"force_enable_save_crystals",
			"title",
		};

		public static readonly RemovedKeyword[] RemovedConstants = new RemovedKeyword[]
		{
			new("cutscene", "Instead, the cutscenes need to be placed in the top-level \"cutscenes\" array."),
			new("exit_to_cine"),
			new("exit_to_level", "Instead, use \"level_complete\". No parameter needed."),
			new("start_game"),
		};

		public static readonly RemovedKeyword[] RemovedProperties = new RemovedKeyword[]
		{
			new("file", "Instead, use the \"path\" property for every level."),
			new("fmv_path", "Instead, use the \"fmv_id\" property."),
			new("level_id"),
			new("music", "Instead, use the \"music_track\" property for every level."),
			new("picture_path", "Instead, use the \"background_path\" property for \"total_stats\", or the \"path\" property for everything else."),
		};
	}

	public record struct RemovedKeyword(string Keyword, string Message = "");
}
