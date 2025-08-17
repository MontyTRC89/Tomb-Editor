using System;
using System.Linq;

namespace TombLib.Scripting.Tomb1Main.Resources
{
	public static class Keywords
	{
		public static readonly string[] Collections = new string[]
		{
			"injections",
			"hidden_config",
			"levels",
			"object_ids",
			"sequence",
			"strings",
			"ambient_tracks",
			"demos",
			"cutscenes",
			"fmvs"
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
			"noop",
			"play_level",
			"load_saved_game",
			"play_cutscene",
			"play_demo",
			"play_fmv",
			"exit_to_title",
			"level_complete",
			"exit_game",
			"select_level",

			// Level property values
			"bonus",
			"current",
			"dummy",
			"gym",
			"normal",

			// Sequence types
			"loop_game",
			"level_stats",
			"total_stats",
			"display_picture",
			"play_cutscene",
			"give_item",
			"play_music",
			"remove_ammo",
			"remove_weapons",
			"remove_medipacks",
			"set_cutscene_angle",
			"disable_floor",

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
			// Level properties
			"path",
			"type",
			"music_track",
			"inherit_injections",
			"enemy_num",

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

			"legal",
			"action",
			"anim",
			"height",
			"credit",
			"background_path",
			"angle",

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
			new("cutscene", new(4, 8), "Instead, the cutscenes need to be placed in the top-level \"cutscenes\" array."),
			new("exit_to_cine", new(4, 8)),
			new("exit_to_level", new(4, 8), "Instead, use \"level_complete\". No parameter needed."),
			new("start_game", new(4, 8)),
			new("draw_distance_fade", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
			new("draw_distance_max", new(4, 10), "Instead, use \"fog_start\" and \"fog_end\" properties for every level."),
		};

		public static readonly RemovedKeyword[] RemovedProperties = new RemovedKeyword[]
		{
			new("file", new(4, 8), "Instead, use the \"path\" property for every level."),
			new("fmv_path", new(4, 8), "Instead, use the \"fmv_id\" property."),
			new("level_id", new(4, 8)),
			new("music", new(4, 8), "Instead, use the \"music_track\" property for every level."),
			new("picture_path", new(4, 8), "Instead, use the \"background_path\" property for \"total_stats\", or the \"path\" property for everything else."),
		};

		public static string[] GetAllCollections(bool isTR2)
		{
			return isTR2 ? Collections : Collections.Concat(TR1Keywords.Collections).ToArray();
		}

		public static string[] GetAllConstants(bool isTR2)
		{
			return Constants
				.Concat(isTR2 ? TR2Keywords.Constants : TR1Keywords.Constants)
				.Concat(RemovedConstants.Select(x => x.Keyword))
				.ToArray();
		}

		public static string[] GetAllProperties(bool isTR2)
		{
			return Properties
				.Concat(isTR2 ? TR2Keywords.Properties : TR1Keywords.Properties)
				.Concat(RemovedProperties.Select(x => x.Keyword))
				.ToArray();
		}
	}

	public record struct RemovedKeyword(string Keyword, Version RemovedVersion, string Message = "");
}
