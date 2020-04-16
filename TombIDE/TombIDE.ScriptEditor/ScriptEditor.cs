using DarkUI.Controls;
using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptEditor.Controls;
using TombIDE.ScriptEditor.Forms;
using TombIDE.ScriptEditor.Resources;
using TombIDE.ScriptEditor.Resources.Syntaxes;
using TombIDE.Shared;
using TombIDE.Shared.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombIDE.ScriptEditor
{
	public partial class ScriptEditor : UserControl
	{
		private IDE _ide;

		private FormFindReplace _formFindReplace;
		private FormCompiling _formCompiling;
		private FormDebugMode _formDebugMode;

		/// <summary>
		/// The currently used AvalonTextBox
		/// </summary>
		private AvalonTextBox _textBox;

		#region Initialization

		public ScriptEditor()
		{
			// Fetch mnemonic constants / plugin mnemonic constants
			KeyWords.SetupConstants();

			InitializeComponent();

			// Initialize the Find & Replace form
			_formFindReplace = new FormFindReplace(tabControl_Editor, tabControl_Info);
			_formCompiling = new FormCompiling();
			_formDebugMode = new FormDebugMode();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
			_ide.IDEEventRaised += OnIDEEventRaised;

			// Check if the previous session crashed and left .backup files
			CheckPreviousSession();

			// Open the project's script file on start
			OpenScriptFile();

			ApplySavedSettings();

			// Initialize side controls
			objectBrowser.Initialize(_ide);
			fileList.Initialize(_ide);
		}

		private void ApplySavedSettings()
		{
			// TextBox specific settings
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				AvalonTextBox textBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;

				System.Windows.Media.Color foregroundColor =
					(System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_ide.Configuration.ScriptColors_Value);

				textBox.Foreground = new System.Windows.Media.SolidColorBrush(foregroundColor);
				textBox.FontFamily = new System.Windows.Media.FontFamily(_ide.Configuration.FontFamily);
				textBox.FontSize = _ide.Configuration.FontSize;
				textBox.DefaultFontSize = _ide.Configuration.FontSize;
				textBox.LiveErrorChecking = _ide.Configuration.LiveErrorDetection;
				textBox.AutoCloseBrackets = _ide.Configuration.AutoCloseBrackets;
				textBox.AutoCloseQuotes = _ide.Configuration.AutoCloseQuotes;
				textBox.WordWrap = _ide.Configuration.WordWrapEnabled;
				textBox.Document.UndoStack.SizeLimit = _ide.Configuration.UndoStackSize;
			}

			// Editor settings
			ToggleObjBrowser(_ide.Configuration.View_ShowObjBrowser);
			ToggleFileList(_ide.Configuration.View_ShowFileList);
			ToggleInfoBox(_ide.Configuration.View_ShowInfoBox);
			ToggleToolStrip(_ide.Configuration.View_ShowToolStrip);
			ToggleStatusStrip(_ide.Configuration.View_ShowStatusStrip);

			ToggleLineNumbers(_ide.Configuration.View_ShowLineNumbers);
			ToggleToolTips(_ide.Configuration.View_ShowToolTips);
			ToggleVisualSpaces(_ide.Configuration.View_ShowVisualSpaces);
			ToggleVisualTabs(_ide.Configuration.View_ShowVisualTabs);

			SwapPanels(_ide.Configuration.View_SwapPanels);

			DoStatusCounting();
		}

		#endregion Initialization

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

				string originalFilePath = GetOriginalFileFromBackupFile(file);

				// Open the original file and replace the whole text of the TextBox with the .backup file content
				OpenFile(originalFilePath);
				_textBox.Text = backupFileContent;

				HandleTextChangedIndicator();
			}
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

		#endregion Session

		#region IDE Events

		private void OnIDEEventRaised(IIDEEvent obj)
		{
			// // // // // // // //
			// Open File

			if (obj is IDE.ScriptEditor_OpenFileEvent)
			{
				string requestedFilePath = ((IDE.ScriptEditor_OpenFileEvent)obj).RequestedFilePath;

				TabPage fileTab = FindTabPageOfFile(requestedFilePath);

				if (fileTab != null) // If the requested file isn't opened already
					tabControl_Editor.SelectTab(fileTab);
				else
					OpenFile(requestedFilePath);
			}

			// // // // // // // //
			// Select Object

			if (obj is IDE.ScriptEditor_SelectObjectEvent)
				SelectObject(((IDE.ScriptEditor_SelectObjectEvent)obj).ObjectName);

			// // // // // // // //
			// Silent Actions

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
					((AvalonTextBox)scriptFileTab.Controls.OfType<ElementHost>().First().Child).IsTextChanged : false;

				TabPage languageFileTab = FindTabPageOfFile(PathHelper.GetLanguageFilePath(_ide.Project, Language.English));
				bool wasLanguageFileAlreadyOpened = languageFileTab != null;
				bool wasLanguageFileFileChanged = wasLanguageFileAlreadyOpened ?
					((AvalonTextBox)languageFileTab.Controls.OfType<ElementHost>().First().Child).IsTextChanged : false;

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
					AddNewNGString(((IDE.ScriptEditor_AddNewNGStringEvent)obj).NGString);
					EndSilentScriptAction(cachedTab, true, !wasLanguageFileFileChanged, !wasLanguageFileAlreadyOpened);
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

			// // // // // // // //
			// Program Closing

			if (obj is IDE.ProgramClosingEvent)
				((IDE.ProgramClosingEvent)obj).CanClose = AreAllFilesSaved();
		}

		#endregion IDE Events

		#region IDE Event Methods

		private void SelectObject(string objectName)
		{
			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);
				string lineText = _textBox.Document.GetText(currentLine.Offset, currentLine.Length);

				bool startsWithSection = lineText.Trim().StartsWith(objectName);
				bool startsWithLevel = Regex.Replace(lineText, @"name\s*?=", string.Empty, RegexOptions.IgnoreCase).Trim().StartsWith(objectName);
				bool startsWithInclude = Regex.Replace(lineText, @"#include\s", string.Empty, RegexOptions.IgnoreCase).Trim().Trim('"').StartsWith(objectName);
				bool startsWithDefine = Regex.Replace(lineText, @"#define\s", string.Empty, RegexOptions.IgnoreCase).Trim().StartsWith(objectName);

				// Find the line that contains the node text
				if (startsWithSection || startsWithLevel || startsWithInclude || startsWithDefine)
				{
					_textBox.Focus();

					// Scroll to the line position
					_textBox.ScrollToLine(i);

					// Select the line
					_textBox.SelectionStart = currentLine.Offset;
					_textBox.SelectionLength = currentLine.Length;

					return;
				}
			}
		}

		private void AppendScriptLines(List<string> inputLines)
		{
			OpenScriptFile(); // Changes the current _textBox as well

			// Join the messages into a single string and append it into the _textBox
			_textBox.AppendText(string.Join(Environment.NewLine, inputLines) + "\n");

			// Scroll to the line where changes were made (the last line)
			_textBox.ScrollToLine(_textBox.LineCount);

			HandleTextChangedIndicator();
		}

		private void AddNewLevelNameString(string levelName)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);

				// Find a free "Level Name " string slot in the language file
				if (_textBox.Document.GetText(currentLine.Offset, currentLine.Length).StartsWith("Level Name "))
				{
					// Select the line and replace its text with the added level name
					_textBox.SelectionStart = currentLine.Offset;
					_textBox.SelectionLength = currentLine.Length;

					_textBox.SelectedText = levelName;

					break;
				}

				if (i == _textBox.LineCount)
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

		private void AddNewNGString(string ngString)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage); // Changes the current _textBox as well

			_textBox.SelectionStart = 0;
			_textBox.SelectionLength = 0;

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine iline = _textBox.Document.GetLineByNumber(i);
				string ilineText = _textBox.Document.GetText(iline.Offset, iline.Length);

				if (ilineText.ToLower().StartsWith("[extrang]"))
				{
					// Check if the string isn't already defined
					for (int j = _textBox.LineCount - 1; j >= i; j--)
					{
						DocumentLine jline = _textBox.Document.GetLineByNumber(j);
						string jlineText = _textBox.Document.GetText(jline.Offset, jline.Length);

						if (Regex.IsMatch(jlineText, @"\A\d*:\s*?" + ngString + @"(;.*)?"))
							return;
					}

					// Add the string
					for (int j = _textBox.LineCount - 1; j >= i; j--)
					{
						DocumentLine jline = _textBox.Document.GetLineByNumber(j);
						string jlineText = _textBox.Document.GetText(jline.Offset, jline.Length);

						if (Regex.IsMatch(jlineText, @"\A\d*:.*"))
						{
							_textBox.CaretOffset = jline.EndOffset;
							_textBox.TextArea.PerformTextInput("\n");

							int prevNumber = int.Parse(Regex.Replace(jlineText, @"\A(\d*):.*", "$1"));

							_textBox.CaretOffset += 1;
							_textBox.SelectedText = prevNumber + 1 + ": " + ngString;

							_textBox.ScrollToLine(j + 1);
							break;
						}
						else if (j == i)
						{
							_textBox.CaretOffset = jline.EndOffset;
							_textBox.TextArea.PerformTextInput("\n");

							_textBox.CaretOffset += 1;
							_textBox.SelectedText = "0: " + ngString;

							_textBox.ScrollToLine(j);
							break;
						}
					}

					break;
				}
			}
		}

		private void RenameRequestedLevelScript(string oldName, string newName)
		{
			OpenScriptFile(); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textBox.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				Regex rgx = new Regex(@"\bName\s?=\s?"); // Regex rule to find lines that start with "Name = "

				if (rgx.IsMatch(line))
				{
					// Get the level name without "Name = " from the line string
					string scriptLevelName = rgx.Replace(line, string.Empty).Trim();

					if (scriptLevelName == oldName)
					{
						line = line.Replace(oldName, newName);
						_textBox.SelectionStart = currentLine.Offset;
						_textBox.SelectionLength = currentLine.Length;
						_textBox.SelectedText = line;

						_textBox.ScrollToLine(i);
						break;
					}
				}
			}

			HandleTextChangedIndicator();
		}

		private void RenameRequestedLanguageString(string oldName, string newName)
		{
			OpenLanguageFile(_ide.Project.DefaultLanguage); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textBox.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

				if (line == oldName)
				{
					line = line.Replace(oldName, newName);
					_textBox.SelectionStart = currentLine.Offset;
					_textBox.SelectionLength = currentLine.Length;
					_textBox.SelectedText = line;

					_textBox.ScrollToLine(i);
					break;
				}
			}

			HandleTextChangedIndicator();
		}

		private bool IsLevelScriptDefined(string levelName)
		{
			OpenScriptFile(); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textBox.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

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
			OpenLanguageFile(_ide.Project.DefaultLanguage); // Changes the current _textBox as well

			// Scan all lines
			for (int i = 1; i < _textBox.LineCount; i++)
			{
				DocumentLine currentLine = _textBox.Document.GetLineByNumber(i);
				string line = Regex.Replace(_textBox.Document.GetText(currentLine.Offset, currentLine.Length), ";.*$", string.Empty).Trim(); // Removed comments

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

		private void File_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void File_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void File_Build_Click(object sender, EventArgs e) => BuildScript();

		private void Edit_Undo_Click(object sender, EventArgs e) => _textBox.Undo();
		private void Edit_Redo_Click(object sender, EventArgs e) => _textBox.Redo();
		private void Edit_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void Edit_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void Edit_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void Edit_FindReplace_Click(object sender, EventArgs e) => _formFindReplace.Show(this, _textBox.SelectedText);

		private void Edit_SelectAll_Click(object sender, EventArgs e)
		{
			_textBox.SelectAll();
			DoStatusCounting(); // So it updates the status strip labels
		}

		private void Tools_CheckErrors_Click(object sender, EventArgs e) => _textBox.CheckForErrors();
		private void Tools_Reindent_Click(object sender, EventArgs e) => _textBox.TidyDocument();
		private void Tools_Trim_Click(object sender, EventArgs e) => _textBox.TidyDocument(true);
		private void Tools_Comment_Click(object sender, EventArgs e) => _textBox.CommentLines();
		private void Tools_Uncomment_Click(object sender, EventArgs e) => _textBox.UncommentLines();
		private void Tools_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();
		private void Tools_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GoToPrevBookmark();
		private void Tools_NextBookmark_Click(object sender, EventArgs e) => _textBox.GoToNextBookmark();
		private void Tools_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void Tools_Settings_Click(object sender, EventArgs e) => ShowSettingsForm();

		private void View_ObjBrowser_Click(object sender, EventArgs e) => ToggleObjBrowser(!menuItem_ObjBrowser.Checked);
		private void View_FileList_Click(object sender, EventArgs e) => ToggleFileList(!menuItem_FileList.Checked);
		private void View_InfoBox_Click(object sender, EventArgs e) => ToggleInfoBox(!menuItem_InfoBox.Checked);
		private void View_ToolStrip_Click(object sender, EventArgs e) => ToggleToolStrip(!menuItem_ToolStrip.Checked);
		private void View_StatusStrip_Click(object sender, EventArgs e) => ToggleStatusStrip(!menuItem_StatusStrip.Checked);
		private void View_LineNumbers_Click(object sender, EventArgs e) => ToggleLineNumbers(!menuItem_LineNumbers.Checked);
		private void View_ToolTips_Click(object sender, EventArgs e) => ToggleToolTips(!menuItem_ToolTips.Checked);
		private void View_VisualSpaces_Click(object sender, EventArgs e) => ToggleVisualSpaces(!menuItem_VisualSpaces.Checked);
		private void View_VisualTabs_Click(object sender, EventArgs e) => ToggleVisualTabs(!menuItem_VisualTabs.Checked);
		private void View_SwapPanels_Click(object sender, EventArgs e) => SwapPanels(!menuItem_SwapPanels.Checked);

		private void Help_About_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ContextMenu_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void ContextMenu_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void ContextMenu_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void ContextMenu_Comment_Click(object sender, EventArgs e) => _textBox.CommentLines();
		private void ContextMenu_Uncomment_Click(object sender, EventArgs e) => _textBox.UncommentLines();
		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();

		private void ToolStrip_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void ToolStrip_SaveAll_Click(object sender, EventArgs e) => SaveAll();
		private void ToolStrip_Undo_Click(object sender, EventArgs e) => _textBox.Undo();
		private void ToolStrip_Redo_Click(object sender, EventArgs e) => _textBox.Redo();
		private void ToolStrip_Cut_Click(object sender, EventArgs e) => _textBox.Cut();
		private void ToolStrip_Copy_Click(object sender, EventArgs e) => _textBox.Copy();
		private void ToolStrip_Paste_Click(object sender, EventArgs e) => _textBox.Paste();
		private void ToolStrip_Comment_Click(object sender, EventArgs e) => _textBox.CommentLines();
		private void ToolStrip_Uncomment_Click(object sender, EventArgs e) => _textBox.UncommentLines();
		private void ToolStrip_ToggleBookmark_Click(object sender, EventArgs e) => _textBox.ToggleBookmark();
		private void ToolStrip_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GoToPrevBookmark();
		private void ToolStrip_NextBookmark_Click(object sender, EventArgs e) => _textBox.GoToNextBookmark();
		private void ToolStrip_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void ToolStrip_Build_Click(object sender, EventArgs e) => BuildScript();

		private void StatusStrip_ResetZoom_Click(object sender, EventArgs e)
		{
			_textBox.Zoom = 100;
			button_ResetZoom.Visible = false;

			DoStatusCounting();
		}

		private void Editor_TextChanged(object sender, EventArgs e)
		{
			if (!textChangedDelayTimer.Enabled)
				textChangedDelayTimer.Start();

			DoStatusCounting();
			UpdateUndoRedoSaveStates();

			treeView_SearchResults.Nodes.Clear();
			treeView_SearchResults.Invalidate();
		}

		private void Editor_StatusChanged(object sender, EventArgs e) => DoStatusCounting();

		private void Editor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
			&& (e.Key == System.Windows.Input.Key.F || e.Key == System.Windows.Input.Key.H))
				_formFindReplace.Show(this, _textBox.SelectedText);
		}

		private void TextChangedDelayTimer_Tick(object sender, EventArgs e)
		{
			objectBrowser.UpdateContent(_textBox.Text);
			HandleTextChangedIndicator();
			textChangedDelayTimer.Stop();
		}

		private void Caret_PositionChanged(object sender, EventArgs e)
		{
			DoStatusCounting();

			DocumentLine currentLine = _textBox.Document.GetLineByOffset(_textBox.CaretOffset);
			string lineText = _textBox.Document.GetText(currentLine.Offset, currentLine.Length);

			string command = string.Empty;

			if (lineText.Contains("="))
				command = lineText.Split('=')[0].Trim();
			else if (lineText.Trim().StartsWith("#"))
				command = lineText.Split(' ')[0].Trim();

			if (command.ToLower() == "level")
				command = CommandVariations.GetCorrectLevelCommand(_textBox.Document, currentLine.LineNumber);
			else if (command.ToLower() == "cut")
				command = CommandVariations.GetCorrectCutCommand(_textBox.Document, currentLine.LineNumber);
			else if (command.ToLower() == "fmv")
				command = CommandVariations.GetCorrectFMVCommand(_textBox.Document, currentLine.LineNumber);

			if (command == null)
				return;

			string content = string.Empty;

			if (Regex.IsMatch(lineText, @"Customize\s*?=.*?,", RegexOptions.IgnoreCase))
			{
				string custKey = lineText.Split('=')[1].Split(',')[0].Trim();

				// Get resources from CustSyntaxes.resx
				ResourceManager custSyntaxResource = new ResourceManager(typeof(CustSyntaxes));
				ResourceSet custResourceSet = custSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				foreach (DictionaryEntry entry in custResourceSet)
				{
					if (custKey.ToLower() == entry.Key.ToString().ToLower())
					{
						content = entry.Value.ToString();
						break;
					}
				}

				if (string.IsNullOrEmpty(content))
				{
					for (int i = 0; i < KeyWords.PluginMnemonics.Length; i++)
					{
						PluginMnemonic pluginMnemonic = KeyWords.PluginMnemonics[i];

						if (pluginMnemonic.Flag.ToLower() == custKey.ToLower())
						{
							content = Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Trim();
							break;
						}
					}
				}
			}
			else if (Regex.IsMatch(lineText, @"Parameters\s*?=.*?,", RegexOptions.IgnoreCase))
			{
				string paramKey = lineText.Split('=')[1].Split(',')[0].Trim();

				// Get resources from ParamSyntaxes.resx
				ResourceManager custSyntaxResource = new ResourceManager(typeof(ParamSyntaxes));
				ResourceSet custResourceSet = custSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				foreach (DictionaryEntry entry in custResourceSet)
				{
					if (paramKey.ToLower() == entry.Key.ToString().ToLower())
					{
						content = entry.Value.ToString();
						break;
					}
				}

				if (string.IsNullOrEmpty(content))
				{
					for (int i = 0; i < KeyWords.PluginMnemonics.Length; i++)
					{
						PluginMnemonic pluginMnemonic = KeyWords.PluginMnemonics[i];

						if (pluginMnemonic.Flag.ToLower() == paramKey.ToLower())
						{
							content = Regex.Split(pluginMnemonic.Description, "syntax:", RegexOptions.IgnoreCase)[1].Replace("\r", string.Empty).Split('\n')[0].Trim();
							break;
						}
					}
				}
			}
			else
			{
				// Get resources from OldCommandSyntaxes.resx
				ResourceManager oldCommandSyntaxResource = new ResourceManager(typeof(OldCommandSyntaxes));
				ResourceSet oldCommandResourceSet = oldCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				// Get resources from NewCommandSyntaxes.resx
				ResourceManager newCommandSyntaxResource = new ResourceManager(typeof(NewCommandSyntaxes));
				ResourceSet newCommandResourceSet = newCommandSyntaxResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

				List<DictionaryEntry> entries = new List<DictionaryEntry>();
				entries.AddRange(oldCommandResourceSet.Cast<DictionaryEntry>().ToList());
				entries.AddRange(newCommandResourceSet.Cast<DictionaryEntry>().ToList());

				foreach (DictionaryEntry entry in entries)
				{
					if (command.ToLower() == entry.Key.ToString().ToLower())
					{
						content = entry.Value.ToString();
						break;
					}
				}
			}

			syntaxPreview.CurrentArgumentIndex = ArgumentHelper.GetCurrentArgumentIndex(_textBox.Document, _textBox.CaretOffset);
			syntaxPreview.Text = content;
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
				AvalonTextBox tabTextBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;

				if (tabTextBox.Document.FileName == null)
					continue;

				SaveFile(tab);

				HandleTextChangedIndicator(tab, tabTextBox);
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

		private void HandleTextChangedIndicator(TabPage tab = null, AvalonTextBox textBox = null)
		{
			if (tab == null)
				tab = tabControl_Editor.SelectedTab;

			if (textBox == null)
				textBox = _textBox;

			if (string.IsNullOrEmpty(textBox.Document.FileName))
			{
				if (!textBox.IsTextChanged)
					tab.Text = "Untitled";
				else if (!tab.Text.EndsWith("*"))
					tab.Text = "Untitled*";
			}
			else
			{
				if (!textBox.IsTextChanged)
					tab.Text = tab.Text.TrimEnd('*');
				else if (!tab.Text.EndsWith("*"))
					tab.Text = Path.GetFileName(textBox.Document.FileName) + "*";
			}
		}

		private void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			if (_textBox.CanUndo)
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
			if (_textBox.CanRedo)
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
			if (_textBox.IsTextChanged)
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
			label_LineNumber.Text = "Line: " + _textBox.TextArea.Caret.Position.Line;
			label_ColNumber.Text = "Column: " + _textBox.TextArea.Caret.Position.Column;

			label_SelectedChars.Text = "Selected: " + _textBox.SelectedText.Length;

			label_Zoom.Text = "Zoom: " + _textBox.Zoom + "%";
			button_ResetZoom.Visible = _textBox.Zoom != 100;
		}

		private void ToggleObjBrowser(bool state)
		{
			menuItem_ObjBrowser.Checked = state;
			splitter_Left.Visible = state;
			objectBrowser.Visible = state;
			_ide.Configuration.View_ShowObjBrowser = state;
		}

		private void ToggleFileList(bool state)
		{
			menuItem_FileList.Checked = state;
			splitter_Right.Visible = state;
			fileList.Visible = state;
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

			panel_Syntax.Visible = state;
		}

		private void ToggleLineNumbers(bool state)
		{
			menuItem_LineNumbers.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				((AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child).ShowLineNumbers = state;

			_ide.Configuration.View_ShowLineNumbers = state;
		}

		private void ToggleToolTips(bool state)
		{
			menuItem_ToolTips.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				((AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child).ShowToolTips = state;

			_ide.Configuration.View_ShowToolTips = state;
		}

		private void ToggleVisualSpaces(bool state)
		{
			menuItem_VisualSpaces.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				((AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child).Options.ShowSpaces = state;

			_ide.Configuration.View_ShowVisualSpaces = state;
		}

		private void ToggleVisualTabs(bool state)
		{
			menuItem_VisualTabs.Checked = state;

			foreach (TabPage tab in tabControl_Editor.TabPages)
				((AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child).Options.ShowTabs = state;

			_ide.Configuration.View_ShowVisualTabs = state;
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

			_ide.Configuration.View_SwapPanels = state;
		}

		private bool IsEveryTabSaved()
		{
			// The difference between this and the AreAllFilesSaved() method is that this one just returns true/false
			// and doesn't prompt the user to save the changes

			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				if (tab.Text.TrimEnd('*') == "Untitled")
					continue;

				AvalonTextBox tabTextBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;

				if (tabTextBox.IsTextChanged)
					return false;
			}

			return true;
		}

		#endregion Event Methods

		#region Compilers

		private void CompileTRNGScript()
		{
			if (!AreLibrariesRegistered())
				return;

			try
			{
				string vgeScriptPath = PathHelper.GetVGEScriptPath();

				// Delete the old /Script/ directory in the VGE if it exists
				if (Directory.Exists(vgeScriptPath))
					Directory.Delete(vgeScriptPath, true);

				// Recreate the directory
				Directory.CreateDirectory(vgeScriptPath);

				// Create all of the subdirectories
				foreach (string dirPath in Directory.GetDirectories(_ide.Project.ScriptPath, "*", SearchOption.AllDirectories))
					Directory.CreateDirectory(dirPath.Replace(_ide.Project.ScriptPath, vgeScriptPath));

				// Copy all the files into the VGE /Script/ folder
				foreach (string newPath in Directory.GetFiles(_ide.Project.ScriptPath, "*.*", SearchOption.AllDirectories))
					File.Copy(newPath, newPath.Replace(_ide.Project.ScriptPath, vgeScriptPath));

				AdjustOldFormatting();

				// Run NG_Center.exe
				var application = TestStack.White.Application.Launch(Path.Combine(PathHelper.GetInternalNGCPath(), "NG_Center.exe"));

				// Do some actions in NG Center
				RunScriptedNGCenterEvents(application);
			}
			catch (Exception ex)
			{
				if (_formCompiling.Visible)
					_formCompiling.Close();

				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void RunScriptedNGCenterEvents(TestStack.White.Application application)
		{
			try
			{
				_formCompiling.Show();

				// Get a list of all windows belonging to the app
				var windowList = application.GetWindows();

				// Check if the list has the main NG Center window (it starts with "NG Center 1.5...")
				var ngWindow = windowList.Find(x => x.Title.Contains("NG Center 1.5"));

				if (ngWindow == null)
				{
					// Refresh the window list and check if a Updater message box appeared
					windowList = application.GetWindows();
					var ngMissingWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

					if (ngMissingWindow != null)
						ngMissingWindow.KeyIn(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ESCAPE);

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
					if (_formCompiling.Visible)
						_formCompiling.Close();

					_formDebugMode.Show();
					return;
				}

				// Find the "Show Log" button
				var logButton = ngWindow.Get<TestStack.White.UIItems.Button>("Show Log");

				// Click the button
				cachedCursorPosition = Cursor.Position;
				logButton.Click();
				Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

				// Read the logs
				string logFilePath = Path.Combine(PathHelper.GetVGEScriptPath(), "script_log.txt");
				string logFileContent = File.ReadAllText(logFilePath);

				// Replace the VGE paths in the log file with the current project ones
				File.WriteAllText(logFilePath, logFileContent.Replace(PathHelper.GetVGEPath(), _ide.Project.EnginePath), Encoding.GetEncoding(1252));

				application.Close(); // Done!

				// Copy the compiled files from the Virtual Game Engine folder to the current project folder
				string compiledScriptFilePath = Path.Combine(PathHelper.GetVGEPath(), "Script.dat");
				string compiledEnglishFilePath = Path.Combine(PathHelper.GetVGEPath(), "English.dat");

				if (File.Exists(compiledScriptFilePath))
					File.Copy(compiledScriptFilePath, Path.Combine(_ide.Project.EnginePath, "Script.dat"), true);

				if (File.Exists(compiledEnglishFilePath))
					File.Copy(compiledEnglishFilePath, Path.Combine(_ide.Project.EnginePath, "English.dat"), true);

				// Read and show the logs in the "Compiler Logs" richTextBox
				richTextBox_Logs.Text = File.ReadAllText(logFilePath);

				// Select the "Compiler Logs" tab
				tabControl_Info.SelectTab(1);
				tabControl_Info.Invalidate();

				System.Threading.Thread.Sleep(100);

				foreach (Process fuckingDieAlreadyYouStupidFuck in Process.GetProcessesByName("notepad"))
				{
					if (fuckingDieAlreadyYouStupidFuck.MainWindowTitle.Contains("script_log"))
						fuckingDieAlreadyYouStupidFuck.Kill();
				}

				if (_formCompiling.Visible)
					_formCompiling.Close();
			}
			catch (ElementNotAvailableException)
			{
				// The "Loading" window just closed, so try again
				RunScriptedNGCenterEvents(application);
			}
		}

		private void AdjustOldFormatting() // Because the compilers really don't like having a space before "="
		{
			string vgeScriptFileContent = File.ReadAllText(Path.Combine(PathHelper.GetVGEScriptPath(), "Script.txt"));

			while (vgeScriptFileContent.Contains(" ="))
				vgeScriptFileContent = vgeScriptFileContent.Replace(" =", "=");

			File.WriteAllText(Path.Combine(PathHelper.GetVGEScriptPath(), "Script.txt"), vgeScriptFileContent, Encoding.GetEncoding(1252));
		}

		private bool AreLibrariesRegistered()
		{
			string MSCOMCTL = Path.Combine(PathHelper.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(PathHelper.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(PathHelper.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(PathHelper.GetSystemDirectory(), "Comdlg32.ocx");

			if (!File.Exists(MSCOMCTL) || !File.Exists(RICHTX32) || !File.Exists(PICFORMAT32) || !File.Exists(COMDLG32))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(PathHelper.GetProgramDirectory(), "TombIDE Library Registration.exe")
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

			// Create the TextBox
			AvalonTextBox newTextBox = new AvalonTextBox
			{
				ShowLineNumbers = _ide.Configuration.View_ShowLineNumbers
			};

			// Bind event methods to the TextBox
			newTextBox.TextArea.Caret.PositionChanged += Caret_PositionChanged;
			newTextBox.TextArea.SelectionChanged += Editor_StatusChanged;
			newTextBox.TextChanged += Editor_TextChanged;
			newTextBox.ZoomChanged += Editor_StatusChanged;
			newTextBox.KeyDown += Editor_KeyDown;

			ElementHost elementHost = new ElementHost
			{
				Size = new Size(newTabPage.Size.Width - 6, newTabPage.Size.Height),
				Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
				Location = new Point(6, 0),
				Child = newTextBox,
				ContextMenuStrip = contextMenu_TextBox
			};

			// Add the host to the tab
			newTabPage.Controls.Add(elementHost);

			// Add the tab to the tabControl and select it
			tabControl_Editor.TabPages.Add(newTabPage);
			tabControl_Editor.SelectTab(newTabPage);

			// Change the active TextBox and apply user settings to it
			_textBox = newTextBox;

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
				// Change the active ScriptTextBox
				_textBox = (AvalonTextBox)tabControl_Editor.SelectedTab.Controls.OfType<ElementHost>().First().Child;

				// Update everything
				UpdateUndoRedoSaveStates();
				objectBrowser.UpdateContent(_textBox.Text);
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
				string scriptFilePath = PathHelper.GetScriptFilePath(_ide.Project);
				TabPage scriptFileTab = FindTabPageOfFile(scriptFilePath);

				if (scriptFileTab != null)
					tabControl_Editor.SelectTab(scriptFileTab);
				else
					OpenFile(scriptFilePath);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenLanguageFile(Language language)
		{
			try
			{
				string languageFilePath = PathHelper.GetLanguageFilePath(_ide.Project, language);
				TabPage languageFileTab = FindTabPageOfFile(languageFilePath);

				if (languageFileTab != null)
					tabControl_Editor.SelectTab(languageFileTab);
				else
					OpenFile(languageFilePath);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFile(string filePath)
		{
			CreateNewTabPage(GetCorrectTabTitle(filePath)); // Changes the current _textBox
			_textBox.OpenFile(filePath);

			menuItem_Save.Enabled = false;
			button_Save.Enabled = false;

			objectBrowser.UpdateContent(_textBox.Text);
		}

		private bool IsFileSaved(TabPage tab)
		{
			AvalonTextBox textBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;

			if (textBox.IsTextChanged)
			{
				if (_ide.SelectedIDETab != IDETab.ScriptEditor)
					_ide.SelectIDETab(IDETab.ScriptEditor);

				tabControl_Editor.SelectTab(tab);

				string fileName = string.IsNullOrEmpty(_textBox.Document.FileName) ? "Untitled" : Path.GetFileName(_textBox.Document.FileName);

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
			AvalonTextBox textBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;

			if (_ide.Configuration.Tidy_ReindentOnSave)
				textBox.TidyDocument();

			try
			{
				if (textBox.Document.FileName == null) // For "Untitled"
				{
					using (FormSaveAs form = new FormSaveAs(_ide))
					{
						if (form.ShowDialog(this) == DialogResult.OK)
						{
							textBox.Document.FileName = form.NewFilePath;
							tab.Text = form.NewFilePath.Replace(_ide.Project.ScriptPath + "\\", "");
						}
						else
							return false;
					}
				}

				textBox.Save(textBox.Document.FileName);
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
			using (FormAbout form = new FormAbout(null))
				form.ShowDialog(this);
		}

		private void ShowSettingsForm()
		{
			_ide.Configuration.Save();

			// Cache critical settings
			int undoStackSizeCache = _ide.Configuration.UndoStackSize;
			bool autocompleteCache = _ide.Configuration.AutocompleteEnabled;

			using (FormSettings form = new FormSettings(_ide))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ApplySavedSettings();

					if (form.RestartItemCount > 0)
					{
						if (!AreAllFilesSaved())
						{
							// Saving failed or the user clicked "Cancel"
							// Therefore restore the previous critical settings
							_ide.Configuration.UndoStackSize = undoStackSizeCache;
							_ide.Configuration.AutocompleteEnabled = autocompleteCache;
							_ide.Configuration.Save();

							ShowSettingsForm();
							return; // DO NOT CLOSE THE APP !!! (ﾉ°Д°)ﾉ︵﻿ ┻━┻
						}

						_ide.RestartApplication();
					}
				}
			}
		}

		#endregion Forms

		private void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to clear all bookmarks from the current document?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				_textBox.BookmarkedLines.Clear();
		}

		public string GetCorrectTabTitle(string filePath)
		{
			return filePath.Replace(_ide.Project.ScriptPath + @"\", "");
		}

		public string GetOriginalFileFromBackupFile(string backupFilePath)
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
					DocumentLine line = _textBox.Document.GetLineByOffset(match.Index);

					_textBox.SelectionStart = match.Index;
					_textBox.SelectionLength = match.Length;

					_textBox.ScrollToLine(line.LineNumber);
				}
				catch { }
			}
		}
	}
}
