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

						if (line.Contains(":$"))
							continue;

						if (!string.IsNullOrWhiteSpace(line) && line.Contains(":"))
						{
							string fullDescription = string.Empty;
							int decimalValue;

							if (line.Contains(";"))
							{
								string description = line.Split(';')[1].Trim();
								string[] descriptionLines = description.Split('>');

								List<string> trimmedText = new List<string>();

								// Trim whitespace on every line and add it to the list
								for (int i = 0; i < descriptionLines.Length; i++)
								{
									string currentLineText = (descriptionLines.Length >= i) ? descriptionLines[i] : Environment.NewLine;
									trimmedText.Add(currentLineText.Trim());
								}

								fullDescription = string.Join("\r\n", trimmedText);

								decimalValue = int.Parse(line.Split(';')[0].Trim().Split(':')[1].Trim());
							}
							else
								decimalValue = int.Parse(line.Split(':')[1].Trim());

							PluginMnemonic mnemonic = new PluginMnemonic
							{
								Flag = line.Split(':')[0].Trim(),
								Description = fullDescription,
								Decimal = (short)decimalValue
							};

							pluginMnemonics.Add(mnemonic);
						}
					}
				}

				PluginMnemonics = pluginMnemonics;
			}
			catch (Exception ex)
			{
				// Yes
			}
		}

		#endregion Methods
	}
}
