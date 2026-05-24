using System;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombIDE.ScriptingStudio.UI;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Strings;

namespace TombIDE.ScriptingStudio.WorkspaceProfile
{
	public static class ScriptingWorkspaceProfileSelector
	{
		public static ScriptingWorkspaceProfile Create(IDE ide)
		{
			if (ide is null)
				throw new ArgumentNullException(nameof(ide));

			return ide.Project.GameVersion switch
			{
				TRVersion.Game.TR4 or TRVersion.Game.TRNG => CreateClassicScriptProfile(ide),
				TRVersion.Game.TR2 or TRVersion.Game.TR3 => CreateGameFlowProfile(ide),
				TRVersion.Game.TR1 or TRVersion.Game.TR2X => CreateTrxProfile(ide),
				TRVersion.Game.TombEngine => CreateLuaProfile(ide),
				_ => throw new NotSupportedException($"Unsupported scripting workspace game version: {ide.Project.GameVersion}.")
			};
		}

		private static ScriptingWorkspaceProfile CreateClassicScriptProfile(IDE ide)
			=> new(
				ScriptingWorkspaceKind.ClassicScript,
				ide.Project.GameVersion,
				new[] { DocumentMode.ClassicScript, DocumentMode.Strings, DocumentMode.PlainText },
				PathHelper.GetScriptFilePath(ide.Project.GetScriptRootDirectory(), TRVersion.Game.TR4),
				CreateViewContributions(
					UICommand.ContentExplorer,
					UICommand.FileExplorer,
					UICommand.ReferenceBrowser,
					UICommand.CompilerLogs,
					UICommand.SearchResults,
					UICommand.ToolStrip,
					UICommand.StatusStrip),
				CreateSharedStatusStripSegments(),
				[new(ScriptingSettingsPageKind.ClassicScript, "TR4 / TRNG Script", new[] { DocumentMode.ClassicScript, DocumentMode.Strings, DocumentMode.PlainText })],
				ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.ClassicScript),
				ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.ClassicScript),
				"*.txt",
				string.Empty,
				";",
				true,
				true,
				DefaultLayouts.ClassicScriptLayout,
				RegisterClassicScriptEditors,
				() => ide.IDEConfiguration.CS_DockPanelState,
				state =>
				{
					ide.IDEConfiguration.CS_DockPanelState = state;
					ide.IDEConfiguration.Save();
				});

		private static ScriptingWorkspaceProfile CreateGameFlowProfile(IDE ide)
			=> new(
				ScriptingWorkspaceKind.GameFlowScript,
				ide.Project.GameVersion,
				new[] { DocumentMode.GameFlowScript, DocumentMode.PlainText },
				PathHelper.GetScriptFilePath(ide.Project.GetScriptRootDirectory(), TRVersion.Game.TR2),
				CreateViewContributions(
					UICommand.ContentExplorer,
					UICommand.FileExplorer,
					UICommand.CompilerLogs,
					UICommand.SearchResults,
					UICommand.ToolStrip,
					UICommand.StatusStrip),
				CreateSharedStatusStripSegments(),
				[new(ScriptingSettingsPageKind.GameFlowScript, "TR2 / TR3 Script", new[] { DocumentMode.GameFlowScript, DocumentMode.PlainText })],
				ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.GameFlowScript),
				ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.GameFlowScript),
				"*.txt",
				string.Empty,
				"//",
				true,
				true,
				DefaultLayouts.GameFlowScriptLayout,
				RegisterGameFlowEditors,
				() => ide.IDEConfiguration.GFL_DockPanelState,
				state =>
				{
					ide.IDEConfiguration.GFL_DockPanelState = state;
					ide.IDEConfiguration.Save();
				});

		private static ScriptingWorkspaceProfile CreateTrxProfile(IDE ide)
			=> new(
				ScriptingWorkspaceKind.TRX,
				ide.Project.GameVersion,
				new[] { DocumentMode.TRX },
				PathHelper.GetScriptFilePath(ide.Project.GetScriptRootDirectory(), ide.Project.GameVersion),
				CreateViewContributions(
					UICommand.ContentExplorer,
					UICommand.FileExplorer,
					UICommand.SearchResults,
					UICommand.ToolStrip,
					UICommand.StatusStrip),
				CreateSharedStatusStripSegments(),
				[new(ScriptingSettingsPageKind.TRX, "TRX Script", new[] { DocumentMode.TRX })],
				ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.TRX),
				ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.TRX),
				"*.json5",
				string.Empty,
				"//",
				false,
				false,
				DefaultLayouts.TRXLayout,
				RegisterTrxEditors,
				() => ide.IDEConfiguration.TRX_DockPanelState,
				state =>
				{
					ide.IDEConfiguration.TRX_DockPanelState = state;
					ide.IDEConfiguration.Save();
				});

		private static ScriptingWorkspaceProfile CreateLuaProfile(IDE ide)
			=> new(
				ScriptingWorkspaceKind.Lua,
				ide.Project.GameVersion,
				new[] { DocumentMode.Lua },
				PathHelper.GetScriptFilePath(ide.Project.GetScriptRootDirectory(), TRVersion.Game.TombEngine),
				CreateViewContributions(
					UICommand.ContentExplorer,
					UICommand.FileExplorer,
					UICommand.SearchResults,
					UICommand.LuaDiagnostics,
					UICommand.LuaReferencesResults,
					UICommand.ToolStrip,
					UICommand.StatusStrip),
				CreateSharedStatusStripSegments(),
				[new(ScriptingSettingsPageKind.Lua, "Lua", new[] { DocumentMode.Lua })],
				ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.Lua),
				ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.Lua),
				"*.lua",
				"Scripts\\Engine",
				"--",
				false,
				false,
				DefaultLayouts.LuaLayout,
				RegisterLuaEditors,
				() => ide.IDEConfiguration.Lua_DockPanelState,
				state =>
				{
					ide.IDEConfiguration.Lua_DockPanelState = state;
					ide.IDEConfiguration.Save();
				});

		private static ScriptingWorkspaceViewContribution[] CreateViewContributions(params UICommand[] commands)
			=> commands.Select(static command => new ScriptingWorkspaceViewContribution(command)).ToArray();

		private static StudioStatusStripSegment[] CreateSharedStatusStripSegments()
			=>
			[
				StudioStatusStripSegment.CaretPosition,
				StudioStatusStripSegment.SelectionLength,
				StudioStatusStripSegment.Zoom
			];

		private static void RegisterClassicScriptEditors(EditorTabControl editorTabControl)
		{
			editorTabControl.RegisterTextEditor(
				static engineVersion => new ClassicScriptEditor(engineVersion),
				DocumentMode.ClassicScript,
				filePath => !FileHelper.IsStringFile(filePath));
			editorTabControl.RegisterStringsEditor(static engineVersion => new StringEditor(engineVersion));
			editorTabControl.RegisterPlainTextEditor(static engineVersion => new ClassicScriptEditor(engineVersion), DocumentMode.ClassicScript);
		}

		private static void RegisterGameFlowEditors(EditorTabControl editorTabControl)
		{
			editorTabControl.RegisterTextEditor(static engineVersion => new GameFlowEditor(engineVersion), DocumentMode.GameFlowScript);
			editorTabControl.RegisterPlainTextEditor(static engineVersion => new GameFlowEditor(engineVersion), DocumentMode.GameFlowScript);
		}

		private static void RegisterTrxEditors(EditorTabControl editorTabControl)
			=> editorTabControl.RegisterJson5Editor(static engineVersion => new TRXEditor(engineVersion), DocumentMode.TRX);

		private static void RegisterLuaEditors(EditorTabControl editorTabControl)
			=> editorTabControl.RegisterLuaEditor(static engineVersion => new LuaEditor(engineVersion), DocumentMode.Lua);
	}
}