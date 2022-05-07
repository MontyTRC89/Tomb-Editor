namespace TombIDE.Shared
{
	public enum IDETab
	{
		LevelManager,
		ProjectManager,
		ScriptingStudio
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
