using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.EditorTabs;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls
{
	public class EditorTabControl : CustomTabControl
	{
		#region Properties

		private string _scriptRootDirectoryPath;
		public string ScriptRootDirectoryPath
		{
			get => _scriptRootDirectoryPath;
			set
			{
				if (AskSaveAll())
				{
					SharedMethods.DisposeItems(TabPages.Cast<TabPage>());

					_scriptRootDirectoryPath = value;
				}
			}
		}

		public IEditorControl CurrentEditor => SelectedTab != null ? GetEditorOfTab(SelectedTab) : null;

		public bool ReloadQueueRunning => _fileReloadCoordinator.IsRunning;

		#endregion Properties

		#region Fields

		private DarkContextMenu _contextMenu = new DarkContextMenu();
		private ToolStripMenuItem menuItem_Save = new ToolStripMenuItem(Strings.Default.Save);
		private ToolStripMenuItem menuItem_Close = new ToolStripMenuItem(Strings.Default.Close);
		private ToolStripMenuItem menuItem_OpenFolder = new ToolStripMenuItem(Strings.Default.OpenContainingFolder);

		private ToolTip _toolTip = new ToolTip();

		private readonly EditorFactoryService _editorFactory = new EditorFactoryService();
		private readonly EditorTabHostService _editorHostService = new EditorTabHostService();
		private readonly FileReloadCoordinator _fileReloadCoordinator = new FileReloadCoordinator();

		private Version _currentEngineVersion = new(0, 0);

		#endregion Fields

		#region Construction

		public EditorTabControl() : this(string.Empty)
		{ }
		public EditorTabControl(string scriptRootDirectoryPath)
		{
			SetNewDefaultSettings();

			ScriptRootDirectoryPath = scriptRootDirectoryPath;

			InitializeContextMenu();

			_currentEngineVersion = IDE.Instance.Project.GetCurrentEngineVersion();
		}

		private void SetNewDefaultSettings()
		{
			AllowDrop = true;
			Dock = DockStyle.Fill;
			DisplayStyle = TabStyle.Dark;
			DisplayStyleProvider.ShowTabCloser = true;
			EnableMiddleClickTabClosing = true;
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
		}

		private void InitializeContextMenu()
		{
			menuItem_Save.Image = Resources.Save_16;
			menuItem_Close.Image = Resources.Delete_16;
			menuItem_OpenFolder.Image = Resources.ForwardArrow_16;

			menuItem_Save.Click += MenuItem_Save_Click;
			menuItem_Close.Click += MenuItem_Close_Click;
			menuItem_OpenFolder.Click += MenuItem_OpenFolder_Click;

			_contextMenu.Items.Add(menuItem_Save);
			_contextMenu.Items.Add(menuItem_Close);
			_contextMenu.Items.Add(new ToolStripSeparator());
			_contextMenu.Items.Add(menuItem_OpenFolder);
		}

		#endregion Construction

		#region Session

		/// <summary>
		/// Checks if the previous session crashed and left unhandled backup files.
		/// </summary>
		public void CheckPreviousSession()
		{
			string[] files = Directory.GetFiles(ScriptRootDirectoryPath, $"*{SupportedFormats.Backup}", SearchOption.AllDirectories);

			if (files.Length != 0) // If backup files exist
			{
				DialogResult result = MessageBox.Show(
					Strings.Default.AskRestoreSession, Strings.Default.RestoreSessionMBT,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					RestoreSession(files);
				else if (result == DialogResult.No)
					SharedMethods.DeleteFiles(files); // Deletes all backup files
			}
		}

		private void RestoreSession(string[] files)
		{
			foreach (string file in files)
			{
				if (!File.Exists(file))
					continue;

				string backupFileContent = File.ReadAllText(file);
				string originalFilePath = FileHelper.GetOriginalFilePathFromBackupFile(file);

				// Open the original file and replace the whole text of the TextEditor with the backup file content
				OpenFile(originalFilePath);
				CurrentEditor.Content = backupFileContent;
			}
		}

		#endregion Session

		#region File opening

		public Control GetHostedControl(TabPage tab)
			=> _editorHostService.GetHostedControl(tab);

		public DocumentMode GetDocumentMode(IEditorControl editor)
			=> editor is null ? DocumentMode.None : _editorFactory.GetDocumentMode(editor);

		public TabPage FindSourceTabPage(string filePath)
			=> FindTabPage(filePath, _editorFactory.GetSourceViewEditorType(filePath));

		public void OpenSourceFile(string filePath, bool silentSession = false)
			=> OpenFile(filePath, _editorFactory.GetSourceViewEditorType(filePath), silentSession);

		public void RegisterJson5Editor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsJson5File, static _ => true, factory);

		public void RegisterLuaEditor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsLuaFile, static _ => true, factory);

		public void RegisterPlainTextEditor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> _editorFactory.SetPlainTextEditorFactory(factory, documentMode);

		public void RegisterStringsEditor(Func<Version, IEditorControl> factory)
			=> RegisterEditor(
				EditorType.Strings,
				DocumentMode.Strings,
				FileHelper.IsTextFile,
				filePath => FileHelper.GetClassicScriptFileKind(filePath) == ClassicScriptFileKind.Strings,
				factory);

		public void RegisterTextEditor(
			Func<Version, IEditorControl> factory,
			DocumentMode documentMode)
			=> RegisterTextEditor(factory, documentMode, _ => true);

		public void RegisterTextEditor(
			Func<Version, IEditorControl> factory,
			DocumentMode documentMode,
			Func<string, bool> isDefaultForFile)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsTextFile, isDefaultForFile, factory);

		public void OpenFile(string filePath, EditorType editorType = EditorType.Default, bool silentSession = false)
		{
			TabPage fileTabPage = FindTabPage(filePath, editorType);

			if (fileTabPage != null)
				SelectTab(fileTabPage);
			else
				OpenFileInNewTabPage(filePath, editorType, silentSession);
		}

		private void OpenFileInNewTabPage(string filePath, EditorType editorType, bool silentSession)
		{
			IEditorControl newEditor = InitializeEditor(filePath, editorType, silentSession);

			if (newEditor is not null)
			{
				string tabPageTitle = BuildTabPageTitleText(newEditor.FilePath, newEditor.EditorType);
				var newTabPage = new TabPage(tabPageTitle);
				Control tabPageContent = InitializeTabPageContent(newEditor);
				newTabPage.Controls.Add(tabPageContent);

				TabPages.Add(newTabPage);
				SelectTab(newTabPage);

				OnFileOpened(EventArgs.Empty);
			}
		}

		private IEditorControl InitializeEditor(string filePath, EditorType editorType, bool silentSession)
		{
			IEditorControl newEditor = _editorFactory.CreateEditor(filePath, editorType, _currentEngineVersion);

			if (newEditor is null)
				return null;

			newEditor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

			if (File.Exists(filePath))
				newEditor.Load(filePath, silentSession);
			else
				newEditor.FilePath = filePath;

			return newEditor;
		}

		private Control InitializeTabPageContent(IEditorControl editor)
			=> _editorHostService.CreateHostControl(editor);

		private void RegisterEditor(
			EditorType editorType,
			DocumentMode documentMode,
			Func<string, bool> supportsFile,
			Func<string, bool> isDefaultForFile,
			Func<Version, IEditorControl> factory)
			=> _editorFactory.Register(new EditorRegistration(editorType, documentMode, supportsFile, isDefaultForFile, factory));

		#endregion File opening

		#region File reloading

		public void AddFileToReloadQueue(string filePath)
			=> _fileReloadCoordinator.QueueFile(filePath);

		public void TryRunFileReloadQueue()
			=> _fileReloadCoordinator.ProcessQueuedFiles(GetOpenEditorsOfFile, ShowFileReloadPrompt);

		private IReadOnlyList<IEditorControl> GetOpenEditorsOfFile(string filePath)
			=> FindTabPagesOfFile(filePath)
				.Select(GetEditorOfTab)
				.Where(editor => editor is not null)
				.ToList();

		private DialogResult ShowFileReloadPrompt(string filePath)
			=> MessageBox.Show(this,
				string.Format(Strings.Default.AskFileReload, filePath), Strings.Default.FileReloadMBT,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		#endregion File reloading

		#region File saving

		public bool AskSaveAll()
		{
			List<string> filePaths = GetFilePaths();

			foreach (string path in filePaths)
			{
				TabPage mostRecentTabOfFile = GetMostRecentlyModifiedTabPageOfFile(path);

				FileSavingResult result = TryAskSaveFile(mostRecentTabOfFile);

				if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
					return false;
			}

			return true;
		}

		public void SaveAll()
		{
			List<string> filePaths = GetFilePaths();

			foreach (string path in filePaths)
			{
				TabPage mostRecentTabOfFile = GetMostRecentlyModifiedTabPageOfFile(path);
				SaveFile(mostRecentTabOfFile);
			}
		}

		public FileSavingResult TryAskSaveFile(TabPage tab)
		{
			IEditorControl editor = GetEditorOfTab(tab);

			if (editor.IsContentChanged)
			{
				SelectTab(tab);

				string fileName = Path.GetFileName(editor.FilePath);

				DialogResult result = DarkMessageBox.Show(this,
					string.Format(Strings.Default.AskUnsavedChanged, fileName), Strings.Default.UnsavedChangedMBT,
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
					return SaveFile(tab);
				else if (result == DialogResult.No)
					return FileSavingResult.Rejected;
				else if (result == DialogResult.Cancel)
					return FileSavingResult.Canceled;
			}

			return FileSavingResult.AlreadySaved;
		}

		/// <summary>
		/// Saves the file from the currently selected tab page.
		/// </summary>
		public FileSavingResult SaveFile()
			=> SaveFile(SelectedTab);

		public FileSavingResult SaveFile(TabPage tab)
		{
			IEditorControl editor = GetEditorOfTab(tab);

			try
			{
				editor.Save();
				UpdateTabPageName(editor);

				SaveOtherTabPagesOfFile(editor);
			}
			catch (Exception ex) // Saving failed somehow
			{
				DialogResult result = DarkMessageBox.Show(this,
					ex.Message, Strings.Default.Error, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

				if (result == DialogResult.Retry)
					return SaveFile(tab); // Retry saving

				return FileSavingResult.Failed;
			}

			return FileSavingResult.Success;
		}

		/// <summary>
		/// Saves the file from the currently selected tab page.
		/// </summary>
		public FileSavingResult SaveFileAs()
			=> SaveFileAs(SelectedTab);

		public FileSavingResult SaveFileAs(TabPage tab)
		{
			IEditorControl editor = GetEditorOfTab(tab);
			string oldFilePath = editor.FilePath;

			string[] ignoredPaths = Array.Empty<string>();

			if (editor.DefaultFileExtension == ".lua") // Dirty hack to ignore /Engine/ directory for TEN scripts
				ignoredPaths = new string[] { @"Scripts\Engine" };

			using (var form = new FormFileCreation(ScriptRootDirectoryPath, FileCreationMode.SavingAs, editor.DefaultFileExtension, null, null, ignoredPaths))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					if (string.IsNullOrWhiteSpace(oldFilePath)
						|| oldFilePath.Equals(form.NewFilePath, StringComparison.OrdinalIgnoreCase))
					{
						editor.FilePath = form.NewFilePath;
						UpdateTabPageName(tab);

						return SaveFile(tab);
					}

					editor.FilePath = form.NewFilePath;
					UpdateTabPageName(tab);

					FileSavingResult result = SaveFile(tab);

					if (result == FileSavingResult.Success)
					{
						if (FindTabPagesOfFile(oldFilePath).Any())
						{
							RenameDocumentTabPage(oldFilePath, form.NewFilePath);
							SaveOtherTabPagesOfFile(editor);
						}
						else
							OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, form.NewFilePath));
					}
					else
					{
						editor.FilePath = oldFilePath;
						UpdateTabPageName(tab);
					}

					return result;
				}
				else
					return FileSavingResult.Canceled;
		}

		private void SaveOtherTabPagesOfFile(IEditorControl excludedEditor)
		{
			foreach (TabPage tabPage in TabPages)
			{
				IEditorControl tabEditor = GetEditorOfTab(tabPage);

				// Same file, different EditorType
				if (tabEditor.FilePath.Equals(excludedEditor.FilePath, StringComparison.OrdinalIgnoreCase)
					&& tabEditor.EditorType != excludedEditor.EditorType)
				{
					if (tabEditor.Content != excludedEditor.Content)
						tabEditor.ApplyPersistedContent(excludedEditor.Content);

					tabEditor.TryRunContentChangedWorker();

					UpdateTabPageName(tabEditor);
				}
			}
		}

		#endregion File saving

		#region Tab finding

		public TabPage FindTabPage(Point location)
		{
			foreach (TabPage tab in TabPages)
			{
				Rectangle tabRect = GetTabRect(TabPages.IndexOf(tab));
				var adjustedRect = new Rectangle(tabRect.Location.X, tabRect.Location.Y, tabRect.Width - 16, tabRect.Height); // IGNORE x button

				if (adjustedRect.Contains(location))
					return tab;
			}

			return null;
		}

		public TabPage FindTabPage(IEditorControl editor)
			=> FindTabPage(editor.FilePath, editor.EditorType);

		public TabPage FindTabPage(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = _editorFactory.GetDefaultEditorType(filePath);

			foreach (TabPage tab in TabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);

				if (editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase) && editor.EditorType == editorType)
					return tab;
			}

			return null;
		}

		public IEnumerable<TabPage> FindTabPagesOfFile(string filePath)
		{
			foreach (TabPage tab in TabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);

				if (editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
					yield return tab;
			}
		}

		#endregion Tab finding

		#region Tab closing

		public void CloseTabPagesOfFile(string filePath)
		{
			TabPage mostRecentTabOfFile = GetMostRecentlyModifiedTabPageOfFile(filePath);

			if (mostRecentTabOfFile != null)
			{
				FileSavingResult result = TryAskSaveFile(mostRecentTabOfFile);

				if (result == FileSavingResult.AlreadySaved || result == FileSavingResult.Success || result == FileSavingResult.Rejected)
				{
					CloseTab(mostRecentTabOfFile);

					IEnumerable<TabPage> fileTabPages = FindTabPagesOfFile(filePath);

					foreach (TabPage tab in fileTabPages)
						CloseTab(tab);
				}
			}
		}

		public void CloseInvalidTabPages()
			=> SharedMethods.DisposeItems(GetTabPagesToClose());

		private IEnumerable<TabPage> GetTabPagesToClose()
		{
			foreach (TabPage tab in TabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);

				if (editor != null && !File.Exists(editor.FilePath))
				{
					if (editor.IsContentChanged)
						editor.FilePath = null;
					else
						yield return tab;
				}
			}
		}

		#endregion Tab closing

		#region Editor finding

		public IEditorControl GetEditorOfTab(int index)
			=> GetEditorOfTab(TabPages[index]);

		public IEditorControl GetEditorOfTab(TabPage tab)
			=> _editorHostService.GetEditor(tab);

		#endregion Editor finding

		#region Events

		public event EventHandler FileOpened;
		protected virtual void OnFileOpened(EventArgs e)
			=> FileOpened?.Invoke(CurrentEditor, e);

		public event EventHandler<DocumentRenamedEventArgs> DocumentRenamed;
		protected virtual void OnDocumentRenamed(DocumentRenamedEventArgs e)
			=> DocumentRenamed?.Invoke(this, e);

		protected override void OnTabClosing(TabControlCancelEventArgs e)
		{
			IEditorControl editorOfTab = GetEditorOfTab(e.TabPage);

			string filePath = editorOfTab.FilePath;
			IEnumerable<TabPage> fileTabPages = FindTabPagesOfFile(filePath);

			if (fileTabPages.Count() == 1)
			{
				FileSavingResult result = TryAskSaveFile(e.TabPage);

				if (result == FileSavingResult.Canceled || result == FileSavingResult.Failed)
					e.Cancel = true;
			}
			else if (IsMostRecentlyModifiedTabPageOfFile(e.TabPage))
				foreach (TabPage tab in fileTabPages)
				{
					IEditorControl editor = GetEditorOfTab(tab);
					editor.Content = editorOfTab.Content;
				}

			if (!e.Cancel)
				editorOfTab.Dispose();

			base.OnTabClosing(e);
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);

			if (CurrentEditor != null)
			{
				var fileTabPages = FindTabPagesOfFile(CurrentEditor.FilePath).ToList();
				TabPage mostRecentTabOfFile = GetMostRecentlyModifiedTabPageOfFile(CurrentEditor.FilePath);

				fileTabPages.Remove(mostRecentTabOfFile);

				foreach (TabPage tabPage in fileTabPages)
				{
					IEditorControl editorOfTab = GetEditorOfTab(tabPage);

					if (editorOfTab.Content != GetEditorOfTab(mostRecentTabOfFile).Content)
						editorOfTab.Content = GetEditorOfTab(mostRecentTabOfFile).Content;
				}
			}
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				TryOpenContextMenu(e.Location);
			else
				base.OnMouseClick(e);

			if (e.Button == MouseButtons.Middle && _toolTip.Active)
				_toolTip.Hide(this);
		}

		private const int MOVE_THRESHOLD = 5;
		private Point _cachedCursorPosition = new Point();

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);

			TabPage locationTabPage = FindTabPage(PointToClient(Cursor.Position));

			if (locationTabPage != null)
			{
				IEditorControl editorOfTabPage = GetEditorOfTab(locationTabPage);

				if (editorOfTabPage != null)
				{
					Point clientCursorPoint = PointToClient(Cursor.Position);
					_cachedCursorPosition = clientCursorPoint;

					_toolTip.Show(editorOfTabPage.FilePath.Replace(ScriptRootDirectoryPath, string.Empty), this,
						clientCursorPoint.X, clientCursorPoint.Y + 24, 2000);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_toolTip.Active)
			{
				int dragDistance = Math.Abs(e.Y - _cachedCursorPosition.Y);

				if (dragDistance < MOVE_THRESHOLD)
					return;

				_toolTip.Hide(this);
				_cachedCursorPosition = PointToClient(Cursor.Position);
			}

			base.OnMouseMove(e);
		}

		private void TryOpenContextMenu(Point location)
		{
			TabPage locationTabPage = FindTabPage(location);

			if (locationTabPage != null)
			{
				SelectTab(locationTabPage);
				_contextMenu.Show(locationTabPage, location);
			}
		}

		private void MenuItem_Save_Click(object sender, EventArgs e)
			=> SaveFile(((sender as ToolStripMenuItem).Owner as DarkContextMenu).SourceControl as TabPage);

		private void MenuItem_Close_Click(object sender, EventArgs e)
			=> CloseTab(((sender as ToolStripMenuItem).Owner as DarkContextMenu).SourceControl as TabPage);

		private void MenuItem_OpenFolder_Click(object sender, EventArgs e)
		{
			var tabPage = ((sender as ToolStripMenuItem).Owner as DarkContextMenu).SourceControl as TabPage;

			if (tabPage != null)
			{
				IEditorControl tabEditor = GetEditorOfTab(tabPage);

				if (!string.IsNullOrWhiteSpace(tabEditor.FilePath))
					SharedMethods.OpenInExplorer(tabEditor.FilePath);
			}
		}

		private void Editor_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
		{
			var senderEditor = sender as IEditorControl;

			UpdateTabPageName(senderEditor);

			if (senderEditor == CurrentEditor)
				foreach (TabPage tab in FindTabPagesOfFile(senderEditor.FilePath))
				{
					IEditorControl tabEditor = GetEditorOfTab(tab);
					tabEditor.IsContentChanged = senderEditor.IsContentChanged;

					UpdateTabPageName(tabEditor);
				}
		}

		#endregion Events

		#region Other methods

		public void UpdateTabPageName(TabPage tab)
			=> UpdateTabPageName(GetEditorOfTab(tab));

		public void UpdateTabPageName(IEditorControl tabPageEditor)
		{
			if (tabPageEditor == null)
				return;

			TabPage tab = FindTabPage(tabPageEditor);

			if (tab != null)
			{
				tab.Text = BuildTabPageTitleText(tabPageEditor.FilePath, tabPageEditor.EditorType);

				if (tabPageEditor.IsContentChanged)
					tab.Text += "*";
			}
		}

		public void RenameDocumentTabPage(string oldFilePath, string newFilePath)
		{
			if (string.IsNullOrWhiteSpace(oldFilePath)
				|| string.IsNullOrWhiteSpace(newFilePath)
				|| oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			List<TabPage> tabPages = FindTabPagesOfFile(oldFilePath).ToList();

			if (tabPages.Count == 0)
				return;

			foreach (TabPage tab in tabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);
				editor.FilePath = newFilePath;

				UpdateTabPageName(editor);
			}

			OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
		}

		private string BuildTabPageTitleText(string filePath, EditorType editorType)
			=> _editorFactory.BuildTabTitle(filePath, editorType);

		/// <summary>
		/// The difference between this and the <c>AreAllFilesSaved()</c> method is that this one<br/>
		/// just returns <c>true</c> or <c>false</c> and doesn't prompt the user to save the changes.
		/// </summary>
		public bool IsEveryTabSaved()
		{
			List<string> filePaths = GetFilePaths();

			foreach (string path in filePaths)
			{
				TabPage tab = GetMostRecentlyModifiedTabPageOfFile(path);

				if (GetEditorOfTab(tab).IsContentChanged)
					return false;
			}

			return true;
		}

		private List<string> GetFilePaths()
		{
			var paths = new List<string>();

			foreach (TabPage tab in TabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);

				if (editor != null && !paths.Contains(editor.FilePath))
					paths.Add(editor.FilePath);
			}

			return paths;
		}

		private bool IsMostRecentlyModifiedTabPageOfFile(TabPage tab)
		{
			IEditorControl editorOfTab = GetEditorOfTab(tab);

			TabPage mostRecent = GetMostRecentlyModifiedTabPageOfFile(editorOfTab.FilePath);
			IEditorControl editorOfMostRecent = GetEditorOfTab(mostRecent);

			return editorOfTab.LastModified == editorOfMostRecent.LastModified;
		}

		private TabPage GetMostRecentlyModifiedTabPageOfFile(string filePath)
		{
			IEnumerable<TabPage> fileTabPages = FindTabPagesOfFile(filePath);
			TabPage mostRecentFound = null;

			foreach (TabPage tab in fileTabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);

				if (mostRecentFound == null || editor.LastModified > GetEditorOfTab(mostRecentFound).LastModified)
					mostRecentFound = tab;
			}

			return mostRecentFound;
		}

		public void EnsureTabFileSynchronization()
		{
			foreach (string file in GetFilePaths())
			{
				TabPage mostRecent = GetMostRecentlyModifiedTabPageOfFile(file);
				SaveOtherTabPagesOfFile(GetEditorOfTab(mostRecent));
			}
		}

		#endregion Other methods
	}
}
