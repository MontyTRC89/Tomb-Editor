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
		public static List<PluginMnemonic> PluginMnemonics { get; internal set; }

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
				// Mhm
			}
		}

		private static void SetupPluginMnemonics()
		{
			List<PluginMnemonic> pluginMnemonics = new List<PluginMnemonic>();

			try
			{
				string ngcPath = Path.Combine(SharedMethods.GetProgramDirectory(), "NGC");

				foreach (string file in Directory.GetFiles(ngcPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
				{
					string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1252));

					foreach (string line in lines)
					{
						if (line.ToLower().StartsWith("<start_constants>"))
							continue;

						if (line.ToLower().StartsWith("<end>"))
							break;

						if (!string.IsNullOrWhiteSpace(line) && line.Contains(":"))
						{
							string description = string.Empty;
							string decimalString = string.Empty;

							if (line.Contains(";"))
							{
								description = string.Join(Environment.NewLine, SyntaxTidying.TrimLines(line.Split(';')[1].Trim().Split('>')));
								decimalString = line.Split(';')[0].Trim().Split(':')[1].Trim();
							}
							else
								decimalString = line.Split(':')[1].Trim();

							try
							{
								short decimalValue = new short();

								if (short.TryParse(decimalString, out short result))
									decimalValue = result;
								else
									decimalValue = Convert.ToInt16(decimalString.Replace("$", string.Empty), 16);

								PluginMnemonic mnemonic = new PluginMnemonic
								{
									Flag = line.Split(':')[0].Trim(),
									Description = description,
									Decimal = decimalValue
								};

								pluginMnemonics.Add(mnemonic);
							}
							catch (Exception)
							{
								// Yes
							}
						}
					}
				}

				PluginMnemonics = pluginMnemonics;
			}
			catch (Exception)
			{
				// https://youtu.be/T9NjXekZ8kA
			}
		}

		#endregion Methods
	}
}
