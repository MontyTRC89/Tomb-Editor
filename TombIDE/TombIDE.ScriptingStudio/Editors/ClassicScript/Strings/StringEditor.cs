using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.Strings
{
	public class StringEditor : DarkTabbedContainer, IEditorControl, INameBasedObjectNavigator
	{
		public EditorType EditorType => EditorType.Strings;
		public string DefaultFileExtension => ".txt";

		#region Properties

		public string FilePath
		{
			get => _contentPersistenceCoordinator.FilePath;
			set => _contentPersistenceCoordinator.FilePath = value;
		}

		public bool IsSilentSession { get; set; }

		public bool CreateBackupFiles
		{
			get => IsSilentSession ? false : _contentPersistenceCoordinator.CreateBackupFiles;
			set => _contentPersistenceCoordinator.CreateBackupFiles = value;
		}

		public string Content
		{
			get => ContentBuilder.BuildContent(DataGrids);
			set => UpdateContent(value);
		}

		public bool IsContentChanged { get; set; }

		public DateTime LastModified { get; set; }

		public int CurrentRow => CurrentDataGrid.CurrentCell?.RowIndex ?? 0;
		public int CurrentColumn => CurrentDataGrid.CurrentCell?.ColumnIndex ?? 0;

		public string? SelectedContent
		{
			get
			{
				if (CurrentDataGrid.IsCurrentCellInEditMode)
					return (CurrentDataGrid.EditingControl as TextBox)?.SelectedText;

				return CurrentDataGrid.CurrentCell?.Value?.ToString();
			}
		}

		public int SelectionLength => CurrentDataGrid.SelectedCells.Count;

		public int MinZoom { get; set; } = 25;
		public int MaxZoom { get; set; } = 400;
		public int ZoomStepSize { get; set; } = 15;

		public StringDataGridView[] DataGrids
		{
			get
			{
				var dataGrids = new List<StringDataGridView>();

				foreach (TabPage tab in TabPages)
					dataGrids.Add(tab.Controls.OfType<StringDataGridView>().First());

				return dataGrids.ToArray();
			}
		}

		public StringDataGridView CurrentDataGrid => GetDataGridOfTab(SelectedTab);

		public bool CanUndo => _undoStack.Count > 0;
		public bool CanRedo => _redoStack.Count > 0;

		public Version EngineVersion { get; set; } = new Version(0, 0);

		#endregion Properties

		#region Configuration

		private int _defaultFontSize = StringEditorDefaults.FontSize;

		/// <summary>
		/// Basically Font.Size but zooming doesn't affect its value.
		/// </summary>
		public int DefaultFontSize
		{
			get => _defaultFontSize;
			set
			{
				_defaultFontSize = value;
				RecalculateFontSizes();
			}
		}

		private string _fontFamily = StringEditorDefaults.FontFamily;

		public string FontFamily
		{
			get => _fontFamily;
			set
			{
				_fontFamily = value;
				RecalculateFontSizes();
			}
		}

		private int _undoStackSize = StringEditorDefaults.UndoStackSize;

		public int UndoStackSize
		{
			get => _undoStackSize;
			set
			{
				_undoStackSize = value;

				ResizeUndoStack(ref _undoStack, _undoStackSize);
				ResizeUndoStack(ref _redoStack, _undoStackSize);
			}
		}

		private void ResizeUndoStack(ref Stack<DataGridUndoItem> stack, int newCapacity)
		{
			var tempStack = new Stack<DataGridUndoItem>(stack.Count);

			while (stack.Count > 0)
				tempStack.Push(stack.Pop());

			var newStack = new Stack<DataGridUndoItem>(newCapacity);

			while (tempStack.Count > 0)
				newStack.Push(tempStack.Pop());

			stack = newStack;
		}

		#endregion Configuration

		#region Fields

		private Stack<DataGridUndoItem> _undoStack;
		private Stack<DataGridUndoItem> _redoStack;

		private ContentPersistenceCoordinator _contentPersistenceCoordinator;

		#endregion Fields

		#region Construction

		public StringEditor(Version engineVersion)
		{
			Dock = DockStyle.Fill;

			_undoStack = new Stack<DataGridUndoItem>(UndoStackSize);
			_redoStack = new Stack<DataGridUndoItem>(UndoStackSize);

			_contentPersistenceCoordinator = new ContentPersistenceCoordinator(() => Content, () => IsSilentSession);
			_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted += ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted;

			EngineVersion = engineVersion;
		}

		#endregion Construction

		#region Override methods

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!CurrentDataGrid.IsCurrentCellInEditMode)
				HandleUndoRedoKeys(keyData);

			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_contentPersistenceCoordinator?.Dispose();

			base.Dispose(disposing);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (ModifierKeys == Keys.Control)
				HandleZoom(e);

			base.OnMouseWheel(e);
		}

		private void HandleUndoRedoKeys(Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Z))
				Undo();
			else if (keyData == (Keys.Control | Keys.Y))
				Redo();
		}

		#endregion Override methods

		#region Events

		public event EventHandler StatusChanged;

		protected virtual void OnStatusChanged(EventArgs e)
			=> StatusChanged?.Invoke(this, e);

		public event EventHandler ZoomChanged;

		protected virtual void OnZoomChanged(EventArgs e)
		{
			ZoomChanged?.Invoke(this, e);
			OnStatusChanged(EventArgs.Empty);
		}

		public event EventHandler ContentChangedWorkerRunCompleted;

		protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
			=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

		private void ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted(object sender, EventArgs e)
		{
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);
		}

		private void DataGrid_SelectionChanged(object sender, EventArgs e)
			=> OnStatusChanged(EventArgs.Empty);

		private void DataGrid_CellContentChanged(object sender, CellContentChangedEventArgs e)
		{
			var source = sender as StringDataGridView;

			_undoStack.Push(new DataGridUndoItem(source, e.ColumnIndex, e.RowIndex, e.OldValue ?? "NULL"));
			_redoStack.Clear();
			LastModified = DateTime.Now;

			RunContentChangedWorker();
		}

		private void ExtraNGDataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			var source = sender as DataGridView;

			DataGridViewCell idCell = source[0, e.RowIndex - 1];
			DataGridViewCell hexCell = source[1, e.RowIndex - 1];

			bool isFirstRow = source.RowCount == 2;

			int nextID = isFirstRow ? 0 : int.Parse(source[0, idCell.RowIndex - 1].Value.ToString()) + 1;

			idCell.Value = nextID;
			hexCell.Value = ContentReader.GetShortHex((short)nextID, 3);
		}

		private void ExtraNGDataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
			=> RunContentChangedWorker();

		#endregion Events

		#region File I/O

		public void Load(string filePath)
			=> Load(filePath, false);

		public void Load(string filePath, bool silentSession)
		{
			string[] fileLines = File.ReadAllLines(filePath);
			UpdateContent(fileLines);

			FilePath = filePath;
			_contentPersistenceCoordinator.SetPersistedContent(Content);

			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			IsSilentSession = silentSession;
		}

		public void Save()
			=> Save(FilePath);

		public void Save(string filePath)
		{
			File.WriteAllText(filePath, Content);
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			LastModified = DateTime.Now;
		}

		#endregion File I/O

		#region Content

		public void RunContentChangedWorker()
		{
			IsContentChanged = _contentPersistenceCoordinator.RunContentChangedCheck();
		}

		public void ApplyPersistedContent(string content)
		{
			UpdateContent(content);
			_contentPersistenceCoordinator.SetPersistedContent(Content);
			IsContentChanged = _contentPersistenceCoordinator.HasChanges(Content);
			LastModified = DateTime.Now;
		}

		private void UpdateContent(string content)
		{
			string[] lines = content.Replace("\r", string.Empty).Split('\n');
			UpdateContent(lines);
		}

		private void UpdateContent(string[] lines)
		{
			TabPages.Clear();

			_undoStack.Clear();
			_redoStack.Clear();

			int currentLineNumber = 0;
			int totalStringCount = 0;

			while (ContentReader.NextSectionExists(lines, currentLineNumber, out int nextSectionLineNumber))
			{
				string currentSectionName = lines[nextSectionLineNumber];
				List<string> strings = ContentReader.GetStrings(lines, nextSectionLineNumber);

				StringDataGridView dataGrid = CreateStringDataGrid(currentSectionName);
				TabPages.Add(CreateDataGridTabPage(dataGrid));

				bool isExtraNG = Regex.IsMatch(currentSectionName, @"^\[ExtraNG\]", RegexOptions.IgnoreCase);

				if (isExtraNG)
					HandleExtraNGDataGrid(dataGrid, strings);
				else
				{
					HandleStringDataGrid(dataGrid, strings, totalStringCount);
					totalStringCount += strings.Count;
				}

				currentLineNumber = nextSectionLineNumber + (strings.Count == 0 ? 1 : strings.Count);
			}

			if (TabPages.Count > 0 && SelectedTab is null)
				SelectTab(TabPages[0]);

			RecalculateFontSizes();
			RunContentChangedWorker();
		}

		private void HandleStringDataGrid(StringDataGridView dataGrid, List<string> strings, int idOffset)
		{
			for (int i = 0; i < strings.Count; i++)
			{
				short id = (short)(idOffset + i);
				string hex = ContentReader.GetShortHex(id, 4);

				dataGrid.Rows.Add(id, hex, strings[i]);

				DataGridViewCell stringCell = dataGrid.Rows[i].Cells[2];

				if (stringCell.Value?.ToString() == "NULL")
					stringCell.Style.ForeColor = Color.Gray;
			}
		}

		private void HandleExtraNGDataGrid(StringDataGridView dataGrid, List<string> strings)
		{
			for (int i = 0; i < strings.Count; i++)
				if (Regex.IsMatch(strings[i], @"^\d+:.*"))
				{
					short id = short.Parse(strings[i].Split(':').First());
					string hex = ContentReader.GetShortHex(id, 3);
					string @string = Regex.Replace(strings[i], @"^\d+:", string.Empty).TrimStart(' ');

					dataGrid.Rows.Add(id, hex, @string);
					DataGridViewCell stringCell = dataGrid.Rows[i].Cells[2];

					if (stringCell.Value?.ToString() == "NULL")
						stringCell.Style.ForeColor = Color.Gray;
				}

			dataGrid.AllowUserToAddRows = true;
			dataGrid.AllowUserToDeleteRows = true;
			dataGrid.RowsAdded += ExtraNGDataGrid_RowsAdded;
			dataGrid.RowsRemoved += ExtraNGDataGrid_RowsRemoved;
		}

		private StringDataGridView CreateStringDataGrid(string sectionName)
		{
			var dataGrid = new StringDataGridView
			{
				Name = sectionName,
				Dock = DockStyle.Fill
			};

			dataGrid.Initialize(sectionName);
			dataGrid.CellContentChanged += DataGrid_CellContentChanged;
			dataGrid.SelectionChanged += DataGrid_SelectionChanged;

			return dataGrid;
		}

		private TabPage CreateDataGridTabPage(StringDataGridView dataGrid)
		{
			var tabPage = new TabPage { Name = dataGrid.Name, BackColor = Color.FromArgb(48, 48, 48) };
			tabPage.Controls.Add(dataGrid);

			return tabPage;
		}

		#endregion Content

		#region Edit methods

		public void Undo() => DoUndoRedo(_undoStack, _redoStack);

		public void Redo() => DoUndoRedo(_redoStack, _undoStack);

		public void Cut()
		{
			if (CurrentDataGrid.IsCurrentCellInEditMode)
				(CurrentDataGrid.EditingControl as TextBox).Cut();
			else
				CurrentDataGrid.CutCellText();
		}

		public void Copy()
		{
			if (CurrentDataGrid.IsCurrentCellInEditMode)
				(CurrentDataGrid.EditingControl as TextBox).Copy();
			else
				CurrentDataGrid.CopyCellText();
		}

		public void Paste()
		{
			if (CurrentDataGrid.IsCurrentCellInEditMode)
				(CurrentDataGrid.EditingControl as TextBox).Paste();
			else
				CurrentDataGrid.PasteCellText();
		}

		public void SelectAll()
		{
			if (CurrentDataGrid.IsCurrentCellInEditMode)
				(CurrentDataGrid.EditingControl as TextBox).SelectAll();
		}

		public void GoToObject(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
		{
			TabPage targetTab = TabPages.Cast<TabPage>().ToList().Find(x => x.Name.Equals($"[{objectName}]", StringComparison.OrdinalIgnoreCase));

			if (targetTab != null)
				SelectTab(targetTab);
		}

		private void DoUndoRedo(Stack<DataGridUndoItem> sourceStack, Stack<DataGridUndoItem> destStack)
		{
			if (sourceStack.Count == 0)
				return;

			DataGridUndoItem item = sourceStack.Pop();
			int totalRowCount = item.Source.AllowUserToAddRows ? item.Source.RowCount - 1 : item.Source.RowCount;

			if (item.RowIndex < totalRowCount)
			{
				destStack.Push(new DataGridUndoItem(
					item.Source, item.ColumnIndex, item.RowIndex, item.Source[item.ColumnIndex, item.RowIndex].Value));

				TabPage sourceTab = FindTabPage(item.Source);

				if (sourceTab != null)
				{
					SelectTab(sourceTab);
					item.Source.CurrentCell = item.Source[item.ColumnIndex, item.RowIndex];
					item.Source.CurrentCell.Value = item.Value;
					item.Source.CurrentCell.Style.ForeColor =
						item.Source.CurrentCell.Value.ToString() == "NULL" ? Color.Gray : Color.LightSalmon;
				}
			}

			LastModified = DateTime.Now;
			RunContentChangedWorker();
		}

		#endregion Edit methods

		#region Zoom

		private int _zoom = 100;

		public int Zoom
		{
			get { return _zoom; }
			set
			{
				_zoom = value;
				RecalculateFontSizes();
			}
		}

		private void HandleZoom(MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				if (_zoom < MaxZoom)
				{
					_zoom += ZoomStepSize;
					RecalculateFontSizes();
				}
			}
			else
			{
				if (_zoom > MinZoom)
				{
					_zoom -= ZoomStepSize;
					RecalculateFontSizes();
				}
			}

			OnZoomChanged(EventArgs.Empty);
		}

		#endregion Zoom

		#region Other methods

		public void UpdateSettings(TombLib.Scripting.UI.Bases.ConfigurationBase configuration)
		{
			if (configuration is not TextEditorConfigBase config)
				return;

			DefaultFontSize = (int)config.FontSize - 4;
			FontFamily = config.FontFamily;
			UndoStackSize = config.UndoStackSize;
			RecalculateFontSizes();
		}

		public void RecalculateFontSizes()
		{
			var font = new Font(FontFamily, DefaultFontSize * _zoom / 100);

			foreach (StringDataGridView dataGrid in DataGrids)
				dataGrid.ApplyFont(font);
		}

		public TabPage FindTabPage(ExtendedDarkDataGridView dataGrid)
		{
			foreach (TabPage tab in TabPages)
				if (GetDataGridOfTab(tab).Name == dataGrid.Name)
					return tab;

			return null;
		}

		public StringDataGridView GetDataGridOfTab(int tabIndex)
			=> GetDataGridOfTab(TabPages[tabIndex]);

		public StringDataGridView GetDataGridOfTab(TabPage tab)
		{
			TabPage resolvedTab = tab ?? SelectedTab ?? TabPages.Cast<TabPage>().FirstOrDefault()
				?? throw new InvalidOperationException("The string editor does not contain any section tabs.");

			return resolvedTab.Controls.OfType<StringDataGridView>().FirstOrDefault()
				?? throw new InvalidOperationException("The selected string-editor tab does not contain a string data grid.");
		}

		public void GoToPreviousSection()
		{
			if (SelectedIndex > 0)
				SelectedIndex--;
		}

		public void GoToNextSection()
		{
			if (SelectedIndex < TabPages.Count)
				SelectedIndex++;
		}

		#endregion Other methods
	}
}
