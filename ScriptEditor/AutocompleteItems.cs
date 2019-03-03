using FastColoredTextBoxNS;
using System.Collections.Generic;

namespace ScriptEditor
{
	public class AutocompleteItems
	{
		public static List<AutocompleteItem> GetItems()
		{
			// Get key words
			List<string> sections = SyntaxKeyWords.Sections();
			List<string> tr5Commands = SyntaxKeyWords.TR5Commands();
			List<string> tr5MainCommands = SyntaxKeyWords.TR5MainCommands();
			List<string> newCommands = SyntaxKeyWords.NewCommands();
			List<string> oldCommands = SyntaxKeyWords.OldCommands();
			List<string> unknown = SyntaxKeyWords.Unknown();

			// Create the Autocomplete list
			List<AutocompleteItem> items = new List<AutocompleteItem>();

			// Add key words to the Autocomplete list
			foreach (string item in sections)
				items.Add(new AutocompleteItem(item));

			foreach (string item in tr5Commands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in tr5MainCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in newCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in oldCommands)
				items.Add(new AutocompleteItem(item));

			foreach (string item in unknown)
				items.Add(new AutocompleteItem(item));

			// Add these additional key words
			items.Add(new AutocompleteItem("ENABLED"));
			items.Add(new AutocompleteItem("DISABLED"));

			return items;
		}
	}
}
