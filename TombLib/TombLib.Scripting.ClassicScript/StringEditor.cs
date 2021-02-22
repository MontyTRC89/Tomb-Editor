using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Controls;
using TombLib.Scripting.ClassicScript.Utils;
using TombLib.Scripting.Controls;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;
using TombLib.Scripting.Workers;

namespace TombLib.Scripting.ClassicScript
{
	public class StringEditor : DarkTabbedContainer, IEditorControl
	{
		public EditorType EditorType => EditorType.Strings;

		#region Properties

		public string FilePath
		{
			get => _contentChangedWorker.FilePath;
			set => _contentChangedWorker.FilePath = value;
		}

		public bool IsSilentSession { get; set; }

		public bool CreateBackupFiles
		{
			get => IsSilentSession ? false : _contentChangedWorker.CreateBackupFiles;
			set => _contentChangedWorker.CreateBackupFiles = value;
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

		public object SelectedContent
		{
			get
			{
				if (CurrentDataGrid.IsCurrentCellInEditMode)
					return (CurrentDataGrid.EditingControl as TextBox).SelectedText;
				else
					return CurrentDataGrid.CurrentCell?.Value;
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

		private ContentChangedWorker _contentChangedWorker;

		#endregion Fields

		#region Construction

		public StringEditor()
		{
			Dock = DockStyle.Fill;

			_undoStack = new Stack<DataGridUndoItem>(UndoStackSize);
			_redoStack = new Stack<DataGridUndoItem>(UndoStackSize);

			_contentChangedWorker = new ContentChangedWorker();
			_contentChangedWorker.RunWorkerCompleted += ContentChangedWorker_RunWorkerCompleted;
		}

		#endregion Construction

		#region Override methods

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!CurrentDataGrid.IsCurrentCellInEditMode)
			{
				HandleUndoRedoKeys(keyData);
				HandleTabSwitchingKeys(ref keyData);
				HandleRowDeletion(keyData);
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_contentChangedWorker?.Dispose();

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

		private void HandleTabSwitchingKeys(ref Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Right))
			{
				if (SelectedIndex < TabPages.Count)
					SelectedIndex++;

				keyData = Keys.None;
			}
			else if (keyData == (Keys.Control | Keys.Left))
			{
				if (SelectedIndex > 0)
					SelectedIndex--;

				keyData = Keys.None;
			}
		}

		private void HandleRowDeletion(Keys keyData)
		{
			bool isLastRowSelected = CurrentDataGrid.CurrentRow.Index == CurrentDataGrid.RowCount - 2;

			if (CurrentDataGrid.AllowUserToDeleteRows && isLastRowSelected && keyData == Keys.Delete)
			{
				object stringValue = CurrentDataGrid.CurrentRow.Cells[2].Value;

				if (stringValue != null && stringValue.ToString() != "NULL")
				{
					DialogResult result = DarkMessageBox.Show(this,
						"Are you sure you want to remove the last row?\nThis action cannot be undone!", "Are you sure?",
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

					if (result == DialogResult.Yes)
						CurrentDataGrid.Rows.Remove(CurrentDataGrid.CurrentRow);
				}
				else
					CurrentDataGrid.Rows.Remove(CurrentDataGrid.CurrentRow);

				TryRunContentChangedWorker();
			}
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

		private void ContentChangedWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			LastModified = DateTime.Now;
			IsContentChanged = (bool)e.Result;
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);
		}

		private void DataGrid_SelectionChanged(object sender, EventArgs e)
			=> OnStatusChanged(EventArgs.Empty);

		private void DataGrid_CellContentChanged(object sender, CellContentChangedEventArgs e)
		{
			var source = sender as StringDataGridView;

			_undoStack.Push(new DataGridUndoItem(source, e.ColumnIndex, e.RowIndex, e.OldValue ?? "NULL"));
			_redoStack.Clear();

			TryRunContentChangedWorker();
		}

		private void ExtraNGDataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			var source = sender as DataGridView;

			DataGridViewCell idCell = source[0, e.RowIndex - 1]; // -1 because the new row that was added is a "Add new row" row
			DataGridViewCell hexCell = source[1, e.RowIndex - 1];

			bool isFirstRow = source.RowCount == 2; // The row we just added + the "Add new row" row = 2

			int nextID = isFirstRow ? 0 : int.Parse(source[0, idCell.RowIndex - 1].Value.ToString()) + 1;

			idCell.Value = nextID;
			hexCell.Value = ContentReader.GetShortHex((short)nextID, 3);
		}

		#endregion Events

		#region File I/O

		public void Load(string filePath)
			=> Load(filePath, false);

		public void Load(string filePath, bool silentSession)
		{
			string[] fileLines = File.ReadAllLines(filePath);
			UpdateContent(fileLines);

			FilePath = filePath;

			IsContentChanged = false;
			IsSilentSession = silentSession;
		}

		public void SaveCurrentFile()
			=> Save(FilePath);

		public void Save(string filePath)
		{
			File.WriteAllText(filePath, Content, Encoding.GetEncoding(1252));

			TryRunContentChangedWorker();
		}

		#endregion File I/O

		#region Content

		public void TryRunContentChangedWorker()
		{
			if (!IsSilentSession && !_contentChangedWorker.IsBusy)
				_contentChangedWorker.RunAsync(Content);
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

				currentLineNumber = nextSectionLineNumber + (strings.Count == 0 ? 1 : strings.Count); // May not always be accurate
			}

			RecalculateFontSizes();
			TryRunContentChangedWorker();
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
					string @string = Regex.Replace(strings[i], @"^\d+:\s*", string.Empty);

					dataGrid.Rows.Add(id, hex, @string);

					DataGridViewCell stringCell = dataGrid.Rows[i].Cells[2];

					if (stringCell.Value?.ToString() == "NULL")
						stringCell.Style.ForeColor = Color.Gray;
				}

			dataGrid.AllowUserToAddRows = true;
			dataGrid.AllowUserToDeleteRows = true;
			dataGrid.RowsAdded += ExtraNGDataGrid_RowsAdded;
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

		public void GoToObject(string objectName, object identifyingObject = null)
		{
			TabPage targetTab = TabPages.Cast<TabPage>().ToList().Find(x => x.Name.Equals($"[{objectName}]", StringComparison.OrdinalIgnoreCase));

			if (targetTab != null)
				SelectTab(targetTab);
		}

		/// <summary>
		/// NOTE: If <c>sourceStack</c> is <c>_undoStack</c>, then <c>destStack</c> has to be <c>_redoStack</c> and vice versa.
		/// </summary>
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

			TryRunContentChangedWorker();
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
			if (e.Delta > 0) // Increase
			{
				if (_zoom < MaxZoom)
				{
					_zoom += ZoomStepSize;
					RecalculateFontSizes();
				}
			}
			else // Decrease
			{
				if (_zoom > MinZoom)
				{
					_zoom -= ZoomStepSize;
					RecalculateFontSizes();
				}
			}

			// Invoke the ZoomChanged event
			OnZoomChanged(EventArgs.Empty);
		}

		#endregion Zoom

		#region Other methods

		public void UpdateSettings(ConfigurationBase configuration) // TODO: Create it's own settings for the StringEditor
		{
			var config = configuration as CS_EditorConfiguration;

			DefaultFontSize = (int)config.FontSize - 4; // -4 because of WPF
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
			=> tab.Controls.OfType<StringDataGridView>().First();

		#endregion Other methods
	}
}
