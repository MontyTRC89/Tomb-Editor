using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using TombLib.Scripting.CodeCleaners;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Resources
{
	public static class ScriptKeyWords
	{
		#region Public variables

		public static List<string> MnemonicConstants { get; private set; }
		public static List<PluginMnemonic> PluginMnemonics { get; private set; }

		/// <summary>
		/// All mnemonic flags combined into one list.
		/// </summary>
		public static List<string> AllMnemonics { get; private set; }

		public static string[] Sections
		{
			get
			{
				return new string[]
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

		public static string[] TR5Commands
		{
			get
			{
				return new string[]
				{
					"GiveItem",
					"TakeAway"
				};
			}
		}

		public static string[] TR5MainCommands
		{
			get
			{
				return new string[]
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

		public static string[] NewCommands
		{
			get
			{
				return new string[]
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

		public static string[] OldCommands
		{
			get
			{
				return new string[]
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

		public static string[] Unknown
		{
			get
			{
				return new string[]
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

		public static void SetupConstants(string referencesPath, string ngcPath)
		{
			MnemonicConstants = GetMnemonicConstants(referencesPath);
			PluginMnemonics = GetPluginMnemonics(ngcPath);

			List<string> allMnemonics = new List<string>();
			allMnemonics.AddRange(MnemonicConstants);

			foreach (PluginMnemonic pluginMnemonic in PluginMnemonics)
				allMnemonics.Add(pluginMnemonic.FlagName);

			AllMnemonics = allMnemonics;
		}

		private static List<string> GetMnemonicConstants(string referencesPath)
		{
			List<string> mnemonicConstants = new List<string>();

			try
			{
				string xmlPath = Path.Combine(referencesPath, "Mnemonic Constants.xml");

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

				return mnemonicConstants;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static List<PluginMnemonic> GetPluginMnemonics(string ngcPath)
		{
			List<PluginMnemonic> pluginMnemonics = new List<PluginMnemonic>();

			try
			{
				foreach (string file in Directory.GetFiles(ngcPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
				{
					string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1252));

					foreach (string line in lines)
					{
						if (line.StartsWith("<start_constants>", StringComparison.OrdinalIgnoreCase))
							continue;

						if (line.StartsWith("<end>", StringComparison.OrdinalIgnoreCase))
							break;

						if (!string.IsNullOrWhiteSpace(line) && line.Contains(":"))
						{
							string description = string.Empty;
							string decimalString = string.Empty;

							if (line.Contains(";"))
							{
								description = string.Join(Environment.NewLine, new BasicCleaner().TrimEndingWhitespaceOnLines(line.Split(';')[1].Trim().Split('>')));
								decimalString = line.Split(';')[0].Trim().Split(':')[1].Trim();
							}
							else
								decimalString = line.Split(':')[1].Trim();

							try
							{
								short decimalValue = 0;

								if (!short.TryParse(decimalString, out decimalValue))
									decimalValue = Convert.ToInt16(decimalString.Replace("$", string.Empty), 16);

								pluginMnemonics.Add(new PluginMnemonic(line.Split(':')[0].Trim(), description, decimalValue));
							}
							catch (Exception) { } // https://youtu.be/T9NjXekZ8kA
						}
					}
				}

				return pluginMnemonics;
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion Methods
	}
}
