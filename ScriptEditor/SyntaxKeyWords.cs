using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxKeyWords
	{
		public static List<string> Headers()
		{
			List<string> list = new List<string>();

			string[] objects =
			{
				"[Language]",
				"[Level]",
				"[Options]",
				"[PCExtensions]",
				"[PSXExtensions]",
				"[Title]"
			};

			list.AddRange(objects);
			return list;
		}

		public static List<string> KeyValues()
		{
			List<string> list = new List<string>();

			string[] keyValues =
			{
				// New commands
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
				"DemoDisc",
				"Demo",
				"Detector",
				"DiagnosticType",
				"Diagnostic",
				"Diary",
				"Elevator",
				"Enemy",
				"Equipment",
				"FMV",
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
				"WorldFarView",

				// Old commands
				"AnimatingMIP",
				"ColAddHorizon",
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

				// Other commands
				"Cut",
				"File",
			};

			list.AddRange(keyValues);
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
