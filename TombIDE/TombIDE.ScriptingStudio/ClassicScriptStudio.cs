﻿using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Bases;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Forms;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.ClassicScript.Utils;
using TombLib.Scripting.ClassicScript.Writers;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;

namespace TombIDE.ScriptingStudio
{
	public sealed class ClassicScriptStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.ClassicScript;

		#region Fields

		private FormReferenceInfo FormReferenceInfo = new FormReferenceInfo();
		private FormNGCompilingStatus FormCompiling = new FormNGCompilingStatus();

		private BackgroundWorker NGCBackgroundWorker = new BackgroundWorker();

		public ReferenceBrowser ReferenceBrowser = new ReferenceBrowser();

		#endregion Fields

		#region Construction

		public ClassicScriptStudio() : base(IDE.Global.Project.ScriptPath, IDE.Global.Project.EnginePath)
		{
			DockPanelState = IDE.Global.IDEConfiguration.CS_DockPanelState;

			EditorTabControl.FileOpened += EditorTabControl_FileOpened;

			NGCBackgroundWorker.DoWork += NGCBackgroundWorker_DoWork;
			NGCBackgroundWorker.RunWorkerCompleted += NGCBackgroundWorker_RunWorkerCompleted;

			ReferenceBrowser.ReferenceDefinitionRequested += ReferenceBrowser_ReferenceDefinitionRequested;

			FileExplorer.Filter = "*.txt";
			FileExplorer.CommentPrefix = ";";

			EditorTabControl.PlainTextTypeOverride = typeof(ClassicScriptEditor);

			EditorTabControl.CheckPreviousSession();

			string initialFilePath = PathHelper.GetScriptFilePath(IDE.Global.Project.ScriptPath, TombLib.LevelData.TRVersion.Game.TR4);

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
			|| obj is IDE.ScriptEditor_AddNewPluginEntryEvent
			|| obj is IDE.ScriptEditor_AddNewNGStringEvent
			|| obj is IDE.ScriptEditor_ScriptPresenceCheckEvent
			|| obj is IDE.ScriptEditor_StringPresenceCheckEvent
			|| obj is IDE.ScriptEditor_RenameLevelEvent;

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (IsSilentAction(obj))
			{
				TabPage cachedTab = EditorTabControl.SelectedTab;

				TabPage scriptFileTab = EditorTabControl.FindTabPage(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));
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
				else if (obj is IDE.ScriptEditor_AddNewPluginEntryEvent anpee)
				{
					bool isChanged = AddNewPluginEntry(anpee.PluginString);
					EndSilentScriptAction(cachedTab, isChanged, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_AddNewNGStringEvent anngse)
				{
					bool isChanged = AddNewNGString(anngse.NGString);
					EndSilentScriptAction(cachedTab, isChanged, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
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
			else if (obj is IDE.ScriptEditor_ReloadSyntaxHighlightingEvent)
			{
				ApplyUserSettings();
			}
			else if (obj is IDE.ProgramClosingEvent)
			{
				IDE.Global.IDEConfiguration.CS_DockPanelState = DockPanel.GetDockPanelState();
				IDE.Global.IDEConfiguration.Save();
			}
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));

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

		private bool AddNewPluginEntry(string pluginString)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));

			if (CurrentEditor is ClassicScriptEditor editor)
				return editor.TryAddNewPluginEntry(pluginString);

			return false;
		}

		private bool AddNewNGString(string ngString)
		{
			EditorTabControl.OpenFile(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English), EditorType.Text);

			if (CurrentEditor is TextEditorBase editor)
				return LanguageStringWriter.WriteNewNGString(editor, ngString);

			return false;
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));

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
			EditorTabControl.OpenFile(PathHelper.GetScriptFilePath(ScriptRootDirectoryPath, TombLib.LevelData.TRVersion.Game.TR4));

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

		#region Events

		private void EditorTabControl_FileOpened(object sender, EventArgs e)
		{
			if (sender is ClassicScriptEditor textEditor)
			{
				textEditor.MouseDoubleClick += TextEditor_MouseDoubleClick;
				textEditor.KeyDown += TextEditor_KeyDown;
				textEditor.WordDefinitionRequested += TextEditor_WordDefinitionRequested;
			}
		}

		private void TextEditor_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (CurrentEditor is ClassicScriptEditor editor)
			{
				if (e.ChangedButton == System.Windows.Input.MouseButton.Left && ModifierKeys == Keys.Control)
					OpenIncludeFile(editor);
			}
		}

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (CurrentEditor is ClassicScriptEditor editor)
			{
				if (e.Key == System.Windows.Input.Key.F5 && ModifierKeys != Keys.Control)
					OpenIncludeFile(editor);
			}
		}

		private void TextEditor_WordDefinitionRequested(object sender, WordDefinitionEventArgs e)
		{
			string word = e.Word;

			ReferenceType type = ReferenceType.MnemonicConstant;

			if (e.Type == WordType.Header)
				type = ReferenceType.OldCommand;
			else if (e.Type == WordType.Command)
				type = RddaReader.GetCommandType(word);
			else if (e.Type == WordType.Directive)
				type = ReferenceType.NewCommand;
			else if (e.Type == WordType.Hexadecimal || e.Type == WordType.Decimal)
			{
				try
				{
					var textEditor = CurrentEditor as ClassicScriptEditor;

					if (textEditor == null)
						return;

					int offset = e.HoveredOffset != -1 ? e.HoveredOffset : textEditor.CaretOffset;
					string currentFlagPrefix = ArgumentParser.GetFlagPrefixOfCurrentArgument(textEditor.Document, offset);

					if (currentFlagPrefix != null)
					{
						DataTable dataTable = MnemonicData.MnemonicConstantsDataTable;
						DataRow row = null;

						if (e.Type == WordType.Hexadecimal)
						{
							row = dataTable.Rows.Cast<DataRow>().FirstOrDefault(r
								=> r[1].ToString().Equals(word, StringComparison.OrdinalIgnoreCase)
								&& r[2].ToString().StartsWith(currentFlagPrefix, StringComparison.OrdinalIgnoreCase));
						}
						else if (e.Type == WordType.Decimal)
						{
							row = dataTable.Rows.Cast<DataRow>().FirstOrDefault(r
								=> r[0].ToString().Equals(word, StringComparison.OrdinalIgnoreCase)
								&& r[2].ToString().StartsWith(currentFlagPrefix, StringComparison.OrdinalIgnoreCase));
						}

						if (row != null)
							word = row[2].ToString();
					}
				}
				catch { }
			}

			FormReferenceInfo.Show(word, type);
		}

		private void NGCBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Process ngCenterProcess = Array.Find(Process.GetProcesses(), x => x.ProcessName.Contains("NG_Center"));

			if (ngCenterProcess != null)
				ngCenterProcess.WaitForExit();
		}

		private void NGCBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (FormCompiling.Visible)
				FormCompiling.Close();
		}

		private void ReferenceBrowser_ReferenceDefinitionRequested(object sender, ReferenceDefinitionEventArgs e)
			=> FormReferenceInfo.Show(e.Keyword, e.Type);

		#endregion Events

		#region Other methods

		private void CreateNewFileAtCaretPosition(ClassicScriptEditor editor)
		{
			string filePath = FileExplorer.CreateNewFile();

			if (filePath != null)
			{
				editor.SuppressAutocomplete = true;

				string includeValue = filePath.Replace(Path.GetDirectoryName(editor.FilePath), string.Empty).TrimStart('\\');
				editor.TextArea.PerformTextInput($"{Environment.NewLine}#INCLUDE \"{includeValue}\"");

				editor.SuppressAutocomplete = false;
			}
		}

		private void OpenIncludeFile(ClassicScriptEditor editor)
		{
			string fullFilePath = CommandParser.GetFullIncludePath(editor.Document, editor.CaretOffset);

			if (File.Exists(fullFilePath))
				EditorTabControl.OpenFile(fullFilePath);
			else
				DarkMessageBox.Show(this, "Couldn't find the target file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			editor.SelectionLength = 0;
		}

		private void CompileTR4Script()
		{
			try
			{
				string logs = TR4Compiler.Compile(ScriptRootDirectoryPath, EngineDirectoryPath);

				if (IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild)
				{
					if (!DockPanel.ContainsContent(CompilerLogs))
					{
						CompilerLogs.DockArea = DarkDockArea.Bottom;
						DockPanel.AddContent(CompilerLogs);
					}

					CompilerLogs.DockGroup.SetVisibleContent(CompilerLogs);
				}

				CompilerLogs.UpdateLogs(logs);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void CompileTRNGScript()
		{
			try
			{
				FormCompiling.ShowCompilingMode();
				FormCompiling.Show();

				bool success = await NGCompiler.Compile(
					ScriptRootDirectoryPath, EngineDirectoryPath,
					IDE.Global.IDEConfiguration.UseNewIncludeMethod);

				if (success)
				{
					string logFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "script_log.txt");
					CompilerLogs.UpdateLogs(File.ReadAllText(logFilePath));

					if (IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild)
					{
						if (!DockPanel.ContainsContent(CompilerLogs))
						{
							CompilerLogs.DockArea = DarkDockArea.Bottom;
							DockPanel.AddContent(CompilerLogs);
						}

						CompilerLogs.DockGroup.SetVisibleContent(CompilerLogs);
					}

					if (FormCompiling.Visible)
						FormCompiling.Close();
				}
				else
				{
					FormCompiling.ShowDebugMode();

					if (!NGCBackgroundWorker.IsBusy)
						NGCBackgroundWorker.RunWorkerAsync();
				}
			}
			catch (Exception ex)
			{
				if (FormCompiling.Visible)
					FormCompiling.Close();

				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected override void ApplyUserSettings(IEditorControl editor)
			=> editor.UpdateSettings(Configs.ClassicScript);

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				ApplyUserSettings(EditorTabControl.GetEditorOfTab(tab));

			StatusStrip.SyntaxPreview.ReloadSettings();

			UpdateSettings();
		}

		protected override void Build()
		{
			EditorTabControl.SaveAll();

			if (IDE.Global.Project.GameVersion == TombLib.LevelData.TRVersion.Game.TR4)
				CompileTR4Script();
			else if (IDE.Global.Project.GameVersion == TombLib.LevelData.TRVersion.Game.TRNG)
				CompileTRNGScript();
		}

		protected override void RestoreDefaultLayout()
		{
			DockPanelState = DefaultLayouts.ClassicScriptLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
		}

		protected override void HandleDocumentCommands(UICommand command)
		{
			if (CurrentEditor is ClassicScriptEditor editor)
			{
				switch (command)
				{
					case UICommand.TypeFirstAvailableId:
						editor.InputFreeIndex();
						break;

					case UICommand.NewFileAtCaret:
						CreateNewFileAtCaretPosition(editor);
						break;
				}
			}

			base.HandleDocumentCommands(command);
		}

		#endregion Other methods
	}
}
