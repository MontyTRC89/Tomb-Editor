namespace TombIDE.ScriptingStudio.UI
{
	public enum UICommand
	{
		None,

		// File:

		NewFile,
		Save,
		SaveAs,
		SaveAll,
		Build,

		// Edit:

		Undo,
		Redo,
		Cut,
		Copy,
		Paste,
		Find,
		SelectAll,

		// Document:

		Convert,

		TabsToSpaces,
		SpacesToTabs,
		IndentationSize,

		Reindent,
		TrimWhiteSpace,
		CommentOut,
		Uncomment,
		ToggleBookmark,
		PrevBookmark,
		NextBookmark,
		ClearBookmarks,

		PrevSection,
		NextSection,
		ClearCell,
		RemoveLastCell,

		// Options:

		UseNewInclude,
		ShowLogsAfterBuild,
		ReindentOnSave,
		Settings,

		// View:

		ToolStrip, // View start

		ContentExplorer,
		FileExplorer,
		ReferenceBrowser,
		CompilerLogs,
		SearchResults,

		StatusStrip, // View end

		// Help:

		About
	}
}
