using DarkUI.Forms;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Scripting.Controls;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Controls
{
	public class StringDataGridView : ExtendedDarkDataGridView
	{
		#region Construction

		public StringDataGridView()
		{
			SetNewDefaultSettings();

			CellBeginEdit += OnCellBeginEdit;
			CellEndEdit += OnCellEndEdit;
		}

		private void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			DataGridViewCell tableCell = this[e.ColumnIndex, e.RowIndex];
			tableCell.Style.ForeColor = Color.Gainsboro;
		}

		private void SetNewDefaultSettings()
		{
			AllowUserToAddRows = false;
			AllowUserToDeleteRows = false;
			AllowUserToDragDropRows = false;
			AllowUserToResizeColumns = false;
			AlternateRowStyles = false;
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			BackgroundColor = Color.FromArgb(32, 32, 32);
			ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
			Dock = DockStyle.Fill;
			DynamicVScrollAdjust = true;
			EvenRowColor = Color.FromArgb(32, 32, 32);
			MultiSelect = false;
			OddRowColor = Color.FromArgb(32, 32, 32);
			PreventScrollOnCtrl = true;
			RowTemplate.DefaultCellStyle.Font = new Font("Consolas", 12f);
			RowTemplate.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			SelectionMode = DataGridViewSelectionMode.CellSelect;
			ShowCellToolTips = false;
		}

		#endregion Construction

		#region Events

		private void OnCellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewCell tableCell = this[e.ColumnIndex, e.RowIndex];

			if (tableCell.Value == null && CachedUndoItem.Value == null)
				OnCellContentChanged(new CellContentChangedEventArgs(
					CachedUndoItem.ColumnIndex, CachedUndoItem.RowIndex,
					"NULL", "NULL"));

			if (tableCell.Value == CachedUndoItem.Value)
				tableCell.Style.ForeColor = tableCell.Value?.ToString() == "NULL" ? Color.Gray : Color.LightSalmon;
		}

		protected override void OnCellContentChanged(CellContentChangedEventArgs e)
		{
			DataGridViewCell cell = this[e.ColumnIndex, e.RowIndex];

			string cellValue = cell.Value?.ToString() ?? "NULL";

			if (string.IsNullOrWhiteSpace(cellValue))
				cellValue = "NULL";

			cell.Value = cellValue;
			e.NewValue = cellValue;

			AutoAdjustRowHeight(e.RowIndex);

			cell.Style.ForeColor = cellValue == "NULL" ? Color.Gray : Color.LightSalmon;

			base.OnCellContentChanged(e);
		}

		#endregion Events

		#region Public methods

		public void Initialize(string sectionName)
		{
			Columns.Add("ID", "ID");
			Columns.Add("Hex", "Hex");
			Columns.Add(sectionName, sectionName);

			Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			Columns[0].Resizable = DataGridViewTriState.False;
			Columns[0].ReadOnly = true;
			Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

			Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			Columns[1].Resizable = DataGridViewTriState.False;
			Columns[1].ReadOnly = true;
			Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

			Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;

			Columns[0].DefaultCellStyle.ForeColor = Color.DarkGray;
			Columns[1].DefaultCellStyle.ForeColor = Color.DarkGray;
			Columns[2].DefaultCellStyle.ForeColor = Color.LightSalmon;

			Columns[0].DefaultCellStyle.BackColor = Color.FromArgb(48, 48, 48);
			Columns[1].DefaultCellStyle.BackColor = Color.FromArgb(48, 48, 48);
		}

		public void ApplyFont(Font font)
		{
			foreach (DataGridViewRow row in Rows)
			{
				row.DefaultCellStyle.Font = font;
				Font = font;

				AutoAdjustRowHeight(row.Index);
			}

			using (Graphics g = CreateGraphics())
			{
				int stringWidth = (int)g.MeasureString("00000000", font).Width;

				Columns[0].Width = stringWidth;
				Columns[1].Width = stringWidth;
			}
		}

		public void ClearSelectedString()
		{
			DataGridViewCell selectedCell = SelectedCells.Count > 0 ? SelectedCells[0] : null;

			if (selectedCell != null)
			{
				DataGridViewCell stringCell = selectedCell.OwningRow.Cells[2];
				string cachedValue = stringCell.Value?.ToString();

				stringCell.Value = "NULL";

				OnCellContentChanged(new CellContentChangedEventArgs(
					stringCell.ColumnIndex, stringCell.RowIndex,
					cachedValue, stringCell.Value));
			}
		}

		public void RemoveLastString()
		{
			if (AllowUserToDeleteRows)
			{
				DialogResult result = DarkMessageBox.Show(this,
					"Are you sure you want to remove the last string entry?\nThis action cannot be undone!", "Are you sure?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

				if (result == DialogResult.Yes)
				{
					DataGridViewRow lastRow = Rows[Rows.Count - 2];
					Rows.Remove(lastRow);
				}
			}
			else
				DarkMessageBox.Show(this,
					"Removing strings in this section is not allowed.", "Not allowed",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		#endregion Public methods
	}
}
