using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace TombIDE.Shared.Scripting
{
	public class KeyWords
	{
		#region Public variables

		public static List<string> MnemonicConstants { get; internal set; }
		public static List<string> PluginMnemonics { get; internal set; }

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

		#endregion Public variables

		#region Methods

		public static void SetupConstants()
		{
			SetupMnemonicConstants();
			SetupPluginMnemonics();
		}

		private static void SetupMnemonicConstants()
		{
			List<string> mnemonicConstants = new List<string>();

			try
			{
				string xmlPath = Path.Combine(SharedMethods.GetProgramDirectory(), "References", "Mnemonic Constants.xml");

				using (XmlReader reader = XmlReader.Create(xmlPath))
				{
					using (DataSet dataSet = new DataSet())
					{
						dataSet.ReadXml(reader);

						DataTable dataTable = dataSet.Tables[0];

						for (int i = 0; i < dataTable.Rows.Count; i++)
							mnemonicConstants.Add(dataTable.Rows[i][2].ToString());
					}
				}

				MnemonicConstants = mnemonicConstants;
			}
			catch (Exception)
			{
				// Yes
			}
		}

		private static void SetupPluginMnemonics()
		{
			List<string> pluginMnemonics = new List<string>();

			try
			{
				string ngcPath = Path.Combine(SharedMethods.GetProgramDirectory(), "NGC");

				foreach (string file in Directory.GetFiles(ngcPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
				{
					string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1252));

					foreach (string line in lines)
					{
						if (!string.IsNullOrWhiteSpace(line) && line.Contains(":"))
							pluginMnemonics.Add(line.Split(':')[0].Trim());
					}
				}

				PluginMnemonics = pluginMnemonics;
			}
			catch (Exception)
			{
				// Yes
			}
		}

		#endregion Methods
	}
}
