using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
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
using TombLib.Scripting.ClassicScript.Utils;
using TombLib.Scripting.Interfaces;

namespace TombIDE.ScriptingStudio
{
	public sealed class ClassicScriptStudio : StudioBase
	{
		public override StudioMode StudioMode => StudioMode.ClassicScript;

		private FormReferenceInfo FormReferenceInfo = new FormReferenceInfo();
		private FormNGCompilingStatus FormCompiling = new FormNGCompilingStatus();

		private BackgroundWorker NGCBackgroundWorker = new BackgroundWorker();

		public ReferenceBrowser ReferenceBrowser = new ReferenceBrowser();

		#region Construction

		public ClassicScriptStudio() : base(
			IDE.Global.Project.ScriptPath,
			IDE.Global.Project.EnginePath,
			PathHelper.GetScriptFilePath(IDE.Global.Project.ScriptPath))
		{
			DockPanelState = DefaultLayouts.ClassicScriptLayout;

			EditorTabControl.FileOpened += EditorTabControl_FileOpened;

			NGCBackgroundWorker.DoWork += NGCBackgroundWorker_DoWork;
			NGCBackgroundWorker.RunWorkerCompleted += NGCBackgroundWorker_RunWorkerCompleted;

			ReferenceBrowser.ReferenceDefinitionRequested += ReferenceBrowser_ReferenceDefinitionRequested;
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
			|| obj is IDE.ScriptEditor_AddNewNGStringEvent
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
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened ?
					EditorTabControl.GetEditorOfTab(scriptFileTab).IsContentChanged : false;

				TabPage languageFileTab = EditorTabControl.FindTabPage(PathHelper.GetLanguageFilePath(ScriptRootDirectoryPath, GameLanguage.English));
				bool wasLanguageFileAlreadyOpened = languageFileTab != null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened ?
					EditorTabControl.GetEditorOfTab(languageFileTab).IsContentChanged : false;

				if (obj is IDE.ScriptEditor_AppendScriptLinesEvent)
				{
					List<string> inputLines = ((IDE.ScriptEditor_AppendScriptLinesEvent)obj).Lines;

					if (inputLines.Count == 0)
						return;

					AppendScriptLines(inputLines);
					EndSilentScriptAction(cachedTab, true, !wasScriptFileFileChanged, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_AddNewLevelStringEvent)
				{
					AddNewLevelNameString(((IDE.ScriptEditor_AddNewLevelStringEvent)obj).LevelName);
					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_AddNewNGStringEvent)
				{
					bool isChanged = AddNewNGString(((IDE.ScriptEditor_AddNewNGStringEvent)obj).PluginName);
					EndSilentScriptAction(cachedTab, isChanged, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent)
				{
					IDE.Global.ScriptDefined = IsLevelScriptDefined(((IDE.ScriptEditor_ScriptPresenceCheckEvent)obj).LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_StringPresenceCheckEvent)
				{
					IDE.Global.StringDefined = IsLevelLanguageStringDefined(((IDE.ScriptEditor_StringPresenceCheckEvent)obj).LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_RenameLevelEvent)
				{
					string oldName = ((IDE.ScriptEditor_RenameLevelEvent)obj).OldName;
					string newName = ((IDE.ScriptEditor_RenameLevelEvent)obj).NewName;

					RenameRequestedLevelScript(oldName, newName);
					RenameRequestedLanguageString(oldName, newName);

					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
			}
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			//OpenScriptFile(true); // Changes the current CurrentTextEditor as well

			//CurrentTextEditor.AppendText(string.Join(Environment.NewLine, inputLines) + Environment.NewLine);
			//CurrentTextEditor.ScrollToLine(CurrentTextEditor.LineCount);
		}

		private void AddNewLevelNameString(string levelName)
		{
			//OpenLanguageFile(DefaultLanguage, true); // Changes the current CurrentTextEditor as well
			//LanguageStringWriter.WriteNewLevelNameString(CurrentTextEditor, levelName);
		}

		private bool AddNewNGString(string ngString)
		{
			//OpenLanguageFile(DefaultLanguage, true); // Changes the current CurrentTextEditor as well
			//return LanguageStringWriter.WriteNewNGString(CurrentTextEditor, ngString);
			return false;
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			//OpenScriptFile(true); // Changes the current CurrentTextEditor as well
			//ScriptReplacer.RenameLevelScript(CurrentTextEditor, oldName, newName);
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			//OpenLanguageFile(DefaultLanguage, true); // Changes the current CurrentTextEditor as well
			//ScriptReplacer.RenameLanguageString(CurrentTextEditor, oldName, newName);
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			//OpenScriptFile(true); // Changes the current CurrentTextEditor as well
			//return DocumentParser.IsLevelScriptDefined(CurrentTextEditor.Document, levelName);

			return false;
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			//OpenLanguageFile(DefaultLanguage, true); // Changes the current CurrentTextEditor as well
			//return DocumentParser.IsLevelLanguageStringDefined(CurrentTextEditor.Document, levelName);

			return false;
		}

		private void EndSilentScriptAction(TabPage previousTab, bool indicateChange, bool saveAffectedFile, bool closeAffectedTab)
		{
			if (indicateChange)
				IDE.Global.ScriptEditor_IndicateExternalChange();

			if (saveAffectedFile)
				EditorTabControl.SaveFile(EditorTabControl.SelectedTab);

			if (closeAffectedTab)
				EditorTabControl.TabPages.Remove(EditorTabControl.SelectedTab);

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
			if (e.ChangedButton == System.Windows.Input.MouseButton.Left && ModifierKeys == Keys.Control)
				OpenIncludeFile();
		}

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.F5)
				OpenIncludeFile();
		}

		private void TextEditor_WordDefinitionRequested(object sender, WordDefinitionEventArgs e)
		{
			string word = e.Word;

			ReferenceType type = ReferenceType.MnemonicConstant;

			if (e.Type == WordType.Header)
			{
				word = "[" + word + "]";
				type = ReferenceType.OldCommand;
			}
			else if (e.Type == WordType.Command)
				type = RddaReader.GetCommandType(word);

			FormReferenceInfo.Show(word, type);
		}

		private void NGCBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			foreach (Process process in Process.GetProcesses())
				if (process.ProcessName.Contains("NG_Center"))
				{
					process.WaitForExit();
					break;
				}
		}

		private void NGCBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (FormCompiling.Visible)
				FormCompiling.Close();
		}

		private void ReferenceBrowser_ReferenceDefinitionRequested(object sender, ReferenceDefinitionEventArgs e)
			=> FormReferenceInfo.Show(e.Keyword, e.Type);

		//private void ObjectBrowser_ObjectClicked(object sender, ObjectClickedEventArgs e)
		//	=> SelectObject(e.ObjectName, e.);

		#endregion Events

		private void OpenIncludeFile()
		{
			if (CurrentEditor is TextEditorBase editor)
			{
				DocumentLine caretLine = editor.Document.GetLineByOffset(editor.CaretOffset);
				string caretLineText = editor.Document.GetText(caretLine.Offset, caretLine.Length);

				if (LineParser.IsValidIncludeLine(caretLineText))
				{
					string pathPart = caretLineText.Split('"')[1];
					string fullFilePath = Path.Combine(ScriptRootDirectoryPath, pathPart);

					if (File.Exists(fullFilePath))
						EditorTabControl.OpenFile(fullFilePath);
					else
						DarkMessageBox.Show(this, "Couldn't find the target file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					editor.SelectionLength = 0;
				}
			}
		}

		private async void CompileTRNGScript()
		{
			EditorTabControl.SaveAll();

			try
			{
				FormCompiling.ShowCompilingMode();
				FormCompiling.Show();

				bool success = await NGCompiler.Compile(
					ScriptRootDirectoryPath, EngineDirectoryPath,
					Configs.ClassicScript.UseNewIncludeMethod);

				if (success)
				{
					string logFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "script_log.txt");
					CompilerLogs.UpdateLogs(File.ReadAllText(logFilePath));

					if (IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild)
						CompilerLogs.DockGroup.SetVisibleContent(CompilerLogs);

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
		{
			editor.UpdateSettings(Configs.ClassicScript);
		}

		protected override void ApplyUserSettings()
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
				EditorTabControl.GetEditorOfTab(tab).UpdateSettings(Configs.ClassicScript);
		}

		protected override void Build()
			=> CompileTRNGScript();
	}
}
