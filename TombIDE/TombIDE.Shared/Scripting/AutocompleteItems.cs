using FastColoredTextBoxNS;
using System.Collections.Generic;

namespace TombIDE.Shared.Scripting
{
	internal class AutocompleteItems
	{
		public static List<AutocompleteItem> GetItems()
		{
			// Create the Autocomplete list
			List<AutocompleteItem> items = new List<AutocompleteItem>();

			// Add key words to the Autocomplete list
			foreach (string item in KeyWords.MnemonicConstants)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.PluginMnemonics)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item);

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.Sections)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item);

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.TR5Commands)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item + "=");

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.TR5MainCommands)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item + "=");

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.NewCommands)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item + "=");

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.OldCommands)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item + "=");

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			foreach (string item in KeyWords.Unknown)
			{
				AutocompleteItem autocompleteItem = new AutocompleteItem(item + "=");

				if (!items.Exists(x => x.Text.ToLower() == autocompleteItem.Text.ToLower()))
					items.Add(autocompleteItem);
			}

			AutocompleteItem enabledItem = new AutocompleteItem("ENABLED");
			AutocompleteItem disabledItem = new AutocompleteItem("DISABLED");

			if (!items.Exists(x => x.Text.ToLower() == enabledItem.Text.ToLower()))
				items.Add(enabledItem);

			if (!items.Exists(x => x.Text.ToLower() == disabledItem.Text.ToLower()))
				items.Add(disabledItem);

			return items;
		}
	}
}
