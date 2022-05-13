using DarkUI.Docking;
using DarkUI.Forms;
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
using TombLib.Scripting.Enums;
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
			DockPanelState = DefaultLayouts.GameFlowScriptLayout;

			FileExplorer.Filter = "*.txt";

			EditorTabControl.PlainTextTypeOverride = typeof(GameFlowScriptStudio);

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
			|| obj is IDE.ScriptEditor_AddNewLevelStringEvent
			|| obj is IDE.ScriptEditor_ScriptPresenceCheckEvent
			|| obj is IDE.ScriptEditor_StringPresenceCheckEvent
			|| obj is IDE.ScriptEditor_RenameLevelEvent;

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (IsSilentAction(obj))
			{
				TabPage cachedTab = EditorTabControl.SelectedTab;

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));
				bool wasScriptFileAlreadyOpened = scriptFileTab != null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened && EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged;

				TabPage languageFileTab = EditorTabControl.FindTabPage(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English));
				bool wasLanguageFileAlreadyOpened = languageFileTab != null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened && EditorTabControl.GetEditorOfTab(languageFileTab).IsContentChanged;

				if (obj is IDE.ScriptEditor_AppendScriptLinesEvent asle && asle.Lines.Count > 0)
				{
					AppendScriptLines(asle.Lines);
					EndSilentScriptAction(cachedTab, true, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_AddNewLevelStringEvent anlse)
				{
					AddNewLevelNameString(anlse.LevelName);
					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent scrpce)
				{
					IDE.Global.ScriptDefined = IsLevelScriptDefined(scrpce.LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_StringPresenceCheckEvent strpce)
				{
					IDE.Global.StringDefined = IsLevelLanguageStringDefined(strpce.String);
					EndSilentScriptAction(cachedTab, false, false, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_RenameLevelEvent rle)
				{
					string oldName = rle.OldName;
					string newName = rle.NewName;

					RenameRequestedLevelScript(oldName, newName);
					RenameRequestedLanguageString(oldName, newName);

					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
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

		private void AddNewLevelNameString(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
				LanguageStringWriter.WriteNewLevelNameString(editor, levelName);
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));

			if (CurrentEditor is TextEditorBase editor)
				ScriptReplacer.RenameLevelScript(editor, oldName, newName);
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
				ScriptReplacer.RenameLanguageString(editor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath));

			if (CurrentEditor is TextEditorBase editor)
				return DocumentParser.IsLevelScriptDefined(editor.Document, levelName);

			return false;
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
				return DocumentParser.IsLevelLanguageStringDefined(editor.Document, levelName);

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
					IDE.Global.Project.GameVersion == TombLib.LevelData.TRVersion.Game.TR3);

				if (IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild)
				{
					if (!DockPanel.ContainsContent(CompilerLogs))
					{
						CompilerLogs.DockArea = DarkDockArea.Bottom;
						DockPanel.AddContent(CompilerLogs);
					}

					CompilerLogs.DockGroup.SetVisibleContent(CompilerLogs);
				}

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

		#endregion Other methods
	}
}
