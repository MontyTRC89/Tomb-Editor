using System.Collections.Generic;

namespace TombIDE.Shared.Scripting
{
	public class KeyWords
	{
		public static List<string> Sections
		{
			get
			{
				return new List<string>
				{
					"Language",
					"Level",
					"Options",
					"PCExtensions",
					"PSXExtensions",
					"Title",
					"Strings",
					"PSXStrings",
					"PCStrings",
					"ExtraNG"
				};
			}
		}

		public static List<string> TR5Commands
		{
			get
			{
				return new List<string>
				{
					"GiveItem",
					"TakeAway"
				};
			}
		}

		public static List<string> TR5MainCommands
		{
			get
			{
				return new List<string>
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
			}
		}

		public static List<string> NewCommands
		{
			get
			{
				return new List<string>
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
			}
		}

		public static List<string> OldCommands
		{
			get
			{
				return new List<string>
				{
					"AnimatingMIP",
					"ColAddHorizon",
					"DemoDisc",
					"Examine",
					"FlyCheat",
					"Fog",
					"Horizon",
					"InputTimeout",
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
			}
		}

		public static List<string> Unknown
		{
			get
			{
				return new List<string>
				{
					"Layer2",
					"NoLevel",
					"Pulse",
					"Security",
					"StarField"
				};
			}
		}
	}
}
