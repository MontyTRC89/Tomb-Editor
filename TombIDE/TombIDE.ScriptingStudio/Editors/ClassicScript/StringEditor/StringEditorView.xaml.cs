#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

public partial class StringEditorView : UserControl, IEditorControl, IStringSectionNavigator
{
	#region IEditorControl properties

	public EditorType EditorType => EditorType.Strings;
	public string DefaultFileExtension => ".txt";

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

	public new string Content
	{
		get => Strings.ContentBuilder.BuildContent(_viewModel.Sections);
		set => UpdateContent(value);
	}

	public bool IsContentChanged { get; set; }

	public DateTime LastModified { get; set; }

	public int CurrentRow
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();
			if (grid is null || grid.CurrentCell.Item is not StringTableRow row)
				return 0;

			return grid.Items.IndexOf(row);
		}
	}

	public int CurrentColumn
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();
			return grid?.CurrentColumn?.DisplayIndex ?? 0;
		}
	}

	public object SelectedContent
	{
		get
		{
			DataGrid? grid = GetCurrentDataGrid();
			if (grid is null)
				return string.Empty;

			if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
				return GetCellValue(row, grid.CurrentColumn.DisplayIndex) ?? string.Empty;

			return string.Empty;
		}
	}

	public int SelectionLength => GetCurrentDataGrid()?.SelectedCells.Count ?? 0;

	public int Zoom
	{
		get => _viewModel.ZoomLevel;
		set
		{
			_viewModel.ZoomLevel = value;
			ApplyZoomToAllGrids();
		}
	}

	public int MinZoom { get; set; } = 25;
	public int MaxZoom { get; set; } = 400;
	public int ZoomStepSize { get; set; } = 15;

	public bool CanUndo => _viewModel.CanUndo;
	public bool CanRedo => _viewModel.CanRedo;

	public Version EngineVersion { get; set; } = new Version(0, 0);

	#endregion IEditorControl properties

	#region IStringSectionNavigator

	string? IStringSectionNavigator.CurrentSectionName => _viewModel.SelectedSection?.SectionName;

	public void GoToPreviousSection()
	{
		if (_viewModel.SelectedSectionIndex > 0)
			_viewModel.SelectedSectionIndex--;
	}

	public void GoToNextSection()
	{
		if (_viewModel.SelectedSectionIndex < _viewModel.Sections.Count - 1)
			_viewModel.SelectedSectionIndex++;
	}

	public void ClearSelectedString()
	{
		DataGrid? grid = GetCurrentDataGrid();
		if (grid?.CurrentCell.Item is StringTableRow row)
		{
			string? cachedValue = row.StringValue;
			row.StringValue = "NULL";

			_viewModel.PushUndo(new DataGridUndoItem(
				_viewModel.SelectedSection?.SectionName ?? string.Empty,
				grid.Items.IndexOf(row),
				2,
				cachedValue));
		}
	}

	public void RemoveLastString()
	{
		StringTableSection? section = _viewModel.SelectedSection;
		if (section is null || !section.IsExtraNG)
			return;

		if (section.Rows.Count > 0)
		{
			StringTableRow lastRow = section.Rows[^1];
			section.Rows.Remove(lastRow);

			IsContentChanged = true;
			TryRunContentChangedWorker();
		}
	}

	#endregion IStringSectionNavigator

	#region Fields

	private readonly StringEditorViewModel _viewModel;
	private readonly ContentPersistenceCoordinator _contentPersistenceCoordinator;

	#endregion Fields

	#region Construction

	public StringEditorView(Version engineVersion)
	{
		_viewModel = new StringEditorViewModel();
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		DataContext = _viewModel;

		_contentPersistenceCoordinator = new ContentPersistenceCoordinator(() => Content, () => IsSilentSession);
		_contentPersistenceCoordinator.ContentChangedWorkerRunCompleted += (s, e) =>
			OnContentChangedWorkerRunCompleted(EventArgs.Empty);

		EngineVersion = engineVersion;

		InitializeComponent();
	}

	#endregion Construction

	#region Dispose

	public void Dispose()
	{
		DetachRowCollectionListeners();
		_contentPersistenceCoordinator?.Dispose();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(StringEditorViewModel.CanUndo) ||
			e.PropertyName == nameof(StringEditorViewModel.CanRedo))
			OnStatusChanged(EventArgs.Empty);
	}

	#endregion Dispose

	#region Events

	public event EventHandler? StatusChanged;

	protected virtual void OnStatusChanged(EventArgs e)
		=> StatusChanged?.Invoke(this, e);

	public event EventHandler? ZoomChanged;

	protected virtual void OnZoomChanged(EventArgs e)
	{
		ZoomChanged?.Invoke(this, e);
		OnStatusChanged(EventArgs.Empty);
	}

	public event EventHandler? ContentChangedWorkerRunCompleted;

	protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

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

	public void TryRunContentChangedWorker()
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
		DetachRowCollectionListeners();
		_viewModel.Sections.Clear();
		_viewModel.ClearUndoRedo();

		int currentLineNumber = 0;
		int totalStringCount = 0;

		while (ContentReader.NextSectionExists(lines, currentLineNumber, out int nextSectionLineNumber))
		{
			string currentSectionName = lines[nextSectionLineNumber];
			List<string> strings = ContentReader.GetStrings(lines, nextSectionLineNumber);

			bool isExtraNG = Regex.IsMatch(currentSectionName, @"^\[ExtraNG\]", RegexOptions.IgnoreCase);

			var section = new StringTableSection
			{
				SectionName = currentSectionName,
				Mode = isExtraNG ? StringTableMode.ExtraNG : StringTableMode.Normal
			};

			if (isExtraNG)
			{
				PopulateExtraNGRows(section, strings);
			}
			else
			{
				PopulateNormalRows(section, strings, totalStringCount);
				totalStringCount += strings.Count;
			}

			_viewModel.Sections.Add(section);

			currentLineNumber = nextSectionLineNumber + (strings.Count == 0 ? 1 : strings.Count);
		}

		if (_viewModel.Sections.Count > 0 && _viewModel.SelectedSectionIndex < 0)
			_viewModel.SelectedSectionIndex = 0;

		AttachRowCollectionListeners();
		ApplyZoomToAllGrids();
		TryRunContentChangedWorker();
	}

	private static void PopulateNormalRows(StringTableSection section, List<string> strings, int idOffset)
	{
		for (int i = 0; i < strings.Count; i++)
		{
			short id = (short)(idOffset + i);
			string hex = ContentReader.GetShortHex(id, 4);

			section.Rows.Add(new StringTableRow
			{
				Id = id,
				HexValue = hex,
				StringValue = strings[i]
			});
		}
	}

	private static void PopulateExtraNGRows(StringTableSection section, List<string> strings)
	{
		for (int i = 0; i < strings.Count; i++)
		{
			if (!Regex.IsMatch(strings[i], @"^\d+:.*"))
				continue;

			short id = short.Parse(strings[i].Split(':').First());
			string hex = ContentReader.GetShortHex(id, 3);
			string value = Regex.Replace(strings[i], @"^\d+:", string.Empty).TrimStart(' ');

			section.Rows.Add(new StringTableRow
			{
				Id = id,
				HexValue = hex,
				StringValue = value
			});
		}
	}

	#endregion Content

	#region Edit methods

	public void Undo()
	{
		_viewModel.Undo();
		LastModified = DateTime.Now;
		TryRunContentChangedWorker();
	}

	public void Redo()
	{
		_viewModel.Redo();
		LastModified = DateTime.Now;
		TryRunContentChangedWorker();
	}

	public void Cut()
	{
		DataGrid? grid = GetCurrentDataGrid();
		if (grid is null)
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
		{
			object? value = GetCellValue(row, grid.CurrentColumn.DisplayIndex);

			if (value is not null)
				Clipboard.SetText(value.ToString());

			if (!grid.CurrentColumn.IsReadOnly)
			{
				string? cachedValue = row.StringValue;
				row.StringValue = string.Empty;

				_viewModel.PushUndo(new DataGridUndoItem(
					_viewModel.SelectedSection?.SectionName ?? string.Empty,
					grid.Items.IndexOf(row),
					grid.CurrentColumn.DisplayIndex,
					cachedValue));
			}
		}
	}

	public void Copy()
	{
		DataGrid? grid = GetCurrentDataGrid();
		if (grid is null)
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null)
		{
			object? value = GetCellValue(row, grid.CurrentColumn.DisplayIndex);

			if (value is not null)
				Clipboard.SetText(value.ToString());
		}
	}

	public void Paste()
	{
		DataGrid? grid = GetCurrentDataGrid();
		if (grid is null)
			return;

		if (!Clipboard.ContainsText())
			return;

		if (grid.CurrentCell.Item is StringTableRow row && grid.CurrentColumn is not null && !grid.CurrentColumn.IsReadOnly)
		{
			string? cachedValue = row.StringValue;
			row.StringValue = Clipboard.GetText();

			_viewModel.PushUndo(new DataGridUndoItem(
				_viewModel.SelectedSection?.SectionName ?? string.Empty,
				grid.Items.IndexOf(row),
				grid.CurrentColumn.DisplayIndex,
				cachedValue));

			IsContentChanged = true;
			TryRunContentChangedWorker();
		}
	}

	public void SelectAll()
	{
		DataGrid? grid = GetCurrentDataGrid();
		grid?.SelectAll();
	}

	public void GoToObject(string objectName, object? identifyingObject = null)
	{
		// Callers pass the bare section name; the model stores it with brackets
		// (e.g. "[Section1]"), matching the old WinForms behavior.
		string bracketedName = $"[{objectName}]";

		for (int i = 0; i < _viewModel.Sections.Count; i++)
		{
			if (_viewModel.Sections[i].SectionName.Equals(bracketedName, StringComparison.OrdinalIgnoreCase))
			{
				_viewModel.SelectedSectionIndex = i;
				return;
			}
		}
	}

	#endregion Edit methods

	#region Settings

	public void UpdateSettings(ConfigurationBase configuration)
	{
		if (configuration is not TextEditorConfigBase config)
			return;

		_viewModel.FontSize = (int)config.FontSize - 4;
		_viewModel.FontFamily = config.FontFamily;

		ApplyZoomToAllGrids();
	}

	#endregion Settings

	#region Zoom

	private void ApplyZoomToAllGrids()
	{
		double fontSize = _viewModel.FontSize * _viewModel.ZoomLevel / 100.0;
		var fontFamily = new FontFamily(_viewModel.FontFamily);

		foreach (DataGrid grid in GetAllDataGrids())
		{
			grid.FontFamily = fontFamily;
			grid.FontSize = fontSize;
			grid.RowHeight = Double.NaN; // Auto row height

			UpdateColumnWidths(grid, fontSize);
		}
	}

	private static void UpdateColumnWidths(DataGrid grid, double fontSize)
	{
		double stringWidth = fontSize * 4.5;

		if (grid.Columns.Count >= 2)
		{
			grid.Columns[0].Width = new DataGridLength(stringWidth);
			grid.Columns[1].Width = new DataGridLength(stringWidth);
		}
	}

	#endregion Zoom

	#region Keyboard handlers

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		// Shift+Enter inserts a newline when editing a DataGrid cell,
		// matching the old WinForms DataGridView behavior with WrapMode.
		if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
		{
			if (Keyboard.FocusedElement is TextBox textBox &&
				textBox.TemplatedParent is DataGridCell)
			{
				int caretIndex = textBox.CaretIndex;
				textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
				textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
				e.Handled = true;
				return;
			}
		}

		if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Undo();
			e.Handled = true;
		}
		else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Redo();
			e.Handled = true;
		}
		else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Cut();
			e.Handled = true;
		}
		else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Copy();
			e.Handled = true;
		}
		else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
		{
			Paste();
			e.Handled = true;
		}
		else if (e.Key == Key.PageUp && Keyboard.Modifiers == ModifierKeys.Control)
		{
			GoToPreviousSection();
			e.Handled = true;
		}
		else if (e.Key == Key.PageDown && Keyboard.Modifiers == ModifierKeys.Control)
		{
			GoToNextSection();
			e.Handled = true;
		}
	}

	private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control)
		{
			if (e.Delta > 0)
			{
				if (_viewModel.ZoomLevel < MaxZoom)
				{
					_viewModel.ZoomLevel += ZoomStepSize;
					ApplyZoomToAllGrids();
				}
			}
			else
			{
				if (_viewModel.ZoomLevel > MinZoom)
				{
					_viewModel.ZoomLevel -= ZoomStepSize;
					ApplyZoomToAllGrids();
				}
			}

			OnZoomChanged(EventArgs.Empty);
			e.Handled = true;
		}
	}

	#endregion Keyboard handlers

	#region DataGrid event handlers

	private void DataGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
	{
		if (e.Row.Item is StringTableRow row && e.Column is not null)
		{
			// Cache the current value for undo before edit begins.
			_cachedBeginEditValue = GetCellValue(row, e.Column.DisplayIndex);
		}
	}

	private void DataGrid_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
	{
		// Let the editing TextBox accept newline characters so Shift+Enter
		// can insert them; the DataGrid PreviewKeyDown handler intercepts
		// Shift+Enter to prevent it from committing the edit.
		if (e.EditingElement is TextBox textBox)
			textBox.AcceptsReturn = true;
	}

	private void DataGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
	{
		if (e.Row.Item is StringTableRow row && e.Column is not null)
		{
			object? newValue = GetCellValue(row, e.Column.DisplayIndex);

			if (e.EditAction == DataGridEditAction.Commit && !Equals(_cachedBeginEditValue, newValue))
			{
				_viewModel.PushUndo(new DataGridUndoItem(
					_viewModel.SelectedSection?.SectionName ?? string.Empty,
					e.Row.GetIndex(),
					e.Column.DisplayIndex,
					_cachedBeginEditValue));

				LastModified = DateTime.Now;
			}

			_cachedBeginEditValue = null;
		}

		// Always run the dirty check when a cell edit ends, so the file is
		// marked as modified regardless of whether the cached begin-edit
		// value matches the final value.
		TryRunContentChangedWorker();
	}

	private void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
	{
		if (e.Row.Item is StringTableRow row)
		{
			// Style cells based on value.
			foreach (DataGridColumn column in ((DataGrid)sender!).Columns)
			{
				if (column.GetCellContent(e.Row) is TextBlock textBlock)
				{
					string? cellValue = GetCellValue(row, column.DisplayIndex)?.ToString();

					if (cellValue == "NULL")
						textBlock.Foreground = Brushes.Gray;
					else
						textBlock.Foreground = Brushes.LightSalmon;
				}
			}
		}
	}

	private void DataGrid_InitializingNewItem(object? sender, InitializingNewItemEventArgs e)
	{
		if (e.NewItem is StringTableRow row)
		{
			StringTableSection? section = _viewModel.SelectedSection;

			if (section is not null && section.IsExtraNG)
			{
				int nextId = section.Rows.Count > 0
					? section.Rows[^1].Id + 1
					: 0;

				row.Id = nextId;
				row.HexValue = ContentReader.GetShortHex((short)nextId, 3);
			}
		}
	}

	#endregion DataGrid event handlers

	#region Helpers

	private object? _cachedBeginEditValue;

	private DataGrid? GetCurrentDataGrid()
	{
		if (_viewModel.SelectedSection is null)
			return null;

		TabItem? selectedTab = SectionTabs.ItemContainerGenerator
			.ContainerFromIndex(_viewModel.SelectedSectionIndex) as TabItem;

		if (selectedTab is null)
			return null;

		return FindVisualChild<DataGrid>(selectedTab);
	}

	private IEnumerable<DataGrid> GetAllDataGrids()
	{
		for (int i = 0; i < _viewModel.Sections.Count; i++)
		{
			if (SectionTabs.ItemContainerGenerator.ContainerFromIndex(i) is TabItem tabItem)
			{
				DataGrid? grid = FindVisualChild<DataGrid>(tabItem);
				if (grid is not null)
					yield return grid;
			}
		}
	}

	private static DataGrid? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);

			if (child is T found)
				return found as DataGrid;

			DataGrid? result = FindVisualChild<T>(child);
			if (result is not null)
				return result;
		}

		return null;
	}

	private static object? GetCellValue(StringTableRow row, int columnIndex) => columnIndex switch
	{
		0 => row.Id,
		1 => row.HexValue,
		2 => row.StringValue,
		_ => null
	};

	#endregion Helpers

	#region Row collection change tracking

	private void AttachRowCollectionListeners()
	{
		foreach (StringTableSection section in _viewModel.Sections)
			section.Rows.CollectionChanged += OnRowsCollectionChanged;
	}

	private void DetachRowCollectionListeners()
	{
		foreach (StringTableSection section in _viewModel.Sections)
			section.Rows.CollectionChanged -= OnRowsCollectionChanged;
	}

	private void OnRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add ||
			e.Action == NotifyCollectionChangedAction.Remove)
		{
			LastModified = DateTime.Now;
			TryRunContentChangedWorker();
		}
	}

	#endregion Row collection change tracking
}
