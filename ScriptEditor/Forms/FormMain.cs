using DarkUI.Controls;
using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class FormMain : DarkForm
	{
		/// <summary>
		/// Store the currently edited file name and path.
		/// </summary>
		private string _currentFilePath;

		/// <summary>
		/// Store the previous zoom value to fix a strange bug.
		/// </summary>
		private int _prevZoom = 100;

		public FormMain()
		{
			InitializeComponent();

			// Disable the interface
			ToggleInterface(false);

			// If Autocomplete is enabled in the settings
			if (Properties.Settings.Default.Autocomplete)
			{
				GenerateAutocompleteMenu();
			}

			// Redraw (or in this case draw) the reference browser to fill it with stuff
			ReferenceBrowser browser = new ReferenceBrowser();
			browser.Invalidate();

			// Add default items to the object browser
			ResetObjectBrowserNodes();

			// Apply saved user settings
			ApplyUserSettings();
		}

		/* Form actions */

		private void FormMain_Closing(object sender, FormClosingEventArgs e)
		{
			// If the file has unsaved changes
			if (!IsFileSaved())
			{
				// Stop closing the form if saving failed or the user clicked "Cancel"
				e.Cancel = true;
			}
		}

		private void FormMain_Shown(object sender, EventArgs e) => CheckRequiredPaths(); // Check if required paths are set

		/* File menu */

		private void File_Change_MenuItem_Click(object sender, EventArgs e) => ShowPathSelection();
		private void File_Save_MenuItem_Click(object sender, EventArgs e) => SaveFile();
		private void File_Exit_MenuItem_Click(object sender, EventArgs e) => Close();

		/* Edit menu */

		private void Edit_Undo_MenuItem_Click(object sender, EventArgs e) => HandleUndo();
		private void Edit_Redo_MenuItem_Click(object sender, EventArgs e) => HandleRedo();

		private void Edit_Cut_MenuItem_Click(object sender, EventArgs e) => textEditor.Cut();
		private void Edit_Copy_MenuItem_Click(object sender, EventArgs e) => textEditor.Copy();
		private void Edit_Paste_MenuItem_Click(object sender, EventArgs e) => textEditor.Paste();

		private void Edit_Find_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowFindDialog();
		private void Edit_Replace_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowReplaceDialog();

		private void Edit_SelectAll_MenuItem_Click(object sender, EventArgs e)
		{
			textEditor.SelectAll();
			DoStatusCounting(); // So it updates the statusbar.
		}

		/* Tools menu */

		private void Tools_ReindentScript_MenuItem_Click(object sender, EventArgs e) => TidyScript();
		private void Tools_TrimWhitespace_MenuItem_Click(object sender, EventArgs e) => TidyScript(true);

		private void Tools_CommentLines_MenuItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void Tools_Uncomment_MenuItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void Tools_ToggleBookmark_MenuItem_Click(object sender, EventArgs e) => ToggleBookmark();
		private void Tools_NextBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void Tools_PrevBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void Tools_ClearAllBookmarks_MenuItem_Click(object sender, EventArgs e) => ClearAllBookmarks();

		private void Tools_Settings_MenuItem_Click(object sender, EventArgs e)
		{
			using (FormSettings form = new FormSettings())
			{
				// If the user pressed "Apply"
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ApplyUserSettings();
				}
			}
		}

		/* View menu */

		private void View_ObjectBrowser_MenuItem_Click(object sender, EventArgs e)
		{
			objectBrowser.Visible = !objectBrowser.Visible;
			searchTextBox.Visible = !searchTextBox.Visible;
			objectBrowserBox.Visible = !objectBrowserBox.Visible;
			objectBrowserToolStripMenuItem.Checked = objectBrowserBox.Visible;
		}

		private void View_ReferenceBrowser_MenuItem_Click(object sender, EventArgs e)
		{
			referenceBrowser.Visible = !referenceBrowser.Visible;
			referenceBrowserToolStripMenuItem.Checked = referenceBrowser.Visible;
		}

		private void View_DocumentMap_MenuItem_Click(object sender, EventArgs e)
		{
			documentMap.Visible = !documentMap.Visible;
			documentMapToolStripMenuItem.Checked = documentMap.Visible;
		}

		/* Help menu */

		private void Help_About_MenuItem_Click(object sender, EventArgs e) => ShowAboutForm();

		/* Editor events */

		private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Re-enable save buttons since the text has changed
			saveToolStripMenuItem.Enabled = true;
			saveToolStripButton.Enabled = true;

			DoSyntaxHighlighting(e);
		}

		private void textEditor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void textEditor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) => HandleToolTips(e);
		private void textEditor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();
		private void textEditor_KeyUp(object sender, KeyEventArgs e) => UpdateObjectBrowser(string.Empty);

		private void textEditor_MouseDown(object sender, MouseEventArgs e)
		{
			// If the user right clicked somewhere in the editor and has nothing selected
			if (e.Button == MouseButtons.Right && string.IsNullOrEmpty(textEditor.SelectedText))
			{
				// Move the caret to the new position
				textEditor.Selection.Start = textEditor.PointToPlace(e.Location);
			}
		}

		private void textEditor_ZoomChanged(object sender, EventArgs e)
		{
			// If zoom increased
			if (textEditor.Zoom > _prevZoom)
			{
				_prevZoom += 5;
				textEditor.Zoom = _prevZoom;
			}

			// If zoom decreased
			if (textEditor.Zoom < _prevZoom)
			{
				_prevZoom -= 5;
				textEditor.Zoom = _prevZoom;
			}

			// Update the label
			zoomLabel.Text = "Zoom: " + textEditor.Zoom + "%";

			resetZoomButton.Visible = textEditor.Zoom != 100;

			// Limit the zoom
			if (textEditor.Zoom > 500)
				textEditor.Zoom = 500;

			if (textEditor.Zoom < 25)
				textEditor.Zoom = 25;
		}

		private void undoRedoState_Changed(object sender, EventArgs e)
		{
			if (textEditor.UndoEnabled)
			{
				undoToolStripMenuItem.Enabled = true;
				undoToolStripMenuItem.Text = "&Undo";
				undoToolStripButton.Enabled = true;
			}
			else
			{
				undoToolStripMenuItem.Enabled = false;
				undoToolStripMenuItem.Text = "Can't Undo";
				undoToolStripButton.Enabled = false;
			}

			if (textEditor.RedoEnabled)
			{
				redoToolStripMenuItem.Enabled = true;
				redoToolStripMenuItem.Text = "&Redo";
				redoToolStripButton.Enabled = true;
			}
			else
			{
				redoToolStripMenuItem.Enabled = false;
				redoToolStripMenuItem.Text = "Can't Redo";
				redoToolStripButton.Enabled = false;
			}
		}

		/* ToolStrip buttons */

		private void ToolStrip_ChangeButton_Click(object sender, EventArgs e) => ShowPathSelection();
		private void ToolStrip_SaveButton_Click(object sender, EventArgs e) => SaveFile();

		private void ToolStrip_UndoButton_Click(object sender, EventArgs e) => HandleUndo();
		private void ToolStrip_RedoButton_Click(object sender, EventArgs e) => HandleRedo();

		private void ToolStrip_CutButton_Click(object sender, EventArgs e) => textEditor.Cut();
		private void ToolStrip_CopyButton_Click(object sender, EventArgs e) => textEditor.Copy();
		private void ToolStrip_PasteButton_Click(object sender, EventArgs e) => textEditor.Paste();

		private void ToolStrip_CommentButton_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ToolStrip_UncommentButton_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ToolStrip_ToggleBookmarkButton_Click(object sender, EventArgs e) => ToggleBookmark();
		private void ToolStrip_PrevBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_NextBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_ClearAllBookmarksButton_Click(object sender, EventArgs e) => ClearAllBookmarks();

		private void ToolStrip_AboutButton_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ToolStrip_StringTableButton_Click(object sender, EventArgs e)
		{
			using (FormStringTable form = new FormStringTable())
			{
				form.ShowDialog(this);
			}
		}

		/* StatusStrip buttons */

		private void StatusStrip_ResetZoomButton_Click(object sender, EventArgs e)
		{
			// Reset zoom values
			_prevZoom = 100;
			textEditor.Zoom = 100;

			resetZoomButton.Visible = false;
		}

		/* ContextMenu items */

		private void ContextMenu_CutItem_Click(object sender, EventArgs e) => textEditor.Cut();
		private void ContextMenu_CopyItem_Click(object sender, EventArgs e) => textEditor.Copy();
		private void ContextMenu_PasteItem_Click(object sender, EventArgs e) => textEditor.Paste();

		private void ContextMenu_CommentItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ContextMenu_UncommentItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();

		/* Application launch methods */

		private void CheckRequiredPaths()
		{
			// If the required paths aren't defined yet (script folder and game folder)
			if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ScriptPath)
				|| string.IsNullOrWhiteSpace(Properties.Settings.Default.GamePath))
			{
				DialogResult result = DarkMessageBox.Show(this, Resources.Messages.PathsNotFound,
					"Paths not found!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (result == DialogResult.Yes)
				{
					// Show path selection dialog
					ShowPathSelection();
				}
			}
			else
			{
				// Read all files from the script folder
				ReadScriptFolder();
			}
		}

		public void ApplyUserSettings()
		{
			textEditor.Font = new Font(Properties.Settings.Default.FontFace, Convert.ToSingle(Properties.Settings.Default.FontSize));
			textEditor.AutoCompleteBrackets = Properties.Settings.Default.CloseBrackets;
			textEditor.WordWrap = Properties.Settings.Default.WordWrap;
			toolStrip.Visible = Properties.Settings.Default.ShowToolbar;
			statusStrip.Visible = Properties.Settings.Default.ShowStatusbar;
		}

		private void GenerateAutocompleteMenu()
		{
			AutocompleteMenu popupMenu = new AutocompleteMenu(textEditor)
			{
				AllowTabKey = true,
				BackColor = Color.FromArgb(64, 73, 74),
				ForeColor = Color.Gainsboro,
				SearchPattern = @"[\w\.:=!<>\[\]]",
				SelectedColor = Color.SteelBlue
			};

			popupMenu.Items.SetAutocompleteItems(AutocompleteItems.GetItems());
		}

		/* ToolTips handling */

		private void HandleToolTips(ToolTipNeededEventArgs e)
		{
			// If tool tips are enabled in the settings and the hovered word isn't whitespace
			if (Properties.Settings.Default.ToolTips && !string.IsNullOrWhiteSpace(e.HoveredWord))
			{
				// If the hovered word is a header
				if (textEditor.GetLineText(e.Place.iLine).StartsWith("["))
				{
					ShowHeaderToolTips(e);
				}
				else
				{
					ShowCommandToolTips(e);
				}
			}
		}

		private void ShowHeaderToolTips(ToolTipNeededEventArgs e)
		{
			// ToolTip title with brackets added
			e.ToolTipTitle = "[" + e.HoveredWord + "]";

			// Get resources from the HeaderToolTips.resx file
			ResourceManager headerToolTipResource = new ResourceManager(typeof(Resources.HeaderToolTips));
			ResourceSet resourceSet = headerToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Loop through resources
			foreach (DictionaryEntry entry in resourceSet)
			{
				// If the hovered word matches a "header key"
				if (e.HoveredWord == entry.Key.ToString())
				{
					e.ToolTipText = entry.Value.ToString();
					return;
				}
			}
		}

		private void ShowCommandToolTips(ToolTipNeededEventArgs e)
		{
			// Get resources from the CommandToolTips.resx file
			ResourceManager commandToolTipResource = new ResourceManager(typeof(Resources.CommandToolTips));
			ResourceSet resourceSet = commandToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// There are different definitions for the "Level" key, so handle them all!
			if (e.HoveredWord == "Level")
			{
				HandleLevelToolTips(e);
			}
			else
			{
				e.ToolTipTitle = e.HoveredWord;

				// Loop through resources
				foreach (DictionaryEntry entry in resourceSet)
				{
					// If the hovered word matches a "command key"
					if (e.HoveredWord == entry.Key.ToString())
					{
						e.ToolTipText = entry.Value.ToString();
						return;
					}
				}
			}
		}

		private void HandleLevelToolTips(ToolTipNeededEventArgs e)
		{
			// Get the current line number
			int i = e.Place.iLine;

			do
			{
				if (i < 0)
				{
					// The line number might go to -1 and it will crash the app, so stop the loop to prevent it!
					return;
				}

				if (textEditor.GetLineText(i).StartsWith("[PSXExtensions]"))
				{
					e.ToolTipTitle = "Level [PSXExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPSX;
					return;
				}
				else if (textEditor.GetLineText(i).StartsWith("[PCExtensions]"))
				{
					e.ToolTipTitle = "Level [PCExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPC;
					return;
				}
				else if (textEditor.GetLineText(i).StartsWith("[Title]"))
				{
					e.ToolTipTitle = "Level [Title]";
					e.ToolTipText = Resources.CommandToolTips.LevelTitle;
					return;
				}
				else if (textEditor.GetLineText(i).StartsWith("[Level]"))
				{
					e.ToolTipTitle = "Level";
					e.ToolTipText = Resources.CommandToolTips.LevelLevel;
					return;
				}

				i--; // Go 1 line higher if no header was found yet
			}
			while (!textEditor.GetLineText(i + 1).StartsWith("["));
		}

		/* Styles and status */

		private void DoSyntaxHighlighting(TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
				SyntaxColors.Comments, SyntaxColors.Regular, SyntaxColors.References, SyntaxColors.Values,
				SyntaxColors.Headers, SyntaxColors.NewCommands, SyntaxColors.OldCommands, SyntaxColors.Unknown);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(SyntaxColors.Comments, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Regular, @"[\[\],=]");
			e.ChangedRange.SetStyle(SyntaxColors.References, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(SyntaxColors.Values, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Headers, @"\[(" + string.Join("|", SyntaxKeyWords.Headers()) + @")\]");
			e.ChangedRange.SetStyle(SyntaxColors.NewCommands, @"\b(" + string.Join("|", SyntaxKeyWords.NewCommands()) + ")");
			e.ChangedRange.SetStyle(SyntaxColors.OldCommands, @"\b(" + string.Join("|", SyntaxKeyWords.OldCommands()) + ")");
			e.ChangedRange.SetStyle(SyntaxColors.Unknown, @"\b(" + string.Join("|", SyntaxKeyWords.Unknown()) + ")");
		}

		private void DoStatusCounting()
		{
			lineNumberLabel.Text = "Line: " + (textEditor.Selection.Start.iLine + 1); // + 1 since it counts from 0.
			colNumberLabel.Text = "Column: " + textEditor.Selection.Start.iChar;
			selectedCharsLabel.Text = "Selected: " + textEditor.SelectedText.Length;
		}

		/* Syntax tidy methods */

		private void TidyScript(bool trimOnly = false)
		{
			// Save set bookmarks to prevent a bug
			int[] bookmarkLines = GetBookmarkedLines();
			textEditor.Bookmarks.Clear();

			// Get current scroll position
			int scrollPosition = textEditor.VerticalScroll.Value;

			List<string> tidiedlines = new List<string>();

			if (trimOnly)
			{
				// Trim whitespace on every line
				tidiedlines = SyntaxTidy.TrimLines(textEditor.Text);
			}
			else
			{
				// Reindent all lines
				tidiedlines = SyntaxTidy.ReindentLines(textEditor.Text);
			}

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// If a line has changed
				if (textEditor.GetLineText(i) != tidiedlines[i])
				{
					textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
					textEditor.InsertText(tidiedlines[i]);
				}
			}

			// Go to last scroll position
			textEditor.VerticalScroll.Value = scrollPosition;
			textEditor.UpdateScrollbars();

			// Add lost bookmarks
			foreach (int line in bookmarkLines)
			{
				textEditor.BookmarkLine(line);
			}

			textEditor.Invalidate();
		}

		/* Bookmark methods */

		private void ToggleBookmark()
		{
			// Get the current line number
			int currentLine = textEditor.Selection.Start.iLine;

			// If there's a bookmark on the current line
			if (textEditor.Bookmarks.Contains(currentLine))
			{
				textEditor.Bookmarks.Remove(currentLine);
			}
			else
			{
				textEditor.Bookmarks.Add(currentLine);
			}
		}

		private void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this,
				Resources.Messages.ClearBookmarks, "Delete all bookmarks?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				textEditor.Bookmarks.Clear();
				textEditor.Invalidate();
			}
		}

		private int[] GetBookmarkedLines()
		{
			List<int> lineNumbers = new List<int>();

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				if (textEditor.Bookmarks.Contains(i))
				{
					lineNumbers.Add(i);
				}
			}

			return lineNumbers.ToArray();
		}

		/* Object browser handling */

		private void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(searchTextBox.Text) || searchTextBox.Text == "Search...")
			{
				UpdateObjectBrowser(string.Empty);
			}
			else
			{
				UpdateObjectBrowser(searchTextBox.Text);
			}
		}

		private void searchTextBox_GotFocus(object sender, EventArgs e)
		{
			if (searchTextBox.Text == "Search...")
			{
				searchTextBox.Text = string.Empty;
			}
		}

		private void searchTextBox_LostFocus(object sender, EventArgs e)
		{
			if (searchTextBox.Text == string.Empty)
			{
				searchTextBox.Text = "Search...";
			}
		}

		private void objectBrowser_Click(object sender, EventArgs e)
		{
			// If the user hasn't selected any nodes
			if (objectBrowser.SelectedNodes.Count < 1)
			{
				return;
			}

			// If the selected node is a default item
			if (objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[0] || objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[1])
			{
				return;
			}

			// If the selected node is empty
			if (objectBrowser.SelectedNodes[0].Text == string.Empty)
			{
				return;
			}

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// Find the line that contains the node text
				if (textEditor.GetLineText(i).ToLower().Contains(objectBrowser.SelectedNodes[0].Text.ToLower()))
				{
					textEditor.Focus();
					textEditor.Navigate(i); // Scroll to the line position

					// Select the line
					textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
					return;
				}
			}
		}

		private void UpdateObjectBrowser(string filter)
		{
			ResetObjectBrowserNodes();

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				AddObjectBrowserNodes(i, filter);
			}

			objectBrowser.Invalidate();
		}

		private void AddObjectBrowserNodes(int lineNumber, string filter)
		{
			// Regex rule to find lines that start with "Name = "
			Regex rgx = new Regex(@"\bName\s?=\s?");

			// If we found a line that starts with our Regex rule ("Name = ")
			if (rgx.IsMatch(textEditor.GetLineText(lineNumber)))
			{
				// Create a new node, remove "Name = " and trim it, so we only get the level name string
				DarkTreeNode levelNode = new DarkTreeNode(rgx.Replace(textEditor.GetLineText(lineNumber), string.Empty).Trim());

				// If the level name matches the filter (It always does if there's nothing in the search bar)
				if (levelNode.Text.ToLower().Contains(filter.ToLower()))
				{
					objectBrowser.Nodes[1].Nodes.Add(levelNode);
				}
			}

			// Get header key words
			List<string> headers = SyntaxKeyWords.Headers();

			// Loop through headers
			foreach (string header in headers)
			{
				// Add brackets to the header
				string fullHeader = "[" + header + "]";

				// If there are any headers except "Level" headers
				if (fullHeader != "[Level]" && textEditor.GetLineText(lineNumber).StartsWith(fullHeader))
				{
					DarkTreeNode headerNode = new DarkTreeNode(fullHeader);

					// If the header name matches the filter (It always does if there's nothing in the search bar)
					if (headerNode.Text.ToLower().Contains(filter.ToLower()))
					{
						objectBrowser.Nodes[0].Nodes.Add(headerNode);
					}
				}
			}
		}

		/* File handling */

		private void ShowPathSelection()
		{
			if (!IsFileSaved())
			{
				return;
			}

			using (FormSelectPaths form = new FormSelectPaths())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					ReadScriptFolder();
				}
			}
		}

		public void ReadScriptFolder()
		{
			try
			{
				string path = Properties.Settings.Default.ScriptPath;

				// Get files from selected directory
				string[] files = Directory.GetFiles(path);
				bool folderHasScriptFile = false;

				foreach (string file in files)
				{
					// If the file is not a text file
					if (!file.ToLower().Contains(".txt"))
					{
						continue;
					}

					// Open the script file if exists
					if (Path.GetFileName(file).ToLower() == "script.txt")
					{
						folderHasScriptFile = true;
						OpenFile(file);
					}
				}

				// If no script file has been found
				if (!folderHasScriptFile)
				{
					DialogResult result = DarkMessageBox.Show(this,
						Resources.Messages.NoScriptFile, "Script file not found.",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						// Create SCRIPT.txt from the template
						string filePath = path + "\\SCRIPT.txt";
						File.Copy("ScriptTemplate.txt", filePath);

						OpenFile(filePath);
					}
					else if (result == DialogResult.No)
					{
						textEditor.Clear();
						UpdateObjectBrowser(string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				UnloadCurrentFile();
			}
		}

		public void OpenFile(string filePath)
		{
			try
			{
				textEditor.OpenFile(filePath, Encoding.GetEncoding(1252));
				ToggleInterface(true);

				// Store the file (with it's path) and open it in the editor
				_currentFilePath = filePath;

				// Disable "Save" buttons since we've just opened the file
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				// Update the object browser with levels (if they exist)
				UpdateObjectBrowser(string.Empty);

				int autoSaveTime = Properties.Settings.Default.AutoSaveTime;

				if (autoSaveTime != 0)
				{
					var saveTimer = new System.Timers.Timer();
					saveTimer.Elapsed += saveTimer_Elapsed;
					saveTimer.Interval = autoSaveTime * (60 * 1000);
					saveTimer.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				UnloadCurrentFile();
			}
		}

		private void saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string currentFolder = Path.GetDirectoryName(_currentFilePath);
			string currentFileName = Path.GetFileNameWithoutExtension(_currentFilePath);
			string autosaveFilePath = currentFolder + "\\" + currentFileName + ".autosave";

			try
			{
				File.WriteAllText(autosaveFilePath, textEditor.Text, Encoding.GetEncoding(1252));
				autoSaveLabel.Invoke(new Action(() => autoSaveLabel_Show(true)));
			}
			catch (Exception)
			{
				autoSaveLabel.Invoke(new Action(() => autoSaveLabel_Show(false)));
			}
		}

		private void autoSaveLabel_Show(bool success)
		{
			if (success)
			{
				string currentTime = DateTime.Now.TimeOfDay.ToString().Substring(0, 5);
				autoSaveLabel.Text = "Autosave Completed! (" + currentTime + ")";
				autoSaveLabel.Visible = true;
			}
			else
			{
				autoSaveLabel.Text = "ERROR: Autosave Failed!";
				autoSaveLabel.Visible = true;
			}
		}

		/* File saving */

		private bool IsFileSaved()
		{
			// If the current file has unsaved changes
			if (textEditor.IsChanged)
			{
				DialogResult result = DarkMessageBox.Show(this,
				string.Format(Resources.Messages.UnsavedChanges, Path.GetFileName(_currentFilePath)), "Unsaved changes!",
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					// Try to save the file
					if (!SaveFile())
					{
						return false; // If file saving failed
					}
				}
				else if (result == DialogResult.Cancel)
				{
					return false; // If the user has cancelled the operation
				}
			}

			return true; // If the file is saved or has just been saved
		}

		private bool SaveFile()
		{
			if (Properties.Settings.Default.ReindentOnSave)
			{
				TidyScript();
			}

			try
			{
				// Save changes to file
				textEditor.SaveToFile(_currentFilePath, Encoding.GetEncoding(1252));
				textEditor.IsChanged = false;

				// Disable "Save" buttons since we've just saved
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				textEditor.Invalidate();
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
				{
					return SaveFile(); // Retry saving
				}

				return false;
			}

			return true; // If saving was successful
		}

		// TODO: UNSORTED CODE STARTS HERE:

		private void ShowAboutForm()
		{
			using (FormAbout form = new FormAbout())
			{
				form.ShowDialog(this);
			}
		}

		private void HandleUndo()
		{
			textEditor.Undo();

			if (textEditor.Text == File.ReadAllText(_currentFilePath))
			{
				textEditor.IsChanged = false;
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;
			}
		}

		private void HandleRedo()
		{
			textEditor.Redo();

			if (textEditor.Text == File.ReadAllText(_currentFilePath))
			{
				textEditor.IsChanged = false;
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;
			}
		}

		private void ToggleInterface(bool state)
		{
			// Toggle editor
			textEditor.Enabled = state;

			// Toggle "File" menu items
			saveToolStripMenuItem.Enabled = state;
			buildToolStripMenuItem.Enabled = state;

			// Toggle "Edit" menu items
			cutToolStripMenuItem.Enabled = state;
			copyToolStripMenuItem.Enabled = state;
			pasteToolStripMenuItem.Enabled = state;
			findToolStripMenuItem.Enabled = state;
			replaceToolStripMenuItem.Enabled = state;
			selectAllToolStripMenuItem.Enabled = state;

			// Toggle "Tools" menu items
			reindentScriptToolStripMenuItem.Enabled = state;
			trimWhitespaceToolStripMenuItem.Enabled = state;
			commentToolStripMenuItem.Enabled = state;
			uncommentToolStripMenuItem.Enabled = state;
			toggleBookmarkToolStripMenuItem.Enabled = state;
			nextBookmarkToolStripMenuItem.Enabled = state;
			prevBookmarkToolStripMenuItem.Enabled = state;
			clearBookmarksToolStripMenuItem.Enabled = state;

			// Toggle ToolStrip buttons
			saveToolStripButton.Enabled = state;
			buildToolStripButton.Enabled = state;
			cutToolStripButton.Enabled = state;
			copyToolStripButton.Enabled = state;
			pasteToolStripButton.Enabled = state;
			commentToolStripButton.Enabled = state;
			uncommentToolStripButton.Enabled = state;
			toggleBookmarkToolStripButton.Enabled = state;
			prevBookmarkToolStripButton.Enabled = state;
			nextBookmarkToolStripButton.Enabled = state;
			clearBookmarksToolStripButton.Enabled = state;
		}

		private void ResetObjectBrowserNodes()
		{
			objectBrowser.Nodes.Clear();
			objectBrowser.Nodes.Add(new DarkTreeNode("Headers"));
			objectBrowser.Nodes.Add(new DarkTreeNode("Levels"));
			objectBrowser.Invalidate();
		}

		private void UnloadCurrentFile()
		{
			// Unload the file
			_currentFilePath = string.Empty;

			// Clear the editor
			textEditor.Clear();
			textEditor.IsChanged = false;

			ResetObjectBrowserNodes();

			// Disable the interface
			ToggleInterface(false);
		}
	}
}
