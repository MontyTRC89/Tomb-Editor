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
		#region Global variables

		/// <summary>
		/// Store the currently edited file name and path.
		/// </summary>
		private string gS_CurrentFilePath;

		/// <summary>
		/// Store the previous zoom value to fix a strange FCTB bug.
		/// </summary>
		private int gI_PrevZoom = 100;

		#endregion Global variables

		#region Constructors

		public FormMain()
		{
			InitializeComponent();

			// Disable the interface since no file has been loaded yet
			ToggleInterface(false);

			// If Autocomplete is enabled in the settings
			if (Properties.Settings.Default.Autocomplete)
			{
				GenerateAutocompleteMenu();
			}

			// Apply saved user settings
			ApplyUserSettings();
		}

		#endregion Constructors

		#region Form actions

		private void FormMain_Shown(object sender, EventArgs e)
		{
			CheckRequiredPaths(); // Check if required paths are set

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

		private void CheckRequiredPaths()
		{
			// If the required paths aren't defined yet (script folder and game folder)
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

		#endregion Application launch methods

		#region Interface methods

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

		#endregion Interface methods

		#region Editor events

		private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Re-enable save buttons since the text has changed
			saveToolStripMenuItem.Enabled = true;
			saveToolStripButton.Enabled = true;

			DoSyntaxHighlighting(e);

			// If "Show Spaces" is enabled and the text contains spaces
			if (Properties.Settings.Default.ShowSpaces && textEditor.Text.Contains(" "))
			{
				// Cache caret position
				Place caretPosition = textEditor.Selection.Start;

				// Scan all lines
				for (int i = 0; i < textEditor.LinesCount; i++)
				{
					// If a line contains whitespace
					if (textEditor.GetLineText(i).Contains(" "))
					{
						textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
						textEditor.InsertText(textEditor.GetLineText(i).Replace(" ", "·"));
					}
				}

				// Restore caret position
				textEditor.Selection.Start = caretPosition;
				textEditor.Invalidate();
			}
			else if (!Properties.Settings.Default.ShowSpaces && textEditor.Text.Contains("·")) // Too lazy to somehow merge these 2 "if" statements...
			{
				// Cache caret position
				Place caretPosition = textEditor.Selection.Start;

				// Scan all lines
				for (int i = 0; i < textEditor.LinesCount; i++)
				{
					// If a line contains a "whitespace dot"
					if (textEditor.GetLineText(i).Contains("·"))
					{
						textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
						textEditor.InsertText(textEditor.GetLineText(i).Replace("·", " "));
					}
				}

				// Restore caret position
				textEditor.Selection.Start = caretPosition;
				textEditor.Invalidate();
			}
		}

		private void textEditor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void textEditor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) => HandleToolTips(e);
		private void textEditor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();
		private void textEditor_KeyUp(object sender, KeyEventArgs e) => UpdateObjectBrowser(string.Empty);

		private void textEditor_MouseDown(object sender, MouseEventArgs e)
		{
			// If the user right clicked somewhere in the editor and didn't click on the selection
			if (e.Button == MouseButtons.Right && !textEditor.Selection.Contains(textEditor.PointToPlace(e.Location)))
			{
				// Move the caret to the new position
				textEditor.Selection.Start = textEditor.PointToPlace(e.Location);
			}
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

			// Update the label
			zoomLabel.Text = "Zoom: " + textEditor.Zoom + "%";

			resetZoomButton.Visible = textEditor.Zoom != 100;

			// Limit the zoom
			if (textEditor.Zoom > 500)
			{
				textEditor.Zoom = 500;
			}
			else if (textEditor.Zoom < 25)
			{
				textEditor.Zoom = 25;
			}
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

		#endregion Editor events

		#region File menu items

		private void File_Change_MenuItem_Click(object sender, EventArgs e) => ShowPathSelectionForm();
		private void File_Save_MenuItem_Click(object sender, EventArgs e) => SaveFile();
		private void File_StringTable_MenuItem_Click(object sender, EventArgs e) => ShowStringTable();
		private void File_Exit_MenuItem_Click(object sender, EventArgs e) => Close();

		#endregion File menu items

		#region Edit menu items

		private void Edit_Undo_MenuItem_Click(object sender, EventArgs e) => HandleUndoRedo(0);
		private void Edit_Redo_MenuItem_Click(object sender, EventArgs e) => HandleUndoRedo(1);

		private void Edit_Cut_MenuItem_Click(object sender, EventArgs e) => CutToClipboard();
		private void Edit_Copy_MenuItem_Click(object sender, EventArgs e) => CopyToClipboard();
		private void Edit_Paste_MenuItem_Click(object sender, EventArgs e) => PasteFromClipboard();

		private void Edit_Find_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowFindDialog();
		private void Edit_Replace_MenuItem_Click(object sender, EventArgs e) => textEditor.ShowReplaceDialog();

		private void Edit_SelectAll_MenuItem_Click(object sender, EventArgs e)
		{
			textEditor.SelectAll();
			DoStatusCounting(); // So it updates the statusbar
		}

		#endregion Edit menu items

		#region Tools menu items

		private void Tools_ReindentScript_MenuItem_Click(object sender, EventArgs e) => TidyScript();
		private void Tools_TrimWhitespace_MenuItem_Click(object sender, EventArgs e) => TidyScript(true);

		private void Tools_CommentLines_MenuItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void Tools_Uncomment_MenuItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void Tools_ToggleBookmark_MenuItem_Click(object sender, EventArgs e) => ToggleBookmark();
		private void Tools_NextBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void Tools_PrevBookmark_MenuItem_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void Tools_ClearAllBookmarks_MenuItem_Click(object sender, EventArgs e) => ClearAllBookmarks();

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

		private void ContextMenu_CutItem_Click(object sender, EventArgs e) => CutToClipboard();
		private void ContextMenu_CopyItem_Click(object sender, EventArgs e) => CopyToClipboard();
		private void ContextMenu_PasteItem_Click(object sender, EventArgs e) => PasteFromClipboard();

		private void ContextMenu_CommentItem_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ContextMenu_UncommentItem_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();

		#endregion Context menu items

		#region ToolStrip buttons

		private void ToolStrip_ChangeButton_Click(object sender, EventArgs e) => ShowPathSelectionForm();
		private void ToolStrip_SaveButton_Click(object sender, EventArgs e) => SaveFile();

		private void ToolStrip_UndoButton_Click(object sender, EventArgs e) => HandleUndoRedo(0);
		private void ToolStrip_RedoButton_Click(object sender, EventArgs e) => HandleUndoRedo(1);

		private void ToolStrip_CutButton_Click(object sender, EventArgs e) => CutToClipboard();
		private void ToolStrip_CopyButton_Click(object sender, EventArgs e) => CopyToClipboard();
		private void ToolStrip_PasteButton_Click(object sender, EventArgs e) => PasteFromClipboard();

		private void ToolStrip_CommentButton_Click(object sender, EventArgs e) => textEditor.InsertLinePrefix(";");
		private void ToolStrip_UncommentButton_Click(object sender, EventArgs e) => textEditor.RemoveLinePrefix(";");

		private void ToolStrip_ToggleBookmarkButton_Click(object sender, EventArgs e) => ToggleBookmark();
		private void ToolStrip_PrevBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoPrevBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_NextBookmarkButton_Click(object sender, EventArgs e) => textEditor.GotoNextBookmark(textEditor.Selection.Start.iLine);
		private void ToolStrip_ClearAllBookmarksButton_Click(object sender, EventArgs e) => ClearAllBookmarks();

		private void ToolStrip_AboutButton_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ToolStrip_StringTableButton_Click(object sender, EventArgs e) => ShowStringTable();

		#endregion ToolStrip buttons

		#region StatusStrip buttons

		private void StatusStrip_ResetZoomButton_Click(object sender, EventArgs e)
		{
			// Reset zoom values
			gI_PrevZoom = 100;
			textEditor.Zoom = 100;

			resetZoomButton.Visible = false;
		}

		#endregion StatusStrip buttons

		#region Undo / Redo methods

		private void HandleUndoRedo(int index) // 0 - Undo, 1 - Redo
		{
			string editorContent = string.Empty;

			// If "Show Spaces" is enabled
			if (Properties.Settings.Default.ShowSpaces)
			{
				TriggerUndoRedo(index);

				while (textEditor.Text.Contains(" "))
				{
					TriggerUndoRedo(index);
				}

				editorContent = textEditor.Text.Replace("·", " ");
			}
			else
			{
				TriggerUndoRedo(index);
				editorContent = textEditor.Text;
			}

			// If the editor content is the same as the file content
			if (editorContent == File.ReadAllText(gS_CurrentFilePath))
			{
				textEditor.IsChanged = false;
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;
			}
		}

		private void TriggerUndoRedo(int index)
		{
			if (index == 0)
			{
				textEditor.Undo();
			}
			else if (index == 1)
			{
				textEditor.Redo();
			}
		}

		#endregion Undo / Redo methods

		#region Clipboard methods

		private void CutToClipboard()
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
				textEditor.SelectedText = string.Empty;
			}
		}

		private void CopyToClipboard()
		{
			if (!string.IsNullOrEmpty(textEditor.SelectedText))
			{
				Clipboard.SetText(textEditor.SelectedText.Replace("·", " "));
			}
		}

		private void PasteFromClipboard()
		{
			textEditor.SelectedText = Clipboard.GetText();
		}

		#endregion Clipboard methods

		#region ToolTips handling

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

			// Get resources from HeaderToolTips.resx
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
			// Get resources from CommandToolTips.resx
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

		#endregion ToolTips handling

		#region Object browser handling

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
			if (string.IsNullOrWhiteSpace(objectBrowser.SelectedNodes[0].Text))
			{
				return;
			}

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// Find the line that contains the node text
				if (textEditor.GetLineText(i).ToLower().Replace("·", " ").Contains(objectBrowser.SelectedNodes[0].Text.ToLower()))
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
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Cache the current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			ResetObjectBrowserNodes();

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				AddObjectBrowserNodes(i, filter);
			}

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Set the previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		private void AddObjectBrowserNodes(int lineNumber, string filter)
		{
			AddLevelNodes(lineNumber, filter);
			AddHeaderNodes(lineNumber, filter);
		}

		private void AddLevelNodes(int lineNumber, string filter)
		{
			// Regex rule to find lines that start with "Name = "
			Regex rgx = new Regex(@"\bName[\s·]?=[\s·]?");

			// If we found a line that starts with our Regex rule ("Name = ")
			if (rgx.IsMatch(textEditor.GetLineText(lineNumber)))
			{
				// Create a new node, remove "Name = ", replace dots with spaces and trim it, so we only get the level name string
				DarkTreeNode levelNode = new DarkTreeNode(rgx.Replace(textEditor.GetLineText(lineNumber), string.Empty).Replace("·", " ").Trim());

				// If the level name matches the filter (It always does if there's nothing in the search bar)
				if (levelNode.Text.ToLower().Contains(filter.ToLower()))
				{
					objectBrowser.Nodes[1].Nodes.Add(levelNode);
				}
			}
		}

		private void AddHeaderNodes(int lineNumber, string filter)
		{
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

		private void ResetObjectBrowserNodes()
		{
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Cache the current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			objectBrowser.Nodes.Clear();
			objectBrowser.Nodes.Add(new DarkTreeNode("Headers"));
			objectBrowser.Nodes.Add(new DarkTreeNode("Levels"));

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Set the previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		#endregion Object browser handling

		#region Styles and status

		private void DoSyntaxHighlighting(TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
				SyntaxColors.Comments, SyntaxColors.Regular, SyntaxColors.References, SyntaxColors.Values,
				SyntaxColors.Headers, SyntaxColors.NewCommands, SyntaxColors.OldCommands, SyntaxColors.Unknown);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(SyntaxColors.Whitespace, "·");
			e.ChangedRange.SetStyle(SyntaxColors.Comments, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Regular, @"[\[\],=]");
			e.ChangedRange.SetStyle(SyntaxColors.References, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(SyntaxColors.Values, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Headers, @"\[(" + string.Join("|", SyntaxKeyWords.Headers()) + @")\]");
			e.ChangedRange.SetStyle(SyntaxColors.NewCommands, @"\b(" + string.Join(@"[\s·]?=[\s·]?|", SyntaxKeyWords.NewCommands()) + ")");
			e.ChangedRange.SetStyle(SyntaxColors.OldCommands, @"\b(" + string.Join(@"[\s·]?=[\s·]?|", SyntaxKeyWords.OldCommands()) + ")");
			e.ChangedRange.SetStyle(SyntaxColors.Unknown, @"\b(" + string.Join(@"[\s·]?=[\s·]?|", SyntaxKeyWords.Unknown()) + ")");
		}

		private void DoStatusCounting()
		{
			lineNumberLabel.Text = "Line: " + (textEditor.Selection.Start.iLine + 1); // + 1 since it counts from 0.
			colNumberLabel.Text = "Column: " + textEditor.Selection.Start.iChar;
			selectedCharsLabel.Text = "Selected: " + textEditor.SelectedText.Length;
		}

		#endregion Styles and status

		#region Syntax tidy methods

		private void TidyScript(bool trimOnly = false)
		{
			// Start AutoUndo to allow the user to undo the tidying process using only a single stack
			textEditor.BeginAutoUndo();

			// Save set bookmarks and remove them from the editor to prevent a bug
			int[] bookmarkLines = GetBookmarkedLines();
			textEditor.Bookmarks.Clear();

			// Store current scroll position
			int scrollPosition = textEditor.VerticalScroll.Value;

			// Store the editor content in a string and replace the "whitespace dots" (if there are any) with whitespace
			string editorContent = textEditor.Text.Replace("·", " ");

			// Setup a list to store all tidied lines
			List<string> tidiedlines = trimOnly ? SyntaxTidy.TrimLines(editorContent) : SyntaxTidy.ReindentLines(editorContent);

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// Also check if the user has "Show Spaces" enabled
				string currentTidiedLine = Properties.Settings.Default.ShowSpaces ? tidiedlines[i].Replace(" ", "·") : tidiedlines[i];

				// If a line has changed
				if (textEditor.GetLineText(i) != currentTidiedLine)
				{
					textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
					textEditor.InsertText(tidiedlines[i]);
				}
			}

			// Go to the last scroll position
			textEditor.VerticalScroll.Value = scrollPosition;
			textEditor.UpdateScrollbars();

			// Add lost bookmarks
			foreach (int line in bookmarkLines)
			{
				textEditor.BookmarkLine(line);
			}

			textEditor.Invalidate();

			// End AutoUndo to stop recording the actions and put them into a single stack
			textEditor.EndAutoUndo();
		}

		#endregion Syntax tidy methods

		#region Bookmark methods

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

		#endregion Bookmark methods

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
				// "Copy" the file contents into a string
				string fileContent = File.ReadAllText(filePath, Encoding.GetEncoding(1252));

				// Replace all spaces with a dot if the user wants to have visible spaces
				if (Properties.Settings.Default.ShowSpaces && fileContent.Contains(" "))
				{
					fileContent = fileContent.Replace(" ", "·");
				}

				// "Paste" the file contents into the textEditor
				textEditor.Text = fileContent;

				textEditor.IsChanged = false; // Opening the file has changed the text, so set it back to false
				textEditor.ClearUndo();

				ToggleInterface(true); // Enable the interface since we got a file we can edit

				// Store the file (with it's path) and open it in the editor
				gS_CurrentFilePath = filePath;

				// Disable "Save" buttons since we've just opened the file
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				// Update the object browser with levels (if they exist)
				UpdateObjectBrowser(string.Empty);

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

			ResetObjectBrowserNodes();

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
				TidyScript();
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
