using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptingStudio.Forms;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;

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

					if (!string.IsNullOrWhiteSpace(ScriptRootDirectoryPath))
						CheckPreviousSession();
				}
			}
		}

		public IEditorControl CurrentEditor => SelectedTab != null ? GetEditorOfTab(SelectedTab) : null;

		public bool ReloadQueueRunning { get; private set; }

		#endregion Properties

		#region Fields

		private DarkContextMenu _contextMenu = new DarkContextMenu();
		private ToolStripMenuItem menuItem_Save = new ToolStripMenuItem(Strings.Default.Save);
		private ToolStripMenuItem menuItem_Close = new ToolStripMenuItem(Strings.Default.Close);
		private ToolStripMenuItem menuItem_OpenFolder = new ToolStripMenuItem(Strings.Default.OpenContainingFolder);

		/// <summary>
		/// This list is used to store file paths of files which should be reloaded after the main window regains focus.
		/// </summary>
		private List<string> _pendingFileReloads = new List<string>();

		#endregion Fields

		#region Construction

		public EditorTabControl() : this(string.Empty)
		{ }
		public EditorTabControl(string scriptRootDirectoryPath)
		{
			SetNewDefaultSettings();

			ScriptRootDirectoryPath = scriptRootDirectoryPath;

			InitializeContextMenu();
		}

		private void SetNewDefaultSettings()
		{
			AllowDrop = true;
			Dock = DockStyle.Fill;
			DisplayStyle = TabStyle.Dark;
			DisplayStyleProvider.ShowTabCloser = true;
			EnableMiddleClickTabClosing = true;
			Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
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
		private void CheckPreviousSession()
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

				string backupFileContent = File.ReadAllText(file, Encoding.GetEncoding(1252));
				string originalFilePath = FileHelper.GetOriginalFilePathFromBackupFile(file);

				// Open the original file and replace the whole text of the TextEditor with the backup file content
				OpenFile(originalFilePath);
				CurrentEditor.Content = backupFileContent;
			}
		}

		#endregion Session

		#region File opening

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
			Type editorClassType = EditorTypeHelper.GetEditorClassType(filePath, editorType);

			if (editorClassType != null)
			{
				IEditorControl newEditor = InitializeEditor(editorClassType, filePath, silentSession);

				string tabPageTitle = BuildTabPageTitleText(newEditor.FilePath, newEditor.EditorType);
				var newTabPage = new TabPage(tabPageTitle);
				Control tabPageContent = InitializeTabPageContent(newEditor);
				newTabPage.Controls.Add(tabPageContent);

				TabPages.Add(newTabPage);
				SelectTab(newTabPage);

				OnFileOpened(EventArgs.Empty);
			}
		}

		private IEditorControl InitializeEditor(Type editorClassType, string filePath, bool silentSession)
		{
			var newEditor = Activator.CreateInstance(editorClassType) as IEditorControl;
			newEditor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

			if (File.Exists(filePath))
				newEditor.Load(filePath, silentSession);
			else
				newEditor.FilePath = filePath;

			return newEditor;
		}

		private Control InitializeTabPageContent(IEditorControl editor)
		{
			bool isWPF = editor.GetType().IsSubclassOf(typeof(System.Windows.UIElement));

			if (isWPF)
				return new ElementHost { Dock = DockStyle.Fill, Child = editor as System.Windows.UIElement };
			else
				return editor as Control;
		}

		#endregion File opening

		#region File reloading

		public void AddFileToReloadQueue(string filePath)
		{
			if (!_pendingFileReloads.Contains(filePath))
				_pendingFileReloads.Add(filePath);
		}

		public void TryRunFileReloadQueue()
		{
			if (ReloadQueueRunning) // Prevents calling the method again if it's already running
				return;

			ReloadQueueRunning = true;

			for (int i = 0; i < _pendingFileReloads.Count; i++)
				try
				{
					string file = _pendingFileReloads[i];
					IEnumerable<TabPage> tabPagesOfFile = FindTabPagesOfFile(file);

					if (tabPagesOfFile.Count() > 0)
						TryAskFileReload(tabPagesOfFile);
				}
				catch (Exception) { }

			_pendingFileReloads.Clear();
			ReloadQueueRunning = false;
		}

		private void TryAskFileReload(IEnumerable<TabPage> tabPagesOfFile)
		{
			DialogResult? result = null;

			foreach (TabPage tab in tabPagesOfFile)
			{
				IEditorControl editor = GetEditorOfTab(tab);
				string fileContent = File.ReadAllText(editor.FilePath, Encoding.GetEncoding(1252));

				if (editor.Content != fileContent)
				{
					if (result == null)
						result = MessageBox.Show(this,
							string.Format(Strings.Default.AskFileReload, editor.FilePath), Strings.Default.FileReloadMBT,
							MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
					{
						// Re-read the file, because the user might've changed the file another time
						fileContent = File.ReadAllText(editor.FilePath, Encoding.GetEncoding(1252));
						editor.Content = fileContent;
					}
					else if (result == DialogResult.No)
						editor.TryRunContentChangedWorker();
				}
			}
		}

		#endregion File reloading

		#region File saving

		public bool AskSaveAll()
		{
			List<string> filePaths = GetFilePaths();

			foreach (string path in filePaths)
			{
				TabPage mostRecentTabOfFile = GetMostRecentlyModifiedTabPageOfFile(path);

				FileSavingResult result = TryAskSaveFile(mostRecentTabOfFile);

				if (result == FileSavingResult.Cancelled || result == FileSavingResult.Failed)
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
					return FileSavingResult.Cancelled;
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

			using (var form = new FormFileCreation(ScriptRootDirectoryPath, FileCreationMode.SavingAs))
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					editor.FilePath = form.NewFilePath;
					UpdateTabPageName(tab);

					return SaveFile(tab);
				}
				else
					return FileSavingResult.Cancelled;
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
						tabEditor.Content = excludedEditor.Content;

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
				if (GetTabRect(TabPages.IndexOf(tab)).Contains(location))
					return tab;

			return null;
		}

		public TabPage FindTabPage(IEditorControl editor)
			=> FindTabPage(editor.FilePath, editor.EditorType);

		public TabPage FindTabPage(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = EditorTypeHelper.GetDefaultEditorType(filePath);

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
		{
			IEnumerable<ElementHost> elementHosts = tab?.Controls.OfType<ElementHost>();

			if (elementHosts?.Count() > 0)
				return elementHosts.First().Child as IEditorControl;
			else
			{
				IEnumerable<IEditorControl> editors = tab?.Controls.OfType<IEditorControl>();
				return editors?.Count() > 0 ? editors.First() : null;
			}
		}

		#endregion Editor finding

		#region Events

		public event EventHandler FileOpened;
		protected virtual void OnFileOpened(EventArgs e)
			=> FileOpened?.Invoke(CurrentEditor, e);

		protected override void OnTabClosing(TabControlCancelEventArgs e)
		{
			IEditorControl editorOfTab = GetEditorOfTab(e.TabPage);

			string filePath = editorOfTab.FilePath;
			IEnumerable<TabPage> fileTabPages = FindTabPagesOfFile(filePath);

			if (fileTabPages.Count() == 1)
			{
				FileSavingResult result = TryAskSaveFile(e.TabPage);

				if (result == FileSavingResult.Cancelled || result == FileSavingResult.Failed)
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
			base.OnMouseClick(e);

			if (e.Button == MouseButtons.Right)
				TryOpenContextMenu(e.Location);
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
			=> SharedMethods.OpenInExplorer(((sender as ToolStripMenuItem).Owner as DarkContextMenu).SourceControl.Tag.ToString());

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
			IEnumerable<TabPage> tabPages = FindTabPagesOfFile(oldFilePath);

			foreach (TabPage tab in tabPages)
			{
				IEditorControl editor = GetEditorOfTab(tab);
				editor.FilePath = newFilePath;

				UpdateTabPageName(editor);
			}
		}

		private string BuildTabPageTitleText(string filePath, EditorType editorType)
		{
			string tabTypeText = editorType == EditorType.Text ? string.Empty : $" [{editorType}]";
			return $"{Path.GetFileName(filePath)}{tabTypeText}";
		}

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

		#endregion Other methods
	}
}
