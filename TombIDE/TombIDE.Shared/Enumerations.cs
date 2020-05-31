namespace TombIDE.Shared
{
	public enum IDETab
	{
		ProjectMaster,
		ScriptEditor
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

	public enum ObjectType
	{
		Section,
		Level,
		Include,
		Define
	}

	public enum ReferenceType
	{
		OCBs,
		OLDCommands,
		NEWCommands,
		Mnemonics
	}

	public enum FileCreationMode
	{
		New,
		Saving
	}

	public enum FindingOrder
	{
		Prev,
		Next
	}
}
