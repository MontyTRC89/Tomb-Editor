using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class FormMain : DarkForm
	{
		#region Global variables

		/// <summary>
		/// Stores the currently edited file name and path.
		/// </summary>
		private string gS_CurrentFilePath;

		/// <summary>
		/// Stores the previous zoom value to fix a strange FCTB bug.
		/// </summary>
		private int gI_PrevZoom = 100;

		#endregion Global variables

		#region Constructors

		public FormMain()
		{
			InitializeComponent();

			// If Autocomplete is enabled in the settings
			if (Properties.Settings.Default.Autocomplete)
			{
				GenerateAutocompleteMenu();
			}
		}

		#endregion Constructors

		#region Form actions

		private void FormMain_Shown(object sender, EventArgs e)
		{
			// Apply saved user settings
			ApplyUserSettings();

			// Disable the interface since no file has been loaded yet
			ToggleInterface(false);

			// Check if required paths are set
			CheckRequiredPaths();

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Force expanded nodes on launch
				objectBrowser.Nodes[0].Expanded = true;
				objectBrowser.Nodes[1].Expanded = true;
			}
		}

		private void FormMain_Closing(object sender, FormClosingEventArgs e)
		{
			// If the current file has unsaved changes
			if (!IsFileSaved())
			{
				// Stop closing the form if saving failed or the user clicked "Cancel"
				e.Cancel = true;
			}
		}

		#endregion Form actions

		#region Application launch methods

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

		private void ApplyUserSettings()
		{
			textEditor.Font = new Font(Properties.Settings.Default.FontFace, Convert.ToSingle(Properties.Settings.Default.FontSize));
			textEditor.AutoCompleteBrackets = Properties.Settings.Default.CloseBrackets;
			textEditor.WordWrap = Properties.Settings.Default.WordWrap;
			textEditor.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;

			toolStrip.Visible = Properties.Settings.Default.ShowToolbar;
			showStringTableButton.Visible = toolStrip.Visible;

			ToggleObjectBrowser(Properties.Settings.Default.ObjBrowserVisible);
			ToggleReferenceBrowser(Properties.Settings.Default.RefBrowserVisible);
			ToggleDocumentMap(Properties.Settings.Default.DocMapVisible);
		}

		private void CheckRequiredPaths()
		{
			// If the required paths aren't defined yet
			if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ScriptPath) || string.IsNullOrWhiteSpace(Properties.Settings.Default.GamePath))
			{
				DialogResult result = DarkMessageBox.Show(this, Resources.Messages.PathsNotFound,
					"Paths not found!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

				if (result == DialogResult.Yes)
				{
					// Show path selection dialog
					ShowPathSelectionForm();
				}
			}
			else
			{
				// Read all files from the script folder
				ReadScriptFolder();
			}
		}

		#endregion Application launch methods

		#region Interface toggling methods

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

		private void ToggleObjectBrowser(bool state)
		{
			objectBrowser.Visible = state;
			searchTextBox.Visible = state;
			objectBrowserBox.Visible = state;
			objectBrowserSplitter.Visible = state;
			objectBrowserToolStripMenuItem.Checked = state;
		}

		private void ToggleReferenceBrowser(bool state)
		{
			referenceBrowser.Visible = state;
			refBrowserSplitter.Visible = state;
			referenceBrowserToolStripMenuItem.Checked = state;
		}

		private void ToggleDocumentMap(bool state)
		{
			documentMap.Visible = state;
			documentMapToolStripMenuItem.Checked = state;
		}

		#endregion Interface toggling methods

		#region Text editor events

		private void textEditor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) => ToolTips.CreateToolTip(textEditor, e);
		private void textEditor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void textEditor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();
		private void textEditor_KeyUp(object sender, KeyEventArgs e) => ObjectBrowser.UpdateNodes(objectBrowser, textEditor, string.Empty);

		private void textEditor_MouseDown(object sender, MouseEventArgs e)
		{
			// If the user right clicked somewhere in the editor and didn't click on the selection
			if (e.Button == MouseButtons.Right && !textEditor.Selection.Contains(textEditor.PointToPlace(e.Location)))
			{
				// Move the caret to the new position
				textEditor.Selection.Start = textEditor.PointToPlace(e.Location);
			}
		}

		private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Re-enable save buttons since the text has changed
			saveToolStripMenuItem.Enabled = true;
			saveToolStripButton.Enabled = true;

			SyntaxHighlighting.DoSyntaxHighlighting(e);
			VisibleSpaces.HandleVisibleSpaces(textEditor);

			// Redraw the editor
			textEditor.Invalidate();
		}

		private void textEditor_ZoomChanged(object sender, EventArgs e)
		{
			// If zoom increased
			if (textEditor.Zoom > gI_PrevZoom)
			{
				gI_PrevZoom += 5;
				textEditor.Zoom = gI_PrevZoom;
			}

			// If zoom decreased
			if (textEditor.Zoom < gI_PrevZoom)
			{
				gI_PrevZoom -= 5;
				textEditor.Zoom = gI_PrevZoom;
			}

			// Update the label with the current zoom value
			zoomLabel.Text = "Zoom: " + textEditor.Zoom + "%";

			// Show the "Reset" button if the zoom value is different than 100
			resetZoomButton.Visible = textEditor.Zoom != 100;

			// Limit the zoom to a minumum of 25 and a maximum of 500
			if (textEditor.Zoom < 25)
			{
				textEditor.Zoom = 25;
			}
			else if (textEditor.Zoom > 500)
			{
				textEditor.Zoom = 500;
			}
		}

		private void UndoRedoState_Changed(object sender, EventArgs e)
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

			// If there's currently a file in the editor
			if (!string.IsNullOrWhiteSpace(gS_CurrentFilePath))
			{
				string editorContent = textEditor.Text.Replace("·", " ");
				string fileContent = File.ReadAllText(gS_CurrentFilePath, Encoding.GetEncoding(1252)).Replace("·", " ");

				// If the editor content is the same as the file content
				if (editorContent == fileContent)
				{
					textEditor.IsChanged = false;

					// Disable the save buttons since there are no changes
					saveToolStripMenuItem.Enabled = false;
					saveToolStripButton.Enabled = false;
				}
			}
		}

		#endregion Text editor events

		#region Object browser events

		private void objectBrowser_Click(object sender, EventArgs e) => ObjectBrowser.GoToSelectedObject(objectBrowser, textEditor);

		private void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(searchTextBox.Text) || searchTextBox.Text == "Search...")
			{
				ObjectBrowser.UpdateNodes(objectBrowser, textEditor, string.Empty);
			}
			else
			{
				ObjectBrowser.UpdateNodes(objectBrowser, textEditor, searchTextBox.Text);
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

		#endregion Object browser events

		#region File menu items

		private void File_Change_MenuItem_Click(object sender, EventArgs e) => ShowPathSelectionForm();
		private void File_Save_MenuItem_Click(object sender, EventArgs e) => SaveFile();
		private void File_StringTable_MenuItem_Click(object sender, EventArgs e) => ShowStringTable();
		private void File_Exit_MenuItem_Click(object sender, EventArgs e) => Close();

		#endregion File menu items

		#region Edit menu items

		private void Edit_Undo_MenuItem_Click(object sender, EventArgs e) => UndoRedoHandling.HandleUndoRedo(textEditor, 0);
		private void Edit_Redo_MenuItem_Click(object sender, EventArgs e) => UndoRedoHandling.HandleUndoRedo(textEditor, 1);

		private void Edit_Cut_MenuItem_Click(object sender, EventArgs e) => ClipboardMethods.Cut(textEditor);
		private void Edit_Copy_MenuItem_Click(object sender, EventArgs e) => ClipboardMethods.Copy(textEditor);
		private void Edit_Paste_MenuItem_Click(object sender, EventArgs e) => ClipboardMethods.Paste(textEditor);

		private void Edit_Find_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowFindDialog();
		private void Edit_Replace_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowReplaceDialog();

		private void Edit_SelectAll_MenuItem_Click(object sender, EventArgs e)
		{
			textEditor.SelectAll();
			DoStatusCounting(); // So it updates the statusbar
		}

		#endregion Edit menu items

		#region Tools menu items

		private void Tools_ReindentScript_MenuItem_Click(object sender, EventArgs e) => SyntaxTidying.TidyScript(textEditor);
		private void Tools_TrimWhitespace_MenuItem_Click(object sender, EventArgs e) => SyntaxTidying.TidyScript(textEditor, true);

		private void Tools_CommentLines_MenuItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void Tools_Uncomment_MenuItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void Tools_ToggleBookmark_MenuItem_Click(object sender, EventArgs e) => Bookmarks.ToggleBookmark(textEditor);
		private void Tools_NextBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void Tools_PrevBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void Tools_ClearAllBookmarks_MenuItem_Click(object sender, EventArgs e) => Bookmarks.ClearAllBookmarks(textEditor);

		private void Tools_Settings_MenuItem_Click(object sender, EventArgs e) => OpenSettingsForm();

		#endregion Tools menu items

		#region View menu items

		private void View_ObjectBrowser_MenuItem_Click(object sender, EventArgs e)
		{
			ToggleObjectBrowser(!objectBrowser.Visible);
			Properties.Settings.Default.ObjBrowserVisible = objectBrowser.Visible;
			Properties.Settings.Default.Save();
		}

		private void View_ReferenceBrowser_MenuItem_Click(object sender, EventArgs e)
		{
			ToggleReferenceBrowser(!referenceBrowser.Visible);
			Properties.Settings.Default.RefBrowserVisible = referenceBrowser.Visible;
			Properties.Settings.Default.Save();
		}

		private void View_DocumentMap_MenuItem_Click(object sender, EventArgs e)
		{
			ToggleDocumentMap(!documentMap.Visible);
			Properties.Settings.Default.DocMapVisible = documentMap.Visible;
			Properties.Settings.Default.Save();
		}

		#endregion View menu items

		#region Help menu items

		private void Help_About_MenuItem_Click(object sender, EventArgs e) => ShowAboutForm();

		#endregion Help menu items

		#region Context menu items

		private void ContextMenu_CutItem_Click(object sender, EventArgs e) => ClipboardMethods.Cut(textEditor);
		private void ContextMenu_CopyItem_Click(object sender, EventArgs e) => ClipboardMethods.Copy(textEditor);
		private void ContextMenu_PasteItem_Click(object sender, EventArgs e) => ClipboardMethods.Paste(textEditor);

		private void ContextMenu_CommentItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ContextMenu_UncommentItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => Bookmarks.ToggleBookmark(textEditor);

		#endregion Context menu items

		#region ToolStrip buttons

		private void ToolStrip_ChangeButton_Click(object sender, EventArgs e) => ShowPathSelectionForm();
		private void ToolStrip_SaveButton_Click(object sender, EventArgs e) => SaveFile();

		private void ToolStrip_UndoButton_Click(object sender, EventArgs e) => UndoRedoHandling.HandleUndoRedo(textEditor, 0);
		private void ToolStrip_RedoButton_Click(object sender, EventArgs e) => UndoRedoHandling.HandleUndoRedo(textEditor, 1);

		private void ToolStrip_CutButton_Click(object sender, EventArgs e) => ClipboardMethods.Cut(textEditor);
		private void ToolStrip_CopyButton_Click(object sender, EventArgs e) => ClipboardMethods.Copy(textEditor);
		private void ToolStrip_PasteButton_Click(object sender, EventArgs e) => ClipboardMethods.Paste(textEditor);

		private void ToolStrip_CommentButton_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ToolStrip_UncommentButton_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ToolStrip_ToggleBookmarkButton_Click(object sender, EventArgs e) => Bookmarks.ToggleBookmark(textEditor);
		private void ToolStrip_PrevBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_NextBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_ClearAllBookmarksButton_Click(object sender, EventArgs e) => Bookmarks.ClearAllBookmarks(textEditor);

		private void ToolStrip_AboutButton_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ToolStrip_StringTableButton_Click(object sender, EventArgs e) => ShowStringTable();

		#endregion ToolStrip buttons

		#region StatusStrip elements

		private void StatusStrip_ResetZoomButton_Click(object sender, EventArgs e)
		{
			// Reset zoom values
			gI_PrevZoom = 100;
			textEditor.Zoom = 100;

			resetZoomButton.Visible = false;
		}

		private void DoStatusCounting()
		{
			lineNumberLabel.Text = "Line: " + (textEditor.Selection.Start.iLine + 1); // + 1 since it counts from 0.
			colNumberLabel.Text = "Column: " + textEditor.Selection.Start.iChar;
			selectedCharsLabel.Text = "Selected: " + textEditor.SelectedText.Length;
		}

		#endregion StatusStrip elements

		#region File handling

		public void ReadScriptFolder()
		{
			try
			{
				string path = Properties.Settings.Default.ScriptPath;

				// Get files from the selected directory
				string[] files = Directory.GetFiles(path);
				bool folderHasScriptFile = false;

				foreach (string file in files)
				{
					// If the file is not a text file
					if (!file.ToLower().EndsWith(".txt"))
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

				// If no script file was found
				if (!folderHasScriptFile)
				{
					DialogResult result = DarkMessageBox.Show(this,
						Resources.Messages.NoScriptFile, "Script file not found.",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						// Create SCRIPT.txt from template
						string filePath = path + "\\SCRIPT.txt";
						File.Copy("ScriptTemplate.txt", filePath);

						OpenFile(filePath);
					}
					else if (result == DialogResult.No)
					{
						textEditor.Clear();
						ObjectBrowser.UpdateNodes(objectBrowser, textEditor, string.Empty);
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
				// "Copy" the file contents into a string
				string fileContent = File.ReadAllText(filePath, Encoding.GetEncoding(1252));

				// Replace all spaces with dots if the user wants to have visible spaces
				if (Properties.Settings.Default.ShowSpaces && fileContent.Contains(" "))
				{
					fileContent = fileContent.Replace(" ", "·");
				}

				// "Paste" the file contents into the textEditor
				textEditor.Text = fileContent;

				// Opening the file has changed the text, so set it back to false
				textEditor.IsChanged = false;
				textEditor.ClearUndo();

				// Enable the interface since we got a file we can edit
				ToggleInterface(true);

				// Store the file (with it's path) and open it in the editor
				gS_CurrentFilePath = filePath;

				// Disable "Save" buttons since we've just opened the file
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				// Update the object browser with objects (if they exist)
				ObjectBrowser.UpdateNodes(objectBrowser, textEditor, string.Empty);

				// If Autosave is enabled
				if (Properties.Settings.Default.AutosaveTime != 0)
				{
					StartAutosaveTimer();
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				UnloadCurrentFile();
			}
		}

		private void UnloadCurrentFile()
		{
			// Unload the file
			gS_CurrentFilePath = string.Empty;

			// Clear the editor
			textEditor.Clear();
			textEditor.IsChanged = false;

			ObjectBrowser.ResetNodes(objectBrowser);

			// Disable the interface
			ToggleInterface(false);
		}

		#endregion File handling

		#region File saving

		private bool IsFileSaved()
		{
			// If the current file has unsaved changes
			if (textEditor.IsChanged)
			{
				DialogResult result = DarkMessageBox.Show(this,
					string.Format(Resources.Messages.UnsavedChanges, Path.GetFileName(gS_CurrentFilePath)), "Unsaved changes!",
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
			// If "Reindent on Save" is enabled
			if (Properties.Settings.Default.ReindentOnSave)
			{
				SyntaxTidying.TidyScript(textEditor);
			}

			try
			{
				// Replace the dots with spaces
				string editorContent = textEditor.Text.Replace("·", " ");

				// Save to file
				File.WriteAllText(gS_CurrentFilePath, editorContent, Encoding.GetEncoding(1252));
				textEditor.IsChanged = false;

				// Disable "Save" buttons since we've just saved
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				// Redraw the editor
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

		private void StartAutosaveTimer()
		{
			var autosaveTimer = new System.Timers.Timer(); // I don't like using vars but ¯\_(ツ)_/¯
			autosaveTimer.Elapsed += autosaveTimer_Elapsed;
			autosaveTimer.Interval = Properties.Settings.Default.AutosaveTime * (60 * 1000);
			autosaveTimer.Enabled = true; // Start the timer
		}

		private void autosaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string currentFolder = Path.GetDirectoryName(gS_CurrentFilePath);
			string currentFileName = Path.GetFileNameWithoutExtension(gS_CurrentFilePath);
			string autosaveFilePath = currentFolder + "\\" + currentFileName + ".autosave";

			try
			{
				// Replace the dots with spaces
				string editorContent = textEditor.Text.Replace("·", " ");

				// Save to file
				File.WriteAllText(autosaveFilePath, editorContent, Encoding.GetEncoding(1252));
				autosaveLabel.Invoke(new Action(() => autosaveLabel_Show(true)));
			}
			catch (Exception)
			{
				autosaveLabel.Invoke(new Action(() => autosaveLabel_Show(false)));
			}
		}

		private void autosaveLabel_Show(bool success)
		{
			if (success)
			{
				string currentTime = DateTime.Now.TimeOfDay.ToString().Substring(0, 5);
				autosaveLabel.Text = "Autosave Completed! (" + currentTime + ")";
				autosaveLabel.Visible = true;
			}
			else
			{
				autosaveLabel.Text = "ERROR: Autosave Failed!";
				autosaveLabel.Visible = true;
			}
		}

		#endregion File saving

		#region Other forms

		private void ShowAboutForm()
		{
			using (FormAbout form = new FormAbout())
			{
				form.ShowDialog(this);
			}
		}

		private void ShowPathSelectionForm()
		{
			// If the current file has unsaved changes
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

		private void ShowStringTable()
		{
			using (FormStringTable form = new FormStringTable())
			{
				form.ShowDialog(this);
			}
		}

		private void OpenSettingsForm()
		{
			// Cache critical settings
			int autosaveCache = Properties.Settings.Default.AutosaveTime;
			bool showSpacesCache = Properties.Settings.Default.ShowSpaces;
			bool autocompleteCache = Properties.Settings.Default.Autocomplete;
			bool toolTipCache = Properties.Settings.Default.ToolTips;

			using (FormSettings form = new FormSettings())
			{
				// If the user pressed "Cancel"
				if (form.ShowDialog(this) == DialogResult.Cancel)
				{
					return;
				}

				ApplyUserSettings();

				if (form.gI_RestartItemCount > 0) // GLOBALZZZ NUUUU THIZ IZ BED !!! Welp, find a better solution then. :P
				{
					// If the current file has unsaved changes
					if (!IsFileSaved())
					{
						// Saving failed or the user clicked "Cancel"
						// Therefore restore the previous critical settings
						Properties.Settings.Default.AutosaveTime = autosaveCache;
						Properties.Settings.Default.ShowSpaces = showSpacesCache;
						Properties.Settings.Default.Autocomplete = autocompleteCache;
						Properties.Settings.Default.ToolTips = toolTipCache;

						Properties.Settings.Default.Save();
						OpenSettingsForm();
						return; // DO NOT CLOSE THE APP !!! (ﾉ°Д°)ﾉ︵﻿ ┻━┻
					}

					textEditor.IsChanged = false; // Prevent re-check of IsFileSaved()
					Application.Restart();
				}
			}
		}

		#endregion Other forms
	}
}
