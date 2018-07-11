using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxKeyWords
	{
		public static List<string> Headers()
		{
			List<string> list = new List<string>();

			string[] headers =
			{
				"Language",
				"Level",
				"Options",
				"PCExtensions",
				"PSXExtensions",
				"Title"
			};

			list.AddRange(headers);
			return list;
		}

		public static List<string> NewCommands()
		{
			List<string> list = new List<string>();

			string[] newCommands =
			{
				"AddEffect",
				"AnimationSlot",
				"Animation",
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
				"DiagnosticType",
				"Diagnostic",
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

			list.AddRange(newCommands);
			return list;
		}

		public static List<string> OldCommands()
		{
			List<string> list = new List<string>();

			string[] oldCommands =
			{
				"AnimatingMIP",
				"ColAddHorizon",
				"DemoDisc",
				"Examine",
				"FlyCheat",
				"FOG",
				"Horizon",
				"InputTimeout",
				"KeyCombo",
				"Key",
				"Layer1",
				"Legend",
				"LensFlare",
				"Level",
				"Lightning",
				"LoadCamera",
				"LoadSave",
				"Mirror",
				"Name",
				"PickupCombo",
				"Pickup",
				"PlayAnyLevel",
				"PuzzleCombo",
				"Puzzle",
				"RemoveAmulet",
				"ResetHUB",
				"ResidentCut",
				"Timer",
				"Title",
				"Train",
				"UVRotate",
				"YoungLara",

				"Cut",
				"File",
				"FMV"
			};

			list.AddRange(oldCommands);
			return list;
		}

		public static List<string> Unknown()
		{
			List<string> list = new List<string>();

			string[] unknown =
			{
				"Weather",
				"StarField",
				"Layer2",
				"Security",
				"Pulse",
				"NoLevel"
			};

			list.AddRange(unknown);
			return list;
		}
	}
}
