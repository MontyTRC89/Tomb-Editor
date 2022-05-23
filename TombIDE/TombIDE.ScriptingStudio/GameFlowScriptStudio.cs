﻿using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Bases;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.GameFlowScript.Utils;
using TombLib.Scripting.GameFlowScript.Writers;
using TombLib.Scripting.Interfaces;

namespace TombIDE.ScriptingStudio
{
	public sealed class GameFlowScriptStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.GameFlowScript;

		#region Construction

		public GameFlowScriptStudio() : base(IDE.Global.Project.ScriptPath, IDE.Global.Project.EnginePath)
		{
			DockPanelState = IDE.Global.IDEConfiguration.GFL_DockPanelState;

			FileExplorer.Filter = "*.txt";
			FileExplorer.CommentPrefix = "//";

			EditorTabControl.PlainTextTypeOverride = typeof(GameFlowScriptStudio);

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Global.Project.ScriptPath);

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		#endregion Construction

		#region IDE Events

		protected override void OnIDEEventRaised(IIDEEvent obj)
		{
			base.OnIDEEventRaised(obj);

			IDEEvent_HandleSilentActions(obj);
		}

		private bool IsSilentAction(IIDEEvent obj)
			=> obj is IDE.ScriptEditor_AppendScriptLinesEvent
			|| obj is IDE.ScriptEditor_ScriptPresenceCheckEvent
			|| obj is IDE.ScriptEditor_RenameLevelEvent;

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (IsSilentAction(obj))
			{
				TabPage cachedTab = EditorTabControl.SelectedTab;

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));
				bool wasScriptFileAlreadyOpened = scriptFileTab != null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened && EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged;

				if (obj is IDE.ScriptEditor_AppendScriptLinesEvent asle && asle.Lines.Count > 0)
				{
					AppendScriptLines(asle.Lines);
					EndSilentScriptAction(cachedTab, true, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent scrpce)
				{
					IDE.Global.ScriptDefined = IsLevelScriptDefined(scrpce.LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_RenameLevelEvent rle)
				{
					string oldName = rle.OldName;
					string newName = rle.NewName;

					RenameRequestedLevelScript(oldName, newName);
					EndSilentScriptAction(cachedTab, true, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
			}
			else if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Global.IDEConfiguration.GFL_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Global.IDEConfiguration.Save();
			}
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));

			if (CurrentEditor is TextEditorBase editor)
			{
				editor.AppendText(string.Join(Environment.NewLine, inputLines) + Environment.NewLine);
				editor.ScrollToLine(editor.LineCount);
			}
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));

			if (CurrentEditor is TextEditorBase editor)
				ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));

			if (CurrentEditor is TextEditorBase editor)
				return DocumentParser.IsLevelScriptDefined(editor.Document, levelName);

			return false;
		}

		private void EndSilentScriptAction(TabPage previousTab, bool indicateChange, bool saveAffectedFile, bool closeAffectedTab)
		{
			if (indicateChange)
			{
				CurrentEditor.LastModified = DateTime.Now;
				IDE.Global.ScriptEditor_IndicateExternalChange();
			}

			if (saveAffectedFile)
				EditorTabControl.SaveFile(EditorTabControl.SelectedTab);

			if (closeAffectedTab)
				EditorTabControl.TabPages.Remove(EditorTabControl.SelectedTab);

			EditorTabControl.EnsureTabFileSynchronization();

			if (previousTab != null)
				EditorTabControl.SelectTab(previousTab);
		}

		#endregion IDE Events

		#region Other methods

		protected override void ApplyUserSettings(IEditorControl editor)
			=> editor.UpdateSettings(Configs.GameFlowScript);

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				ApplyUserSettings(EditorTabControl.GetEditorOfTab(tab));

			UpdateSettings();
		}

		protected override void Build()
		{
			EditorTabControl.SaveAll();

			try
			{
				bool success = ScriptCompiler.Compile(
					ScriptRootDirectoryPath, EngineDirectoryPath,
					IDE.Global.Project.GameVersion == TombLib.LevelData.TRVersion.Game.TR3,
					IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild);

				if (success)
					CompilerLogs.UpdateLogs("Script compiled successfully!");
				else
					CompilerLogs.UpdateLogs("ERROR: Couldn't compile script.");
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.GameFlowScriptLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
		}

		#endregion Other methods
	}
}
