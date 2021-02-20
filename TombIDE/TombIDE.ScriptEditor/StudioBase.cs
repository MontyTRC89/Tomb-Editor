using DarkUI.Docking;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptEditor.Controls;
using TombIDE.ScriptEditor.Forms;
using TombIDE.ScriptEditor.Helpers;
using TombIDE.ScriptEditor.Objects;
using TombIDE.ScriptEditor.Properties;
using TombIDE.ScriptEditor.Settings;
using TombIDE.ScriptEditor.ToolStrips;
using TombIDE.ScriptEditor.ToolWindows;
using TombIDE.ScriptEditor.UI;
using TombIDE.Shared;
using TombLib.Forms;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptEditor
{
	public abstract partial class StudioBase : UserControl
	{
		#region Properties

		public new Control Parent
		{
			get => base.Parent;
			set
			{
				base.Parent = value;
				InitializeDockPanel();
			}
		}

		public abstract StudioMode StudioMode { get; }

		public DocumentMode DocumentMode
		{
			get => MenuStrip.DocumentMode;
			set
			{
				MenuStrip.DocumentMode = value;
				ToolStrip.DocumentMode = value;
				StatusStrip.DocumentMode = value;

				ContentExplorer.DocumentMode = value;
			}
		}

		public string ScriptRootDirectoryPath
		{
			get => EditorTabControl.ScriptRootDirectoryPath;
			set
			{
				EditorTabControl.ScriptRootDirectoryPath = value;
				FileExplorer.RootDirectoryPath = value;
			}
		}

		public string EngineDirectoryPath { get; set; }

		/// <summary>
		/// The current tab's Editor control.
		/// </summary>
		public IEditorControl CurrentEditor => EditorTabControl.CurrentEditor;

		public DockPanelState DockPanelState { get; set; }

		public bool IsMainWindowFocued { get; set; }

		#endregion Properties

		#region Fields

		protected ConfigurationCollection Configs = new ConfigurationCollection();

		protected FormFindReplace FormFindReplace;

		protected DarkDockPanel DockPanel;

		// Document:

		public EditorTabControl EditorTabControl = new EditorTabControl();

		/// <summary>
		/// Dockable document. (Parent of <c>EditorTabControl</c>)
		/// </summary>
		public DarkDocument EditorTabControlDocument = new DarkDocument();

		// Left:

		public ContentExplorer ContentExplorer = new ContentExplorer();

		// Right:

		public FileExplorer FileExplorer = new FileExplorer();

		// Bottom:

		public CompilerLogs CompilerLogs = new CompilerLogs();
		public SearchResults SearchResults = new SearchResults();

		/* Very frequently accessed items */

		private ToolStripItem UndoMenuItem;
		private ToolStripItem UndoToolStripButton;

		private ToolStripItem RedoMenuItem;
		private ToolStripItem RedoToolStripButton;

		private ToolStripItem SaveMenuItem;
		private ToolStripItem SaveToolStripButton;

		private ToolStripItem SaveAllMenuItem;
		private ToolStripItem SaveAllToolStripButton;

		private ToolStripMenuItem ToolStripViewItem;
		private ToolStripMenuItem ContentExplorerViewItem;
		private ToolStripMenuItem FileExplorerViewItem;
		private ToolStripMenuItem ReferenceBrowserViewItem;
		private ToolStripMenuItem CompilerLogsViewItem;
		private ToolStripMenuItem SearchResultsViewItem;
		private ToolStripMenuItem StatusStripViewItem;

		#endregion Fields

		#region Construction

		public StudioBase(string scriptRootDirectoryPath, string engineDirectoryPath)
			: this(scriptRootDirectoryPath, engineDirectoryPath, string.Empty)
		{ }
		public StudioBase(string scriptRootDirectoryPath, string engineDirectoryPath, string initialFilePath)
		{
			IDE.Global.IDEEventRaised += OnIDEEventRaised;

			InitializeComponent();

			ScriptRootDirectoryPath = scriptRootDirectoryPath;
			EngineDirectoryPath = engineDirectoryPath;

			InitializeTabControl();
			InitializeFileExplorer();

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				InitializeFindReplaceForm();

				MenuStrip.StudioMode = StudioMode;
				ToolStrip.StudioMode = StudioMode;

				InitializeFrequentlyAccessedItems(); // For quick control state updating

				ContentExplorer.ObjectClicked += ContentExplorer_ObjectClicked;
			}

			if (!string.IsNullOrWhiteSpace(initialFilePath))
				EditorTabControl.OpenFile(initialFilePath);
		}

		private void InitializeTabControl()
		{
			EditorTabControlDocument.Controls.Add(EditorTabControl);
			EditorTabControl.SelectedIndexChanged += EditorTabControl_SelectedIndexChanged;
			EditorTabControl.FileOpened += EditorTabControl_FileOpened;
		}

		private void InitializeFileExplorer()
		{
			FileExplorer.FileOpened += FileTreeView_FileOpened;
			FileExplorer.FileChanged += FileExplorer_FileChanged;
			FileExplorer.FileRenamed += FileTreeView_FileRenamed;
			FileExplorer.FileDeleted += FileTreeView_FileDeleted;
		}

		private void InitializeFindReplaceForm()
		{
			FormFindReplace = new FormFindReplace(EditorTabControl);
			FormFindReplace.FindAllPerformed += FormFindReplace_FindAllPerformed;
		}

		private void InitializeDockPanel()
		{
			DockPanel = new DarkDockPanel();

			DockPanel.Dock = DockStyle.Fill;
			DockPanel.EqualizeGroupSizes = true;
			DockPanel.Padding = new Padding(2);
			DockPanel.PrioritizeLeft = false;
			DockPanel.PrioritizeRight = false;
			DockPanel.ContentAdded += DockPanel_ContentChanged;
			DockPanel.ContentRemoved += DockPanel_ContentChanged;

			Controls.Add(DockPanel);

			DockPanel.BringToFront();

			// We have to initialize a default layout before applying the user defined one
			// otherwise we're gonna experience control priority issues.

			DockPanel.RestoreDockPanelState(WindowLayout.DefaultLayout, FindDockContentByKey);

			// We can remove the content after the initialization is done
			DockPanel.RemoveContent();

			// The fact that we have to do this for it to work correctly is complete bullsh*t

			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);

			ApplyMessageFilters();
		}

		private void InitializeFrequentlyAccessedItems()
		{
			UndoMenuItem = MenuStrip.FindItem(UIElement.Undo);
			UndoToolStripButton = ToolStrip.FindItem(UIElement.Undo);

			RedoMenuItem = MenuStrip.FindItem(UIElement.Redo);
			RedoToolStripButton = ToolStrip.FindItem(UIElement.Redo);

			SaveMenuItem = MenuStrip.FindItem(UIElement.Save);
			SaveToolStripButton = ToolStrip.FindItem(UIElement.Save);

			SaveAllMenuItem = MenuStrip.FindItem(UIElement.SaveAll);
			SaveAllToolStripButton = ToolStrip.FindItem(UIElement.SaveAll);

			ToolStripViewItem = MenuStrip.FindMenuItem(UIElement.ToolStrip);
			ContentExplorerViewItem = MenuStrip.FindMenuItem(UIElement.ContentExplorer);
			FileExplorerViewItem = MenuStrip.FindMenuItem(UIElement.FileExplorer);
			ReferenceBrowserViewItem = MenuStrip.FindMenuItem(UIElement.ReferenceBrowser);
			CompilerLogsViewItem = MenuStrip.FindMenuItem(UIElement.CompilerLogs);
			SearchResultsViewItem = MenuStrip.FindMenuItem(UIElement.SearchResults);
			StatusStripViewItem = MenuStrip.FindMenuItem(UIElement.StatusStrip);
		}

		private void ApplyMessageFilters()
		{
			Application.AddMessageFilter(DockPanel.DockContentDragFilter);
			Application.AddMessageFilter(DockPanel.DockResizeFilter);
		}

		#endregion Construction

		#region Events

		protected virtual void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ProgramClosingEvent)
				(obj as IDE.ProgramClosingEvent).CanClose = EditorTabControl.AskSaveAll();
		}

		private void DockPanel_ContentChanged(object sender, DockContentEventArgs e)
			=> UpdateViewMenu();

		private void ToolStrip_ItemClicked(object sender, EventArgs e)
			=> OnToolStripItemClicked((UIElement)(sender as ToolStripItem).Tag);

		private void EditorTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (CurrentEditor != null)
				DocumentMode = FileHelper.GetDocumentModeForFile(CurrentEditor.FilePath, CurrentEditor.EditorType);

			StatusStrip.UpdateStatus(CurrentEditor);
			UpdateUndoRedoSaveStates();
		}

		private void EditorTabControl_FileOpened(object sender, EventArgs e)
		{
			var editor = sender as IEditorControl;

			editor.StatusChanged += Editor_StatusChanged;
			editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

			if (editor is TextEditorBase textEditor)
				textEditor.KeyDown += TextEditor_KeyDown;

			if (editor is ClassicScriptEditor scriptEditor)
				scriptEditor.UpdateSettings(Configs.ClassicScript);

			DocumentMode = FileHelper.GetDocumentModeForFile(CurrentEditor.FilePath, CurrentEditor.EditorType);
			InitializeFrequentlyAccessedItems();
		}

		private void Editor_StatusChanged(object sender, EventArgs e)
			=> StatusStrip.UpdateStatus(CurrentEditor);

		private void Editor_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
			=> UpdateUndoRedoSaveStates();

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
			&& (e.Key == System.Windows.Input.Key.F || e.Key == System.Windows.Input.Key.H))
				FormFindReplace.Show(this, (CurrentEditor as TextEditorBase).SelectedText);
		}

		private void FileTreeView_FileOpened(object sender, FileOpenedEventArgs e)
			=> EditorTabControl.OpenFile(e.FilePath, e.EditorType, false);

		private void FileExplorer_FileChanged(object sender, FileSystemEventArgs e)
		{
			if (!IsMainWindowFocued)
				EditorTabControl.AddFileToReloadQueue(e.FullPath);
		}

		private void FileTreeView_FileRenamed(object sender, RenamedEventArgs e)
			=> EditorTabControl.RenameDocumentTabPage(e.OldFullPath, e.FullPath);

		private void FileTreeView_FileDeleted(object sender, FileSystemEventArgs e)
			=> EditorTabControl.CloseInvalidTabPages();

		private void FormFindReplace_FindAllPerformed(object sender, FindReplaceEventArgs e)
		{
			if (!DockPanel.ContainsContent(SearchResults))
			{
				SearchResults.DockArea = DarkDockArea.Bottom;
				DockPanel.AddContent(SearchResults);
			}

			SearchResults.DockGroup.SetVisibleContent(SearchResults);

			SearchResults.UpdateResults(e);
		}

		#endregion Events

		#region Event methods

		protected virtual void OnToolStripItemClicked(UIElement e)
		{
			HandleGlobalCommands(e);
			HandleTextEditorCommands(e);
		}

		protected void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			UndoMenuItem.Enabled = CurrentEditor != null && CurrentEditor.CanUndo;
			UndoMenuItem.Text = UndoMenuItem.Enabled ? Strings.Default.Undo : Strings.Default.CantUndo;

			UndoToolStripButton.Enabled = UndoMenuItem.Enabled;
			UndoToolStripButton.ToolTipText = UndoMenuItem.Text;

			// Redo buttons
			RedoMenuItem.Enabled = CurrentEditor != null && CurrentEditor.CanRedo;
			RedoMenuItem.Text = RedoMenuItem.Enabled ? Strings.Default.Redo : Strings.Default.CantRedo;

			RedoToolStripButton.Enabled = RedoMenuItem.Enabled;
			RedoToolStripButton.ToolTipText = RedoMenuItem.Text;

			// Save buttons
			SaveMenuItem.Enabled = CurrentEditor != null && CurrentEditor.IsContentChanged;
			SaveToolStripButton.Enabled = SaveMenuItem.Enabled;

			SaveAllMenuItem.Enabled = !EditorTabControl.IsEveryTabSaved();
			SaveAllToolStripButton.Enabled = SaveAllMenuItem.Enabled;
		}

		protected void UpdateViewMenu()
		{
			SetCheckedIfNotNull(ToolStripViewItem);
			SetCheckedIfNotNull(ContentExplorerViewItem);
			SetCheckedIfNotNull(FileExplorerViewItem);
			SetCheckedIfNotNull(ReferenceBrowserViewItem);
			SetCheckedIfNotNull(CompilerLogsViewItem);
			SetCheckedIfNotNull(SearchResultsViewItem);
			SetCheckedIfNotNull(StatusStripViewItem);
		}

		private void HandleGlobalCommands(UIElement command)
		{
			switch (command)
			{
				// File
				case UIElement.NewFile: FileExplorer.CreateNewFile(); break;
				case UIElement.Save: EditorTabControl.SaveFile(); UpdateUndoRedoSaveStates(); break;
				case UIElement.SaveAs: EditorTabControl.SaveFileAs(); UpdateUndoRedoSaveStates(); break;
				case UIElement.SaveAll: EditorTabControl.SaveAll(); UpdateUndoRedoSaveStates(); break;
				case UIElement.Build: Build(); break;

				// Edit
				case UIElement.Undo: CurrentEditor.Undo(); UpdateUndoRedoSaveStates(); break;
				case UIElement.Redo: CurrentEditor.Redo(); UpdateUndoRedoSaveStates(); break;
				case UIElement.Cut: CurrentEditor.Cut(); break;
				case UIElement.Copy: CurrentEditor.Copy(); break;
				case UIElement.Paste: CurrentEditor.Paste(); break;
				case UIElement.Find: FormFindReplace.Show(this, CurrentEditor.SelectedContent?.ToString()); break;
				case UIElement.SelectAll: CurrentEditor.SelectAll(); StatusStrip.UpdateStatus(CurrentEditor); break;

				// Options
				case UIElement.UseNewInclude:
				case UIElement.ShowLogsAfterBuild:
				case UIElement.ReindentOnSave:
					ToggleSetting(command); break;

				case UIElement.Settings: ShowSettingsForm(); break;

				// Help
				case UIElement.About: ShowAboutForm(); break;
			}

			if (command >= UIElement.ToolStrip && command <= UIElement.StatusStrip) // All "View" menu items
				ToggleItemVisibility(command);
		}

		private void HandleTextEditorCommands(UIElement command)
		{
			if (CurrentEditor is TextEditorBase textEditor)
				switch (command)
				{
					case UIElement.Reindent: textEditor.TidyCode(); break;
					case UIElement.TrimWhiteSpace: textEditor.TidyCode(true); break;
					case UIElement.CommentOut: textEditor.CommentOutLines(); break;
					case UIElement.Uncomment: textEditor.UncommentLines(); break;
					case UIElement.ToggleBookmark: textEditor.ToggleBookmark(); break;
					case UIElement.PrevBookmark: textEditor.GoToPrevBookmark(); break;
					case UIElement.NextBookmark: textEditor.GoToNextBookmark(); break;
					case UIElement.ClearBookmarks: textEditor.ClearAllBookmarks(this); break;
				}
		}

		protected void ToggleItemVisibility(UIElement command)
		{
			Control control = GetControlByKey<Control>(command.ToString());
			ToolStripMenuItem menuItem = MenuStrip.FindMenuItem(command);

			if (control is DarkToolWindow toolWindow)
			{
				ToggleToolWindow(toolWindow);
				menuItem.Checked = DockPanel.ContainsContent(toolWindow);
			}
			else
			{
				control.Visible = !control.Visible;
				menuItem.Checked = control.Visible;
			}
		}

		#endregion Event methods

		#region Abstract methods

		protected abstract void ApplyUserSettings(IEditorControl editor);
		protected abstract void ApplyUserSettings();
		protected abstract void Build();

		#endregion Abstract methods

		#region Other methods

		protected void ShowAboutForm()
		{
			using (var form = new FormAbout(Resources.AboutScreen_800))
				form.ShowDialog(this);
		}

		/// <summary>
		/// NOTE: Can only catch <c>public</c> fields. Returns <c>null</c> on failure.
		/// </summary>
		protected DarkDockContent FindDockContentByKey(string key)
			=> GetControlByKey<DarkDockContent>(key);

		/// <summary>
		/// NOTE: Can only catch <c>public</c> fields. Returns <c>null</c> on failure.
		/// </summary>
		protected T GetControlByKey<T>(string key) where T : Control
		{
			FieldInfo field = GetType().GetField(key);
			return field != null ? (field.GetValue(this) as T) : null;
		}

		protected void ToggleToolWindow(DarkToolWindow toolWindow)
		{
			if (toolWindow.DockPanel == null)
				DockPanel.AddContent(toolWindow);
			else
				DockPanel.RemoveContent(toolWindow);
		}

		protected void SetCheckedIfNotNull(ToolStripMenuItem item)
		{
			if (item != null)
			{
				var command = (UIElement)item.Tag;

				if (command == UIElement.ToolStrip || command == UIElement.StatusStrip)
					item.Checked = GetControlByKey<Control>(command.ToString()).Visible;
				else
					item.Checked = DockPanel.ContainsContent(GetControlByKey<DarkToolWindow>(command.ToString()));
			}
		}

		protected void ToggleSetting(UIElement command)
		{
			ToolStripMenuItem menuItem = MenuStrip.FindMenuItem(command);

			switch (command)
			{
				case UIElement.UseNewInclude: Configs.ClassicScript.UseNewIncludeMethod = menuItem.Checked; break;
				case UIElement.ShowLogsAfterBuild: IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild = menuItem.Checked; break;
				case UIElement.ReindentOnSave: IDE.Global.IDEConfiguration.ReindentOnSave = menuItem.Checked; break;
			}

			Configs.SaveAllConfigs();
			IDE.Global.IDEConfiguration.Save();
		}

		protected void ShowSettingsForm()
		{
			using (var form = new FormTextEditorSettings())
				if (form.ShowDialog() == DialogResult.OK)
				{
					Configs = new ConfigurationCollection();
					ApplyUserSettings();
				}
		}

		#endregion Other methods

		private void ContentExplorer_ObjectClicked(object sender, ObjectClickedEventArgs e)
			=> CurrentEditor.GoTo(e.ObjectName, e.IdentifyingObject);
	}
}

//{
//	if (CurrentEditor is ScriptTextEditor scriptEditor)
//		scriptEditor.UpdateSettings(ConfigurationCollection.CS_EditorConfiguration);
//	else if (CurrentEditor is StringEditor stringEditor)
//		stringEditor.UpdateSettings(null);
//	else if (CurrentEditor is LuaTextEditor luaEditor)
//		luaEditor.UpdateSettings(ConfigurationCollection.Lua_EditorConfiguration);

//	ToolStripMenuItem useNewInclude = MenuStrip.FindMenuItem(UICommand.UseNewInclude);
//	ToolStripMenuItem showLogsAfterBuild = MenuStrip.FindMenuItem(UICommand.ShowLogsAfterBuild);
//	ToolStripMenuItem reindentOnSave = MenuStrip.FindMenuItem(UICommand.ReindentOnSave);

//	if (useNewInclude != null)
//		useNewInclude.Checked = IDE.Global.IDEConfiguration.UseNewIncludeMethod;
//	if (showLogsAfterBuild != null)
//		showLogsAfterBuild.Checked = IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild;
//	if (reindentOnSave != null)
//		reindentOnSave.Checked = IDE.Global.IDEConfiguration.ReindentOnSave;
//}
