using DarkUI.Controls;
using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using TombLib.Script;

namespace ScriptEditor
{
	public partial class FormMain : DarkForm
	{
		/// <summary>
		/// A list of all available projects.
		/// </summary>
		private List<Project> _availableProjects;

		/// <summary>
		/// Stores the currently edited project.
		/// </summary>
		private Project _currentProject;

		/// <summary>
		/// Stores the previous zoom value to fix a strange FCTB bug.
		/// </summary>
		private int _prevZoom = 100;

		#region Initialization

		public FormMain()
		{
			InitializeComponent();
			_availableProjects = new List<Project>();

			if (Properties.Settings.Default.Autocomplete)
				GenerateAutocompleteMenu();
		}

		protected override void OnShown(EventArgs e)
		{
			ToggleInterface(false);
			ApplyUserSettings();
			GetAvailableProjects(); // From Projects.xml
			OpenLastProject();

			base.OnShown(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!IsFileSaved()) // If current file has unsaved changes
				e.Cancel = true; // Stop closing form if saving failed or the user clicked "Cancel"

			base.OnClosing(e);
		}

		private void GenerateAutocompleteMenu()
		{
			AutocompleteMenu popupMenu = new AutocompleteMenu(scriptEditor)
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
			scriptEditor.Font = new Font(Properties.Settings.Default.FontFace, Convert.ToSingle(Properties.Settings.Default.FontSize));
			scriptEditor.AutoCompleteBrackets = Properties.Settings.Default.CloseBrackets;
			scriptEditor.WordWrap = Properties.Settings.Default.WordWrap;
			scriptEditor.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;
			toolStrip.Visible = Properties.Settings.Default.ShowToolbar;

			ToggleObjectBrowser(Properties.Settings.Default.ObjBrowserVisible);
			ToggleReferenceBrowser(Properties.Settings.Default.RefBrowserVisible);
			ToggleDocumentMap(Properties.Settings.Default.DocMapVisible);
		}

		private void GetAvailableProjects()
		{
			try
			{
				using (StreamReader stream = new StreamReader("Projects.xml")) // Read Projects.xml
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
					_availableProjects = (List<Project>)serializer.Deserialize(stream);
				}

				projectsToolStripMenuItem.DropDownItems.Clear();

				foreach (Project project in _availableProjects) // Add all projects to the "Projects" DropDown
					projectsToolStripMenuItem.DropDownItems.Add(project.Name);
			}
			catch (IOException) // Projects.xml doesn't exist
			{
				using (StreamWriter stream = new StreamWriter("Projects.xml")) // Create a new Projects.xml file
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
					serializer.Serialize(stream, new List<Project>());
				}

				Properties.Settings.Default.LastProject = string.Empty; // Reset this because we just created a new Projects.xml file
				Properties.Settings.Default.Save();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenLastProject()
		{
			if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastProject))
			{
				foreach (Project proj in _availableProjects)
				{
					if (proj.Name == Properties.Settings.Default.LastProject)
						_currentProject = proj;
				}

				currentProjectLabel.Text = _currentProject.Name;
				OpenProjectFiles();
			}
			else // This should only happen when the user never added a project or deleted Projects.xml
				ShowProjectSetupForm();
		}

		#endregion Initialization

		#region Object events

		private void textEditor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) => CreateToolTip(e);
		private void textEditor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void textEditor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();
		private void textEditor_KeyUp(object sender, KeyEventArgs e) => UpdateObjectBrowserNodes();

		private void textEditor_MouseDown(object sender, MouseEventArgs e)
		{
			// If user right clicked somewhere in the editor but didn't click on selected text
			if (e.Button == MouseButtons.Right && !scriptEditor.Selection.Contains(scriptEditor.PointToPlace(e.Location)))
				scriptEditor.Selection.Start = scriptEditor.PointToPlace(e.Location); // Move caret to the new position
		}

		private void textEditor_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Re-enable save buttons since the text has changed
			saveToolStripMenuItem.Enabled = true;
			saveToolStripButton.Enabled = true;

			SyntaxHighlighting.DoSyntaxHighlighting(e);
			HandleVisibleSpaces();

			scriptEditor.Invalidate();
		}

		private void textEditor_ZoomChanged(object sender, EventArgs e)
		{
			if (scriptEditor.Zoom > _prevZoom) // If zoom increased
			{
				_prevZoom += 5;
				scriptEditor.Zoom = _prevZoom;
			}

			if (scriptEditor.Zoom < _prevZoom) // If zoom decreased
			{
				_prevZoom -= 5;
				scriptEditor.Zoom = _prevZoom;
			}

			zoomLabel.Text = "Zoom: " + scriptEditor.Zoom + "%";
			resetZoomButton.Visible = scriptEditor.Zoom != 100;

			// Limit zoom to a minumum of 25 and a maximum of 500
			if (scriptEditor.Zoom < 25)
				scriptEditor.Zoom = 25;
			else if (scriptEditor.Zoom > 500)
				scriptEditor.Zoom = 500;
		}

		private void UndoRedoState_Changed(object sender, EventArgs e)
		{
			if (scriptEditor.UndoEnabled)
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

			if (scriptEditor.RedoEnabled)
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

			if (_currentProject != null) // Just in case...
			{
				string scriptFilePath = _currentProject.GamePath + @"\Script\script.txt";

				string editorContent = scriptEditor.Text.Replace("·", " ");
				string fileContent = File.ReadAllText(scriptFilePath, Encoding.GetEncoding(1252)).Replace("·", " ");

				if (editorContent == fileContent)
				{
					scriptEditor.IsChanged = false;

					// Disable save buttons since there are no changes
					saveToolStripMenuItem.Enabled = false;
					saveToolStripButton.Enabled = false;
				}
			}
		}

		private void objectBrowser_Click(object sender, EventArgs e) => GoToSelectedObject();

		private void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(searchTextBox.Text) || searchTextBox.Text == "Search...")
				UpdateObjectBrowserNodes();
			else
				UpdateObjectBrowserNodes(searchTextBox.Text);
		}

		private void searchTextBox_GotFocus(object sender, EventArgs e)
		{
			if (searchTextBox.Text == "Search...")
				searchTextBox.Text = string.Empty;
		}

		private void searchTextBox_LostFocus(object sender, EventArgs e)
		{
			if (searchTextBox.Text == string.Empty)
				searchTextBox.Text = "Search...";
		}

		private void File_NewProject_MenuItem_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void File_Save_MenuItem_Click(object sender, EventArgs e) => SaveFile();
		private void File_Build_MenuItem_Click(object sender, EventArgs e) => BuildScript();
		private void File_StringTable_MenuItem_Click(object sender, EventArgs e) => ShowStringTable();
		private void File_Exit_MenuItem_Click(object sender, EventArgs e) => Close();

		private void Edit_Undo_MenuItem_Click(object sender, EventArgs e) => HandleUndo();
		private void Edit_Redo_MenuItem_Click(object sender, EventArgs e) => HandleRedo();
		private void Edit_Cut_MenuItem_Click(object sender, EventArgs e) => Cut();
		private void Edit_Copy_MenuItem_Click(object sender, EventArgs e) => Copy();
		private void Edit_Paste_MenuItem_Click(object sender, EventArgs e) => Paste();
		private void Edit_Find_MenuItem_Click(object sender, EventArgs e) => scriptEditor.ShowFindDialog();
		private void Edit_Replace_MenuItem_Click(object sender, EventArgs e) => scriptEditor.ShowReplaceDialog();

		private void Edit_SelectAll_MenuItem_Click(object sender, EventArgs e)
		{
			scriptEditor.SelectAll();
			DoStatusCounting(); // So it updates the statusbar
		}

		private void Tools_ReindentScript_MenuItem_Click(object sender, EventArgs e) => TidyScript();
		private void Tools_TrimWhitespace_MenuItem_Click(object sender, EventArgs e) => TidyScript(true);
		private void Tools_CommentLines_MenuItem_Click(object sender, EventArgs e) => scriptEditor.InsertLinePrefix(";");
		private void Tools_Uncomment_MenuItem_Click(object sender, EventArgs e) => scriptEditor.RemoveLinePrefix(";");
		private void Tools_ToggleBookmark_MenuItem_Click(object sender, EventArgs e) => ToggleBookmark();
		private void Tools_NextBookmark_MenuItem_Click(object sender, EventArgs e) => scriptEditor.GotoNextBookmark(scriptEditor.Selection.Start.iLine);
		private void Tools_PrevBookmark_MenuItem_Click(object sender, EventArgs e) => scriptEditor.GotoPrevBookmark(scriptEditor.Selection.Start.iLine);
		private void Tools_ClearAllBookmarks_MenuItem_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void Tools_Settings_MenuItem_Click(object sender, EventArgs e) => ShowSettingsForm();

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

		private void Help_About_MenuItem_Click(object sender, EventArgs e) => ShowAboutForm();

		private void Projects_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) => OpenSelectedDropDownProject(e);

		private void ContextMenu_CutItem_Click(object sender, EventArgs e) => Cut();
		private void ContextMenu_CopyItem_Click(object sender, EventArgs e) => Copy();
		private void ContextMenu_PasteItem_Click(object sender, EventArgs e) => Paste();
		private void ContextMenu_CommentItem_Click(object sender, EventArgs e) => scriptEditor.InsertLinePrefix(";");
		private void ContextMenu_UncommentItem_Click(object sender, EventArgs e) => scriptEditor.RemoveLinePrefix(";");
		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();

		private void ToolStrip_NewProjectButton_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void ToolStrip_SaveButton_Click(object sender, EventArgs e) => SaveFile();
		private void ToolStrip_BuildButton_Click(object sender, EventArgs e) => BuildScript();
		private void ToolStrip_UndoButton_Click(object sender, EventArgs e) => HandleUndo();
		private void ToolStrip_RedoButton_Click(object sender, EventArgs e) => HandleRedo();
		private void ToolStrip_CutButton_Click(object sender, EventArgs e) => Cut();
		private void ToolStrip_CopyButton_Click(object sender, EventArgs e) => Copy();
		private void ToolStrip_PasteButton_Click(object sender, EventArgs e) => Paste();
		private void ToolStrip_CommentButton_Click(object sender, EventArgs e) => scriptEditor.InsertLinePrefix(";");
		private void ToolStrip_UncommentButton_Click(object sender, EventArgs e) => scriptEditor.RemoveLinePrefix(";");
		private void ToolStrip_ToggleBookmarkButton_Click(object sender, EventArgs e) => ToggleBookmark();
		private void ToolStrip_PrevBookmarkButton_Click(object sender, EventArgs e) => scriptEditor.GotoPrevBookmark(scriptEditor.Selection.Start.iLine);
		private void ToolStrip_NextBookmarkButton_Click(object sender, EventArgs e) => scriptEditor.GotoNextBookmark(scriptEditor.Selection.Start.iLine);
		private void ToolStrip_ClearAllBookmarksButton_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void ToolStrip_AboutButton_Click(object sender, EventArgs e) => ShowAboutForm();
		private void ToolStrip_StringTableButton_Click(object sender, EventArgs e) => ShowStringTable();

		private void StatusStrip_ResetZoomButton_Click(object sender, EventArgs e)
		{
			_prevZoom = 100;
			scriptEditor.Zoom = 100;

			resetZoomButton.Visible = false;
		}

		#endregion Object events

		#region Object event methods

		private void ToggleInterface(bool state)
		{
			// Toggle editor
			scriptEditor.Enabled = state;

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

		private void CreateToolTip(ToolTipNeededEventArgs e)
		{
			if (Properties.Settings.Default.ToolTips && !string.IsNullOrWhiteSpace(e.HoveredWord))
			{
				if (scriptEditor.GetLineText(e.Place.iLine).StartsWith("["))
					ShowHeaderToolTip(e);
				else
					ShowCommandToolTip(e);
			}
		}

		private void ShowHeaderToolTip(ToolTipNeededEventArgs e)
		{
			e.ToolTipTitle = "[" + e.HoveredWord + "]";

			// Get resources from HeaderToolTips.resx
			ResourceManager headerToolTipResource = new ResourceManager(typeof(Resources.HeaderToolTips));
			ResourceSet resourceSet = headerToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			foreach (DictionaryEntry entry in resourceSet)
			{
				if (e.HoveredWord == entry.Key.ToString())
				{
					e.ToolTipText = entry.Value.ToString();
					return;
				}
			}
		}

		private void ShowCommandToolTip(ToolTipNeededEventArgs e)
		{
			// Get resources from CommandToolTips.resx
			ResourceManager commandToolTipResource = new ResourceManager(typeof(Resources.CommandToolTips));
			ResourceSet resourceSet = commandToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			if (e.HoveredWord == "Level")
				HandleLevelToolTip(e); // There are different definitions for the "Level" key, so handle them all
			else
			{
				e.ToolTipTitle = e.HoveredWord;

				foreach (DictionaryEntry entry in resourceSet)
				{
					if (e.HoveredWord == entry.Key.ToString())
					{
						e.ToolTipText = entry.Value.ToString();
						return;
					}
				}
			}
		}

		private void HandleLevelToolTip(ToolTipNeededEventArgs e)
		{
			int i = e.Place.iLine; // Current line number

			do
			{
				if (i < 0)
					return; // Line number might go to -1 and it will crash the app, so stop the loop to prevent it

				if (scriptEditor.GetLineText(i).StartsWith("[PSXExtensions]"))
				{
					e.ToolTipTitle = "Level [PSXExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPSX;
					return;
				}
				else if (scriptEditor.GetLineText(i).StartsWith("[PCExtensions]"))
				{
					e.ToolTipTitle = "Level [PCExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPC;
					return;
				}
				else if (scriptEditor.GetLineText(i).StartsWith("[Title]"))
				{
					e.ToolTipTitle = "Level [Title]";
					e.ToolTipText = Resources.CommandToolTips.LevelTitle;
					return;
				}
				else if (scriptEditor.GetLineText(i).StartsWith("[Level]"))
				{
					e.ToolTipTitle = "Level";
					e.ToolTipText = Resources.CommandToolTips.LevelLevel;
					return;
				}

				i--; // Go 1 line higher if no header was found yet
			}
			while (!scriptEditor.GetLineText(i + 1).StartsWith("["));
		}

		private void DoStatusCounting()
		{
			lineNumberLabel.Text = "Line: " + (scriptEditor.Selection.Start.iLine + 1); // + 1 since it counts from 0
			colNumberLabel.Text = "Column: " + scriptEditor.Selection.Start.iChar;
			selectedCharsLabel.Text = "Selected: " + scriptEditor.SelectedText.Length;
		}

		private void UpdateObjectBrowserNodes(string filter = "")
		{
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			if (objectBrowser.Nodes.Count == 2) // If all default nodes are set
			{
				// Cache current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			ResetObjectBrowserNodes();

			for (int i = 0; i < scriptEditor.LinesCount; i++) // Scan all lines
			{
				AddHeaderNode(i, filter);
				AddLevelNode(i, filter);
			}

			if (objectBrowser.Nodes.Count == 2) // If all default nodes are set
			{
				// Set previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		private void ResetObjectBrowserNodes()
		{
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			if (objectBrowser.Nodes.Count == 2) // If all default nodes are set
			{
				// Cache current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			objectBrowser.Nodes.Clear();
			objectBrowser.Nodes.Add(new DarkTreeNode("Headers"));
			objectBrowser.Nodes.Add(new DarkTreeNode("Levels"));

			if (objectBrowser.Nodes.Count == 2) // If all default nodes are set
			{
				// Set previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		private void GoToSelectedObject()
		{
			// If user hasn't selected any node or selected node is empty
			if (objectBrowser.SelectedNodes.Count < 1 || string.IsNullOrWhiteSpace(objectBrowser.SelectedNodes[0].Text))
				return;

			// If selected node is a default item
			if (objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[0] || objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[1])
				return;

			for (int i = 0; i < scriptEditor.LinesCount; i++) // Scan all lines
			{
				// Find line that contains the node text
				if (scriptEditor.GetLineText(i).ToLower().Replace("·", " ").Contains(objectBrowser.SelectedNodes[0].Text.ToLower()))
				{
					scriptEditor.Focus();
					scriptEditor.Navigate(i); // Scroll to the line position

					scriptEditor.Selection = new Range(scriptEditor, 0, i, scriptEditor.GetLineText(i).Length, i); // Select the line
					return;
				}
			}
		}

		private void AddHeaderNode(int lineNumber, string filter)
		{
			List<string> headers = SyntaxKeyWords.Headers();

			foreach (string header in headers)
			{
				string fullHeader = "[" + header + "]";

				// If current line starts with a header but it's not a [Level] header
				if (scriptEditor.GetLineText(lineNumber).StartsWith(fullHeader) && fullHeader != "[Level]")
				{
					DarkTreeNode headerNode = new DarkTreeNode(fullHeader);

					// If header name matches the filter (It always does if there's nothing in the search bar)
					if (headerNode.Text.ToLower().Contains(filter.ToLower()))
						objectBrowser.Nodes[0].Nodes.Add(headerNode);
				}
			}
		}

		private void AddLevelNode(int lineNumber, string filter)
		{
			Regex rgx = new Regex(@"\bName[\s·]?=[\s·]?"); // Regex rule to find lines that start with "Name = "

			if (rgx.IsMatch(scriptEditor.GetLineText(lineNumber)))
			{
				// Create a new node, remove "Name = ", replace dots with spaces and trim it, so we only get the level name string
				DarkTreeNode levelNode = new DarkTreeNode(rgx.Replace(scriptEditor.GetLineText(lineNumber), string.Empty).Replace("·", " ").Trim());

				// If level name matches the filter (It always does if there's nothing in the search bar)
				if (levelNode.Text.ToLower().Contains(filter.ToLower()))
					objectBrowser.Nodes[1].Nodes.Add(levelNode);
			}
		}

		private void HandleUndo()
		{
			if (Properties.Settings.Default.ShowSpaces)
			{
				do scriptEditor.Undo();
				while (scriptEditor.Text.Contains(" "));
			}
			else
				scriptEditor.Undo();
		}

		private void HandleRedo()
		{
			if (Properties.Settings.Default.ShowSpaces)
			{
				do scriptEditor.Redo();
				while (scriptEditor.Text.Contains(" "));
			}
			else
				scriptEditor.Redo();
		}

		private void Cut()
		{
			if (!string.IsNullOrEmpty(scriptEditor.SelectedText))
			{
				Clipboard.SetText(scriptEditor.SelectedText.Replace("·", " "));
				scriptEditor.SelectedText = string.Empty;
			}
		}

		private void Copy()
		{
			if (!string.IsNullOrEmpty(scriptEditor.SelectedText))
				Clipboard.SetText(scriptEditor.SelectedText.Replace("·", " "));
		}

		private void Paste()
		{
			scriptEditor.SelectedText = Clipboard.GetText();
		}

		private void ToggleBookmark()
		{
			int currentLine = scriptEditor.Selection.Start.iLine;

			if (scriptEditor.Bookmarks.Contains(currentLine))
				scriptEditor.Bookmarks.Remove(currentLine);
			else
				scriptEditor.Bookmarks.Add(currentLine);
		}

		private void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this,
				Resources.Messages.ClearBookmarks, "Delete all bookmarks?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				scriptEditor.Bookmarks.Clear();
				scriptEditor.Invalidate();
			}
		}

		private void TidyScript(bool trimOnly = false)
		{
			scriptEditor.BeginAutoUndo(); // Start AutoUndo to allow the user to undo the tidying process using only a single stack

			// Save set bookmarks and remove them from the editor to prevent a bug
			BaseBookmarks bookmarkedLines = scriptEditor.Bookmarks;
			scriptEditor.Bookmarks.Clear();

			int scrollPosition = scriptEditor.VerticalScroll.Value; // Cache current scroll position

			// Store editor content in a string and replace the "whitespace dots" (if there are any) with whitespace
			string editorContent = scriptEditor.Text.Replace("·", " ");

			// Setup a list to store all tidied lines
			List<string> tidiedlines = trimOnly ? SyntaxTidying.TrimLines(editorContent) : SyntaxTidying.ReindentLines(editorContent);

			for (int i = 0; i < scriptEditor.LinesCount; i++) // Scan all lines
			{
				// Also check if user has "Show Spaces" enabled
				string currentTidiedLine = Properties.Settings.Default.ShowSpaces ? tidiedlines[i].Replace(" ", "·") : tidiedlines[i];

				if (scriptEditor.GetLineText(i) != currentTidiedLine) // If a line has changed
				{
					scriptEditor.Selection = new Range(scriptEditor, 0, i, scriptEditor.GetLineText(i).Length, i);
					scriptEditor.InsertText(tidiedlines[i]);
				}
			}

			// Restore last scroll position
			scriptEditor.VerticalScroll.Value = scrollPosition;
			scriptEditor.UpdateScrollbars();

			scriptEditor.Bookmarks = bookmarkedLines; // Restore lost bookmarks

			scriptEditor.EndAutoUndo(); // End AutoUndo to stop recording actions and put them into a single stack
		}

		private void HandleVisibleSpaces()
		{
			// If "Show Spaces" is enabled and the text contains spaces
			if (Properties.Settings.Default.ShowSpaces && scriptEditor.Text.Contains(" "))
			{
				Place caretPosition = scriptEditor.Selection.Start; // Cache caret position

				scriptEditor.Text = scriptEditor.Text.Replace(' ', '·');

				scriptEditor.Selection.Start = caretPosition; // Restore caret position
			}
			else if (!Properties.Settings.Default.ShowSpaces && scriptEditor.Text.Contains("·"))
			{
				Place caretPosition = scriptEditor.Selection.Start; // Cache caret position

				scriptEditor.Text = scriptEditor.Text.Replace('·', ' ');

				scriptEditor.Selection.Start = caretPosition; // Restore caret position
			}
		}

		#endregion Object event methods

		#region Project / File handling

		private void OpenSelectedDropDownProject(ToolStripItemClickedEventArgs e)
		{
			bool projectFound = false;

			foreach (Project project in _availableProjects)
			{
				if (project.Name == e.ClickedItem.Text)
				{
					_currentProject = project;
					currentProjectLabel.Text = _currentProject.Name;

					OpenProjectFiles();

					Properties.Settings.Default.LastProject = _currentProject.Name;
					Properties.Settings.Default.Save();

					projectFound = true;
				}
			}

			if (!projectFound)
			{
				DialogResult result = DarkMessageBox.Show(this, Resources.Messages.SelectedProjectNotFound, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

				if (result == DialogResult.Yes)
					projectsToolStripMenuItem.DropDownItems.Remove(e.ClickedItem);
			}
		}

		private void OpenProjectFiles() // TODO: Rewrite this mess and add Languages support
		{
			try
			{
				string scriptFilePath = _currentProject.GamePath + @"\Script\script.txt";
				string fileContent = File.ReadAllText(scriptFilePath, Encoding.GetEncoding(1252));

				// Replace all spaces with dots if the user wants to have visible spaces
				if (Properties.Settings.Default.ShowSpaces && fileContent.Contains(" "))
					fileContent = fileContent.Replace(" ", "·");

				scriptEditor.Text = fileContent;

				// Opening the file has changed the text, so set it back to false
				scriptEditor.IsChanged = false;
				scriptEditor.ClearUndo();

				ToggleInterface(true); // Enable interface since we got a file we can edit

				// Disable "Save" buttons since we've just opened the file
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				UpdateObjectBrowserNodes(); // Update object browser with objects (if they exist)

				if (objectBrowser.Nodes.Count == 2) // If all default nodes are set
				{
					// Force expanded nodes
					objectBrowser.Nodes[0].Expanded = true;
					objectBrowser.Nodes[1].Expanded = true;
				}

				if (Properties.Settings.Default.AutosaveTime != 0)
					StartAutosaveTimer();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				UnloadCurrentProject();
			}
		}

		private void UnloadCurrentProject()
		{
			_currentProject = null;

			scriptEditor.Clear();
			scriptEditor.IsChanged = false;

			ResetObjectBrowserNodes();
			ToggleInterface(false);
		}

		private bool IsFileSaved()
		{
			if (scriptEditor.IsChanged)
			{
				string scriptFilePath = Path.GetDirectoryName(_currentProject.GamePath) + @"\Script\script.txt";

				DialogResult result = DarkMessageBox.Show(this,
					string.Format(Resources.Messages.UnsavedChanges, Path.GetFileName(scriptFilePath)), "Unsaved changes!",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					if (!SaveFile()) // Try to save the file
						return false; // If file saving failed
				}
				else if (result == DialogResult.Cancel)
					return false; // If user has cancelled the operation
			}

			return true; // If file is saved or has just been saved
		}

		private bool SaveFile()
		{
			if (Properties.Settings.Default.ReindentOnSave)
				TidyScript();

			try
			{
				// Replace dots with spaces
				string editorContent = scriptEditor.Text.Replace("·", " ");

				// Save to file
				string scriptFilePath = Path.GetDirectoryName(_currentProject.GamePath) + @"\Script\script.txt";
				File.WriteAllText(scriptFilePath, editorContent, Encoding.GetEncoding(1252));
				scriptEditor.IsChanged = false;

				// Disable "Save" buttons since we've just saved
				saveToolStripMenuItem.Enabled = false;
				saveToolStripButton.Enabled = false;

				scriptEditor.Invalidate();
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
					return SaveFile(); // Retry saving

				return false;
			}

			return true; // If saving was successful
		}

		private void StartAutosaveTimer()
		{
			var autosaveTimer = new System.Timers.Timer(); // I don't like using vars but ¯\_(ツ)_/¯
			autosaveTimer.Elapsed += autosaveTimer_Elapsed;
			autosaveTimer.Interval = Properties.Settings.Default.AutosaveTime * (60 * 1000);
			autosaveTimer.Start();
		}

		private void autosaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			string scriptFolderPath = Path.GetDirectoryName(_currentProject.GamePath) + @"\Script";
			string autosaveFilePath = scriptFolderPath + @"\script.autosave";

			try
			{
				// Replace dots with spaces
				string editorContent = scriptEditor.Text.Replace("·", " ");

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

		#endregion Project / File handling

		#region Other forms

		private void ShowAboutForm()
		{
			using (var form = new TombLib.Forms.FormAbout(null))
				form.ShowDialog(this);
		}

		private void ShowProjectSetupForm()
		{
			if (!IsFileSaved()) // If current file has unsaved changes
				return;

			using (FormProjectSetup form = new FormProjectSetup())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					List<Project> projects = new List<Project>();

					using (StreamReader stream = new StreamReader("Projects.xml"))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
						projects = (List<Project>)serializer.Deserialize(stream);
					}

					_currentProject = projects[projects.Count - 1];
					_availableProjects.Add(_currentProject);

					projectsToolStripMenuItem.DropDownItems.Add(_currentProject.Name);
					currentProjectLabel.Text = _currentProject.Name;

					OpenProjectFiles();

					Properties.Settings.Default.LastProject = _currentProject.Name;
					Properties.Settings.Default.Save();
				}
			}
		}

		private void ShowStringTable()
		{
			using (FormStringTable form = new FormStringTable())
				form.ShowDialog(this);
		}

		private void ShowSettingsForm()
		{
			// Cache critical settings
			int autosaveCache = Properties.Settings.Default.AutosaveTime;
			bool showSpacesCache = Properties.Settings.Default.ShowSpaces;
			bool autocompleteCache = Properties.Settings.Default.Autocomplete;
			bool toolTipCache = Properties.Settings.Default.ToolTips;

			using (FormSettings form = new FormSettings())
			{
				if (form.ShowDialog(this) == DialogResult.Cancel)
					return;

				ApplyUserSettings();

				if (form._restartItemCount > 0) // GLOBALZZZ NUUUU THIZ IZ BED !!! Welp, find a better solution then. :P
				{
					if (!IsFileSaved()) // If current file has unsaved changes
					{
						// Saving failed or the user clicked "Cancel"
						// Therefore restore the previous critical settings
						Properties.Settings.Default.AutosaveTime = autosaveCache;
						Properties.Settings.Default.ShowSpaces = showSpacesCache;
						Properties.Settings.Default.Autocomplete = autocompleteCache;
						Properties.Settings.Default.ToolTips = toolTipCache;

						Properties.Settings.Default.Save();
						ShowSettingsForm();
						return; // DO NOT CLOSE THE APP !!! (ﾉ°Д°)ﾉ︵﻿ ┻━┻
					}

					scriptEditor.IsChanged = false; // Prevent re-check of IsFileSaved()
					Application.Restart();
				}
			}
		}

		#endregion Other forms

		// UNSORTED CODE STARTS HERE:

		private void BuildScript()
		{
			string scriptFolderPath = _currentProject.GamePath + @"\Script";
			IScriptCompiler compiler;

			switch (_currentProject.Compiler)
			{
				case ScriptCompilers.TRLENew:
					compiler = new ScriptCompilerNew(TombLib.LevelData.GameVersion.TR4);
					break;

				case ScriptCompilers.NGCenter:
					compiler = new ScriptCompilerTRNG(_currentProject.NGCenterPath);
					break;

				case ScriptCompilers.TR5New:
					compiler = new ScriptCompilerNew(TombLib.LevelData.GameVersion.TR5);
					break;

				case ScriptCompilers.TR5Main:
					compiler = new ScriptCompilerTR5Main();
					break;

				default:
					compiler = new ScriptCompilerLegacy(scriptFolderPath);
					break;
			}

			bool result = compiler.CompileScripts(scriptFolderPath, _currentProject.GamePath);

			if (!result)
				DarkMessageBox.Show(this, "Error while compiling scripts", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
