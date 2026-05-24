using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

internal static class ScriptingWorkspaceCommandSurfaceFactory
{
	public static IReadOnlyList<StudioToolStripItem> CreateMenuStripContributions(ScriptingWorkspaceKind kind)
		=> kind switch
		{
			ScriptingWorkspaceKind.ClassicScript => CreateClassicScriptMenuStripContributions(),
			ScriptingWorkspaceKind.GameFlowScript => CreateGameFlowMenuStripContributions(),
			ScriptingWorkspaceKind.TRX => CreateTrxMenuStripContributions(),
			ScriptingWorkspaceKind.Lua => CreateLuaMenuStripContributions(),
			_ => []
		};

	public static IReadOnlyList<StudioToolStripItem> CreateToolStripContributions(ScriptingWorkspaceKind kind)
		=> kind switch
		{
			ScriptingWorkspaceKind.ClassicScript => CreateClassicScriptToolStripContributions(),
			ScriptingWorkspaceKind.GameFlowScript => CreateGameFlowToolStripContributions(),
			ScriptingWorkspaceKind.TRX => CreateTrxToolStripContributions(),
			ScriptingWorkspaceKind.Lua => CreateLuaToolStripContributions(),
			_ => []
		};

	private static IReadOnlyList<StudioToolStripItem> CreateClassicScriptMenuStripContributions()
		=>
		[
			CreateRootItem("File", "0",
				CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16", keys: "NewFile"),
				CreateSeparator(),
				CreateCommandItem("Save", UICommand.Save, icon: "Save_16", keys: "Save"),
				CreateCommandItem("SaveAs", UICommand.SaveAs),
				CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16", keys: "SaveAll"),
				CreateSeparator(),
				CreateCommandItem("Build", UICommand.Build, icon: "Play_16", keys: "Build"),
				CreateSeparator(),
				CreateCommandItem("Exit", UICommand.Exit, icon: "Exit_16", keys: "Exit")),

			CreateRootItem("Edit", "1",
				CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16", keysDisplay: "Ctrl+Z"),
				CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16", keysDisplay: "Ctrl+Y"),
				CreateSeparator(),
				CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16", keysDisplay: "Ctrl+X"),
				CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16", keysDisplay: "Ctrl+C"),
				CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16", keysDisplay: "Ctrl+V"),
				CreateSeparator(),
				CreateCommandItem("Find", UICommand.Find, icon: "Search_16", keysDisplay: "Ctrl+F / Ctrl+H"),
				CreateSeparator(),
				CreateCommandItem("SelectAll", UICommand.SelectAll, keysDisplay: "Ctrl+A")),

			CreateRootItem("Options", "3",
				CreateCommandItem("UseNewInclude", UICommand.UseNewInclude, checkOnClick: true),
				CreateCommandItem("ShowLogsAfterBuild", UICommand.ShowLogsAfterBuild, checkOnClick: true),
				CreateSeparator(),
				CreateCommandItem("Settings", UICommand.Settings, icon: "Settings_16")),

			CreateViewRootItem(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.ReferenceBrowser,
				UICommand.CompilerLogs,
				UICommand.SearchResults),

			CreateRootItem("Help", "5",
				CreateCommandItem("ScriptingDocumentation", UICommand.ScriptingDocumentation),
				CreateSeparator(),
				CreateCommandItem("About", UICommand.About, icon: "About_16"))
		];

	private static IReadOnlyList<StudioToolStripItem> CreateGameFlowMenuStripContributions()
		=>
		[
			CreateRootItem("File", "0",
				CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16", keys: "NewFile"),
				CreateSeparator(),
				CreateCommandItem("Save", UICommand.Save, icon: "Save_16", keys: "Save"),
				CreateCommandItem("SaveAs", UICommand.SaveAs),
				CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16", keys: "SaveAll"),
				CreateSeparator(),
				CreateCommandItem("Build", UICommand.Build, icon: "Play_16", keys: "Build"),
				CreateSeparator(),
				CreateCommandItem("Exit", UICommand.Exit, icon: "Exit_16", keys: "Exit")),

			CreateRootItem("Edit", "1",
				CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16", keysDisplay: "Ctrl+Z"),
				CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16", keysDisplay: "Ctrl+Y"),
				CreateSeparator(),
				CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16", keysDisplay: "Ctrl+X"),
				CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16", keysDisplay: "Ctrl+C"),
				CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16", keysDisplay: "Ctrl+V"),
				CreateSeparator(),
				CreateCommandItem("Find", UICommand.Find, icon: "Search_16", keysDisplay: "Ctrl+F / Ctrl+H"),
				CreateSeparator(),
				CreateCommandItem("SelectAll", UICommand.SelectAll, keysDisplay: "Ctrl+A")),

			CreateRootItem("Options", "3",
				CreateCommandItem("ShowLogsAfterBuild", UICommand.ShowLogsAfterBuild, checkOnClick: true),
				CreateSeparator(),
				CreateCommandItem("Settings", UICommand.Settings, icon: "Settings_16")),

			CreateViewRootItem(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.CompilerLogs,
				UICommand.SearchResults),

			CreateRootItem("Help", "5",
				CreateCommandItem("ScriptingDocumentation", UICommand.ScriptingDocumentation),
				CreateCommandItem("Tomb3ExtraCommands", UICommand.Tomb3ExtraCommands),
				CreateSeparator(),
				CreateCommandItem("About", UICommand.About, icon: "About_16"))
		];

	private static IReadOnlyList<StudioToolStripItem> CreateTrxMenuStripContributions()
		=>
		[
			CreateRootItem("File", "0",
				CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16", keys: "NewFile"),
				CreateSeparator(),
				CreateCommandItem("Save", UICommand.Save, icon: "Save_16", keys: "Save"),
				CreateCommandItem("SaveAs", UICommand.SaveAs),
				CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16", keys: "SaveAll"),
				CreateSeparator(),
				CreateCommandItem("Exit", UICommand.Exit, icon: "Exit_16", keys: "Exit")),

			CreateRootItem("Edit", "1",
				CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16", keysDisplay: "Ctrl+Z"),
				CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16", keysDisplay: "Ctrl+Y"),
				CreateSeparator(),
				CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16", keysDisplay: "Ctrl+X"),
				CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16", keysDisplay: "Ctrl+C"),
				CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16", keysDisplay: "Ctrl+V"),
				CreateSeparator(),
				CreateCommandItem("Find", UICommand.Find, icon: "Search_16", keysDisplay: "Ctrl+F / Ctrl+H"),
				CreateSeparator(),
				CreateCommandItem("SelectAll", UICommand.SelectAll, keysDisplay: "Ctrl+A")),

			CreateRootItem("Options", "3",
				CreateCommandItem("Settings", UICommand.Settings, icon: "Settings_16")),

			CreateViewRootItem(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.SearchResults),

			CreateRootItem("Help", "5",
				CreateCommandItem("About", UICommand.About, icon: "About_16"))
		];

	private static IReadOnlyList<StudioToolStripItem> CreateLuaMenuStripContributions()
		=>
		[
			CreateRootItem("File", "0",
				CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16", keys: "NewFile"),
				CreateSeparator(),
				CreateCommandItem("Save", UICommand.Save, icon: "Save_16", keys: "Save"),
				CreateCommandItem("SaveAs", UICommand.SaveAs),
				CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16", keys: "SaveAll"),
				CreateSeparator(),
				CreateCommandItem("Exit", UICommand.Exit, icon: "Exit_16", keys: "Exit")),

			CreateRootItem("Edit", "1",
				CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16", keysDisplay: "Ctrl+Z"),
				CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16", keysDisplay: "Ctrl+Y"),
				CreateSeparator(),
				CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16", keysDisplay: "Ctrl+X"),
				CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16", keysDisplay: "Ctrl+C"),
				CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16", keysDisplay: "Ctrl+V"),
				CreateSeparator(),
				CreateCommandItem("Find", UICommand.Find, icon: "Search_16", keysDisplay: "Ctrl+F / Ctrl+H"),
				CreateCommandItem("NavigateBack", UICommand.NavigateBack, icon: "Left_16", keys: "NavigateBack"),
				CreateCommandItem("NavigateForward", UICommand.NavigateForward, icon: "Right_16", keys: "NavigateForward"),
				CreateSeparator(),
				CreateCommandItem("SelectAll", UICommand.SelectAll, keysDisplay: "Ctrl+A")),

			CreateRootItem("Options", "3",
				CreateCommandItem("Settings", UICommand.Settings, icon: "Settings_16")),

			CreateViewRootItem(
				UICommand.FileExplorer,
				UICommand.SearchResults,
				UICommand.LuaDiagnostics,
				UICommand.LuaReferencesResults),

			CreateRootItem("Help", "5",
				CreateCommandItem("LuaBasics", UICommand.LuaBasics),
				CreateSeparator(),
				CreateCommandItem("About", UICommand.About, icon: "About_16"))
		];

	private static IReadOnlyList<StudioToolStripItem> CreateClassicScriptToolStripContributions()
		=> CreateFileEditToolStrip(includeBuild: true);

	private static IReadOnlyList<StudioToolStripItem> CreateGameFlowToolStripContributions()
		=> CreateFileEditToolStrip(includeBuild: true);

	private static IReadOnlyList<StudioToolStripItem> CreateTrxToolStripContributions()
		=> CreateFileEditToolStrip(includeBuild: false);

	private static IReadOnlyList<StudioToolStripItem> CreateLuaToolStripContributions()
		=>
		[
			CreateCommandItem("NavigateBack", UICommand.NavigateBack, icon: "Left_16"),
			CreateCommandItem("NavigateForward", UICommand.NavigateForward, icon: "Right_16"),
			CreateSeparator(),
			CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16"),
			CreateSeparator(),
			CreateCommandItem("Save", UICommand.Save, icon: "Save_16"),
			CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16"),
			CreateSeparator(),
			CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16"),
			CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16"),
			CreateSeparator(),
			CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16"),
			CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16"),
			CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16")
		];

	private static IReadOnlyList<StudioToolStripItem> CreateFileEditToolStrip(bool includeBuild)
	{
		var items = new List<StudioToolStripItem>
		{
			CreateCommandItem("NewFile", UICommand.NewFile, icon: "New_16"),
			CreateSeparator(),
			CreateCommandItem("Save", UICommand.Save, icon: "Save_16"),
			CreateCommandItem("SaveAll", UICommand.SaveAll, icon: "SaveAll_16"),
			CreateSeparator(),
			CreateCommandItem("Undo", UICommand.Undo, icon: "Undo_16"),
			CreateCommandItem("Redo", UICommand.Redo, icon: "Redo_16"),
			CreateSeparator(),
			CreateCommandItem("Cut", UICommand.Cut, icon: "Cut_16"),
			CreateCommandItem("Copy", UICommand.Copy, icon: "Copy_16"),
			CreateCommandItem("Paste", UICommand.Paste, icon: "Clipboard_16")
		};

		if (includeBuild)
		{
			items.Add(CreateSeparator());
			items.Add(CreateButtonItem("Build", UICommand.Build, "Play_16"));
		}

		return items;
	}

	private static StudioToolStripItem CreateCommandItem(
		string langKey,
		UICommand command,
		string icon = "",
		string keys = "",
		string keysDisplay = "",
		bool checkOnClick = false)
		=> new StudioToolStripItem
		{
			LangKey = langKey,
			Command = command.ToString(),
			Icon = icon,
			Keys = keys,
			KeysDisplay = keysDisplay,
			CheckOnClick = checkOnClick
		};

	private static StudioToolStripButton CreateButtonItem(string langKey, UICommand command, string icon)
		=> new StudioToolStripButton
		{
			LangKey = langKey,
			Command = command.ToString(),
			Icon = icon
		};

	private static StudioToolStripItem CreateGroupItem(string langKey, params StudioToolStripItem[] dropDownItems)
		=> new StudioToolStripItem
		{
			LangKey = langKey,
			DropDownItems = [..dropDownItems]
		};

	private static StudioToolStripItem CreateRootItem(string langKey, string position, params StudioToolStripItem[] dropDownItems)
		=> new StudioToolStripItem
		{
			LangKey = langKey,
			Position = position,
			DropDownItems = [..dropDownItems]
		};

	private static StudioSeparator CreateSeparator() => new();

	private static StudioToolStripItem CreateViewRootItem(params UICommand[] viewCommands)
	{
		var items = new List<StudioToolStripItem>
		{
			CreateCommandItem("RestoreDefaultLayout", UICommand.RestoreDefaultLayout),
			CreateSeparator()
		};

		for (int i = 0; i < viewCommands.Length; i++)
			items.Add(CreateViewCommandItem(viewCommands[i]));

		items.Add(CreateSeparator());
		items.Add(CreateCommandItem("ToolStrip", UICommand.ToolStrip, checkOnClick: true));
		items.Add(CreateCommandItem("StatusStrip", UICommand.StatusStrip, checkOnClick: true));

		return CreateRootItem("View", "4", [..items]);
	}

	private static StudioToolStripItem CreateViewCommandItem(UICommand command)
		=> CreateCommandItem(command.ToString(), command, checkOnClick: true);
}