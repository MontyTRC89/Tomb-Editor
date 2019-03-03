using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxKeyWords
	{
		public static List<string> Sections()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"Language",
				"Level",
				"Options",
				"PCExtensions",
				"PSXExtensions",
				"Title"
			};

			list.AddRange(keyWords);
			return list;
		}

		public static List<string> TR5Commands()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"GiveItem",
				"TakeAway"
			};

			list.AddRange(keyWords);
			return list;
		}

		public static List<string> TR5MainCommands()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"AmbientTrack",
				"Debug",
				"DistantFog",
				"LaraStartPos",
				"LaraType",
				"LevelFarView",
				"LoadScreen",
				"OnBeginFrame",
				"OnEndFrame",
				"OnLaraDeath",
				"OnLevelControl",
				"OnLevelFinish",
				"OnLevelStart",
				"OnLoadGame",
				"OnSaveGame",
				"ResetInventory",
				"Weather"
			};

			list.AddRange(keyWords);
			return list;
		}

		public static List<string> NewCommands()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"AddEffect",
				"Animation",
				"AnimationSlot",
				"AssignSlot",
				"ColorRGB",
				"CombineItems",
				"CRS",
				"Customize",
				"CutScene",
				"Damage",
				"DefaultWindowsFont",
				"Demo",
				"Detector",
				"Diagnostic",
				"DiagnosticType",
				"Diary",
				"Elevator",
				"Enemy",
				"Equipment",
				"FogRange",
				"ForceBumpMapping",
				"ForceVolumetricFX",
				"GlobalTrigger",
				"Image",
				"ImportFile",
				"ItemGroup",
				"KeyPad",
				"LaraStartPos",
				"LevelFarView",
				"LogItem",
				"MirrorEffect",
				"MultEnvCondition",
				"NewSoundEngine",
				"Organizer",
				"Parameters",
				"Plugin",
				"PreserveInventory",
				"Rain",
				"SavegamePanel",
				"Settings",
				"ShowLaraInTitle",
				"Snow",
				"SoundSettings",
				"StandBy",
				"StaticMIP",
				"Switch",
				"TestPosition",
				"TextFormat",
				"TextureSequence",
				"TriggerGroup",
				"Turbo",
				"WindowsFont",
				"WindowTitle",
				"WorldFarView"
			};

			list.AddRange(keyWords);
			return list;
		}

		public static List<string> OldCommands()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"AnimatingMIP",
				"ColAddHorizon",
				"DemoDisc",
				"Examine",
				"FlyCheat",
				"FOG",
				"Horizon",
				"InputTimeOut",
				"Key",
				"KeyCombo",
				"Layer1",
				"Legend",
				"LensFlare",
				"Level",
				"Lightning",
				"LoadCamera",
				"LoadSave",
				"Mirror",
				"Name",
				"Pickup",
				"PickupCombo",
				"PlayAnyLevel",
				"Puzzle",
				"PuzzleCombo",
				"RemoveAmulet",
				"ResetHUB",
				"ResidentCut",
				"Timer",
				"Title",
				"Train",
				"UVRotate",
				"YoungLara",

				// Dunno where these belong :P
				"Cut",
				"File",
				"FMV"
			};

			list.AddRange(keyWords);
			return list;
		}

		public static List<string> Unknown()
		{
			List<string> list = new List<string>();

			string[] keyWords =
			{
				"Layer2",
				"NoLevel",
				"Pulse",
				"Security",
				"StarField"
			};

			list.AddRange(keyWords);
			return list;
		}
	}
}
