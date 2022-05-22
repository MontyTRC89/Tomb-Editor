namespace TombIDE.Shared
{
	public enum IDETab
	{
		LevelManager,
		ScriptingStudio,
		PluginManager,
		Miscellaneous
	}

	public enum GameLanguage
	{
		// Default TR4 languages

		English,
		French,
		German,
		Italian,
		Spanish,
		Japanese,
		Dutch
	}

	public enum FileCreationMode
	{
		New,
		SavingAs
	}

	public enum FileSavingResult
	{
		AlreadySaved,
		Success,
		Rejected,
		Cancelled,
		Failed
	}
}
