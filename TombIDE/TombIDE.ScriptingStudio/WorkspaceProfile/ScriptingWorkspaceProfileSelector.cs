#nullable enable

using System;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

public static class ScriptingWorkspaceProfileSelector
{
	public static ScriptingWorkspaceProfile Create(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, ClassicScriptLanguageServices classicScriptServices, GameFlowLanguageServices gameFlowServices, TRXLanguageServices trxServices)
	{
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(settingsStore);
		ArgumentNullException.ThrowIfNull(classicScriptServices);
		ArgumentNullException.ThrowIfNull(gameFlowServices);
		ArgumentNullException.ThrowIfNull(trxServices);

		return projectContext.Project.GameVersion switch
		{
			TRVersion.Game.TR4 or TRVersion.Game.TRNG => CreateClassicScriptProfile(projectContext, settingsStore, classicScriptServices),
			TRVersion.Game.TR2 or TRVersion.Game.TR3 => CreateGameFlowProfile(projectContext, settingsStore, gameFlowServices),
			TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR3X => CreateTrxProfile(projectContext, settingsStore, trxServices),
			TRVersion.Game.TombEngine => CreateLuaProfile(projectContext, settingsStore),
			_ => throw new NotSupportedException($"Unsupported scripting workspace game version: {projectContext.Project.GameVersion}.")
		};
	}

	private static ScriptingWorkspaceProfile CreateClassicScriptProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, ClassicScriptLanguageServices languageServices)
	{
		ScriptingWorkspaceProfile? workspaceProfile = null;
		workspaceProfile = new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.ClassicScript,
			projectContext.Project.GameVersion,
			[DocumentMode.ClassicScript, DocumentMode.Strings, DocumentMode.PlainText],
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TR4),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.ReferenceBrowser,
				UICommand.CompilerLogs,
				UICommand.SearchResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.ClassicScript, "TR4 / TRNG Script", [DocumentMode.ClassicScript, DocumentMode.Strings, DocumentMode.PlainText])],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.ClassicScript),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.ClassicScript),
			"*.txt",
			string.Empty,
			";",
			true,
			true,
			DefaultLayouts.ClassicScriptLayout,
			documentController => RegisterClassicScriptEditors(documentController, languageServices),
			() => LoadLayoutSettings(settingsStore, workspaceProfile).DockPanelState,
			() => LoadLayoutSettings(settingsStore, workspaceProfile).AvalonDockLayoutXml,
			xml => SaveLayoutXml(settingsStore, workspaceProfile, xml));

		return workspaceProfile;
	}

	private static ScriptingWorkspaceProfile CreateGameFlowProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, GameFlowLanguageServices gameFlowServices)
	{
		ScriptingWorkspaceProfile? workspaceProfile = null;
		workspaceProfile = new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.GameFlowScript,
			projectContext.Project.GameVersion,
			[DocumentMode.GameFlowScript, DocumentMode.PlainText],
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TR2),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.CompilerLogs,
				UICommand.SearchResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.GameFlowScript, "TR2 / TR3 Script", [DocumentMode.GameFlowScript, DocumentMode.PlainText])],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.GameFlowScript),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.GameFlowScript),
			"*.txt",
			string.Empty,
			"//",
			true,
			true,
			DefaultLayouts.GameFlowScriptLayout,
			documentController => RegisterGameFlowEditors(documentController, gameFlowServices),
			() => LoadLayoutSettings(settingsStore, workspaceProfile).DockPanelState,
			() => LoadLayoutSettings(settingsStore, workspaceProfile).AvalonDockLayoutXml,
			xml => SaveLayoutXml(settingsStore, workspaceProfile, xml));

		return workspaceProfile;
	}

	private static ScriptingWorkspaceProfile CreateTrxProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, TRXLanguageServices trxServices)
	{
		ScriptingWorkspaceProfile? workspaceProfile = null;
		workspaceProfile = new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.TRX,
			projectContext.Project.GameVersion,
			[DocumentMode.TRX],
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, projectContext.Project.GameVersion),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.SearchResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.TRX, "TRX Script", [DocumentMode.TRX])],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.TRX),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.TRX),
			"*.json5",
			string.Empty,
			"//",
			false,
			false,
			DefaultLayouts.TRXLayout,
			dc => RegisterTrxEditors(dc, trxServices),
			() => LoadLayoutSettings(settingsStore, workspaceProfile).DockPanelState,
			() => LoadLayoutSettings(settingsStore, workspaceProfile).AvalonDockLayoutXml,
			xml => SaveLayoutXml(settingsStore, workspaceProfile, xml));

		return workspaceProfile;
	}

	private static ScriptingWorkspaceProfile CreateLuaProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore)
	{
		ScriptingWorkspaceProfile? workspaceProfile = null;
		workspaceProfile = new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.Lua,
			projectContext.Project.GameVersion,
			[DocumentMode.Lua],
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TombEngine),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.SearchResults,
				UICommand.LuaDiagnostics,
				UICommand.LuaReferencesResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.Lua, "Lua", [DocumentMode.Lua])],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.Lua),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.Lua),
			"*.lua",
			"Scripts\\Engine",
			"--",
			false,
			false,
			DefaultLayouts.LuaLayout,
			RegisterLuaEditors,
			() => LoadLayoutSettings(settingsStore, workspaceProfile).DockPanelState,
			() => LoadLayoutSettings(settingsStore, workspaceProfile).AvalonDockLayoutXml,
			xml => SaveLayoutXml(settingsStore, workspaceProfile, xml));

		return workspaceProfile;
	}

	private static ScriptingStudioShellWorkspaceSettings LoadLayoutSettings(
		IScriptingStudioShellSettingsStore settingsStore,
		ScriptingWorkspaceProfile? workspaceProfile)
		=> settingsStore.Load(workspaceProfile ?? throw new InvalidOperationException("The workspace profile must be initialized before layout settings can be loaded."));

	private static void SaveLayoutXml(
		IScriptingStudioShellSettingsStore settingsStore,
		ScriptingWorkspaceProfile? workspaceProfile,
		string xml)
	{
		ScriptingWorkspaceProfile profile = workspaceProfile ?? throw new InvalidOperationException("The workspace profile must be initialized before layout settings can be saved.");
		ScriptingStudioShellWorkspaceSettings settings = settingsStore.Load(profile);
		settings.AvalonDockLayoutXml = xml ?? string.Empty;
		settingsStore.Save(profile.Kind, settings);
	}

	private static ScriptingWorkspaceViewContribution[] CreateViewContributions(params UICommand[] commands)
		=> [.. commands.Select(static command => new ScriptingWorkspaceViewContribution(command))];

	private static StudioStatusStripSegment[] CreateSharedStatusStripSegments() =>
	[
		StudioStatusStripSegment.CaretPosition,
		StudioStatusStripSegment.SelectionLength,
		StudioStatusStripSegment.Zoom
	];

	private static void RegisterClassicScriptEditors(IEditorDocumentController documentController, ClassicScriptLanguageServices languageServices)
	{
		documentController.RegisterTextEditor(
			engineVersion => new ClassicScriptEditor(engineVersion, languageServices),
			DocumentMode.ClassicScript,
			filePath => !FileHelper.IsStringFile(filePath, languageServices.LineService));
		documentController.RegisterStringsEditor(static engineVersion => new StringEditorView(engineVersion));
		documentController.RegisterPlainTextEditor(engineVersion => new ClassicScriptEditor(engineVersion, languageServices), DocumentMode.ClassicScript);
	}

	private static void RegisterGameFlowEditors(IEditorDocumentController documentController, GameFlowLanguageServices gameFlowServices)
	{
		documentController.RegisterTextEditor(engineVersion => new GameFlowEditor(engineVersion, gameFlowServices), DocumentMode.GameFlowScript);
		documentController.RegisterPlainTextEditor(engineVersion => new GameFlowEditor(engineVersion, gameFlowServices), DocumentMode.GameFlowScript);
	}

	private static void RegisterTrxEditors(IEditorDocumentController documentController, TRXLanguageServices trxServices)
		=> documentController.RegisterJson5Editor(engineVersion => new TRXEditor(engineVersion, trxServices), DocumentMode.TRX);

	private static void RegisterLuaEditors(IEditorDocumentController documentController)
		=> documentController.RegisterLuaEditor(static engineVersion => new LuaEditor(engineVersion), DocumentMode.Lua);
}
