using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace TombIDE.Shared.Scripting
{
	internal class AutocompleteItems
	{
		public static List<AutocompleteItem> GetItems()
		{
			// Create the Autocomplete list
			List<AutocompleteItem> items = new List<AutocompleteItem>();

			// Add key words to the Autocomplete list
			foreach (string item in KeyWords.Sections)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.TR5Commands)
				items.Add(new AutocompleteItem(item + "="));

			foreach (string item in KeyWords.TR5MainCommands)
				items.Add(new AutocompleteItem(item + "="));

			foreach (string item in KeyWords.NewCommands)
				items.Add(new AutocompleteItem(item + "="));

			foreach (string item in KeyWords.OldCommands)
				items.Add(new AutocompleteItem(item + "="));

			foreach (string item in KeyWords.Unknown)
				items.Add(new AutocompleteItem(item + "="));

			// Add these additional key words
			items.Add(new AutocompleteItem("ENABLED"));
			items.Add(new AutocompleteItem("DISABLED"));

			List<AutocompleteItem> mnemonicConstants = GetMnemonicConstants();

			if (mnemonicConstants != null)
				items.AddRange(mnemonicConstants);

			List<AutocompleteItem> pluginMnemonics = GetPluginMnemonics();

			if (pluginMnemonics != null)
				items.AddRange(pluginMnemonics);

			return items;
		}

		private static List<AutocompleteItem> GetMnemonicConstants()
		{
			List<AutocompleteItem> mnemonicConstants = new List<AutocompleteItem>();

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
							mnemonicConstants.Add(new AutocompleteItem(dataTable.Rows[i][2].ToString()));
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			return mnemonicConstants;
		}

		private static List<AutocompleteItem> GetPluginMnemonics()
		{
			List<AutocompleteItem> pluginMnemonics = new List<AutocompleteItem>();

			try
			{
				string ngcPath = Path.Combine(SharedMethods.GetProgramDirectory(), "NGC");

				foreach (string file in Directory.GetFiles(ngcPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
				{
					string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1252));

					foreach (string line in lines)
					{
						if (!string.IsNullOrWhiteSpace(line) && line.Contains(":"))
							pluginMnemonics.Add(new AutocompleteItem(line.Split(':')[0].Trim()));
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			return pluginMnemonics;
		}
	}
}
