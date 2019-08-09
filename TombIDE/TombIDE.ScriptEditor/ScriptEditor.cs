using DarkUI.Controls;
using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.Scripting;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Script;

namespace TombIDE.ScriptEditor
{
	public partial class ScriptEditor : UserControl
	{
		private IDE _ide;

		/// <summary>
		/// The currently used ScriptTextBox
		/// </summary>
		private ScriptTextBox _textBox;

		#region Initialization

		public ScriptEditor()
		{
			CommandManager.MaxHistoryLength = 512; // TODO: Add this as a setting in FormSettings

			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			// Check if the previous session crashed and left .backup files in the project's /Script/ folder
			CheckPreviousSession();

			// Open the project's script file on start
			OpenScriptFile();

			ApplyUserSettings();

			// Run the scriptFolderWatcher and update the File List
			scriptFolderWatcher.Path = _ide.Project.ScriptPath;
			UpdateFileList();
		}

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.LevelAddedEvent)
			{
				List<string> scriptMessages = ((IDE.LevelAddedEvent)obj).ScriptMessages;

				// Check if the message is not empty
				if (scriptMessages.Count > 0)
				{
					AppendMessagesToScriptFile(scriptMessages);
					AddLevelNameToLanguageFile(scriptMessages);

					// Show the main Script file after everything is done
					OpenScriptFile(); // Changes the current _textBox as well

					// Scroll to the line where changes were made (the last line)
					_textBox.Navigate(_textBox.Lines.Count - 1);
				}
			}
			else if (obj is IDE.LevelRenameFormOpened)
			{
				string levelName = ((IDE.LevelRenameFormOpened)obj).LevelName;

				TabPage cachedTab = tabControl_Editor.SelectedTab;

				bool wasScriptAlreadyOpened = IsFileAlreadyOpened(GetScriptFilePath());
				bool wasLanguageFileAlreadyOpened = IsFileAlreadyOpened(GetLanguageFilePath("english"));

				bool levelDefinedInScript = false;
				bool levelDefinedInLanguageFile = false;

				OpenScriptFile(); // Changes the current _textBox as well

				// Scan all lines
				foreach (string line in _textBox.Lines)
				{
					Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

					if (rgx.IsMatch(line))
					{
						// Get the level name without "Name = " from the line string
						string scriptLevelName = rgx.Replace(line, string.Empty).Trim();

						if (scriptLevelName == levelName)
							levelDefinedInScript = true;
					}
				}

				OpenLanguageFile("english"); // Changes the current _textBox as well

				// Scan all lines
				foreach (string line in _textBox.Lines)
				{
					if (line == levelName)
						levelDefinedInLanguageFile = true;
				}

				_ide.LevelDefined = levelDefinedInScript && levelDefinedInLanguageFile;

				if (!wasScriptAlreadyOpened)
				{
					OpenScriptFile();
					tabControl_Editor.TabPages.Remove(tabControl_Editor.SelectedTab);
				}

				if (!wasLanguageFileAlreadyOpened)
				{
					OpenLanguageFile("english");
					tabControl_Editor.TabPages.Remove(tabControl_Editor.SelectedTab);
				}

				tabControl_Editor.SelectTab(cachedTab);
			}
			else if (obj is IDE.AskedForScriptEntryRename)
			{
				string oldName = ((IDE.AskedForScriptEntryRename)obj).PreviousName;
				string newName = ((IDE.AskedForScriptEntryRename)obj).CurrentName;

				OpenScriptFile(); // Changes the current _textBox as well

				int affectedScriptFileLine = 0;

				// Scan all lines
				for (int i = 0; i < _textBox.LinesCount; i++)
				{
					Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

					string line = _textBox.GetLineText(i);

					if (rgx.IsMatch(line))
					{
						// Get the level name without "Name = " from the line string
						string scriptLevelName = rgx.Replace(line, string.Empty).Trim();

						if (scriptLevelName == oldName)
						{
							line = line.Replace(oldName, newName);
							_textBox.Selection = new Range(_textBox, 0, i, _textBox.GetLineText(i).Length, i);
							_textBox.SelectedText = line;

							_textBox.Navigate(i);
							break;
						}
					}
				}

				HandleTextChangedIndicator();

				OpenLanguageFile("english"); // Changes the current _textBox as well

				// Scan all lines
				for (int i = 0; i < _textBox.LinesCount; i++)
				{
					string line = _textBox.GetLineText(i);

					if (line == oldName)
					{
						line = line.Replace(oldName, newName);
						_textBox.Selection = new Range(_textBox, 0, i, _textBox.GetLineText(i).Length, i);
						_textBox.SelectedText = line;

						_textBox.Navigate(i);
						break;
					}
				}

				HandleTextChangedIndicator();

				OpenScriptFile();
			}
			else if (obj is IDE.ProgramClosingEvent)
			{
				if (AreAllFilesSaved())
				{
					_ide.ClosingCancelled = false;
					_textBox.Dispose();
				}
				else
					_ide.ClosingCancelled = true; // Only happens when the user clicked "Cancel" or when saving failed somehow
			}
		}

		private void CheckPreviousSession()
		{
			string[] files = Directory.GetFiles(_ide.Project.ScriptPath, "*.backup", SearchOption.AllDirectories);

			if (files.Length != 0) // If a .backup file exists in the project's /Script/ folder
			{
				DialogResult result = DarkMessageBox.Show(this,
					"Looks like your previous Script Editor session was unsuccessfully closed.\n" +
					"Would you like to restore the unsaved files?", "Restore session?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					RestorePreviousSession(files);
				else if (result == DialogResult.No)
				{
					// Delete all .backup files
					foreach (string file in files)
					{
						if (File.Exists(file))
							File.Delete(file);
					}
				}
			}
		}

		private void RestorePreviousSession(string[] files)
		{
			foreach (string file in files)
			{
				string backupFileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));

				// Get the original file path by trimming ".backup" at the end of the backup file path
				string originalFilePath = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));

				// Open the original file and replace the whole text of the ScriptTextBox with the .backup file content
				OpenFile(originalFilePath);
				_textBox.Text = backupFileContent;

				HandleTextChangedIndicator();
			}
		}

		private void ApplyUserSettings()
		{
			// ScriptTextBox specific settings
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				tab.Controls.OfType<ScriptTextBox>().First().Font = new Font(_ide.Configuration.FontFamily, _ide.Configuration.FontSize);
				tab.Controls.OfType<ScriptTextBox>().First().AutoCompleteBrackets = _ide.Configuration.AutoCloseBrackets;
				tab.Controls.OfType<ScriptTextBox>().First().WordWrap = _ide.Configuration.WordWrap;
				tab.Controls.OfType<ScriptTextBox>().First().ShowLineNumbers = _ide.Configuration.View_ShowLineNumbers;
				tab.Controls.OfType<ScriptTextBox>().First().ShowToolTips = _ide.Configuration.View_ShowToolTips;
			}

			// Editor settings
			ToggleObjBrowser(_ide.Configuration.View_ShowObjBrowser);
			ToggleFileList(_ide.Configuration.View_ShowFileList);
			ToggleInfoBox(_ide.Configuration.View_ShowInfoBox);
			ToggleToolStrip(_ide.Configuration.View_ShowToolStrip);
			ToggleStatusStrip(_ide.Configuration.View_ShowStatusStrip);
			ToggleLineNumbers(_ide.Configuration.View_ShowLineNumbers);
			ToggleToolTips(_ide.Configuration.View_ShowToolTips);
		}

		private bool AreAllFilesSaved()
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				if (!IsFileSaved(tab))
					return false;
			}

			return true;
		}

		#endregion Initialization

		#region Events

		private void File_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void File_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void File_Build_Click(object sender, EventArgs e) => BuildScript();

		private void Edit_Undo_Click(object sender, EventArgs e) => _textBox.Undo();
		private void Edit_Redo_Click(object sender, EventArgs e) => _textBox.Redo();
		private void Edit_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void Edit_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void Edit_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void Edit_Find_Click(object sender, EventArgs e) => _textBox.ShowFindDialog();
		private void Edit_Replace_Click(object sender, EventArgs e) => _textBox.ShowReplaceDialog();

		private void Edit_SelectAll_Click(object sender, EventArgs e)
		{
			_textBox.SelectAll();
			DoStatusCounting(); // So it updates the status strip labels
		}

		private void Tools_Reindent_Click(object sender, EventArgs e) => _textBox.TidyDocument();
		private void Tools_Trim_Click(object sender, EventArgs e) => _textBox.TidyDocument(true);
		private void Tools_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void Tools_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void Tools_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();
		private void Tools_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GotoPrevBookmark(_textBox.Selection.Start.iLine);
		private void Tools_NextBookmark_Click(object sender, EventArgs e) => _textBox.GotoNextBookmark(_textBox.Selection.Start.iLine);
		private void Tools_ClearBookmarks_Click(object sender, EventArgs e) => _textBox.ClearAllBookmarks();
		private void Tools_Settings_Click(object sender, EventArgs e) => ShowSettingsForm();

		private void View_ObjBrowser_Click(object sender, EventArgs e) => ToggleObjBrowser(!menuItem_ObjBrowser.Checked);
		private void View_FileList_Click(object sender, EventArgs e) => ToggleFileList(!menuItem_FileList.Checked);
		private void View_InfoBox_Click(object sender, EventArgs e) => ToggleInfoBox(!menuItem_InfoBox.Checked);
		private void View_ToolStrip_Click(object sender, EventArgs e) => ToggleToolStrip(!menuItem_ToolStrip.Checked);
		private void View_StatusStrip_Click(object sender, EventArgs e) => ToggleStatusStrip(!menuItem_StatusStrip.Checked);
		private void View_LineNumbers_Click(object sender, EventArgs e) => ToggleLineNumbers(!menuItem_LineNumbers.Checked);
		private void View_ToolTips_Click(object sender, EventArgs e) => ToggleToolTips(!menuItem_ToolTips.Checked);

		private void Help_About_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ContextMenu_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void ContextMenu_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void ContextMenu_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void ContextMenu_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void ContextMenu_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();

		private void ToolStrip_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void ToolStrip_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void ToolStrip_Undo_Click(object sender, EventArgs e) => _textBox.Undo();
		private void ToolStrip_Redo_Click(object sender, EventArgs e) => _textBox.Redo();
		private void ToolStrip_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void ToolStrip_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void ToolStrip_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void ToolStrip_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void ToolStrip_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void ToolStrip_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();
		private void ToolStrip_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GotoPrevBookmark(_textBox.Selection.Start.iLine);
		private void ToolStrip_NextBookmark_Click(object sender, EventArgs e) => _textBox.GotoNextBookmark(_textBox.Selection.Start.iLine);
		private void ToolStrip_ClearBookmarks_Click(object sender, EventArgs e) => _textBox.ClearAllBookmarks();
		private void ToolStrip_Build_Click(object sender, EventArgs e) => BuildScript();

		private void StatusStrip_ResetZoom_Click(object sender, EventArgs e)
		{
			_textBox.Zoom = 100;
			button_ResetZoom.Visible = false;
		}

		private void inputTimer_Tick(object sender, EventArgs e)
		{
			// The inputTimer is used to update the Object Browser when the user hasn't typed anything for 3 seconds
			// This boosts performance
			inputTimer.Stop();
			UpdateObjectBrowser();
		}

		private void Editor_TextChangedDelayed(object sender, TextChangedEventArgs e)
		{
			HandleTextChangedIndicator();

			// Reset the timer
			inputTimer.Stop();
			inputTimer.Start();
		}

		private void Editor_UndoRedoState_Changed(object sender, EventArgs e) => UpdateUndoRedoSaveStates();
		private void Editor_ZoomChanged(object sender, EventArgs e) => DoStatusCounting();
		private void Editor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void Editor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();

		#endregion Events

		#region Event Methods

		private void AppendMessagesToScriptFile(List<string> scriptMessages)
		{
			OpenScriptFile(); // Changes the current _textBox as well

			// Join the messages into a single string and append it into the _textBox
			_textBox.AppendText(string.Join(Environment.NewLine, scriptMessages) + "\n");

			HandleTextChangedIndicator();
		}

		private void AddLevelNameToLanguageFile(List<string> scriptMessages)
		{
			OpenLanguageFile("english"); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 0; i < _textBox.LinesCount; i++)
			{
				// Find a free "Level Name String" slot in the language file
				if (_textBox.GetLineText(i).ToLower().StartsWith("level name "))
				{
					// Select the line and replace its text with the added level name
					_textBox.Selection = new Range(_textBox, 0, i, _textBox.GetLineText(i).Length, i);

					string levelName = scriptMessages[1].Replace("Name= ", string.Empty);
					_textBox.SelectedText = levelName;
					break;
				}

				if (i == _textBox.LinesCount - 1)
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

		private void OnSaveButtonClicked()
		{
			if (!SaveFile(_textBox))
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
				ScriptTextBox tabTextBox = tab.Controls.OfType<ScriptTextBox>().First();
				SaveFile(tabTextBox);

				HandleTextChangedIndicator(tab, tabTextBox);
				UpdateUndoRedoSaveStates();
			}
		}

		private void BuildScript()
		{
			switch (_ide.Project.GameVersion)
			{
				case GameVersion.TR4:
					CompileTR4Script();
					break;

				case GameVersion.TRNG:
					CompileTRNGScript();
					break;
			}
		}

		private void AdjustOldFormatting() // Because the compilers really don't like having a space before "="
		{
			string vgeScriptFileContent = File.ReadAllText(@"NGC\VGE\Script\Script.txt");

			while (vgeScriptFileContent.Contains(" ="))
				vgeScriptFileContent = vgeScriptFileContent.Replace(" =", "=");

			File.WriteAllText(@"NGC\VGE\Script\Script.txt", vgeScriptFileContent, Encoding.GetEncoding(1252));
		}

		private void HandleTextChangedIndicator(TabPage tab = null, ScriptTextBox textBox = null)
		{
			if (tab == null)
				tab = tabControl_Editor.SelectedTab;

			if (textBox == null)
				textBox = _textBox;

			if (string.IsNullOrEmpty(textBox.FilePath))
			{
				if (!textBox.IsChanged)
					tab.Text = "Untitled";
				else if (!tab.Text.EndsWith("*"))
					tab.Text = "Untitled*";
			}
			else
			{
				if (!textBox.IsChanged)
					tab.Text = tab.Text.TrimEnd('*');
				else if (!tab.Text.EndsWith("*"))
					tab.Text = Path.GetFileName(textBox.FilePath) + "*";
			}
		}

		private void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			if (_textBox.UndoEnabled)
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
			if (_textBox.RedoEnabled)
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
			if (_textBox.IsContentChanged())
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
			label_LineNumber.Text = "Line: " + (_textBox.Selection.Start.iLine + 1);
			label_ColNumber.Text = "Column: " + (_textBox.Selection.Start.iChar + 1);

			if (_textBox.Selection.Start.iChar < 0) // FCTB sucks
				_textBox.Selection.Start = new Place(0, _textBox.Selection.Start.iLine);

			label_SelectedChars.Text = "Selected: " + _textBox.SelectedText.Length;

			label_Zoom.Text = "Zoom: " + _textBox.Zoom + "%";
			button_ResetZoom.Visible = _textBox.Zoom != 100;
		}

		private void ToggleObjBrowser(bool state)
		{
			menuItem_ObjBrowser.Checked = state;
			splitter_Left.Visible = state;
			sectionPanel_ObjBrowser.Visible = state;
			_ide.Configuration.View_ShowObjBrowser = state;
		}

		private void ToggleFileList(bool state)
		{
			menuItem_FileList.Checked = state;
			splitter_Right.Visible = state;
			sectionPanel_Files.Visible = state;
			_ide.Configuration.View_ShowFileList = state;
		}

		private void ToggleInfoBox(bool state)
		{
			menuItem_InfoBox.Checked = state;
			splitter_Bottom.Visible = state;
			sectionPanel_InfoBox.Visible = state;
			_ide.Configuration.View_ShowInfoBox = state;
		}

		private void ToggleToolStrip(bool state)
		{
			menuItem_ToolStrip.Checked = state;
			toolStrip.Visible = state;
			_ide.Configuration.View_ShowToolStrip = state;
		}

		private void ToggleStatusStrip(bool state)
		{
			menuItem_StatusStrip.Checked = state;
			statusStrip.Visible = state;
			_ide.Configuration.View_ShowStatusStrip = state;
		}

		private void ToggleLineNumbers(bool state)
		{
			menuItem_LineNumbers.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				tab.Controls.OfType<ScriptTextBox>().First().ShowLineNumbers = state;

			_ide.Configuration.View_ShowLineNumbers = state;
		}

		private void ToggleToolTips(bool state)
		{
			menuItem_ToolTips.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				tab.Controls.OfType<ScriptTextBox>().First().ShowToolTips = state;

			_ide.Configuration.View_ShowToolTips = state;
		}

		private bool IsEveryTabSaved()
		{
			// The difference between this and the AreAllFilesSaved() method is that this one just returns true/false
			// and doesn't prompt the user to save the changes

			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				ScriptTextBox tabTextBox = tab.Controls.OfType<ScriptTextBox>().First();

				if (tabTextBox.IsChanged)
					return false;
			}

			return true;
		}

		#endregion Event Methods

		#region Compilers

		private void CompileTR4Script()
		{
			IScriptCompiler compiler = new ScriptCompilerNew(GameVersion.TR4);

			if (compiler.CompileScripts(_ide.Project.ScriptPath, _ide.Project.ProjectPath))
				DarkMessageBox.Show(this, "Script compiled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			else
				DarkMessageBox.Show(this, "Error while compiling the script.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void CompileTRNGScript()
		{
			if (!AreLibrariesRegistered())
				return;

			try
			{
				// Delete the old /Script/ directory in the VGE if it exists
				if (Directory.Exists(@"NGC\VGE\Script"))
					Directory.Delete(@"NGC\VGE\Script", true);

				// Recreate the directory
				Directory.CreateDirectory(@"NGC\VGE\Script");

				string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

				// Create all of the subdirectories
				foreach (string dirPath in Directory.GetDirectories(_ide.Project.ScriptPath, "*", SearchOption.AllDirectories))
					Directory.CreateDirectory(dirPath.Replace(_ide.Project.ScriptPath, currentDir + @"\NGC\VGE\Script"));

				// Copy all the files into the VGE /Script/ folder
				foreach (string newPath in Directory.GetFiles(_ide.Project.ScriptPath, "*.*", SearchOption.AllDirectories))
					File.Copy(newPath, newPath.Replace(_ide.Project.ScriptPath, currentDir + @"\NGC\VGE\Script"));

				AdjustOldFormatting();

				// Run NG_Center.exe
				var application = TestStack.White.Application.Launch(@"NGC\NG_Center.exe");

				// Do some actions in NG Center
				RunScriptedNGCenterEvents(application);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void RunScriptedNGCenterEvents(TestStack.White.Application application)
		{
			try
			{
				// Get a list of all windows belonging to the app
				var windowList = application.GetWindows();

				// Check if the list has the main NG Center window (it starts with "NG Center 1.5...")
				var ngWindow = windowList.Find(x => x.Title.Contains("NG Center 1.5"));

				if (ngWindow == null)
				{
					// If not, then try again because we're most probably seeing the "Loading" window
					RunScriptedNGCenterEvents(application);
					return;
				}

				Point cachedCursorPosition = new Point();

				// Refresh the window list and check if a Updater message box appeared
				windowList = application.GetWindows();
				var ngUpdaterWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

				if (ngUpdaterWindow != null)
					ngUpdaterWindow.KeyIn(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ESCAPE);

				// Find the "Build" button
				var buildButton = ngWindow.Get<TestStack.White.UIItems.Button>("Build");

				// Click the button
				cachedCursorPosition = Cursor.Position;
				buildButton.Click();
				Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

				// Refresh the window list and check if an error message box appeared
				windowList = application.GetWindows();
				var ngErrorWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

				if (ngErrorWindow != null)
				{
					// Stop the program here and wait for the error message box to close
					while (!ngErrorWindow.IsClosed)
					{ continue; }
				}

				// Find the "Show Log" button
				var logButton = ngWindow.Get<TestStack.White.UIItems.Button>("Show Log");

				// Click the button
				cachedCursorPosition = Cursor.Position;
				logButton.Click();
				Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

				// Read the logs
				string logFilePath = @"NGC\VGE\Script\script_log.txt";
				string logFileContent = File.ReadAllText(logFilePath);

				// Replace the VGE paths in the log file with the current project ones
				string vgePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\NGC\VGE";
				File.WriteAllText(logFilePath, logFileContent.Replace(vgePath, _ide.Project.ProjectPath), Encoding.GetEncoding(1252));

				application.Close(); // Done!

				// Copy the compiled files from the Virtual Game Engine folder to the current project folder
				string compiledScriptFilePath = @"NGC\VGE\Script.dat";
				string compiledEnglishFilePath = @"NGC\VGE\English.dat";

				if (File.Exists(compiledScriptFilePath))
					File.Copy(compiledScriptFilePath, Path.Combine(_ide.Project.ProjectPath, "Script.dat"), true);

				if (File.Exists(compiledEnglishFilePath))
					File.Copy(compiledEnglishFilePath, Path.Combine(_ide.Project.ProjectPath, "English.dat"), true);

				// Read and show the logs in the "Compiler Logs" richTextBox
				richTextBox_Logs.Text = File.ReadAllText(logFilePath);

				// Select the "Compiler Logs" tab
				tabControl_Info.SelectTab(1);
				tabControl_Info.Invalidate();
			}
			catch (ElementNotAvailableException)
			{
				// The "Loading" window just closed, so try again
				RunScriptedNGCenterEvents(application);
			}
		}

		private bool AreLibrariesRegistered()
		{
			string MSCOMCTL = Path.Combine(SharedMethods.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(SharedMethods.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Comdlg32.ocx");

			if (!File.Exists(MSCOMCTL) || !File.Exists(RICHTX32) || !File.Exists(PICFORMAT32) || !File.Exists(COMDLG32))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = "TombIDE Library Registration.exe"
				};

				try
				{
					Process process = Process.Start(startInfo);
					process.WaitForExit();
				}
				catch
				{
					return false;
				}
			}

			return true;
		}

		#endregion Compilers

		#region ObjectBrowser

		private void UpdateObjectBrowser()
		{
			// Only allow the Object Browser to work for actual script files (might add support for more files later)
			if (tabControl_Editor.SelectedTab.Text.TrimEnd('*') != Path.GetFileName(GetScriptFilePath()))
			{
				treeView_Objects.Nodes.Clear();
				treeView_Objects.Invalidate();
				return;
			}

			string filter = string.Empty;

			if (!string.IsNullOrWhiteSpace(textBox_SearchObj.Text) && textBox_SearchObj.Text != "Search...")
				filter = textBox_SearchObj.Text.Trim();

			treeView_Objects.Nodes.Clear();

			// Add the default nodes
			treeView_Objects.Nodes.Add(new DarkTreeNode("Sections"));
			treeView_Objects.Nodes.Add(new DarkTreeNode("Levels"));

			// Add all subnodes
			foreach (string line in _textBox.Lines)
			{
				AddSectionNode(line, filter);
				AddLevelNode(line, filter);
			}

			// Expand the default nodes
			treeView_Objects.Nodes[0].Expanded = true;
			treeView_Objects.Nodes[1].Expanded = true;

			treeView_Objects.Invalidate();
		}

		private void AddSectionNode(string line, string filter)
		{
			foreach (string section in KeyWords.Sections)
			{
				string sectionName = "[" + section + "]";

				// Exclude [Level] sections
				if (sectionName == "[Level]")
					continue;

				// Check if the current line starts a section
				if (line.StartsWith(sectionName))
				{
					// Add the node if the section name matches the filter (it always does if there's nothing in the search bar)
					if (sectionName.ToLower().Contains(filter.ToLower()))
						treeView_Objects.Nodes[0].Nodes.Add(new DarkTreeNode(sectionName));
				}
			}
		}

		private void AddLevelNode(string line, string filter)
		{
			Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

			if (rgx.IsMatch(line))
			{
				// Get the level name without "Name = " from the line string
				string levelName = rgx.Replace(line, string.Empty).Trim();

				// Add the node if the level name matches the filter (it always does if there's nothing in the search bar)
				if (levelName.ToLower().Contains(filter.ToLower()))
					treeView_Objects.Nodes[1].Nodes.Add(new DarkTreeNode(levelName));
			}
		}

		private void ObjBrowser_TreeView_Click(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView_Objects.SelectedNodes.Count == 0 || string.IsNullOrWhiteSpace(treeView_Objects.SelectedNodes[0].Text))
				return;

			// If the selected node is a default item ("Sections" or "Levels")
			if (treeView_Objects.SelectedNodes[0] == treeView_Objects.Nodes[0] || treeView_Objects.SelectedNodes[0] == treeView_Objects.Nodes[1])
				return;

			// Scan all lines
			for (int i = 0; i < _textBox.LinesCount; i++)
			{
				// Find the line that contains the node text
				if (_textBox.GetLineText(i).ToLower().Contains(treeView_Objects.SelectedNodes[0].Text.ToLower()))
				{
					_textBox.Focus();

					// Scroll to the line position
					_textBox.Navigate(i);

					// Select the line
					_textBox.Selection = new Range(_textBox, 0, i, _textBox.GetLineText(i).Length, i);
					return;
				}
			}
		}

		private void ObjBrowser_Search_GotFocus(object sender, EventArgs e)
		{
			if (textBox_SearchObj.Text == "Search...")
				textBox_SearchObj.Text = string.Empty;
		}

		private void ObjBrowser_Search_LostFocus(object sender, EventArgs e)
		{
			if (textBox_SearchObj.Text == string.Empty)
				textBox_SearchObj.Text = "Search...";
		}

		private void ObjBrowser_Search_TextChanged(object sender, EventArgs e) => UpdateObjectBrowser();

		#endregion ObjectBrowser

		#region FileList

		private void UpdateFileList()
		{
			treeView_Files.Nodes.Clear();

			Stack<DarkTreeNode> stack = new Stack<DarkTreeNode>();
			DirectoryInfo scriptDirectory = new DirectoryInfo(_ide.Project.ScriptPath);

			DarkTreeNode node = new DarkTreeNode("Script Folder")
			{
				Icon = Properties.Resources.folder.ToBitmap(),
				Tag = scriptDirectory
			};

			stack.Push(node);

			while (stack.Count > 0)
			{
				DarkTreeNode currentNode = stack.Pop();
				DirectoryInfo info = (DirectoryInfo)currentNode.Tag;

				foreach (DirectoryInfo directory in info.GetDirectories())
				{
					DarkTreeNode childDirectoryNode = new DarkTreeNode(directory.Name)
					{
						Icon = Properties.Resources.folder.ToBitmap(),
						Tag = directory
					};

					currentNode.Nodes.Add(childDirectoryNode);
					stack.Push(childDirectoryNode);
				}

				foreach (FileInfo file in info.GetFiles())
				{
					if (file.Name.ToLower().EndsWith(".txt") || file.Name.ToLower().EndsWith(".lua"))
					{
						if (file.Name.ToLower() == "script.txt" || file.Name.ToLower() == "english.txt")
							continue;

						DarkTreeNode fileNode = new DarkTreeNode(file.Name)
						{
							Icon = Properties.Resources.file.ToBitmap(),
							Tag = file.FullName
						};

						currentNode.Nodes.Add(fileNode);
					}
				}
			}

			node.Expanded = true;
			treeView_Files.Nodes.Add(node);
		}

		private void FolderWatcher_Changed(object sender, FileSystemEventArgs e) => UpdateFileList();
		private void FolderWatcher_Renamed(object sender, RenamedEventArgs e) => UpdateFileList();

		private void FileList_EditScript_Click(object sender, EventArgs e) => OpenScriptFile();
		private void FileList_EditStrings_Click(object sender, EventArgs e) => OpenLanguageFile("english");

		private void FileList_OpenInExplorer_Click(object sender, EventArgs e) =>
			SharedMethods.OpenFolderInExplorer(_ide.Project.ScriptPath);

		private void FileList_TreeView_DoubleClick(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView_Files.SelectedNodes.Count == 0 || string.IsNullOrWhiteSpace(treeView_Files.SelectedNodes[0].Text))
				return;

			// If the selected node is not a .txt or .lua file
			if (!treeView_Files.SelectedNodes[0].Text.ToLower().EndsWith(".txt") && !treeView_Files.SelectedNodes[0].Text.ToLower().EndsWith(".lua"))
				return;

			string clickedFilePath = treeView_Files.SelectedNodes[0].Tag.ToString();

			if (IsFileAlreadyOpened(clickedFilePath))
				return;

			OpenFile(clickedFilePath);
		}

		private void FileList_Splitter_Moved(object sender, SplitterEventArgs e) // 𝓡𝓮𝓼𝓹𝓸𝓷𝓼𝓲𝓿𝓮𝓷𝓮𝓼𝓼
		{
			button_EditScript.Width = (sectionPanel_Files.Width / 2) - 5;
			button_EditLanguages.Location = new Point((sectionPanel_Files.Width / 2), button_EditLanguages.Location.Y);

			if (sectionPanel_Files.Width % 2 > 0)
				button_EditLanguages.Width = (sectionPanel_Files.Width / 2) - 5;
			else
				button_EditLanguages.Width = (sectionPanel_Files.Width / 2) - 6;
		}

		#endregion FileList

		#region Tabs

		private void CreateNewTabPage(string tabPageTitle)
		{
			// Create the tab
			TabPage newTabPage = new TabPage(tabPageTitle)
			{
				UseVisualStyleBackColor = false,
				BackColor = Color.FromArgb(60, 63, 65),
				Size = tabControl_Editor.Size
			};

			// Create the ScriptTextBox
			ScriptTextBox newTextBox = new ScriptTextBox
			{
				ShowLineNumbers = _ide.Configuration.View_ShowLineNumbers,
				ContextMenuStrip = contextMenu_TextBox
			};

			// Bind event methods to the ScriptTextBox
			newTextBox.KeyPress += Editor_KeyPress;
			newTextBox.SelectionChanged += Editor_SelectionChanged;
			newTextBox.TextChangedDelayed += Editor_TextChangedDelayed;
			newTextBox.UndoRedoStateChanged += Editor_UndoRedoState_Changed;
			newTextBox.ZoomChanged += Editor_ZoomChanged;

			// Add the ScriptTextBox to the tab
			newTabPage.Controls.Add(newTextBox);

			// Add the tab to the tabControl and select it
			tabControl_Editor.TabPages.Add(newTabPage);
			tabControl_Editor.SelectTab(newTabPage);

			// Change the active ScriptTextBox and apply user settings to it
			_textBox = newTextBox;
			ApplyUserSettings();
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
				// Change the active ScriptTextBox
				_textBox = tabControl_Editor.SelectedTab.Controls.OfType<ScriptTextBox>().First();

				// Update everything
				UpdateUndoRedoSaveStates();
				UpdateObjectBrowser();
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

		private void OpenScriptFile()
		{
			try
			{
				string scriptFilePath = GetScriptFilePath();

				if (IsFileAlreadyOpened(scriptFilePath))
					return;

				OpenFile(scriptFilePath);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Valid arguments: "english", "spanish", "german" etc.
		/// </summary>
		private void OpenLanguageFile(string language)
		{
			try
			{
				string languageFilePath = GetLanguageFilePath(language);

				if (IsFileAlreadyOpened(languageFilePath))
					return;

				OpenFile(languageFilePath);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFile(string filePath)
		{
			CreateNewTabPage(Path.GetFileName(filePath)); // Changes the current _textBox
			_textBox.OpenFile(filePath);

			menuItem_Save.Enabled = false;
			button_Save.Enabled = false;

			UpdateObjectBrowser();
		}

		private bool IsFileSaved(TabPage tab)
		{
			ScriptTextBox textBox = tab.Controls.OfType<ScriptTextBox>().First();

			if (textBox.IsChanged)
			{
				if (_ide.SelectedIDETab != "Script Editor")
					_ide.SelectIDETab("Script Editor");

				tabControl_Editor.SelectTab(tab);

				string fileName = string.IsNullOrEmpty(_textBox.FilePath) ? "Untitled" : Path.GetFileName(_textBox.FilePath);

				DialogResult result = DarkMessageBox.Show(this,
					"Do you want save changes to " + fileName + " ?", "Unsaved changes!",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					return SaveFile(textBox);
				else if (result == DialogResult.Cancel)
					return false;
			}

			if (_textBox.FilePath != string.Empty)
			{
				string backupFilePath = _textBox.FilePath + ".backup";

				if (File.Exists(backupFilePath))
					File.Delete(backupFilePath); // We don't need the backup file when the original file is saved
			}

			tab.Dispose();
			return true; // When the file is saved
		}

		private bool SaveFile(ScriptTextBox textBox)
		{
			if (_ide.Configuration.Tidy_ReindentOnSave)
				textBox.TidyDocument();

			try
			{
				if (_textBox.FilePath == null) // For "Untitled"
				{
					SaveFileDialog dialog = new SaveFileDialog()
					{
						Filter = "Text Files|*.txt|LUA Files|*.lua|All Files|*.*",
						InitialDirectory = _ide.Project.ScriptPath
					};

					if (dialog.ShowDialog(this) == DialogResult.OK)
						textBox.FilePath = dialog.FileName;
					else
						return false;
				}

				textBox.SaveCurrentFile();
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
					return SaveFile(textBox); // Retry saving

				return false;
			}

			return true; // When saving was successful
		}

		private string GetScriptFilePath()
		{
			string scriptFilePath = string.Empty;

			// Find the script file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(_ide.Project.ScriptPath))
			{
				if (Path.GetFileName(file).ToLower() == "script.txt")
				{
					scriptFilePath = file;
					break;
				}
			}

			if (string.IsNullOrEmpty(scriptFilePath))
				throw new FileNotFoundException("Couldn't find the SCRIPT.TXT file.");

			return scriptFilePath;
		}

		private string GetLanguageFilePath(string language)
		{
			string languageFilePath = string.Empty;

			// Find the language file in the project's /Script/ folder
			foreach (string file in Directory.GetFiles(_ide.Project.ScriptPath))
			{
				if (Path.GetFileName(file).ToLower() == language.ToLower() + ".txt")
				{
					languageFilePath = file;
					break;
				}
			}

			if (string.IsNullOrEmpty(languageFilePath))
				throw new FileNotFoundException("Couldn't find the " + language.ToUpper() + ".TXT file.");

			return languageFilePath;
		}

		private bool IsFileAlreadyOpened(string filePath)
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				if (tab.Text.TrimEnd('*') == Path.GetFileName(filePath))
				{
					// Select the tab
					tabControl_Editor.SelectTab(tabControl_Editor.TabPages.IndexOf(tab));
					return true;
				}
			}

			return false;
		}

		#endregion Files

		#region Forms

		private void ShowAboutForm()
		{
			using (FormAbout form = new FormAbout(null))
				form.ShowDialog(this);
		}

		private void ShowSettingsForm()
		{
			_ide.Configuration.Save();

			// Cache critical settings
			bool autocompleteCache = _ide.Configuration.Autocomplete;

			using (FormSettings form = new FormSettings(_ide))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ApplyUserSettings();

					if (form.RestartItemCount > 0)
					{
						if (!AreAllFilesSaved())
						{
							// Saving failed or the user clicked "Cancel"
							// Therefore restore the previous critical settings
							_ide.Configuration.Autocomplete = autocompleteCache;
							_ide.Configuration.Save();

							ShowSettingsForm();
							return; // DO NOT CLOSE THE APP !!! (ﾉ°Д°)ﾉ︵﻿ ┻━┻
						}

						Application.Restart();
					}
				}
			}
		}

		#endregion Forms
	}
}
