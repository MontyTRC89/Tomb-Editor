using DarkUI.Controls;
using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
		// IMPORTANT:
		// _currentTab.Tag - Stores the full path of the file that's being modified in the tab's ScriptTextBox.
		// _textBox.Tag - Stores the previous zoom value to fix a strange FCTB bug.

		/// <summary>
		/// Configuration object.
		/// </summary>
		private Configuration _config = Configuration.Load();

		/// <summary>
		/// A list of all available projects.
		/// </summary>
		private List<Project> _availableProjects = new List<Project>();

		/// <summary>
		/// Currently edited project.
		/// </summary>
		private Project _currentProject = new Project();

		/// <summary>
		/// Currently selected tab page.
		/// </summary>
		private TabPage _currentTab = new TabPage();

		/// <summary>
		/// Currently selected tab TextBox.
		/// </summary>
		private ScriptTextBox _textBox = new ScriptTextBox();

		/// <summary>
		/// Tells whether the editor is in "Backup Mode".
		/// </summary>
		private bool _backupMode = false;

		#region Initialization

		public FormMain()
		{
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			ApplyUserSettings(); // From ScriptEditorConfiguration.xml
			GetProjectsFromXML(); // From ScriptEditorProjects.xml

			if (LastSessionCrashed())
				return;

			if (string.IsNullOrWhiteSpace(_config.LastProjectName)) // This should only happen when the program was never used before
			{
				ToggleInterface(false);
				ShowProjectSetupForm();
			}
			else
				OpenLastProject();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!AreAllFilesSaved()) // Check all opened tabs
				e.Cancel = true;

			_config.Save(); // Save settings before closing
		}

		private void ApplyUserSettings()
		{
			// ScriptTextBox specific settings
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				foreach (ScriptTextBox textBox in tab.Controls.OfType<ScriptTextBox>())
				{
					textBox.Font = new Font(_config.FontFamily, _config.FontSize);

					textBox.AutoCompleteBrackets = _config.AutoCloseBrackets;
					textBox.WordWrap = _config.WordWrap;

					textBox.ShowLineNumbers = _config.View_ShowLineNumbers;
				}
			}

			// Editor settings
			ToggleObjBrowser(_config.View_ShowObjBrowser);
			ToggleProjExplorer(_config.View_ShowProjExplorer);
			ToggleInfoBox(_config.View_ShowInfoBox);

			ToggleToolStrip(_config.View_ShowToolStrip);
			ToggleStatusStrip(_config.View_ShowStatusStrip);

			ToggleLineNumbers(_config.View_ShowLineNumbers);
			menuItem_ToolTips.Checked = _config.View_ShowToolTips;
		}

		private void GetProjectsFromXML()
		{
			try
			{
				using (StreamReader reader = new StreamReader("ScriptEditorProjects.xml")) // Read ScriptEditorProjects.xml
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
					_availableProjects = (List<Project>)serializer.Deserialize(reader);
				}

				comboBox_Projects.Items.Clear();

				foreach (Project project in _availableProjects) // Add all available projects into comboBox_Projects
					comboBox_Projects.Items.Add(project.Name);
			}
			catch (IOException) // ScriptEditorProjects.xml doesn't exist
			{
				using (StreamWriter writer = new StreamWriter("ScriptEditorProjects.xml")) // Create a new ScriptEditorProjects.xml file
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
					serializer.Serialize(writer, new List<Project>());
				}

				_config.LastProjectName = string.Empty; // Reset this because we just created a new ScriptEditorProjects.xml file
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool LastSessionCrashed()
		{
			foreach (Project proj in _availableProjects)
			{
				string[] files = Directory.GetFiles(proj.ScriptPath, "*.backup", SearchOption.AllDirectories);

				if (files.Count() != 0) // If a .backup file exists in the current project
				{
					RestorePreviousSession(proj, files);
					return true;
				}
			}

			return false;
		}

		private void RestorePreviousSession(Project project, string[] files)
		{
			_backupMode = true; // To prevent unwanted events such as Explorer_Projects_SelectedIndexChanged

			_currentProject = project;
			comboBox_Projects.SelectedItem = project.Name;

			UpdateExplorerTreeView();

			foreach (string file in files)
			{
				string backupFileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));

				string originalFilePath = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file);
				OpenFile(originalFilePath);

				_textBox.Text = backupFileContent;
			}

			_backupMode = false;
		}

		private void OpenLastProject()
		{
			bool projectFound = false;

			foreach (Project proj in _availableProjects)
			{
				if (proj.Name == _config.LastProjectName)
				{
					projectFound = true;
					comboBox_Projects.SelectedItem = proj.Name; // Selecting the item will trigger Explorer_Projects_SelectedIndexChanged
				}
			}

			if (!projectFound) // ScriptEditorProjects.xml was probably modified or deleted
			{
				ToggleInterface(false);
				ShowProjectSetupForm();
			}
		}

		#endregion Initialization

		#region Events

		private void File_AddProject_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void File_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void File_Build_Click(object sender, EventArgs e) => BuildScript();
		private void File_Exit_Click(object sender, EventArgs e) => Close();

		private void Edit_Undo_Click(object sender, EventArgs e) => Undo();
		private void Edit_Redo_Click(object sender, EventArgs e) => Redo();
		private void Edit_Cut_Click(object sender, EventArgs e) => Cut();
		private void Edit_Copy_Click(object sender, EventArgs e) => Copy();
		private void Edit_Paste_Click(object sender, EventArgs e) => Paste();
		private void Edit_Find_Click(object sender, EventArgs e) => _textBox.ShowFindDialog();
		private void Edit_Replace_Click(object sender, EventArgs e) => _textBox.ShowReplaceDialog();

		private void Edit_SelectAll_Click(object sender, EventArgs e)
		{
			_textBox.SelectAll();
			DoStatusCounting(); // So it updates the status strip labels
		}

		private void Tools_Reindent_Click(object sender, EventArgs e) => TidyDocument();
		private void Tools_Trim_Click(object sender, EventArgs e) => TidyDocument(true);
		private void Tools_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void Tools_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void Tools_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();
		private void Tools_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GotoPrevBookmark(_textBox.Selection.Start.iLine);
		private void Tools_NextBookmark_Click(object sender, EventArgs e) => _textBox.GotoNextBookmark(_textBox.Selection.Start.iLine);
		private void Tools_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void Tools_Settings_Click(object sender, EventArgs e) => ShowSettingsForm();

		private void View_ObjBrowser_Click(object sender, EventArgs e)
		{
			ToggleObjBrowser(!groupBox_ObjBrowser.Visible);
			_config.View_ShowObjBrowser = groupBox_ObjBrowser.Visible;
		}

		private void View_ProjExplorer_Click(object sender, EventArgs e)
		{
			ToggleProjExplorer(!groupBox_ProjExplorer.Visible);
			_config.View_ShowProjExplorer = groupBox_ProjExplorer.Visible;
		}

		private void View_InfoBox_Click(object sender, EventArgs e)
		{
			ToggleInfoBox(!groupBox_InfoBox.Visible);
			_config.View_ShowInfoBox = groupBox_InfoBox.Visible;
		}

		private void View_ToolStrip_Click(object sender, EventArgs e)
		{
			ToggleToolStrip(!toolStrip.Visible);
			_config.View_ShowToolStrip = toolStrip.Visible;
		}

		private void View_StatusStrip_Click(object sender, EventArgs e)
		{
			ToggleStatusStrip(!statusStrip.Visible);
			_config.View_ShowStatusStrip = statusStrip.Visible;
		}

		private void View_LineNumbers_Click(object sender, EventArgs e)
		{
			ToggleLineNumbers(!_textBox.ShowLineNumbers);
			_config.View_ShowLineNumbers = _textBox.ShowLineNumbers;
		}

		private void View_ToolTips_Click(object sender, EventArgs e)
		{
			menuItem_ToolTips.Checked = !menuItem_ToolTips.Checked;
			_config.View_ShowToolTips = menuItem_ToolTips.Checked;
		}

		private void Help_About_Click(object sender, EventArgs e) => ShowAboutForm();

		private void ContextMenu_Cut_Click(object sender, EventArgs e) => Cut();
		private void ContextMenu_Copy_Click(object sender, EventArgs e) => Copy();
		private void ContextMenu_Paste_Click(object sender, EventArgs e) => Paste();
		private void ContextMenu_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void ContextMenu_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void ContextMenu_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();

		private void ToolStrip_AddProject_Click(object sender, EventArgs e) => ShowProjectSetupForm();
		private void ToolStrip_Save_Click(object sender, EventArgs e) => OnSaveButtonClicked();
		private void ToolStrip_Build_Click(object sender, EventArgs e) => BuildScript();
		private void ToolStrip_Undo_Click(object sender, EventArgs e) => Undo();
		private void ToolStrip_Redo_Click(object sender, EventArgs e) => Redo();
		private void ToolStrip_Cut_Click(object sender, EventArgs e) => Cut();
		private void ToolStrip_Copy_Click(object sender, EventArgs e) => Copy();
		private void ToolStrip_Paste_Click(object sender, EventArgs e) => Paste();
		private void ToolStrip_Comment_Click(object sender, EventArgs e) => _textBox.InsertLinePrefix(_textBox.CommentPrefix);
		private void ToolStrip_Uncomment_Click(object sender, EventArgs e) => _textBox.RemoveLinePrefix(_textBox.CommentPrefix);
		private void ToolStrip_ToggleBookmark_Click(object sender, EventArgs e) => ToggleBookmark();
		private void ToolStrip_PrevBookmark_Click(object sender, EventArgs e) => _textBox.GotoPrevBookmark(_textBox.Selection.Start.iLine);
		private void ToolStrip_NextBookmark_Click(object sender, EventArgs e) => _textBox.GotoNextBookmark(_textBox.Selection.Start.iLine);
		private void ToolStrip_ClearBookmarks_Click(object sender, EventArgs e) => ClearAllBookmarks();
		private void ToolStrip_About_Click(object sender, EventArgs e) => ShowAboutForm();

		private void StatusStrip_ResetZoom_Click(object sender, EventArgs e)
		{
			_textBox.Tag = 100;
			_textBox.Zoom = 100;
			button_ResetZoom.Visible = false;
		}

		private void Editor_TextChangedDelayed(object sender, TextChangedEventArgs e) => HandleTextChangedIndicator();
		private void Editor_UndoRedoState_Changed(object sender, EventArgs e) => UpdateUndoRedoSaveStates();
		private void Editor_SelectionChanged(object sender, EventArgs e) => DoStatusCounting();
		private void Editor_KeyPress(object sender, KeyPressEventArgs e) => DoStatusCounting();

		private void Editor_TextChanged(object sender, TextChangedEventArgs e)
		{
			HandleVisibleSpaces(); // Oh boy...

			if (_currentTab.Tag != null)
			{
				// Create live backup file so the app can restore lost progress if it crashes
				string backupFilePath = _currentTab.Tag.ToString() + ".backup";
				File.WriteAllText(backupFilePath, _textBox.Text.Replace("·", " "));
			}

			if (_backupMode)
				HandleTextChangedIndicator();
		}

		private void Editor_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && !_textBox.Selection.Contains(_textBox.PointToPlace(e.Location)))
				_textBox.Selection.Start = _textBox.PointToPlace(e.Location); // Move caret to the right-clicked line
		}

		private void Editor_ZoomChanged(object sender, EventArgs e)
		{
			if (_textBox.Zoom > (int)_textBox.Tag) // If zoom increased
			{
				_textBox.Tag = (int)_textBox.Tag + 5;
				_textBox.Zoom = (int)_textBox.Tag;
			}

			if (_textBox.Zoom < (int)_textBox.Tag) // If zoom decreased
			{
				_textBox.Tag = (int)_textBox.Tag - 5;
				_textBox.Zoom = (int)_textBox.Tag;
			}

			label_Zoom.Text = "Zoom: " + _textBox.Zoom + "%";
			button_ResetZoom.Visible = _textBox.Zoom != 100;

			// Limit zoom to a minumum of 25 and a maximum of 500
			if (_textBox.Zoom < 25)
				_textBox.Zoom = 25;
			else if (_textBox.Zoom > 500)
				_textBox.Zoom = 500;
		}

		private void OnSaveButtonClicked()
		{
			if (!SaveFile(_textBox))
				return;

			string backupFilePath = _currentTab.Tag.ToString() + ".backup";
			File.Delete(backupFilePath); // We don't need it when saving was successful

			_textBox.IsChanged = false;
			_currentTab.Text = _currentTab.Text.TrimEnd('*');

			menuItem_Save.Enabled = false;
			button_Save.Enabled = false;

			UpdateObjectBrowserNodes();
			_textBox.Invalidate();
		}

		private void UpdateUndoRedoSaveStates()
		{
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

			if (_currentTab.Tag != null)
			{
				string editorContent = _textBox.Text.Replace("·", " ");
				string fileContent = File.ReadAllText(_currentTab.Tag.ToString(), Encoding.GetEncoding(1252)).Replace("·", " ");

				if (editorContent == fileContent)
				{
					string backupFilePath = _currentTab.Tag.ToString() + ".backup";
					File.Delete(backupFilePath); // We don't need it when there are no changes made

					_textBox.IsChanged = false;

					menuItem_Save.Enabled = false;
					button_Save.Enabled = false;
				}
				else
				{
					menuItem_Save.Enabled = true;
					button_Save.Enabled = true;
				}
			}
			else // This should only happen in a "Untitled" tab page
			{
				if (string.IsNullOrEmpty(_textBox.Text))
				{
					_textBox.IsChanged = false;

					menuItem_Save.Enabled = false;
					button_Save.Enabled = false;
				}
				else
				{
					menuItem_Save.Enabled = true;
					button_Save.Enabled = true;
				}
			}
		}

		private void DoStatusCounting()
		{
			label_LineNumber.Text = "Line: " + (_textBox.Selection.Start.iLine + 1);
			label_ColNumber.Text = "Column: " + (_textBox.Selection.Start.iChar + 1);

			if (_textBox.Selection.Start.iChar < 0) // FCTB sucks
				_textBox.Selection.Start = new Place(0, _textBox.Selection.Start.iLine);

			label_SelectedChars.Text = "Selected: " + _textBox.SelectedText.Length;
		}

		private void Undo()
		{
			Bookmark[] bookmarks = _textBox.Bookmarks.ToArray(); // Cache
			_textBox.Bookmarks.Clear();

			if (_config.ShowSpaces)
			{
				do _textBox.Undo();
				while (_textBox.Text.Contains(" "));
			}
			else
			{
				do _textBox.Undo();
				while (_textBox.Text.Contains("·"));
			}

			foreach (Bookmark bookmark in bookmarks) // Restore
				_textBox.Bookmarks.Add(bookmark);
		}

		private void Redo()
		{
			Bookmark[] bookmarks = _textBox.Bookmarks.ToArray(); // Cache
			_textBox.Bookmarks.Clear();

			if (_config.ShowSpaces)
			{
				do _textBox.Redo();
				while (_textBox.Text.Contains(" "));
			}
			else
			{
				do _textBox.Redo();
				while (_textBox.Text.Contains("·"));
			}

			foreach (Bookmark bookmark in bookmarks) // Restore
				_textBox.Bookmarks.Add(bookmark);
		}

		private void Cut()
		{
			if (!string.IsNullOrEmpty(_textBox.SelectedText))
			{
				Clipboard.SetText(_textBox.SelectedText.Replace("·", " ")); // Do not copy the dots
				_textBox.SelectedText = string.Empty;
			}
		}

		private void Copy()
		{
			if (!string.IsNullOrEmpty(_textBox.SelectedText))
				Clipboard.SetText(_textBox.SelectedText.Replace("·", " ")); // Do not copy the dots
		}

		private void Paste()
		{
			_textBox.SelectedText = _config.ShowSpaces ? Clipboard.GetText().Replace(" ", "·") : Clipboard.GetText();
		}

		private void ToggleBookmark()
		{
			int currentLine = _textBox.Selection.Start.iLine;

			if (_textBox.Bookmarks.Contains(currentLine))
				_textBox.Bookmarks.Remove(currentLine);
			else
				_textBox.Bookmarks.Add(currentLine);
		}

		private void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this,
				Resources.Messages.ClearBookmarks, "Delete all bookmarks?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				_textBox.Bookmarks.Clear();
		}

		private void TidyDocument(bool trimOnly = false)
		{
			// Cache
			int scrollPosition = _textBox.VerticalScroll.Value;
			Bookmark[] bookmarks = _textBox.Bookmarks.ToArray();

			_textBox.Bookmarks.Clear();

			// Tidy
			string editorContent = _textBox.Text.Replace("·", " ");
			_textBox.Text = trimOnly ? SyntaxTidying.Trim(editorContent) : SyntaxTidying.Reindent(editorContent);

			// Restore
			_textBox.VerticalScroll.Value = scrollPosition;
			_textBox.UpdateScrollbars();

			foreach (Bookmark bookmark in bookmarks)
				_textBox.Bookmarks.Add(bookmark);
		}

		private void HandleVisibleSpaces() // Crazy stuff going on here
		{
			if (_config.ShowSpaces && _textBox.Text.Contains(" "))
			{
				Place caretPosition = _textBox.Selection.Start;

				_textBox.Selection = new Range(_textBox, new Place(_textBox.Selection.Start.iChar - 1, _textBox.Selection.Start.iLine), _textBox.Selection.End);
				_textBox.SelectedText = "·";

				_textBox.Selection.Start = caretPosition;
			}
			else if (!_config.ShowSpaces && _textBox.Text.Contains("·"))
			{
				Place caretPosition = _textBox.Selection.Start;

				_textBox.Selection = new Range(_textBox, new Place(_textBox.Selection.Start.iChar - 1, _textBox.Selection.Start.iLine), _textBox.Selection.End);
				_textBox.SelectedText = " ";

				_textBox.Selection.Start = caretPosition;
			}

			_textBox.Invalidate();
		}

		private void HandleTextChangedIndicator()
		{
			if (_currentTab.Tag != null)
			{
				if (!_textBox.IsChanged) // Remove * if it's there
					_currentTab.Text = _currentTab.Text.TrimEnd('*');
				else if (!_currentTab.Text.EndsWith("*")) // Add * if it's NOT there
					_currentTab.Text = Path.GetFileName(_currentTab.Tag.ToString()) + "*";
			}
			else
			{
				if (!_textBox.IsChanged) // Remove * if it's there
					_currentTab.Text = "Untitled";
				else if (!_currentTab.Text.EndsWith("*")) // Add * if it's NOT there
					_currentTab.Text = "Untitled*";
			}
		}

		private void ToggleInterface(bool state)
		{
			// Toggle "File" menu items
			menuItem_Save.Enabled = state;
			menuItem_Build.Enabled = state;

			// Toggle "Edit" menu items
			menuItem_Cut.Enabled = state;
			menuItem_Copy.Enabled = state;
			menuItem_Paste.Enabled = state;
			menuItem_Find.Enabled = state;
			menuItem_Replace.Enabled = state;
			menuItem_SelectAll.Enabled = state;

			// Toggle "Tools" menu items
			menuItem_Reindent.Enabled = state;
			menuItem_Trim.Enabled = state;
			menuItem_Comment.Enabled = state;
			menuItem_Uncomment.Enabled = state;
			menuItem_ToggleBookmark.Enabled = state;
			menuItem_PrevBookmark.Enabled = state;
			menuItem_NextBookmark.Enabled = state;
			menuItem_ClearBookmarks.Enabled = state;

			// Toggle "View" menu items
			menuItem_ToolStrip.Enabled = state;
			menuItem_ObjBrowser.Enabled = state;
			menuItem_ProjExplorer.Enabled = state;
			menuItem_InfoBox.Enabled = state;
			menuItem_StatusStrip.Enabled = state;
			menuItem_LineNumbers.Enabled = state;
			menuItem_ToolTips.Enabled = state;

			if (state == false)
			{
				ToggleObjBrowser(false);
				ToggleProjExplorer(false);
				ToggleInfoBox(false);
				ToggleToolStrip(false);
				ToggleStatusStrip(false);
			}
			else
			{
				ToggleObjBrowser(_config.View_ShowObjBrowser);
				ToggleProjExplorer(_config.View_ShowProjExplorer);
				ToggleInfoBox(_config.View_ShowInfoBox);
				ToggleToolStrip(_config.View_ShowToolStrip);
				ToggleStatusStrip(_config.View_ShowStatusStrip);
			}
		}

		private void ToggleObjBrowser(bool state)
		{
			groupBox_ObjBrowser.Visible = state;
			splitter_Left.Visible = state;
			menuItem_ObjBrowser.Checked = state;
		}

		private void ToggleProjExplorer(bool state)
		{
			groupBox_ProjExplorer.Visible = state;
			splitter_Right.Visible = state;
			menuItem_ProjExplorer.Checked = state;
		}

		private void ToggleInfoBox(bool state)
		{
			groupBox_InfoBox.Visible = state;
			splitter_Bottom.Visible = state;
			menuItem_InfoBox.Checked = state;
		}

		private void ToggleToolStrip(bool state)
		{
			toolStrip.Visible = state;
			menuItem_ToolStrip.Checked = state;
		}

		private void ToggleStatusStrip(bool state)
		{
			statusStrip.Visible = state;
			menuItem_StatusStrip.Checked = state;
		}

		private void ToggleLineNumbers(bool state)
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				foreach (ScriptTextBox textBox in tab.Controls.OfType<ScriptTextBox>())
					textBox.ShowLineNumbers = state;
			}

			menuItem_LineNumbers.Checked = state;
		}

		#endregion Events

		#region ToolTips

		private void Editor_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
		{
			if (_config.View_ShowToolTips && !string.IsNullOrWhiteSpace(e.HoveredWord))
			{
				if (_textBox.GetLineText(e.Place.iLine).StartsWith("["))
					ShowSectionToolTip(e);
				else
					ShowCommandToolTip(e);
			}
		}

		private void ShowSectionToolTip(ToolTipNeededEventArgs e)
		{
			e.ToolTipTitle = "[" + e.HoveredWord + "]";

			// Get resources from SectionToolTips.resx
			ResourceManager sectionToolTipResource = new ResourceManager(typeof(Resources.SectionToolTips));
			ResourceSet resourceSet = sectionToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

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
					return; // The line number might go to -1 and it will crash the app, so stop the loop to prevent it

				if (_textBox.GetLineText(i).StartsWith("[PSXExtensions]"))
				{
					e.ToolTipTitle = "Level [PSXExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPSX;
					return;
				}
				else if (_textBox.GetLineText(i).StartsWith("[PCExtensions]"))
				{
					e.ToolTipTitle = "Level [PCExtensions]";
					e.ToolTipText = Resources.CommandToolTips.LevelPC;
					return;
				}
				else if (_textBox.GetLineText(i).StartsWith("[Title]"))
				{
					e.ToolTipTitle = "Level [Title]";
					e.ToolTipText = Resources.CommandToolTips.LevelTitle;
					return;
				}
				else if (_textBox.GetLineText(i).StartsWith("[Level]"))
				{
					e.ToolTipTitle = "Level";
					e.ToolTipText = Resources.CommandToolTips.LevelLevel;
					return;
				}

				i--; // Go 1 line higher if no section header was found yet
			}
			while (!_textBox.GetLineText(i + 1).StartsWith("["));
		}

		#endregion ToolTips

		#region Object Browser

		private void ObjBrowser_TreeView_Click(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView_ObjBrowser.SelectedNodes.Count < 1 || string.IsNullOrWhiteSpace(treeView_ObjBrowser.SelectedNodes[0].Text))
				return;

			// If the selected node is a default item ("Sections" or "Levels")
			if (treeView_ObjBrowser.SelectedNodes[0] == treeView_ObjBrowser.Nodes[0] || treeView_ObjBrowser.SelectedNodes[0] == treeView_ObjBrowser.Nodes[1])
				return;

			for (int i = 0; i < _textBox.LinesCount; i++) // Scan all lines
			{
				// Find the line that contains the node text
				if (_textBox.GetLineText(i).ToLower().Replace("·", " ").Contains(treeView_ObjBrowser.SelectedNodes[0].Text.ToLower()))
				{
					_textBox.Focus();
					_textBox.Navigate(i); // Scroll to the line position

					_textBox.Selection = new Range(_textBox, 0, i, _textBox.GetLineText(i).Length, i); // Select the line
					return;
				}
			}
		}

		private void ObjBrowser_Search_GotFocus(object sender, EventArgs e)
		{
			if (textBox_ObjBrowserSearch.Text == "Search...")
				textBox_ObjBrowserSearch.Text = string.Empty;
		}

		private void ObjBrowser_Search_LostFocus(object sender, EventArgs e)
		{
			if (textBox_ObjBrowserSearch.Text == string.Empty)
				textBox_ObjBrowserSearch.Text = "Search...";
		}

		private void ObjBrowser_Search_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(textBox_ObjBrowserSearch.Text) || textBox_ObjBrowserSearch.Text == "Search...")
				UpdateObjectBrowserNodes();
			else
				UpdateObjectBrowserNodes(textBox_ObjBrowserSearch.Text);
		}

		private void UpdateObjectBrowserNodes(string filter = "")
		{
			treeView_ObjBrowser.Nodes.Clear();
			treeView_ObjBrowser.Nodes.Add(new DarkTreeNode("Sections"));
			treeView_ObjBrowser.Nodes.Add(new DarkTreeNode("Levels"));

			for (int i = 0; i < _textBox.LinesCount; i++) // Scan all lines
			{
				AddSectionNode(i, filter);
				AddLevelNode(i, filter);
			}

			treeView_ObjBrowser.Nodes[0].Expanded = true;
			treeView_ObjBrowser.Nodes[1].Expanded = true;

			treeView_ObjBrowser.Invalidate();
		}

		private void AddSectionNode(int lineNumber, string filter)
		{
			List<string> sections = SyntaxKeyWords.Sections();

			foreach (string section in sections)
			{
				string sectionName = "[" + section + "]";

				// If the current line starts a section and it's not a [Level] section
				if (_textBox.GetLineText(lineNumber).StartsWith(sectionName) && sectionName != "[Level]")
				{
					DarkTreeNode sectionNode = new DarkTreeNode(sectionName);

					// If the section name matches the filter (It always does if there's nothing in the search bar)
					if (sectionNode.Text.ToLower().Contains(filter.ToLower()))
						treeView_ObjBrowser.Nodes[0].Nodes.Add(sectionNode);
				}
			}
		}

		private void AddLevelNode(int lineNumber, string filter)
		{
			Regex rgx = new Regex(@"\bName[\s·]?=[\s·]?"); // Regex rule to find lines that start with "Name = "

			if (rgx.IsMatch(_textBox.GetLineText(lineNumber)))
			{
				// Create a new node, remove "Name = ", replace dots with spaces and trim it, so we only get the level name string
				DarkTreeNode levelNode = new DarkTreeNode(rgx.Replace(_textBox.GetLineText(lineNumber), string.Empty).Replace("·", " ").Trim());

				// If the level name matches the filter (It always does if there's nothing in the search bar)
				if (levelNode.Text.ToLower().Contains(filter.ToLower()))
					treeView_ObjBrowser.Nodes[1].Nodes.Add(levelNode);
			}
		}

		#endregion Object Browser

		#region Project Explorer

		private void Explorer_Projects_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBox_Projects.SelectedItem.ToString() == _currentProject.Name) // If the user selected the same project
				return;

			if (!AreAllFilesSaved())
			{
				comboBox_Projects.SelectedItem = _currentProject.Name; // Reset SelectedItem back to the previous project
				return;
			}

			if (_backupMode)
				return;

			bool projectFound = false;

			foreach (Project proj in _availableProjects)
			{
				if (proj.Name == comboBox_Projects.SelectedItem.ToString())
				{
					projectFound = true;

					_currentProject = null; // Set this to null so we won't trigger Editor_TabControl_Selecting in the next line
					tabControl_Editor.TabPages.Clear();

					_currentProject = proj;
					OpenProjectFiles();

					_config.LastProjectName = _currentProject.Name;
				}
			}

			if (!projectFound)
			{
				DialogResult result = DarkMessageBox.Show(this,
					Resources.Messages.SelectedProjectNotFound, "Error",
					MessageBoxButtons.YesNo, MessageBoxIcon.Error);

				if (result == DialogResult.Yes)
					comboBox_Projects.Items.Remove(comboBox_Projects.SelectedItem);
			}
		}

		private void Explorer_TreeView_DoubleClick(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView_Files.SelectedNodes.Count < 1 || string.IsNullOrWhiteSpace(treeView_Files.SelectedNodes[0].Text))
				return;

			// If the selected node is not a .txt or .lua file
			if (!treeView_Files.SelectedNodes[0].Text.ToLower().EndsWith(".txt") && !treeView_Files.SelectedNodes[0].Text.ToLower().EndsWith(".lua"))
				return;

			foreach (TabPage tab in tabControl_Editor.TabPages) // Check if the file isn't opened already
			{
				if (tab.Text.TrimEnd('*') == treeView_Files.SelectedNodes[0].Text)
				{
					tabControl_Editor.SelectTab(tabControl_Editor.TabPages.IndexOf(tab));
					return;
				}
			}

			OpenFile(treeView_Files.SelectedNodes[0].Tag.ToString());
		}

		private void Explorer_EditScript_Click(object sender, EventArgs e)
		{
			string scriptFilePath = string.Empty;

			foreach (string file in Directory.GetFiles(_currentProject.ScriptPath))
			{
				if (file.ToLower().EndsWith("\\script.txt"))
					scriptFilePath = file;
			}

			if (string.IsNullOrEmpty(scriptFilePath))
				return; // TODO: ADD ERROR MESSAGE HERE!

			foreach (TabPage tab in tabControl_Editor.TabPages) // Check if the file isn't opened already
			{
				if (tab.Text.TrimEnd('*') == Path.GetFileName(scriptFilePath))
				{
					tabControl_Editor.SelectTab(tabControl_Editor.TabPages.IndexOf(tab));
					return;
				}
			}

			OpenFile(scriptFilePath);
		}

		private void Explorer_EditLanguages_Click(object sender, EventArgs e)
		{
			// TODO !!!
			using (FormStringTable form = new FormStringTable())
				form.ShowDialog(this);
			// TODO !!!
		}

		private void Explorer_OpenInExplorer_Click(object sender, EventArgs e)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				Arguments = _currentProject.ScriptPath,
				FileName = "explorer.exe"
			};

			Process.Start(startInfo);
		}

		private void Explorer_Splitter_Moved(object sender, SplitterEventArgs e)
		{
			button_EditScript.Width = (groupBox_ProjExplorer.Width / 2) - 5;
			button_EditLanguages.Location = new Point((groupBox_ProjExplorer.Width / 2) + 2, button_EditLanguages.Location.Y);
			button_EditLanguages.Width = (groupBox_ProjExplorer.Width / 2) - 5;
		}

		private void UpdateExplorerTreeView()
		{
			treeView_Files.Nodes.Clear();

			Stack<DarkTreeNode> stack = new Stack<DarkTreeNode>();
			DirectoryInfo scriptDirectory = new DirectoryInfo(_currentProject.ScriptPath);

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
						if (file.Name.ToLower() == "script.txt")
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

		#endregion Project Explorer

		#region Tabs

		private void Editor_TabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (tabControl_Editor.TabCount == 0 && _currentProject != null) // If the last tab page was closed
				CreateNewTabPage(); // As "Untitled"
		}

		private void Editor_TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl_Editor.TabCount == 0)
				return;

			_currentTab = tabControl_Editor.TabPages[tabControl_Editor.SelectedIndex];

			foreach (ScriptTextBox textBox in _currentTab.Controls.OfType<ScriptTextBox>())
			{
				_textBox = textBox;
				UpdateUndoRedoSaveStates();
				DoStatusCounting();

				UpdateObjectBrowserNodes();

				label_Zoom.Text = "Zoom: " + _textBox.Zoom + "%";
				button_ResetZoom.Visible = _textBox.Zoom != 100;

				return;
			}
		}

		private void Editor_TabControl_MouseClick(object sender, MouseEventArgs e)
		{
			// Middle mouse click tab closing
			if (e.Button == MouseButtons.Middle)
			{
				int i = 0;

				foreach (TabPage tab in tabControl_Editor.TabPages)
				{
					if (tabControl_Editor.GetTabRect(i).Contains(e.Location))
					{
						tabControl_Editor.SelectTab(tab);

						foreach (ScriptTextBox textBox in tab.Controls.OfType<ScriptTextBox>())
						{
							if (!IsFileSaved(tab, textBox))
								return;
						}

						tab.Dispose();
						return;
					}

					i++;
				}
			}
		}

		private void Editor_TabControl_TabClosing(object sender, TabControlCancelEventArgs e)
		{
			foreach (ScriptTextBox textBox in e.TabPage.Controls.OfType<ScriptTextBox>())
			{
				if (!IsFileSaved(e.TabPage, textBox))
					e.Cancel = true;
			}
		}

		private void CreateNewTabPage(string filePath = null)
		{
			string tabPageTitle = filePath == null ? "Untitled" : Path.GetFileName(filePath);

			TabPage newTabPage = new TabPage(tabPageTitle)
			{
				UseVisualStyleBackColor = false,
				BackColor = Color.FromArgb(60, 63, 65),
				Size = tabControl_Editor.Size
			};

			ScriptTextBox newTextBox = new ScriptTextBox
			{
				ShowLineNumbers = _config.View_ShowLineNumbers,
				ContextMenuStrip = contextMenu_TextBox
			};

			newTextBox.KeyPress += Editor_KeyPress;
			newTextBox.MouseDown += Editor_MouseDown;
			newTextBox.SelectionChanged += Editor_SelectionChanged;
			newTextBox.TextChanged += Editor_TextChanged;
			newTextBox.TextChangedDelayed += Editor_TextChangedDelayed;
			newTextBox.ToolTipNeeded += Editor_ToolTipNeeded;
			newTextBox.UndoRedoStateChanged += Editor_UndoRedoState_Changed;
			newTextBox.ZoomChanged += Editor_ZoomChanged;

			newTextBox.DelayedTextChangedInterval = 1;

			if (!string.IsNullOrWhiteSpace(filePath))
				newTabPage.Tag = filePath; // Store the full file path in the tag

			newTextBox.Tag = 100; // For more info about why this is here, scroll up to the start of the class

			newTabPage.Controls.Add(newTextBox);
			tabControl_Editor.TabPages.Add(newTabPage);
			tabControl_Editor.SelectTab(newTabPage);

			_currentTab = newTabPage;
			_textBox = newTextBox;
		}

		#endregion Tabs

		#region Files

		private void OpenProjectFiles()
		{
			try
			{
				ToggleInterface(true); // Enable the interface since we got files we can edit
				UpdateExplorerTreeView(); // Add the \Script\ folder files and subfolders into the TreeView

				scriptFolderWatcher.Path = _currentProject.ScriptPath; // Enable the scriptFolderWatcher for the current project

				bool scriptFileFound = false;

				foreach (string file in Directory.GetFiles(_currentProject.ScriptPath))
				{
					if (file.ToLower().EndsWith("\\script.txt"))
					{
						scriptFileFound = true;
						OpenFile(file);
					}
				}

				if (!scriptFileFound)
				{
					// TODO: Add error message for missing script.txt file here
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OpenFile(string filePath)
		{
			try
			{
				CreateNewTabPage(filePath);
				_textBox.OpenFile(filePath, Encoding.GetEncoding(1252));

				if (_config.ShowSpaces && _textBox.Text.Contains(" "))
				{
					_textBox.Text = _textBox.Text.Replace(" ", "·");

					_textBox.IsChanged = false;
					_currentTab.Text = _currentTab.Text.TrimEnd('*');

					_textBox.ClearUndo();

					menuItem_Save.Enabled = false;
					button_Save.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_currentTab.Tag = null;

				if (_backupMode)
					File.Delete(filePath + ".backup");
			}

			UpdateObjectBrowserNodes();
		}

		private bool AreAllFilesSaved()
		{
			foreach (TabPage tab in tabControl_Editor.TabPages)
			{
				foreach (ScriptTextBox textBox in tab.Controls.OfType<ScriptTextBox>())
				{
					if (!IsFileSaved(tab, textBox))
						return false;
				}
			}

			return true;
		}

		private bool IsFileSaved(TabPage tab, ScriptTextBox textBox)
		{
			if (textBox.IsChanged)
			{
				tabControl_Editor.SelectTab(tab);

				string fileName = _currentTab.Tag == null ? "Untitled" : Path.GetFileName(_currentTab.Tag.ToString());

				DialogResult result = DarkMessageBox.Show(this,
					string.Format(Resources.Messages.UnsavedChanges, fileName), "Unsaved changes!",
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					return SaveFile(textBox);
				else if (result == DialogResult.Cancel)
					return false;
			}

			if (_currentTab.Tag != null)
			{
				string backupFilePath = _currentTab.Tag.ToString() + ".backup";
				File.Delete(backupFilePath); // We don't need it when the file is saved
			}

			tab.Dispose();
			return true; // If the file is saved
		}

		private bool SaveFile(ScriptTextBox textBox)
		{
			if (_config.Tidy_ReindentOnSave)
				TidyDocument();

			try
			{
				if (_currentTab.Tag == null) // For "Untitled"
					ShowSaveAsDialog(textBox);
				else
				{
					string editorContent = textBox.Text.Replace("·", " ");
					File.WriteAllText(_currentTab.Tag.ToString(), editorContent, Encoding.GetEncoding(1252));
				}
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
					return SaveFile(textBox); // Retry saving

				return false;
			}

			return true; // If saving was successful
		}

		private void ShowSaveAsDialog(ScriptTextBox textBox)
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				Filter = "Text Files (*.txt)|*.txt|LUA Files (*.lua)|*.lua|All Files (*.*)|*.*",
				RestoreDirectory = true,
				InitialDirectory = _currentProject.ScriptPath
			};

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				string editorContent = textBox.Text.Replace("·", " ");
				File.WriteAllText(dialog.FileName, editorContent, Encoding.GetEncoding(1252));

				_currentTab.Tag = dialog.FileName;
			}
		}

		#endregion Files

		#region Forms

		private void ShowAboutForm()
		{
			using (var form = new TombLib.Forms.FormAbout(null))
				form.ShowDialog(this);
		}

		private void ShowProjectSetupForm()
		{
			using (FormProjectSetup form = new FormProjectSetup())
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					List<Project> projects = new List<Project>();

					using (StreamReader reader = new StreamReader("ScriptEditorProjects.xml"))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(List<Project>));
						projects = (List<Project>)serializer.Deserialize(reader);
					}

					Project newProject = projects[projects.Count - 1]; // The last project from the XML file (the one we just added)

					_availableProjects.Add(newProject);
					comboBox_Projects.Items.Add(newProject.Name);

					comboBox_Projects.SelectedItem = newProject.Name; // Selecting the item will trigger Explorer_Projects_SelectedIndexChanged
				}
			}
		}

		private void ShowSettingsForm()
		{
			_config.Save();

			// Cache critical settings
			bool autocompleteCache = _config.Autocomplete;
			bool showSpacesCache = _config.ShowSpaces;

			using (FormSettings form = new FormSettings())
			{
				if (form.ShowDialog(this) == DialogResult.Cancel)
					return;

				_config = Configuration.Load();
				ApplyUserSettings();

				if (form.RestartItemCount > 0)
				{
					if (!AreAllFilesSaved())
					{
						// Saving failed or the user clicked "Cancel"
						// Therefore restore the previous critical settings
						_config.Autocomplete = autocompleteCache;
						_config.ShowSpaces = showSpacesCache;
						_config.Save();

						ShowSettingsForm();
						return; // DO NOT CLOSE THE APP !!! (ﾉ°Д°)ﾉ︵﻿ ┻━┻
					}

					Application.Restart();
				}
			}
		}

		#endregion Forms

		// UNSORTED CODE STARTS HERE:

		private void BuildScript()
		{
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
					compiler = new ScriptCompilerLegacy(_currentProject.ScriptPath);
					break;
			}

			bool result = compiler.CompileScripts(_currentProject.ScriptPath, _currentProject.GamePath);

			if (!result)
				DarkMessageBox.Show(this, "Error while compiling scripts", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void scriptFolderWatcher_Changed(object sender, FileSystemEventArgs e) => UpdateExplorerTreeView();
		private void scriptFolderWatcher_Renamed(object sender, RenamedEventArgs e) => UpdateExplorerTreeView();
	}
}
