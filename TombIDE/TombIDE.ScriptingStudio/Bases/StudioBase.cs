using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Forms;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Strings;
using FileExplorerToolWindow = TombIDE.ScriptingStudio.FileExplorer.FileExplorer;

namespace TombIDE.ScriptingStudio.Bases
{
	public abstract class StudioBase : Control
	{
		private static readonly UICommand[] CapabilityDrivenDocumentCommands =
		{
			UICommand.TrimWhiteSpace,
			UICommand.ToggleComment,
			UICommand.CommentOut,
			UICommand.Uncomment,
			UICommand.ToggleBookmark,
			UICommand.PrevBookmark,
			UICommand.NextBookmark,
			UICommand.ClearBookmarks,
			UICommand.PrevSection,
			UICommand.NextSection,
			UICommand.ClearString,
			UICommand.RemoveLastString,
			UICommand.Reindent,
			UICommand.GoToDefinition,
			UICommand.FindReferences,
			UICommand.RenameSymbol,
			UICommand.TypeFirstAvailableId,
			UICommand.NewFileAtCaret
		};

		private static readonly UICommand[] WorkspaceViewCommands =
		{
			UICommand.ToolStrip,
			UICommand.ContentExplorer,
			UICommand.FileExplorer,
			UICommand.ReferenceBrowser,
			UICommand.CompilerLogs,
			UICommand.SearchResults,
			UICommand.LuaDiagnostics,
			UICommand.LuaReferencesResults,
			UICommand.StatusStrip
		};

		#region Properties

		public DocumentMode DocumentMode
		{
			get => MenuStrip.DocumentMode;
			set
			{
				MenuStrip.DocumentModeContributionItems = GetDocumentMenuStripContributions(CurrentEditor, value);
				ToolStrip.DocumentModeContributionItems = GetDocumentToolStripContributions(CurrentEditor, value);
				EditorContextMenu.DocumentModeContributionItems = GetDocumentContextMenuContributions(CurrentEditor, value);
				UpdateStatusStripContributions(CurrentEditor, value);

				MenuStrip.DocumentMode = value;
				ToolStrip.DocumentMode = value;
				StatusStrip.DocumentMode = value;
				EditorContextMenu.DocumentMode = value;

				ContentExplorer.DocumentMode = value;
			}
		}

		public string ScriptRootDirectoryPath
		{
			get => EditorTabControl.ScriptRootDirectoryPath;
			set
			{
				EditorTabControl.ScriptRootDirectoryPath = value;

				if (FileExplorer != null)
				{
					FileExplorer.RootDirectoryPath = value;
					FileExplorer.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
				}
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

		protected ConfigurationCollection Configs;

		protected FormFindReplace FindReplaceForm;
		private readonly StudioShortcutBindingService _shortcutBindings;
		private readonly StudioEditorLifecycleCoordinator _editorLifecycleCoordinator;
		private readonly Dictionary<string, Control> _viewControlRegistry = new(StringComparer.Ordinal);
		private readonly Dictionary<string, DarkDockContent> _paneRegistryByKey = new(StringComparer.Ordinal);
		private readonly Dictionary<UICommand, DarkDockContent> _paneRegistryByCommand = new();

		public StudioMenuStrip MenuStrip;
		public StudioToolStrip ToolStrip;
		public StudioStatusStrip StatusStrip;
		public EditorContextMenu EditorContextMenu;

		protected DarkDockPanel DockPanel;

		/// <summary>
		/// Dockable document. (Parent of <c>EditorTabControl</c>)
		/// </summary>
		public DarkDocument EditorTabControlDocument;

		public EditorTabControl EditorTabControl;

		public ContentExplorer ContentExplorer;
		public FileExplorerToolWindow FileExplorer;
		public CompilerLogs CompilerLogs;
		public SearchResults SearchResults;

		/* Very frequently accessed items */

		protected ToolStripItem UndoMenuItem;
		protected ToolStripItem UndoToolStripButton;
		protected ToolStripItem RedoMenuItem;
		protected ToolStripItem RedoToolStripButton;
		protected ToolStripItem SaveMenuItem;
		protected ToolStripItem SaveToolStripButton;
		protected ToolStripItem SaveAsMenuItem;
		protected ToolStripItem SaveAllMenuItem;
		protected ToolStripItem SaveAllToolStripButton;

		protected ToolStripMenuItem ToolStripViewItem;
		protected ToolStripMenuItem ContentExplorerViewItem;
		protected ToolStripMenuItem FileExplorerViewItem;
		protected ToolStripMenuItem ReferenceBrowserViewItem;
		protected ToolStripMenuItem CompilerLogsViewItem;
		protected ToolStripMenuItem SearchResultsViewItem;
		protected ToolStripMenuItem LuaDiagnosticsViewItem;
		protected ToolStripMenuItem LuaReferencesResultsViewItem;
		protected ToolStripMenuItem StatusStripViewItem;

		#endregion Fields

		#region Construction

		public StudioBase(string scriptRootDirectoryPath, string engineDirectoryPath)
		{
			Configs = new ConfigurationCollection();
			_shortcutBindings = new StudioShortcutBindingService();

			InitializeToolStrips();
			InitializeTabControl();
			InitializeContentExplorer();
			InitializeFileExplorer();
			InitializeFindReplaceForm();
			_editorLifecycleCoordinator = new StudioEditorLifecycleCoordinator(
				EditorTabControl,
				ApplyUserSettings,
				UpdateUI,
				UpdateUndoRedoSaveStates,
				OnToolStripItemClicked,
				CanExecuteCommand,
				_shortcutBindings);
			_editorLifecycleCoordinator.Attach();

			CompilerLogs = new CompilerLogs();
			SearchResults = new SearchResults(NavigateToSearchResult);
			RegisterBuiltInPaneContributions();

			IDE.Instance.IDEEventRaised += OnIDEEventRaised;

			ScriptRootDirectoryPath = scriptRootDirectoryPath;
			EngineDirectoryPath = engineDirectoryPath;
		}

		private void InitializeToolStrips()
		{
			MenuStrip = new StudioMenuStrip() { Dock = DockStyle.Top, ShortcutBindingService = _shortcutBindings, StudioMode = StudioMode.None };
			MenuStrip.ItemClicked += ToolStrip_ItemClicked;
			MenuStrip.StudioModeChanged += MenuStrip_StudioModeChanged;

			ToolStrip = new StudioToolStrip() { Dock = DockStyle.Top, StudioMode = StudioMode.None };
			ToolStrip.ItemClicked += ToolStrip_ItemClicked;
			ToolStrip.StudioModeChanged += ToolStrip_StudioModeChanged;

			StatusStrip = new StudioStatusStrip() { Dock = DockStyle.Bottom };

			EditorContextMenu = new EditorContextMenu() { ShortcutBindingService = _shortcutBindings };
			EditorContextMenu.ItemClicked += ToolStrip_ItemClicked;

			Controls.Add(StatusStrip);
			Controls.Add(ToolStrip);
			Controls.Add(MenuStrip);

			RegisterViewControl(nameof(ToolStrip), ToolStrip);
			RegisterViewControl(nameof(StatusStrip), StatusStrip);

			InitializeFrequentlyAccessedMenuStripItems();
			InitializeFrequentlyAccessedToolStripItems();
		}

		private void InitializeTabControl()
		{
			EditorTabControl = new EditorTabControl();
			EditorTabControl.SelectedIndexChanged += EditorTabControl_SelectedIndexChanged;
			EditorTabControl.TabClosing += EditorTabControl_TabClosing;

			EditorTabControlDocument = new DarkDocument();
			EditorTabControlDocument.SerializationKey = "EditorTabControlDocument";
			EditorTabControlDocument.Controls.Add(EditorTabControl);
		}

		private void InitializeContentExplorer()
		{
			ContentExplorer = new ContentExplorer();
			ContentExplorer.ObjectClicked += ContentExplorer_ObjectClicked;
		}

		private void InitializeFileExplorer()
		{
			FileExplorer = new FileExplorerToolWindow();
			FileExplorer.FileOpened += FileExplorer_FileOpened;
			FileExplorer.FileChanged += FileExplorer_FileChanged;
			FileExplorer.FileRenamed += FileExplorer_FileRenamed;
			FileExplorer.FileDeleted += FileExplorer_FileDeleted;
		}

		private void InitializeFindReplaceForm()
		{
			FindReplaceForm = new FormFindReplace(EditorTabControl);
			FindReplaceForm.FindAllPerformed += FormFindReplace_FindAllPerformed;
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
			OnDockPanelLayoutRestored();

			ApplyMessageFilters();
		}

		private void InitializeFrequentlyAccessedMenuStripItems()
		{
			UndoMenuItem = MenuStrip.FindItem(UICommand.Undo);
			RedoMenuItem = MenuStrip.FindItem(UICommand.Redo);
			SaveMenuItem = MenuStrip.FindItem(UICommand.Save);
			SaveAsMenuItem = MenuStrip.FindItem(UICommand.SaveAs);
			SaveAllMenuItem = MenuStrip.FindItem(UICommand.SaveAll);

			ToolStripViewItem = MenuStrip.FindItem(UICommand.ToolStrip) as ToolStripMenuItem;
			ContentExplorerViewItem = MenuStrip.FindItem(UICommand.ContentExplorer) as ToolStripMenuItem;
			FileExplorerViewItem = MenuStrip.FindItem(UICommand.FileExplorer) as ToolStripMenuItem;
			ReferenceBrowserViewItem = MenuStrip.FindItem(UICommand.ReferenceBrowser) as ToolStripMenuItem;
			CompilerLogsViewItem = MenuStrip.FindItem(UICommand.CompilerLogs) as ToolStripMenuItem;
			SearchResultsViewItem = MenuStrip.FindItem(UICommand.SearchResults) as ToolStripMenuItem;
			LuaDiagnosticsViewItem = MenuStrip.FindItem(UICommand.LuaDiagnostics) as ToolStripMenuItem;
			LuaReferencesResultsViewItem = MenuStrip.FindItem(UICommand.LuaReferencesResults) as ToolStripMenuItem;
			StatusStripViewItem = MenuStrip.FindItem(UICommand.StatusStrip) as ToolStripMenuItem;

			ApplyWorkspaceCommandSurface();
		}

		private void InitializeFrequentlyAccessedToolStripItems()
		{
			UndoToolStripButton = ToolStrip.FindItem(UICommand.Undo);
			RedoToolStripButton = ToolStrip.FindItem(UICommand.Redo);
			SaveToolStripButton = ToolStrip.FindItem(UICommand.Save);
			SaveAllToolStripButton = ToolStrip.FindItem(UICommand.SaveAll);

			ApplyWorkspaceCommandSurface();
		}

		private void ApplyMessageFilters()
		{
			Application.AddMessageFilter(DockPanel.DockContentDragFilter);
			Application.AddMessageFilter(DockPanel.DockResizeFilter);
		}

		protected void ApplyWorkspaceProfileCommandSurfaceContributions()
		{
			if (WorkspaceProfile is null)
				return;

			MenuStrip.StudioModeContributionItems = WorkspaceProfile.MenuStripContributions;
			ToolStrip.StudioModeContributionItems = WorkspaceProfile.ToolStripContributions;

			MenuStrip.RebuildStudioModeItems();
			ToolStrip.RebuildStudioModeItems();
			UpdateStatusStripContributions(CurrentEditor, DocumentMode);
		}

		protected void ApplyWorkspaceProfileStartupPolicy()
		{
			if (WorkspaceProfile is null)
				return;

			DockPanelState = WorkspaceProfile.LoadDockPanelState();
			FileExplorer.ExcludedDirectoryFilter = WorkspaceProfile.FileExplorerExcludedDirectoryFilter;
			FileExplorer.Filter = WorkspaceProfile.FileExplorerFilter;
			FileExplorer.CommentPrefix = WorkspaceProfile.CommentPrefix;

			WorkspaceProfile.RegisterEditors(EditorTabControl);
			EditorTabControl.CheckPreviousSession();

			if (!string.IsNullOrWhiteSpace(WorkspaceProfile.InitialFilePath))
				EditorTabControl.OpenFile(WorkspaceProfile.InitialFilePath);
		}

		protected void ApplyPaneContributionProvider()
		{
			if (PaneContributionProvider is null)
				return;

			foreach (StudioPaneContribution contribution in PaneContributionProvider.GetPaneContributions())
				RegisterPaneContribution(contribution);
		}

		#endregion Construction

		#region Override / new region

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

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			UpdateViewMenu();
		}

		#endregion Override / new region

		#region Virtual region

		protected virtual void OnIDEEventRaised(IIDEEvent obj)
		{
			if (obj is IDE.ProgramClosingEvent e)
			{
				e.CanClose = EditorTabControl.AskSaveAll();

				if (WorkspaceProfile is not null)
					WorkspaceProfile.SaveDockPanelState(DockPanel.GetDockPanelState());
			}

			WorkspaceAutomationProvider?.HandleIDEEvent(obj);
		}

		protected virtual void OnToolStripItemClicked(UICommand e)
		{
			HandleGlobalCommands(e);
			HandleDocumentCommands(e);
		}

		protected virtual void OnDockPanelLayoutRestored()
		{ }

		protected virtual bool CanExecuteUndo()
			=> CurrentEditor is not null && CurrentEditor.CanUndo;

		protected virtual bool CanExecuteRedo()
			=> CurrentEditor is not null && CurrentEditor.CanRedo;

		protected virtual void ExecuteUndo()
			=> CurrentEditor?.Undo();

		protected virtual void ExecuteRedo()
			=> CurrentEditor?.Redo();

		protected virtual IStudioDocumentCommandStatusProvider DocumentCommandStatusProvider
			=> null;

		protected virtual IStudioDocumentCommandHandler DocumentCommandHandler
			=> null;

		protected virtual ScriptingWorkspaceProfile WorkspaceProfile
			=> null;

		protected virtual IStudioDocumentCommandSurfaceProvider DocumentCommandSurfaceProvider
			=> XmlDocumentCommandSurfaceProvider.Instance;

		protected virtual IStudioPaneContributionProvider PaneContributionProvider
			=> null;

		protected virtual IStudioDocumentStatusStripProvider DocumentStatusStripProvider
			=> null;

		protected virtual IStudioWorkspaceAutomationProvider WorkspaceAutomationProvider
			=> null;

		protected virtual IReadOnlyList<StudioToolStripItem> GetDocumentContextMenuContributions(IEditorControl editor, DocumentMode documentMode)
			=> editor is null || DocumentCommandSurfaceProvider is null
				? []
				: DocumentCommandSurfaceProvider.GetContextMenuItems(editor, documentMode);

		protected virtual IReadOnlyList<StudioToolStripItem> GetDocumentMenuStripContributions(IEditorControl editor, DocumentMode documentMode)
			=> editor is null || DocumentCommandSurfaceProvider is null
				? []
				: DocumentCommandSurfaceProvider.GetMenuStripItems(editor, documentMode);

		protected virtual IReadOnlyList<StudioToolStripItem> GetDocumentToolStripContributions(IEditorControl editor, DocumentMode documentMode)
			=> editor is null || DocumentCommandSurfaceProvider is null
				? []
				: DocumentCommandSurfaceProvider.GetToolStripItems(editor, documentMode);

		protected virtual IReadOnlyList<StudioStatusStripSegment> GetDocumentStatusStripSegments(IEditorControl editor, DocumentMode documentMode)
			=> editor is null || DocumentStatusStripProvider is null
				? []
				: DocumentStatusStripProvider.GetSegments(editor, documentMode);

		protected bool CanExecuteCommand(UICommand command)
		{
			switch (command)
			{
				case UICommand.Undo:
					return CanExecuteUndo();

				case UICommand.Redo:
					return CanExecuteRedo();

				case UICommand.Save:
					return CurrentEditor != null && CurrentEditor.IsContentChanged;

				case UICommand.SaveAs:
				case UICommand.Cut:
				case UICommand.Copy:
				case UICommand.Paste:
				case UICommand.Find:
				case UICommand.SelectAll:
					return CurrentEditor != null;

				case UICommand.SaveAll:
					return !EditorTabControl.IsEveryTabSaved();

				default:
					if (TryGetDocumentCommandEnabled(command, out bool isEnabled))
						return isEnabled;

					return true;
			}
		}

		#endregion Virtual region

		#region Abstract region

		protected abstract void ApplyUserSettings(IEditorControl editor);
		protected abstract void ApplyUserSettings();
		protected virtual void Build()
			=> WorkspaceAutomationProvider?.Build();

		protected virtual void RestoreDefaultLayout()
		{
			if (WorkspaceProfile is null)
				return;

			DockPanelState = WorkspaceProfile.DefaultLayout;

			DockPanel.RemoveContent();
			DockPanel.RestoreDockPanelState(DockPanelState, FindDockContentByKey);
			OnDockPanelLayoutRestored();
		}

		protected virtual void ShowDocumentation()
			=> WorkspaceAutomationProvider?.ShowDocumentation();

		#endregion Abstract region

		#region Events

		private void DockPanel_ContentChanged(object sender, DockContentEventArgs e)
			=> UpdateViewMenu();

		private void MenuStrip_StudioModeChanged(object sender, EventArgs e)
			=> InitializeFrequentlyAccessedMenuStripItems();

		private void ToolStrip_StudioModeChanged(object sender, EventArgs e)
			=> InitializeFrequentlyAccessedToolStripItems();

		private void ToolStrip_ItemClicked(object sender, EventArgs e)
			=> OnToolStripItemClicked(((sender as ToolStripItem).Tag as UIElementArgs).Command);

		private void EditorTabControl_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdateUI();

		private void EditorTabControl_TabClosing(object sender, TabControlCancelEventArgs e)
		{
			if (!e.Cancel && EditorTabControl.TabCount == 1)
				DocumentMode = DocumentMode.None;
		}

		private void ContentExplorer_ObjectClicked(object sender, ObjectClickedEventArgs e)
			=> CurrentEditor.GoToObject(e.ObjectName, e.IdentifyingObject);

		private void FileExplorer_FileOpened(object sender, FileOpenedEventArgs e)
		{
			if (e.OpenSourceView)
				EditorTabControl.OpenSourceFile(e.FilePath);
			else
				EditorTabControl.OpenFile(e.FilePath, e.EditorType);
		}

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
			ShowPane(UICommand.SearchResults);
			SearchResults.UpdateResults(e);
		}

		#endregion Events

		#region Event methods

		protected void UpdateUI()
		{
			if (CurrentEditor != null)
				DocumentMode = EditorTabControl.GetDocumentMode(CurrentEditor);

			if (EditorTabControl.SelectedTab != null)
			{
				Control hostedControl = EditorTabControl.GetHostedControl(EditorTabControl.SelectedTab);

				if (hostedControl != null)
					hostedControl.ContextMenuStrip = EditorContextMenu;
			}

			ContentExplorer.EditorControl = CurrentEditor;
			StatusStrip.EditorControl = CurrentEditor;
			UpdateStatusStripContributions(CurrentEditor, DocumentMode);

			UpdateUndoRedoSaveStates();
			UpdateDocumentCommandStates();
		}

		protected void UpdateUndoRedoSaveStates()
		{
			// Undo buttons
			UndoMenuItem.Enabled = CanExecuteUndo();
			UndoMenuItem.Text = UndoMenuItem.Enabled ? Strings.Default.Undo : Strings.Default.CantUndo;

			UndoToolStripButton.Enabled = UndoMenuItem.Enabled;
			UndoToolStripButton.ToolTipText = UndoMenuItem.Text;

			// Redo buttons
			RedoMenuItem.Enabled = CanExecuteRedo();
			RedoMenuItem.Text = RedoMenuItem.Enabled ? Strings.Default.Redo : Strings.Default.CantRedo;

			RedoToolStripButton.Enabled = RedoMenuItem.Enabled;
			RedoToolStripButton.ToolTipText = RedoMenuItem.Text;

			// Save buttons
			SaveMenuItem.Enabled = CurrentEditor != null && CurrentEditor.IsContentChanged;
			SaveToolStripButton.Enabled = SaveMenuItem.Enabled;

			SaveAsMenuItem.Enabled = CurrentEditor != null;

			SaveAllMenuItem.Enabled = !EditorTabControl.IsEveryTabSaved();
			SaveAllToolStripButton.Enabled = SaveAllMenuItem.Enabled;
		}

		protected void UpdateDocumentCommandStates()
		{
			foreach (UICommand command in CapabilityDrivenDocumentCommands)
				SetCommandEnabled(command, CanExecuteCommand(command));
		}

		protected void SetCommandEnabled(UICommand command, bool isEnabled)
		{
			SetCommandEnabled(MenuStrip.FindItem(command), isEnabled);
			SetCommandEnabled(ToolStrip.FindItem(command), isEnabled);
			SetCommandEnabled(EditorContextMenu.FindItem(command), isEnabled);
		}

		private bool TryGetDocumentCommandEnabled(UICommand command, out bool isEnabled)
		{
			if (TryGetBuiltInDocumentCommandEnabled(command, out isEnabled))
			{
				if (CurrentEditor is not null
					&& DocumentCommandStatusProvider is not null
					&& DocumentCommandStatusProvider.TryGetEnabled(CurrentEditor, command, out bool providerEnabled))
				{
					isEnabled = providerEnabled;
				}

				return true;
			}

			if (CurrentEditor is not null
				&& DocumentCommandStatusProvider is not null
				&& DocumentCommandStatusProvider.TryGetEnabled(CurrentEditor, command, out isEnabled))
			{
				return true;
			}

			isEnabled = false;
			return false;
		}

		private bool TryGetBuiltInDocumentCommandEnabled(UICommand command, out bool isEnabled)
		{
			if (CurrentEditor is TextEditorBase)
			{
				switch (command)
				{
					case UICommand.TabsToSpaces:
					case UICommand.SpacesToTabs:
					case UICommand.Reindent:
					case UICommand.TrimWhiteSpace:
					case UICommand.ToggleComment:
					case UICommand.CommentOut:
					case UICommand.Uncomment:
					case UICommand.ToggleBookmark:
					case UICommand.PrevBookmark:
					case UICommand.NextBookmark:
					case UICommand.ClearBookmarks:
						isEnabled = true;
						return true;
				}
			}

			if (CurrentEditor is StringEditor)
			{
				switch (command)
				{
					case UICommand.PrevSection:
					case UICommand.NextSection:
					case UICommand.ClearString:
					case UICommand.RemoveLastString:
						isEnabled = true;
						return true;
				}
			}

			switch (command)
			{
				case UICommand.TrimWhiteSpace:
				case UICommand.ToggleComment:
				case UICommand.CommentOut:
				case UICommand.Uncomment:
				case UICommand.ToggleBookmark:
				case UICommand.PrevBookmark:
				case UICommand.NextBookmark:
				case UICommand.ClearBookmarks:
				case UICommand.PrevSection:
				case UICommand.NextSection:
				case UICommand.ClearString:
				case UICommand.RemoveLastString:
				case UICommand.Reindent:
				case UICommand.GoToDefinition:
				case UICommand.FindReferences:
				case UICommand.RenameSymbol:
				case UICommand.TypeFirstAvailableId:
				case UICommand.NewFileAtCaret:
					isEnabled = false;
					return true;
				default:
					isEnabled = false;
					return false;
			}
		}

		private static void SetCommandEnabled(ToolStripItem item, bool isEnabled)
		{
			if (item != null)
				item.Enabled = isEnabled;
		}

		protected void UpdateViewMenu()
		{
			ApplyWorkspaceCommandSurface();

			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				if (field.Name.EndsWith("ViewItem") && field.GetValue(this) is ToolStripMenuItem fieldValue)
					SetCheckedIfNotNull(fieldValue);
		}

		private void ApplyWorkspaceCommandSurface()
		{
			if (WorkspaceProfile is null)
				return;

			foreach (UICommand command in WorkspaceViewCommands)
				SetCommandVisible(command, WorkspaceProfile.SupportsView(command));

			SetCommandVisible(UICommand.Build, WorkspaceProfile.SupportsBuild);
			SetCommandVisible(UICommand.ShowLogsAfterBuild, WorkspaceProfile.SupportsBuild);
			SetCommandVisible(UICommand.ScriptingDocumentation, WorkspaceProfile.SupportsDocumentation);
		}

		private void SetCommandVisible(UICommand command, bool isVisible)
		{
			SetCommandVisible(MenuStrip.FindItem(command), isVisible);
			SetCommandVisible(ToolStrip.FindItem(command), isVisible);
			SetCommandVisible(EditorContextMenu.FindItem(command), isVisible);
		}

		private static void SetCommandVisible(ToolStripItem item, bool isVisible)
		{
			if (item != null)
				item.Visible = isVisible;
		}

		private void HandleGlobalCommands(UICommand command)
		{
			switch (command)
			{
				// File
				case UICommand.NewFile: FileExplorer.CreateNewFile(); break;
				case UICommand.Save: EditorTabControl.SaveFile(); break;
				case UICommand.SaveAs: EditorTabControl.SaveFileAs(); break;
				case UICommand.SaveAll: EditorTabControl.SaveAll(); break;
				case UICommand.Build:
					if (WorkspaceProfile is null || WorkspaceProfile.SupportsBuild)
						Build();
					break;
				case UICommand.Exit: IDE.Instance.RequestProgramClose(); break;

				// Edit
					case UICommand.Undo: ExecuteUndo(); break;
					case UICommand.Redo: ExecuteRedo(); break;
				case UICommand.Cut: CurrentEditor?.Cut(); break;
				case UICommand.Copy: CurrentEditor?.Copy(); break;
				case UICommand.Paste: CurrentEditor?.Paste(); break;
				case UICommand.Find: FindReplaceForm.Show(this, CurrentEditor?.SelectedContent?.ToString()); break;
				case UICommand.SelectAll: CurrentEditor?.SelectAll(); break;

				// Options
				case UICommand.UseNewInclude:
				case UICommand.ShowLogsAfterBuild:
				case UICommand.ReindentOnSave:
					ToggleSetting(command); break;

				case UICommand.Settings: ShowSettingsForm(); break;

				// View
				case UICommand.RestoreDefaultLayout: RestoreDefaultLayout(); break;

				// Help
				case UICommand.ScriptingDocumentation:
					if (WorkspaceProfile is null || WorkspaceProfile.SupportsDocumentation)
						ShowDocumentation();
					break;
				case UICommand.About: ShowAboutForm(); break;
			}

			if (command >= UICommand.ToolStrip && command <= UICommand.StatusStrip) // All "View" menu items
				ToggleItemVisibility(command);

			UpdateUndoRedoSaveStates();
		}

		protected virtual void HandleDocumentCommands(UICommand command)
		{
			if (DocumentCommandHandler?.TryHandle(command) == true)
				return;

			if (CurrentEditor is TextEditorBase textEditor)
				switch (command)
				{
					case UICommand.TabsToSpaces: textEditor.ConvertTabsToSpaces(); break;
					case UICommand.SpacesToTabs: textEditor.ConvertSpacesToTabs(); break;
					case UICommand.Reindent: textEditor.TidyCode(); break;
					case UICommand.TrimWhiteSpace: textEditor.TidyCode(true); break;
					case UICommand.ToggleComment: textEditor.ToggleCommentLines(); break;
					case UICommand.CommentOut: textEditor.CommentOutLines(); break;
					case UICommand.Uncomment: textEditor.UncommentLines(); break;
					case UICommand.ToggleBookmark: textEditor.ToggleBookmark(); break;
					case UICommand.PrevBookmark: textEditor.GoToPrevBookmark(); break;
					case UICommand.NextBookmark: textEditor.GoToNextBookmark(); break;
					case UICommand.ClearBookmarks: textEditor.ClearAllBookmarks(() => ConfirmBookmarkClear(this)); break;
				}

			if (CurrentEditor is StringEditor stringEditor)
				switch (command)
				{
					case UICommand.PrevSection:
						stringEditor.GoToPreviousSection();
						ContentExplorer.SelectNode(stringEditor.CurrentDataGrid.Name);
						break;

					case UICommand.NextSection:
						stringEditor.GoToNextSection();
						ContentExplorer.SelectNode(stringEditor.CurrentDataGrid.Name);
						break;

					case UICommand.ClearString: stringEditor.CurrentDataGrid?.ClearSelectedString(); break;
					case UICommand.RemoveLastString: stringEditor.CurrentDataGrid?.RemoveLastString(); break;
				}
		}

		private static bool ConfirmBookmarkClear(IWin32Window promptOwner)
			=> DarkMessageBox.Show(
				promptOwner,
				"Are you sure you want to clear all bookmarks from the current document?",
				"Are you sure?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.Yes;

		protected virtual void NavigateToSearchResult(string filePath, FindReplaceItem item)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return;

			EditorTabControl.OpenFile(filePath);

			if (CurrentEditor is not TextEditorBase textEditor)
				return;

			if (!EditorNavigationHelper.TryCreateSearchResultLocation(textEditor, filePath, item, out EditorNavigationLocation? location)
				|| location is null)
				return;

			EditorNavigationHelper.ApplyLocation(textEditor, location.Value);
		}

		protected void ToggleItemVisibility(UICommand command)
		{
			Control control = GetControlByKey<Control>(command.ToString());
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			if (control is null || menuItem is null)
				return;

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
			=> _paneRegistryByKey.TryGetValue(key, out DarkDockContent content)
				? content
				: GetControlByKey<DarkDockContent>(key);

		/// <summary>
		/// NOTE: Can only catch <c>public</c> fields. Returns <c>null</c> on failure.
		/// </summary>
		protected T GetControlByKey<T>(string key) where T : Control
		{
			if (_viewControlRegistry.TryGetValue(key, out Control registeredControl))
				return registeredControl as T;

			FieldInfo field = GetType().GetField(key);
			return field != null ? (field.GetValue(this) as T) : null;
		}

		protected T GetPaneContent<T>(UICommand command) where T : DarkDockContent
			=> _paneRegistryByCommand.TryGetValue(command, out DarkDockContent content)
				? content as T
				: null;

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
				UICommand command = (item.Tag as UIElementArgs).Command;

				if (command == UICommand.ToolStrip || command == UICommand.StatusStrip)
					item.Checked = GetControlByKey<Control>(command.ToString())?.Visible ?? false;
				else
				{
					DarkToolWindow toolWindow = GetControlByKey<DarkToolWindow>(command.ToString());
					item.Checked = toolWindow != null && DockPanel != null && DockPanel.ContainsContent(toolWindow);
				}
			}
		}

		protected void UpdateSettings()
		{
			UpdateSetting(UICommand.UseNewInclude);
			UpdateSetting(UICommand.ShowLogsAfterBuild);
			UpdateSetting(UICommand.ReindentOnSave);
		}

		protected void ApplyUserSettingsToOpenEditors(Action<IEditorControl> afterApply = null, Action afterAll = null)
		{
			foreach (TabPage tab in EditorTabControl.TabPages)
			{
				IEditorControl editor = EditorTabControl.GetEditorOfTab(tab);
				ApplyUserSettings(editor);
				afterApply?.Invoke(editor);
			}

			afterAll?.Invoke();
			UpdateSettings();
		}

		protected void UpdateSetting(UICommand command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			if (menuItem != null)
				switch (command)
				{
					case UICommand.UseNewInclude: menuItem.Checked = IDE.Instance.IDEConfiguration.UseNewIncludeMethod; break;
					case UICommand.ShowLogsAfterBuild: menuItem.Checked = IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild; break;
					case UICommand.ReindentOnSave: menuItem.Checked = IDE.Instance.IDEConfiguration.ReindentOnSave; break;
				}
		}

		protected void ToggleSetting(UICommand command)
		{
			var menuItem = MenuStrip.FindItem(command) as ToolStripMenuItem;

			switch (command)
			{
				case UICommand.UseNewInclude: IDE.Instance.IDEConfiguration.UseNewIncludeMethod = menuItem.Checked; break;
				case UICommand.ShowLogsAfterBuild: IDE.Instance.IDEConfiguration.ShowCompilerLogsAfterBuild = menuItem.Checked; break;
				case UICommand.ReindentOnSave: IDE.Instance.IDEConfiguration.ReindentOnSave = menuItem.Checked; break;
			}

			Configs.SaveAllConfigs();
			IDE.Instance.IDEConfiguration.Save();
		}

		protected void ShowSettingsForm()
		{
			if (WorkspaceProfile is null)
				return;

			var viewModel = new ScriptingSettingsWindowViewModel(WorkspaceProfile, DocumentMode);
			var window = new ScriptingSettingsWindow { DataContext = viewModel };

			if (FindForm() is Form ownerForm)
				new System.Windows.Interop.WindowInteropHelper(window).Owner = ownerForm.Handle;

			if (window.ShowDialog() == true)
			{
				Configs = new ConfigurationCollection();
				ApplyUserSettings();
			}
		}

		protected void ShowPane(UICommand command, DarkDockArea defaultDockArea = DarkDockArea.Bottom)
		{
			if (GetPaneContent<DarkToolWindow>(command) is not DarkToolWindow toolWindow)
				return;

			if (!DockPanel.ContainsContent(toolWindow))
			{
				toolWindow.DockArea = defaultDockArea;
				DockPanel.AddContent(toolWindow);
			}

			toolWindow.DockGroup.SetVisibleContent(toolWindow);
		}

		private void RegisterBuiltInPaneContributions()
		{
			RegisterPaneContribution(new StudioPaneContribution(UICommand.ContentExplorer, nameof(ContentExplorer), () => ContentExplorer));
			RegisterPaneContribution(new StudioPaneContribution(UICommand.FileExplorer, nameof(FileExplorer), () => FileExplorer));
			RegisterPaneContribution(new StudioPaneContribution(UICommand.CompilerLogs, nameof(CompilerLogs), () => CompilerLogs));
			RegisterPaneContribution(new StudioPaneContribution(UICommand.SearchResults, nameof(SearchResults), () => SearchResults));
		}

		private void RegisterPaneContribution(StudioPaneContribution contribution)
		{
			if (contribution is null || _paneRegistryByCommand.ContainsKey(contribution.Command))
				return;

			DarkDockContent content = contribution.CreateContent();
			if (content is null)
				return;

			_paneRegistryByCommand[contribution.Command] = content;
			_paneRegistryByKey[contribution.SerializationKey] = content;
			RegisterViewControl(contribution.Command.ToString(), content);
			RegisterViewControl(contribution.SerializationKey, content);
		}

		private void RegisterViewControl(string key, Control control)
		{
			if (string.IsNullOrWhiteSpace(key) || control is null)
				return;

			_viewControlRegistry[key] = control;
		}

		private void UpdateStatusStripContributions(IEditorControl editor, DocumentMode documentMode)
		{
			var segments = new HashSet<StudioStatusStripSegment>();

			if (WorkspaceProfile?.StatusStripSegments is not null)
				foreach (StudioStatusStripSegment segment in WorkspaceProfile.StatusStripSegments)
					segments.Add(segment);

			foreach (StudioStatusStripSegment segment in GetDocumentStatusStripSegments(editor, documentMode))
				segments.Add(segment);

			StatusStrip.SegmentContributions = segments.ToArray();
		}

		#endregion Other methods
	}
}
