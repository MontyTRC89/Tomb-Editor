using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Interfaces;

namespace TombIDE.ScriptingStudio
{
	public sealed class LuaStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.Lua;

		#region Construction

		public LuaStudio() : base(IDE.Global.Project.ScriptPath, IDE.Global.Project.EnginePath)
		{
			DockPanelState = IDE.Global.IDEConfiguration.Lua_DockPanelState;

			FileExplorer.Filter = "*.lua";
			FileExplorer.CommentPrefix = "--";

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Global.Project.ScriptPath, TombLib.LevelData.TRVersion.Game.TombEngine);

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		#endregion Construction

		#region IDE Events

		protected override void OnIDEEventRaised(IIDEEvent obj)
		{
			base.OnIDEEventRaised(obj);

			IDEEvent_HandleSilentActions(obj);

			if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Global.IDEConfiguration.Lua_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Global.IDEConfiguration.Save();
			}
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

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TombEngine));
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
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			// TODO
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			// TODO
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			return false;
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.LuaLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
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
			=> editor.UpdateSettings(Configs.Lua);

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				ApplyUserSettings(EditorTabControl.GetEditorOfTab(tab));

			UpdateSettings();
		}

		protected override void Build()
		{
			// Nothing.
		}

		#endregion Other methods
	}
}
