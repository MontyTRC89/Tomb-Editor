using DarkUI.Controls;
using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptEditor.Forms;
using TombIDE.ScriptEditor.Helpers;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Scripting.Compilers;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors;
using TombLib.Scripting.TextEditors.Controls;
using TombLib.Scripting.TextEditors.Forms;
using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombIDE.ScriptEditor
{
	public partial class ScriptEditor : UserControl
	{
		private IDE _ide;
		private TextEditorConfigs _editorConfigs = TextEditorConfigs.Load();

		private FormFindReplace _formFindReplace;
		private FormNGCompilingStatus _formCompiling;

		/// <summary>
		/// The current tab's TextEditor.
		/// </summary>
		private TextEditorBase _textEditor;

		#region Construction and public methods

		public ScriptEditor()
		{
			// Fetch mnemonic constants / plugin mnemonic constants
			ScriptKeywords.SetupConstants(DefaultPaths.GetReferencesPath(), DefaultPaths.GetInternalNGCPath());

			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			// Initialize watchers
			scriptFolderWatcher.Path = _ide.Project.ScriptPath;

			// Initialize forms
			_formFindReplace = new FormFindReplace(ref tabControl_Editor, ref tabControl_Info);
			_formCompiling = new FormNGCompilingStatus();

			// Initialize side controls
			objectBrowser.Initialize(_ide);
			fileList.Initialize(_ide);
			referenceBrowser.Initialize(_ide);

			// Check if the previous session crashed and left unhandled .backup files
			CheckPreviousSession();

			// Open the project's script file on start
			OpenScriptFile();
		}

		#endregion Construction and public methods

		#region Session

		private void CheckPreviousSession()
		{
			string[] files = Directory.GetFiles(_ide.Project.ScriptPath, "*.backup", SearchOption.AllDirectories);

			if (files.Length != 0) // If a .backup file exists
			{
				DialogResult result = DarkMessageBox.Show(this,
					"Looks like your previous Script Editor session was unsuccessfully closed.\n" +
					"Would you like to restore the unsaved files?", "Restore session?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					RestoreSession(files);
				else if (result == DialogResult.No)
					FileHelper.DeleteFiles(files); // Deletes all .backup files
			}
		}

		private void RestoreSession(string[] files)
		{
			foreach (string file in files)
			{
				string backupFileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));

				string originalFilePath = GetOriginalFilePathFromBackupFile(file);

				// Open the original file and replace the whole text of the TextEditor with the .backup file content
				OpenFile(originalFilePath);
				_textEditor.Text = backupFileContent;

				HandleTextChangedIndicator();
			}
		}

		private bool AreAllFilesSaved()
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
				if (!IsFileSaved(tab))
					return false;

			return true;
		}

		#endregion Session

		#region IDE Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			IDEEvent_HandleFileOpening(obj);
			IDEEvent_HandleObjectSelection(obj);
			IDEEvent_HandleSilentActions(obj);
			IDEEvent_HandleProgramClosing(obj);
		}

		private void IDEEvent_HandleFileOpening(IIDEEvent obj)
		{
			if (obj is IDE.ScriptEditor_OpenFileEvent)
			{
				var e = obj as IDE.ScriptEditor_OpenFileEvent;
				var fileTab = FindTabPageOfFile(e.RequestedFilePath);

				if (fileTab != null) // If the requested file is already opened
					tabControl_Editor.SelectTab(fileTab);
				else
					OpenFile(e.RequestedFilePath);
			}
		}

		private void IDEEvent_HandleObjectSelection(IIDEEvent obj)
		{
			if (obj is IDE.ScriptEditor_SelectObjectEvent)
			{
				var e = (IDE.ScriptEditor_SelectObjectEvent)obj;
				SelectObject(e.ObjectName, e.ObjectType);
			}
		}

		private void IDEEvent_HandleSilentActions(IIDEEvent obj)
		{
			if (obj is IDE.ScriptEditor_AppendScriptLinesEvent // Append Script Lines
				|| obj is IDE.ScriptEditor_AddNewLevelStringEvent // Add New Level String
				|| obj is IDE.ScriptEditor_AddNewNGStringEvent // Add New NG String
				|| obj is IDE.ScriptEditor_ScriptPresenceCheckEvent // Script Presence Check Requested
				|| obj is IDE.ScriptEditor_StringPresenceCheckEvent // String Presence Check Requested
				|| obj is IDE.ScriptEditor_RenameLevelEvent) // Rename Level
			{
				TabPage cachedTab = tabControl_Editor.SelectedTab;

				TabPage scriptFileTab = FindTabPageOfFile(PathHelper.GetScriptFilePath(_ide.Project));
				bool wasScriptFileAlreadyOpened = scriptFileTab != null;
				bool wasScriptFileFileChanged = wasScriptFileAlreadyOpened ?
					GetTextEditorOfTab(scriptFileTab).IsTextChanged : false;

				TabPage languageFileTab = FindTabPageOfFile(PathHelper.GetLanguageFilePath(_ide.Project, GameLanguage.English));
				bool wasLanguageFileAlreadyOpened = languageFileTab != null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened ?
					GetTextEditorOfTab(languageFileTab).IsTextChanged : false;

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
					bool isChanged = AddNewNGString(((IDE.ScriptEditor_AddNewNGStringEvent)obj).NGString);
					EndSilentScriptAction(cachedTab, isChanged, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_ScriptPresenceCheckEvent)
				{
					_ide.ScriptDefined = IsLevelScriptDefined(((IDE.ScriptEditor_ScriptPresenceCheckEvent)obj).LevelName);
					EndSilentScriptAction(cachedTab, false, false, !wasScriptFileAlreadyOpened);
				}
				else if (obj is IDE.ScriptEditor_StringPresenceCheckEvent)
				{
					_ide.StringDefined = IsLevelLanguageStringDefined(((IDE.ScriptEditor_StringPresenceCheckEvent)obj).LevelName);
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

		private void IDEEvent_HandleProgramClosing(IIDEEvent obj)
		{
			if (obj is IDE.ProgramClosingEvent)
				(obj as IDE.ProgramClosingEvent).CanClose = AreAllFilesSaved();
		}

		#endregion IDE Events

		#region IDE Event Methods

		private void SelectObject(string objectName, ObjectType type)
		{
			DocumentLine objectLine = FindDocumentLineOfObject(objectName, type);

			if (objectLine != null)
			{
				_textEditor.Focus();
				_textEditor.ScrollToLine(objectLine.LineNumber);
				_textEditor.SelectLine(objectLine);
			}
		}

		private DocumentLine FindDocumentLineOfObject(string objectName, ObjectType type)
		{
			foreach (DocumentLine line in _textEditor.Document.Lines)
			{
				string lineText = _textEditor.Document.GetText(line.Offset, line.Length);

				switch (type)
				{
					case ObjectType.Section:
						if (lineText.StartsWith(objectName))
							return line;
						break;

					case ObjectType.Level:
						if (Regex.Replace(lineText, @"name\s*?=", string.Empty, RegexOptions.IgnoreCase).TrimStart().StartsWith(objectName))
							return line;
						break;

					case ObjectType.Include:
						if (Regex.Replace(lineText, @"#include\s*", string.Empty, RegexOptions.IgnoreCase).TrimStart('"').StartsWith(objectName))
							return line;
						break;

					case ObjectType.Define:
						if (Regex.Replace(lineText, @"#define\s*", string.Empty, RegexOptions.IgnoreCase).StartsWith(objectName))
							return line;
						break;
				}
			}

			return null;
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			OpenScriptFile(true); // Changes the current _textEditor as well

			// Join the messages into a single string and append it into the _textEditor
			_textEditor.AppendText(string.Join(Environment.NewLine, inputLines) + Environment.NewLine);

			// Scroll to the line where changes were made (the last line)
			_textEditor.ScrollToLine(_textEditor.LineCount);

			HandleTextChangedIndicator();
		}

		private void AddNewLevelNameString(string levelName)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage, true); // Changes the current _textEditor as well

			// Scan all lines
			for (int i = 1; i < _textEditor.LineCount; i++)
			{
				DocumentLine currentLine = _textEditor.Document.GetLineByNumber(i);

				// Find a free "Level Name " string slot in the language file
				if (_textEditor.Document.GetText(currentLine.Offset, currentLine.Length).StartsWith("Level Name "))
				{
					// Select the line and replace its text with the added level name
					_textEditor.SelectionStart = currentLine.Offset;
					_textEditor.SelectionLength = currentLine.Length;

					_textEditor.SelectedText = levelName;

					break;
				}

				if (i == _textEditor.LineCount)
				{
					DarkMessageBox.Show(this,
						"Warning, you ran out of free Level Name String slots in the main Language File.\n" +
						"Your current level will not be visible in-game after compiling the script.", "Warning",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);

					return;
				}
			}

			HandleTextChangedIndicator();
		}

		private bool AddNewNGString(string ngString)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage, true); // Changes the current _textEditor as well

			_textEditor.SelectionStart = 0;
			_textEditor.SelectionLength = 0;

			// Scan all lines
			for (int i = 1; i <= _textEditor.LineCount; i++)
			{
				DocumentLine iline = _textEditor.Document.GetLineByNumber(i);
				string ilineText = _textEditor.Document.GetText(iline.Offset, iline.Length);

				if (ilineText.StartsWith("[extrang]", StringComparison.OrdinalIgnoreCase))
				{
					// Check if the string isn't already defined
					for (int j = _textEditor.LineCount; j >= i; j--)
					{
						DocumentLine jline = _textEditor.Document.GetLineByNumber(j);
						string jlineText = _textEditor.Document.GetText(jline.Offset, jline.Length);

						if (Regex.IsMatch(jlineText, @"\A\d*:\s*?" + ngString + @"(;.*)?"))
							return false;
					}

					// Add the string
					for (int j = _textEditor.LineCount; j >= i; j--)
					{
						DocumentLine jline = _textEditor.Document.GetLineByNumber(j);
						string jlineText = _textEditor.Document.GetText(jline.Offset, jline.Length);

						if (Regex.IsMatch(jlineText, @"\A\d*:.*"))
						{
							_textEditor.CaretOffset = jline.EndOffset;
							_textEditor.TextArea.PerformTextInput(Environment.NewLine);

							int prevNumber = int.Parse(Regex.Replace(jlineText, @"\A(\d*):.*", "$1"));

							_textEditor.SelectedText = prevNumber + 1 + ": " + ngString;

							_textEditor.ScrollToLine(j + 1);
							break;
						}
						else if (j == i)
						{
							_textEditor.CaretOffset = jline.EndOffset;
							_textEditor.TextArea.PerformTextInput(Environment.NewLine);

							_textEditor.SelectedText = "0: " + ngString;

							_textEditor.ScrollToLine(j);
							break;
						}
					}

					break;
				}
			}

			return true;
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			OpenScriptFile(true); // Changes the current _textEditor as well

			// Scan all lines
			for (int i = 1; i < _textEditor.LineCount; i++)
			{
				DocumentLine currentLine = _textEditor.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textEditor.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

				if (rgx.IsMatch(line))
				{
					// Get the level name without "Name = " from the line string
					string scriptLevelName = rgx.Replace(line, string.Empty).Trim();

					if (scriptLevelName == oldName)
					{
						line = line.Replace(oldName, newName);
						_textEditor.SelectionStart = currentLine.Offset;
						_textEditor.SelectionLength = currentLine.Length;
						_textEditor.SelectedText = line;

						_textEditor.ScrollToLine(i);
						break;
					}
				}
			}

			HandleTextChangedIndicator();
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage, true); // Changes the current _textEditor as well

			// Scan all lines
			for (int i = 1; i < _textEditor.LineCount; i++)
			{
				DocumentLine currentLine = _textEditor.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textEditor.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				if (line == oldName)
				{
					line = line.Replace(oldName, newName);
					_textEditor.SelectionStart = currentLine.Offset;
					_textEditor.SelectionLength = currentLine.Length;
					_textEditor.SelectedText = line;

					_textEditor.ScrollToLine(i);
					break;
				}
			}

			HandleTextChangedIndicator();
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			OpenScriptFile(true); // Changes the current _textEditor as well

			// Scan all lines
			for (int i = 1; i < _textEditor.LineCount; i++)
			{
				DocumentLine currentLine = _textEditor.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textEditor.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

				if (rgx.IsMatch(line))
				{
					// Get the level name without "Name = " from the line string
					string scriptLevelName = rgx.Replace(line, string.Empty).Trim();

					if (scriptLevelName == levelName)
						return true;
				}
			}

			return false;
		}

		private bool IsLevelLanguageStringDefined(string levelName)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage, true); // Changes the current _textEditor as well

			// Scan all lines
			for (int i = 1; i < _textEditor.LineCount; i++)
			{
				DocumentLine currentLine = _textEditor.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textEditor.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				if (line == levelName)
					return true;
			}

			return false;
		}

		private void EndSilentScriptAction(TabPage previousTab, bool indicateChange, bool saveAffectedFile, bool closeAffectedTab)
		{
			if (indicateChange)
				_ide.ScriptEditor_IndicateExternalChange();

			if (saveAffectedFile)
				SaveFile(tabControl_Editor.SelectedTab);

			if (closeAffectedTab)
				tabControl_Editor.TabPages.Remove(tabControl_Editor.SelectedTab);

			tabControl_Editor.SelectTab(previousTab);
		}

		#endregion IDE Event Methods

		#region Events

		private void File_NewFile_Click(object sender, EventArgs e) => CreateNewFile();
		private void File_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void File_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void File_Build_Click(object sender, EventArgs e) => BuildScript();

		private void Edit_Undo_Click(object sender, EventArgs e) => _textEditor.Undo();
		private void Edit_Redo_Click(object sender, EventArgs e) => _textEditor.Redo();
		private void Edit_Cut_Click(object sender, EventArgs e) => _textEditor.Cut();
		private void Edit_Copy_Click(object sender, EventArgs e) => _textEditor.Copy();
		private void Edit_Paste_Click(object sender, EventArgs e) => _textEditor.Paste();
		private void Edit_FindReplace_Click(object sender, EventArgs e) => _formFindReplace.Show(this, _textEditor.SelectedText);

		private void Edit_SelectAll_Click(object sender, EventArgs e)
		{
			_textEditor.SelectAll();
			DoStatusCounting(); // So it updates the status strip labels
		}

		private void Tools_CheckErrors_Click(object sender, EventArgs e) => ((ScriptTextEditor)_textEditor).ManuallyCheckForErrors();
		private void Tools_Reindent_Click(object sender, EventArgs e) => ((ScriptTextEditor)_textEditor).TidyDocument();
		private void Tools_Trim_Click(object sender, EventArgs e) => ((ScriptTextEditor)_textEditor).TidyDocument(true);
		private void Tools_Comment_Click(object sender, EventArgs e) => _textEditor.CommentLines();
		private void Tools_Uncomment_Click(object sender, EventArgs e) => _textEditor.UncommentLines();
		private void Tools_ToggleBookmark_Click(object sender, EventArgs e) => _textEditor.ToggleBookmark();
		private void Tools_PrevBookmark_Click(object sender, EventArgs e) => _textEditor.GoToPrevBookmark();
		private void Tools_NextBookmark_Click(object sender, EventArgs e) => _textEditor.GoToNextBookmark();
		private void Tools_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void Tools_Settings_Click(object sender, EventArgs e) => ShowSettingsForm();

		private void View_ObjBrowser_Click(object sender, EventArgs e) => ToggleObjBrowser(!menuItem_ObjBrowser.Checked);
		private void View_FileList_Click(object sender, EventArgs e) => ToggleFileList(!menuItem_FileList.Checked);
		private void View_InfoBox_Click(object sender, EventArgs e) => ToggleInfoBox(!menuItem_InfoBox.Checked);
		private void View_ToolStrip_Click(object sender, EventArgs e) => ToggleToolStrip(!menuItem_ToolStrip.Checked);
		private void View_StatusStrip_Click(object sender, EventArgs e) => ToggleStatusStrip(!menuItem_StatusStrip.Checked);
		private void View_SwapPanels_Click(object sender, EventArgs e) => SwapPanels(!menuItem_SwapPanels.Checked);

		private void Help_About_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ContextMenu_Cut_Click(object sender, EventArgs e) => _textEditor.Cut();
		private void ContextMenu_Copy_Click(object sender, EventArgs e) => _textEditor.Copy();
		private void ContextMenu_Paste_Click(object sender, EventArgs e) => _textEditor.Paste();
		private void ContextMenu_Comment_Click(object sender, EventArgs e) => _textEditor.CommentLines();
		private void ContextMenu_Uncomment_Click(object sender, EventArgs e) => _textEditor.UncommentLines();
		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => _textEditor.ToggleBookmark();

		private void ToolStrip_NewFile_Click(object sender, EventArgs e) => CreateNewFile();
		private void ToolStrip_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void ToolStrip_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void ToolStrip_Undo_Click(object sender, EventArgs e) => _textEditor.Undo();
		private void ToolStrip_Redo_Click(object sender, EventArgs e) => _textEditor.Redo();
		private void ToolStrip_Cut_Click(object sender, EventArgs e) => _textEditor.Cut();
		private void ToolStrip_Copy_Click(object sender, EventArgs e) => _textEditor.Copy();
		private void ToolStrip_Paste_Click(object sender, EventArgs e) => _textEditor.Paste();
		private void ToolStrip_Comment_Click(object sender, EventArgs e) => _textEditor.CommentLines();
		private void ToolStrip_Uncomment_Click(object sender, EventArgs e) => _textEditor.UncommentLines();
		private void ToolStrip_ToggleBookmark_Click(object sender, EventArgs e) => _textEditor.ToggleBookmark();
		private void ToolStrip_PrevBookmark_Click(object sender, EventArgs e) => _textEditor.GoToPrevBookmark();
		private void ToolStrip_NextBookmark_Click(object sender, EventArgs e) => _textEditor.GoToNextBookmark();
		private void ToolStrip_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void ToolStrip_Build_Click(object sender, EventArgs e) => BuildScript();

		private void StatusStrip_ResetZoom_Click(object sender, EventArgs e)
		{
			_textEditor.Zoom = 100;
			button_ResetZoom.Visible = false;

			DoStatusCounting();
		}

		private void Editor_TextChanged(object sender, EventArgs e)
		{
			if (!textChangedDelayTimer.Enabled)
				textChangedDelayTimer.Start();

			treeView_SearchResults.Nodes.Clear();
			treeView_SearchResults.Invalidate();
		}

		private void Editor_StatusChanged(object sender, EventArgs e) => DoStatusCounting();

		private void Editor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
			&& (e.Key == System.Windows.Input.Key.F || e.Key == System.Windows.Input.Key.H))
				_formFindReplace.Show(this, _textEditor.SelectedText);

			if (e.Key == System.Windows.Input.Key.F5)
				OpenIncludeFile();
		}

		private void TextChangedDelayTimer_Tick(object sender, EventArgs e)
		{
			objectBrowser.UpdateContent(_textEditor.Text);

			DoStatusCounting();
			UpdateUndoRedoSaveStates();
			HandleTextChangedIndicator();

			textChangedDelayTimer.Stop();
		}

		private void Caret_PositionChanged(object sender, EventArgs e)
		{
			DoStatusCounting();
			UpdateSyntaxPreview();
		}

		#endregion Events

		#region Event Methods

		private void OnSaveButtonClicked()
		{
			if (!SaveFile(tabControl_Editor.SelectedTab))
				return; // File saving failed

			HandleTextChangedIndicator();

			menuItem_Save.Enabled = false;
			button_Save.Enabled = false;

			if (IsEveryTabSaved())
			{
				menuItem_SaveAll.Enabled = false;
				button_SaveAll.Enabled = false;
			}
		}

		private void SaveAll()
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				TextEditorBase tabTextEditor = GetTextEditorOfTab(tab);

				if (tabTextEditor.Document.FileName == null)
					continue;

				SaveFile(tab);

				HandleTextChangedIndicator(tab, tabTextEditor);
				UpdateUndoRedoSaveStates();
			}
		}

		private void BuildScript()
		{
			SaveAll();

			switch (_ide.Project.GameVersion)
			{
				case TRVersion.Game.TRNG:
					CompileTRNGScript();
					break;
			}
		}

		private void HandleTextChangedIndicator(TabPage tab = null, TextEditorBase textEditor = null)
		{
			if (tab == null)
				tab = tabControl_Editor.SelectedTab;

			if (textEditor == null)
				textEditor = _textEditor;

			if (string.IsNullOrEmpty(textEditor.Document.FileName))
			{
				if (!textEditor.IsTextChanged)
					tab.Text = "Untitled";
				else if (!tab.Text.EndsWith("*"))
					tab.Text = "Untitled*";
			}
			else
			{
				if (!textEditor.IsTextChanged)
					tab.Text = tab.Text.TrimEnd('*');
				else if (!tab.Text.EndsWith("*"))
					tab.Text = GetCorrectTabTitle(textEditor.Document.FileName) + "*";
			}
		}

		private void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			if (_textEditor.CanUndo)
			{
				menuItem_Undo.Enabled = true;
				menuItem_Undo.Text = "&Undo";
				button_Undo.Enabled = true;
			}
			else
			{
				menuItem_Undo.Enabled = false;
				menuItem_Undo.Text = "Can't Undo";
				button_Undo.Enabled = false;
			}

			// Redo buttons
			if (_textEditor.CanRedo)
			{
				menuItem_Redo.Enabled = true;
				menuItem_Redo.Text = "&Redo";
				button_Redo.Enabled = true;
			}
			else
			{
				menuItem_Redo.Enabled = false;
				menuItem_Redo.Text = "Can't Redo";
				button_Redo.Enabled = false;
			}

			// Save buttons
			if (_textEditor.IsTextChanged)
			{
				menuItem_Save.Enabled = true;
				button_Save.Enabled = true;
			}
			else
			{
				menuItem_Save.Enabled = false;
				button_Save.Enabled = false;
			}

			menuItem_SaveAll.Enabled = !IsEveryTabSaved();
			button_SaveAll.Enabled = !IsEveryTabSaved();
		}

		private void DoStatusCounting()
		{
			label_LineNumber.Text = "Line: " + _textEditor.TextArea.Caret.Position.Line;
			label_ColNumber.Text = "Column: " + _textEditor.TextArea.Caret.Position.Column;

			label_SelectedChars.Text = "Selected: " + _textEditor.SelectedText.Length;

			label_Zoom.Text = "Zoom: " + _textEditor.Zoom + "%";
			button_ResetZoom.Visible = _textEditor.Zoom != 100;
		}

		private void UpdateSyntaxPreview()
		{
			syntaxPreview.CurrentArgumentIndex = ArgumentHelper.GetArgumentIndexAtOffset(_textEditor.Document, _textEditor.CaretOffset);
			syntaxPreview.Text = CommandHelper.GetCommandSyntax(_textEditor.Document, _textEditor.CaretOffset);
		}

		private void ToggleObjBrowser(bool state)
		{
			menuItem_ObjBrowser.Checked = state;
			splitter_Left.Visible = state;
			objectBrowser.Visible = state;
			_ide.IDEConfiguration.View_ShowObjBrowser = state;
		}

		private void ToggleFileList(bool state)
		{
			menuItem_FileList.Checked = state;
			splitter_Right.Visible = state;
			fileList.Visible = state;
			_ide.IDEConfiguration.View_ShowFileList = state;
		}

		private void ToggleInfoBox(bool state)
		{
			menuItem_InfoBox.Checked = state;
			splitter_Bottom.Visible = state;
			sectionPanel_InfoBox.Visible = state;
			_ide.IDEConfiguration.View_ShowInfoBox = state;
		}

		private void ToggleToolStrip(bool state)
		{
			menuItem_ToolStrip.Checked = state;
			toolStrip.Visible = state;
			_ide.IDEConfiguration.View_ShowToolStrip = state;
		}

		private void ToggleStatusStrip(bool state)
		{
			menuItem_StatusStrip.Checked = state;
			statusStrip.Visible = state;
			_ide.IDEConfiguration.View_ShowStatusStrip = state;

			panel_Syntax.Visible = state;
		}

		private void SwapPanels(bool state)
		{
			menuItem_SwapPanels.Checked = state;

			if (menuItem_SwapPanels.Checked)
			{
				fileList.Dock = DockStyle.Bottom;
				fileList.Height = 200;

				sectionPanel_InfoBox.Dock = DockStyle.Right;
				sectionPanel_InfoBox.Width = 384;

				splitter_Right.MinSize = 384;

				Controls.SetChildIndex(splitter_Right, 5);
				Controls.SetChildIndex(splitter_Bottom, 3);
			}
			else
			{
				fileList.Dock = DockStyle.Right;
				fileList.Width = 200;

				sectionPanel_InfoBox.Dock = DockStyle.Bottom;
				sectionPanel_InfoBox.Height = 200;

				splitter_Right.MinSize = 200;

				Controls.SetChildIndex(splitter_Bottom, 5);
				Controls.SetChildIndex(splitter_Right, 3);
			}

			_ide.IDEConfiguration.View_SwapPanels = state;
		}

		private bool IsEveryTabSaved()
		{
			// The difference between this and the AreAllFilesSaved() method is that this one just returns true/false
			// and doesn't prompt the user to save the changes

			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				if (tab.Text.TrimEnd('*') == "Untitled")
					continue;

				TextEditorBase tabTextEditor = GetTextEditorOfTab(tab);

				if (tabTextEditor.IsTextChanged)
					return false;
			}

			return true;
		}

		#endregion Event Methods

		#region Compilers

		private async void CompileTRNGScript()
		{
			if (!AreLibrariesRegistered())
				return;

			try
			{
				_formCompiling.ShowCompilingMode();
				_formCompiling.Show();

				if (await NGCompiler.Compile(
					_ide.Project.ScriptPath,
					_ide.Project.EnginePath,
					DefaultPaths.GetInternalNGCPath(),
					DefaultPaths.GetVGEPath()))
				{
					// Read the logs
					string logFilePath = Path.Combine(DefaultPaths.GetVGEScriptPath(), "script_log.txt");

					// Read and show the logs in the "Compiler Logs" richTextBox
					richTextBox_Logs.Text = File.ReadAllText(logFilePath);

					// Select the "Compiler Logs" tab
					tabControl_Info.SelectTab(1);
					tabControl_Info.Invalidate();

					if (_formCompiling.Visible)
						_formCompiling.Close();
				}
				else
				{
					_formCompiling.ShowDebugMode();

					if (!backgroundWorker_NGC.IsBusy)
						backgroundWorker_NGC.RunWorkerAsync();
				}
			}
			catch (Exception ex)
			{
				if (_formCompiling.Visible)
					_formCompiling.Close();

				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion Compilers

		#region Tabs

		private void CreateNewTabPage(string tabPageTitle)
		{
			// Create the tab
			TabPage newTabPage = new TabPage(tabPageTitle)
			{
				UseVisualStyleBackColor = false,
				BackColor = Color.FromArgb(32, 32, 32),
				Size = tabControl_Editor.Size
			};

			// Create the TextEditor
			ScriptTextEditor newTextEditor = new ScriptTextEditor
			{
				ShowLineNumbers = _editorConfigs.ClassicScript.ShowLineNumbers,
				ShowSectionSeparators = _editorConfigs.ClassicScript.ShowSectionSeparators
			};

			newTextEditor.TextArea.Margin = new System.Windows.Thickness(6, 0, 0, 0);

			// Bind event methods to the TextEditor
			newTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
			newTextEditor.TextArea.SelectionChanged += Editor_StatusChanged;
			newTextEditor.TextChanged += Editor_TextChanged;
			newTextEditor.ZoomChanged += Editor_StatusChanged;
			newTextEditor.KeyDown += Editor_KeyDown;
			newTextEditor.MouseDoubleClick += Editor_MouseDoubleClick;

			ElementHost elementHost = new ElementHost
			{
				Size = new Size(newTabPage.Size.Width - 6, newTabPage.Size.Height),
				Dock = DockStyle.Fill,
				Child = newTextEditor,
				ContextMenuStrip = contextMenu_TextEditor
			};

			// Add the host to the tab
			newTabPage.Controls.Add(elementHost);

			// Add the tab to the tabControl and select it
			tabControl_Editor.TabPages.Add(newTabPage);
			tabControl_Editor.SelectTab(newTabPage);

			// Change the active TextEditor and apply user settings to it
			_textEditor = newTextEditor;

			ApplySavedSettings();
		}

		private TabPage FindTabPageOfFile(string filePath)
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				if (tab.Text.TrimEnd('*') == GetCorrectTabTitle(filePath))
					return tab;
			}

			return null;
		}

		private void Editor_TabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			// Check if the last tab page was closed, if so, then create a new "Untitled" tab page
			if (tabControl_Editor.TabCount == 0)
				CreateNewTabPage("Untitled");
		}

		private void Editor_TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl_Editor.TabCount > 0)
			{
				// Change the active TextEditor
				_textEditor = GetTextEditorOfTab(tabControl_Editor.SelectedTab);

				// Update everything
				UpdateUndoRedoSaveStates();
				objectBrowser.UpdateContent(_textEditor.Text);
				DoStatusCounting();
			}
		}

		private void Editor_TabControl_MouseClick(object sender, MouseEventArgs e)
		{
			/* Middle mouse click tab closing */
			if (e.Button == MouseButtons.Middle)
			{
				int i = 0;

				// Check which tab page was middle-clicked
				foreach (TabPage tab in tabControl_Editor.TabPages)
				{
					if (tabControl_Editor.GetTabRect(i).Contains(e.Location))
					{
						// Select the tab
						tabControl_Editor.SelectTab(tab);

						// Check if the tab's content is saved
						if (!IsFileSaved(tab))
							return;

						// Close the tab
						tab.Dispose();
						break;
					}

					i++;
				}
			}
		}

		private void Editor_TabControl_TabClosing(object sender, TabControlCancelEventArgs e)
		{
			// Check if the tab's content is saved
			if (!IsFileSaved(e.TabPage))
				e.Cancel = true;
		}

		#endregion Tabs

		#region Files

		private void OpenScriptFile(bool silentAction = false)
		{
			try
			{
				string scriptFilePath = PathHelper.GetScriptFilePath(_ide.Project);
				TabPage scriptFileTab = FindTabPageOfFile(scriptFilePath);

				if (scriptFileTab != null)
					tabControl_Editor.SelectTab(scriptFileTab);
				else
					OpenFile(scriptFilePath, silentAction);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenLanguageFile(GameLanguage language, bool silentAction = false)
		{
			try
			{
				string languageFilePath = PathHelper.GetLanguageFilePath(_ide.Project, language);
				TabPage languageFileTab = FindTabPageOfFile(languageFilePath);

				if (languageFileTab != null)
					tabControl_Editor.SelectTab(languageFileTab);
				else
					OpenFile(languageFilePath, silentAction);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFile(string filePath, bool silentAction = false)
		{
			CreateNewTabPage(GetCorrectTabTitle(filePath)); // Changes the current _textEditor
			_textEditor.OpenFile(filePath, silentAction);

			menuItem_Save.Enabled = false;
			button_Save.Enabled = false;

			objectBrowser.UpdateContent(_textEditor.Text);
		}

		private bool IsFileSaved(TabPage tab)
		{
			TextEditorBase textEditor = GetTextEditorOfTab(tab);

			if (textEditor.IsTextChanged)
			{
				if (_ide.SelectedIDETab != IDETab.ScriptEditor)
					_ide.SelectIDETab(IDETab.ScriptEditor);

				tabControl_Editor.SelectTab(tab);

				string fileName = string.IsNullOrEmpty(_textEditor.Document.FileName) ? "Untitled" : Path.GetFileName(_textEditor.Document.FileName);

				DialogResult result = DarkMessageBox.Show(this,
					"Do you want save changes to " + fileName + " ?", "Unsaved changes!",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					return SaveFile(tab);
				else if (result == DialogResult.Cancel)
					return false;
			}

			tab.Dispose();
			return true;
		}

		private bool SaveFile(TabPage tab)
		{
			TextEditorBase textEditor = GetTextEditorOfTab(tab);

			if (_ide.IDEConfiguration.Tidy_ReindentOnSave)
				((ScriptTextEditor)textEditor).TidyDocument();

			try
			{
				if (textEditor.Document.FileName == null) // For "Untitled"
				{
					string initialNodePath = null;
					string initialFileName = null;

					if (!tab.Text.TrimEnd('*').Equals("untitled", StringComparison.OrdinalIgnoreCase))
					{
						initialFileName = tab.Text.TrimEnd('*').Split('.')[0];

						if (initialFileName.Contains('\\'))
						{
							string partialPath = initialFileName.Split('\\').Last();

							initialNodePath = "Script\\" + initialFileName.Remove(initialFileName.Length - partialPath.Length - 1, partialPath.Length + 1);
							initialFileName = partialPath;
						}
					}

					using (FormFileCreation form = new FormFileCreation(_ide, FileCreationMode.Saving, initialNodePath, initialFileName))
					{
						if (form.ShowDialog(this) == DialogResult.OK)
						{
							textEditor.Document.FileName = form.NewFilePath;
							tab.Text = form.NewFilePath.Replace(_ide.Project.ScriptPath + "\\", "");
						}
						else
							return false;
					}
				}

				textEditor.Save(textEditor.Document.FileName);
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
					return SaveFile(tab); // Retry saving

				return false;
			}

			return true; // When saving was successful
		}

		#endregion Files

		#region Forms

		private void ShowAboutForm()
		{
			using (FormAbout form = new FormAbout(Properties.Resources.AboutScreen_800))
				form.ShowDialog(this);
		}

		private void ShowSettingsForm()
		{
			using (FormTextEditorSettings form = new FormTextEditorSettings())
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					_editorConfigs = TextEditorConfigs.Load();
					syntaxPreview.ReloadSettings();
					ApplySavedSettings();
				}
		}

		#endregion Forms

		private void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to clear all bookmarks from the current document?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				foreach (DocumentLine line in _textEditor.Document.Lines)
				{
					if (line.IsBookmarked)
						line.IsBookmarked = false;
				}

				_textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
			}
		}

		public string GetCorrectTabTitle(string filePath)
		{
			return filePath.Replace(_ide.Project.ScriptPath + @"\", "");
		}

		public string GetOriginalFilePathFromBackupFile(string backupFilePath)
		{
			return Path.Combine(Path.GetDirectoryName(backupFilePath), Path.GetFileNameWithoutExtension(backupFilePath));
		}

		private void treeView_SearchResults_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			foreach (DarkTreeNode node in treeView_SearchResults.Nodes)
			{
				if (treeView_SearchResults.SelectedNodes[0] == node)
					return;
			}

			string sourceFilePath = Path.Combine(
				_ide.Project.ScriptPath, treeView_SearchResults.SelectedNodes[0].ParentNode.Text.Split('(')[0].Trim());

			Match match = (Match)treeView_SearchResults.SelectedNodes[0].Tag;

			TabPage tab = FindTabPageOfFile(sourceFilePath);

			if (tab != null)
			{
				tabControl_Editor.SelectTab(tab);

				try
				{
					DocumentLine line = _textEditor.Document.GetLineByOffset(match.Index);

					_textEditor.SelectionStart = match.Index;
					_textEditor.SelectionLength = match.Length;

					_textEditor.ScrollToLine(line.LineNumber);
				}
				catch { }
			}
		}

		private void backgroundWorker_NGC_DoWork(object sender, DoWorkEventArgs e)
		{
			foreach (Process process in Process.GetProcesses())
			{
				if (process.ProcessName.Contains("NG_Center"))
				{
					process.WaitForExit();
					break;
				}
			}
		}

		private void backgroundWorker_NGC_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_formCompiling.Visible)
				_formCompiling.Close();
		}

		private void CreateNewFile()
		{
			using (FormFileCreation form = new FormFileCreation(_ide, FileCreationMode.New))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					FileStream stream = File.Create(form.NewFilePath);
					stream.Close();

					OpenFile(form.NewFilePath);
				}
			}
		}

		private void scriptFolderWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			fileList.UpdateFileList();

			List<TabPage> tabsToClose = new List<TabPage>();

			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(tab);

				if (string.IsNullOrEmpty(textEditor.Document.FileName))
					continue;

				if (!File.Exists(textEditor.Document.FileName))
				{
					if (textEditor.IsTextChanged)
					{
						textEditor.Document.FileName = null;
						HandleTextChangedIndicator();
					}
					else
						tabsToClose.Add(tab);
				}
			}

			foreach (TabPage tab in tabsToClose)
				tabControl_Editor.TabPages.Remove(tab);
		}

		private void scriptFolderWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			fileList.UpdateFileList();

			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(tab);

				if (textEditor.Document.FileName == e.OldFullPath)
				{
					textEditor.Document.FileName = e.FullPath;
					tab.Text = GetCorrectTabTitle(e.FullPath);

					HandleTextChangedIndicator();

					break;
				}
			}
		}

		private void ApplySavedSettings()
		{
			// TextEditor specific settings
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(tab);

				if (textEditor is ScriptTextEditor)
					UpdateTextEditorSettings((ScriptTextEditor)textEditor);
				else if (textEditor is LuaTextEditor)
					UpdateTextEditorSettings((LuaTextEditor)textEditor);
			}

			// Editor settings
			ToggleObjBrowser(_ide.IDEConfiguration.View_ShowObjBrowser);
			ToggleFileList(_ide.IDEConfiguration.View_ShowFileList);
			ToggleInfoBox(_ide.IDEConfiguration.View_ShowInfoBox);
			ToggleToolStrip(_ide.IDEConfiguration.View_ShowToolStrip);
			ToggleStatusStrip(_ide.IDEConfiguration.View_ShowStatusStrip);
			SwapPanels(_ide.IDEConfiguration.View_SwapPanels);

			DoStatusCounting();
		}

		private void UpdateTextEditorSettings(ScriptTextEditor scriptTextEditor)
		{
			scriptTextEditor.SyntaxHighlighting = new ClassicScriptSyntaxHighlighting(_editorConfigs.ClassicScript.ColorScheme);

			scriptTextEditor.Background = new System.Windows.Media.SolidColorBrush
			(
				(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString
				(
					_editorConfigs.ClassicScript.ColorScheme.Background
				)
			);

			scriptTextEditor.Foreground = new System.Windows.Media.SolidColorBrush
			(
				(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString
				(
					_editorConfigs.ClassicScript.ColorScheme.Foreground
				)
			);

			scriptTextEditor.ShowSectionSeparators = _editorConfigs.ClassicScript.ShowSectionSeparators;

			scriptTextEditor.Cleaner.PreEqualSpace = _editorConfigs.ClassicScript.Tidy_PreEqualSpace;
			scriptTextEditor.Cleaner.PostEqualSpace = _editorConfigs.ClassicScript.Tidy_PostEqualSpace;
			scriptTextEditor.Cleaner.PreCommaSpace = _editorConfigs.ClassicScript.Tidy_PreCommaSpace;
			scriptTextEditor.Cleaner.PostCommaSpace = _editorConfigs.ClassicScript.Tidy_PostCommaSpace;
			scriptTextEditor.Cleaner.ReduceSpaces = _editorConfigs.ClassicScript.Tidy_ReduceSpaces;

			scriptTextEditor.UpdateSettings(_editorConfigs.ClassicScript);
		}

		private void UpdateTextEditorSettings(LuaTextEditor luaTextEditor)
		{
			luaTextEditor.SyntaxHighlighting = new LuaSyntaxHighlighting(_editorConfigs.Lua.ColorScheme);
			luaTextEditor.UpdateSettings(_editorConfigs.Lua);
		}

		private TextEditorBase GetTextEditorOfTab(TabPage tab)
		{
			return (TextEditorBase)tab.Controls.OfType<ElementHost>().First().Child;
		}

		private void Editor_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == System.Windows.Input.MouseButton.Left && ModifierKeys == Keys.Control)
				OpenIncludeFile();
		}

		private void OpenIncludeFile()
		{
			DocumentLine caretLine = _textEditor.Document.GetLineByOffset(_textEditor.CaretOffset);
			string caretLineText = _textEditor.Document.GetText(caretLine.Offset, caretLine.Length);

			if (caretLineText.StartsWith("#include", StringComparison.OrdinalIgnoreCase) && caretLineText.Contains('"'))
			{
				string pathPart = caretLineText.Split('"')[1];
				string fullFilePath = Path.Combine(_ide.Project.ScriptPath, pathPart);

				if (File.Exists(fullFilePath))
				{
					TabPage fileTab = FindTabPageOfFile(fullFilePath);

					if (fileTab != null)
						tabControl_Editor.SelectTab(fileTab);
					else
						OpenFile(fullFilePath);
				}
				else
					DarkMessageBox.Show(this, "Couldn't find the target file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				_textEditor.SelectionLength = 0;
			}
		}

		public static bool AreLibrariesRegistered()
		{
			string MSCOMCTL = Path.Combine(DefaultPaths.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(DefaultPaths.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(DefaultPaths.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(DefaultPaths.GetSystemDirectory(), "Comdlg32.ocx");

			if (!File.Exists(MSCOMCTL) || !File.Exists(RICHTX32) || !File.Exists(PICFORMAT32) || !File.Exists(COMDLG32))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(DefaultPaths.GetProgramDirectory(), "TombIDE Library Registration.exe")
				};

				try
				{
					Process process = Process.Start(startInfo);
					process.WaitForExit();
				}
				catch { return false; }
			}

			return true;
		}
	}
}
