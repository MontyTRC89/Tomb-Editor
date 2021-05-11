using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Resources
{
	public static class MnemonicData
	{
		static MnemonicData()
			=> SetupConstants(DefaultPaths.InternalNGCDirectory);

		public static List<string> StandardConstants = new List<string>();
		public static List<PluginConstant> PluginConstants = new List<PluginConstant>();

		/// <summary>
		/// All flags combined into one list.
		/// </summary>
		public static List<string> AllConstantFlags = new List<string>();

		public static void SetupConstants(string ngcPath)
		{
			StandardConstants = GetMnemonicConstants(DefaultPaths.ReferencesDirectory);
			PluginConstants = GetPluginMnemonics(ngcPath);

			var allMnemonics = new List<string>();
			allMnemonics.AddRange(StandardConstants);

			foreach (PluginConstant pluginMnemonic in PluginConstants)
				allMnemonics.Add(pluginMnemonic.FlagName);

			AllConstantFlags = allMnemonics;
		}

		private static List<string> GetMnemonicConstants(string referencesPath)
		{
			var mnemonicConstants = new List<string>();

			try
			{
				string xmlPath = Path.Combine(referencesPath, "MnemonicConstants.xml");

				using (var reader = XmlReader.Create(xmlPath))
				using (var dataSet = new DataSet())
				{
					dataSet.ReadXml(reader);

					DataTable dataTable = dataSet.Tables[0];

					for (int i = 0; i < dataTable.Rows.Count; i++)
						mnemonicConstants.Add(dataTable.Rows[i][2].ToString());
				}

				return mnemonicConstants;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static List<PluginConstant> GetPluginMnemonics(string ngcPath)
		{
			var pluginMnemonics = new List<PluginConstant>();

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

						if (!string.IsNullOrWhiteSpace(line))
						{
							int decimalStart = line.IndexOf(':');

							if (decimalStart != -1)
							{
								int descriptionStart = line.IndexOf(';');

								string decimalString = string.Empty;
								string description = string.Empty;

								if (descriptionStart != -1)
								{
									string[] descriptionLines = line.Substring(descriptionStart + 1).Trim().Split('>');
									description = string.Join(Environment.NewLine, BasicCleaner.TrimEndingWhitespaceOnLines(descriptionLines));

									int decimalLength = descriptionStart - decimalStart - 1;

									if (decimalLength > 0)
										decimalString = line.Substring(decimalStart + 1, decimalLength).Trim();
								}
								else
									decimalString = line.Substring(decimalStart + 1).Trim();

								try
								{
									short decimalValue = 0;

									if (!short.TryParse(decimalString, out decimalValue))
										decimalValue = Convert.ToInt16(decimalString.Replace("$", string.Empty), 16);

									pluginMnemonics.Add(new PluginConstant(line.Substring(0, decimalStart).Trim(), description, decimalValue));
								}
								catch (Exception) { } // https://youtu.be/T9NjXekZ8kA
							}
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
	}
}
