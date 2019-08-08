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
			foreach (string item in KeyWords.Sections)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.TR5Commands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.TR5MainCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.NewCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.OldCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in KeyWords.Unknown)
				items.Add(new AutocompleteItem(item));

			// Add these additional key words
			items.Add(new AutocompleteItem("ENABLED"));
			items.Add(new AutocompleteItem("DISABLED"));

			return items;
		}
	}
}
