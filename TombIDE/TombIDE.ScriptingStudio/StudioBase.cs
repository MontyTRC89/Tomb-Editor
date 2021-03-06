using DarkUI.Docking;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Forms;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Forms;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio
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

				if (value is Form)
				{
					InitializeDockPanel();
					ApplyUserSettings();
				}
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

		public EditorTabControl EditorTabControl;

		/// <summary>
		/// Dockable document. (Parent of <c>EditorTabControl</c>)
		/// </summary>
		public DarkDocument EditorTabControlDocument;

		// Left:

		public ContentExplorer ContentExplorer;

		// Right:

		public FileExplorer FileExplorer;

		// Bottom:

		public CompilerLogs CompilerLogs;
		public SearchResults SearchResults;

		/* Very frequently accessed items */

		private ToolStripItem UndoMenuItem;
		private ToolStripItem UndoToolStripButton;

		private ToolStripItem RedoMenuItem;
		private ToolStripItem RedoToolStripButton;

		private ToolStripItem SaveMenuItem;
		private ToolStripItem SaveToolStripButton;

		private ToolStripItem SaveAsMenuItem;

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
			InitializeComponent();

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				MenuStrip.StudioMode = StudioMode;
				ToolStrip.StudioMode = StudioMode;

				InitializeFrequentlyAccessedItems(); // For quick control state updating

				InitializeTabControl();
				InitializeContentExplorer();
				InitializeFileExplorer();
				InitializeFindReplaceForm();

				CompilerLogs = new CompilerLogs();
				SearchResults = new SearchResults(EditorTabControl);

				IDE.Global.IDEEventRaised += OnIDEEventRaised;

				ScriptRootDirectoryPath = scriptRootDirectoryPath;
				EngineDirectoryPath = engineDirectoryPath;

				if (!string.IsNullOrWhiteSpace(initialFilePath))
					EditorTabControl.OpenFile(initialFilePath);
			}
		}

		private void InitializeTabControl()
		{
			EditorTabControl = new EditorTabControl();
			EditorTabControl.SelectedIndexChanged += EditorTabControl_SelectedIndexChanged;
			EditorTabControl.FileOpened += EditorTabControl_FileOpened;
			EditorTabControl.TabClosing += EditorTabControl_TabClosing;

			EditorTabControlDocument = new DarkDocument();
			EditorTabControlDocument.Controls.Add(EditorTabControl);
		}

		private void InitializeContentExplorer()
		{
			ContentExplorer = new ContentExplorer();
			ContentExplorer.ObjectClicked += ContentExplorer_ObjectClicked;
		}

		private void InitializeFileExplorer()
		{
			FileExplorer = new FileExplorer();
			FileExplorer.FileOpened += FileExplorer_FileOpened;
			FileExplorer.FileChanged += FileExplorer_FileChanged;
			FileExplorer.FileRenamed += FileExplorer_FileRenamed;
			FileExplorer.FileDeleted += FileExplorer_FileDeleted;
		}

		private void InitializeFindReplaceForm()
		{
			FormFindReplace = new FormFindReplace(EditorTabControl);
			FormFindReplace.FindAllPerformed += FormFindReplace_FindAllPerformed;
		}

		private void InitializeDockPanel()
		{
			DockPanel = new DarkDockPanel
			{
				Dock = DockStyle.Fill,
				EqualizeGroupSizes = true,
				Padding = new Padding(2),
				PrioritizeLeft = false,
				PrioritizeRight = false
			};

			DockPanel.ContentAdded += DockPanel_ContentChanged;
			DockPanel.ContentRemoved += DockPanel_ContentChanged;

			Controls.Add(DockPanel);

			DockPanel.BringToFront();

			// We have to initialize a dummy layout before applying any other one
			// otherwise we're gonna experience control priority issues.
			DockPanel.RestoreDockPanelState(DefaultLayouts.DummyLayout, FindDockContentByKey);

			// We can remove the content after the initialization is done
			DockPanel.RemoveContent();

			// The fact that we have to do this for it to work correctly is complete bullsh*t

			// Apply the current layout
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

			SaveAsMenuItem = MenuStrip.FindItem(UIElement.SaveAs);

			SaveAllMenuItem = MenuStrip.FindItem(UIElement.SaveAll);
			SaveAllToolStripButton = ToolStrip.FindItem(UIElement.SaveAll);

			ToolStripViewItem = MenuStrip.FindItem(UIElement.ToolStrip) as ToolStripMenuItem;
			ContentExplorerViewItem = MenuStrip.FindItem(UIElement.ContentExplorer) as ToolStripMenuItem;
			FileExplorerViewItem = MenuStrip.FindItem(UIElement.FileExplorer) as ToolStripMenuItem;
			ReferenceBrowserViewItem = MenuStrip.FindItem(UIElement.ReferenceBrowser) as ToolStripMenuItem;
			CompilerLogsViewItem = MenuStrip.FindItem(UIElement.CompilerLogs) as ToolStripMenuItem;
			SearchResultsViewItem = MenuStrip.FindItem(UIElement.SearchResults) as ToolStripMenuItem;
			StatusStripViewItem = MenuStrip.FindItem(UIElement.StatusStrip) as ToolStripMenuItem;
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
			if (obj is IDE.ProgramClosingEvent e)
				e.CanClose = EditorTabControl.AskSaveAll();
		}

		private void DockPanel_ContentChanged(object sender, DockContentEventArgs e)
			=> UpdateViewMenu();

		private void ToolStrip_ItemClicked(object sender, EventArgs e)
			=> OnToolStripItemClicked((UIElement)(sender as ToolStripItem).Tag);

		private void EditorTabControl_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdateUI();

		private void EditorTabControl_FileOpened(object sender, EventArgs e)
		{
			var editor = sender as IEditorControl;

			editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

			if (editor is TextEditorBase textEditor)
				textEditor.KeyDown += TextEditor_KeyDown;

			ApplyUserSettings(editor);

			UpdateUI();
		}

		private void EditorTabControl_TabClosing(object sender, TabControlCancelEventArgs e)
		{
			if (!e.Cancel && EditorTabControl.TabCount == 1)
				DocumentMode = DocumentMode.None;
		}

		private void Editor_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
			=> UpdateUndoRedoSaveStates();

		private void TextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
			&& (e.Key == System.Windows.Input.Key.F || e.Key == System.Windows.Input.Key.H))
				FormFindReplace.Show(this, (CurrentEditor as TextEditorBase).SelectedText);
		}

		private void ContentExplorer_ObjectClicked(object sender, ObjectClickedEventArgs e)
			=> CurrentEditor.GoToObject(e.ObjectName, e.IdentifyingObject);

		private void FileExplorer_FileOpened(object sender, FileOpenedEventArgs e)
			=> EditorTabControl.OpenFile(e.FilePath, e.EditorType);

		private void FileExplorer_FileChanged(object sender, FileSystemEventArgs e)
		{
			if (!IsMainWindowFocued)
				EditorTabControl.AddFileToReloadQueue(e.FullPath);
		}

		private void FileExplorer_FileRenamed(object sender, RenamedEventArgs e)
			=> EditorTabControl.RenameDocumentTabPage(e.OldFullPath, e.FullPath);

		private void FileExplorer_FileDeleted(object sender, FileSystemEventArgs e)
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

		private void UpdateUI()
		{
			if (CurrentEditor != null)
				DocumentMode = FileHelper.GetDocumentModeForFile(CurrentEditor.FilePath, CurrentEditor.EditorType);

			ContentExplorer.EditorControl = CurrentEditor;
			StatusStrip.EditorControl = CurrentEditor;

			UpdateUndoRedoSaveStates();
		}

		protected virtual void OnToolStripItemClicked(UIElement e)
		{
			HandleGlobalCommands(e);
			HandleDocumentCommands(e);
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

			SaveAsMenuItem.Enabled = EditorTabControl.SelectedTab != null;

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
				case UIElement.Save: EditorTabControl.SaveFile(); break;
				case UIElement.SaveAs: EditorTabControl.SaveFileAs(); break;
				case UIElement.SaveAll: EditorTabControl.SaveAll(); break;
				case UIElement.Build: Build(); break;

				// Edit
				case UIElement.Undo: CurrentEditor?.Undo(); break;
				case UIElement.Redo: CurrentEditor?.Redo(); break;
				case UIElement.Cut: CurrentEditor?.Cut(); break;
				case UIElement.Copy: CurrentEditor?.Copy(); break;
				case UIElement.Paste: CurrentEditor?.Paste(); break;
				case UIElement.Find: FormFindReplace.Show(this, CurrentEditor?.SelectedContent?.ToString()); break;
				case UIElement.SelectAll: CurrentEditor?.SelectAll(); break;

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

			UpdateUndoRedoSaveStates();
		}

		private void HandleDocumentCommands(UIElement command)
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
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

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

		protected void UpdateSettings()
		{
			UpdateSetting(UIElement.UseNewInclude);
			UpdateSetting(UIElement.ShowLogsAfterBuild);
			UpdateSetting(UIElement.ReindentOnSave);
		}

		protected void UpdateSetting(UIElement command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			if (menuItem != null)
				switch (command)
				{
					case UIElement.UseNewInclude: menuItem.Checked = IDE.Global.IDEConfiguration.UseNewIncludeMethod; break;
					case UIElement.ShowLogsAfterBuild: menuItem.Checked = IDE.Global.IDEConfiguration.ShowCompilerLogsAfterBuild; break;
					case UIElement.ReindentOnSave: menuItem.Checked = IDE.Global.IDEConfiguration.ReindentOnSave; break;
				}
		}

		protected void ToggleSetting(UIElement command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			switch (command)
			{
				case UIElement.UseNewInclude: IDE.Global.IDEConfiguration.UseNewIncludeMethod = menuItem.Checked; break;
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
	}
}
