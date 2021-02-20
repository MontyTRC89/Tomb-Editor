using TombLib.Scripting.Controls;

namespace TombLib.Scripting.Objects
{
	public class DataGridUndoItem
	{
		public ExtendedDarkDataGridView Source { get; }
		public int ColumnIndex { get; }
		public int RowIndex { get; }
		public object Value { get; }

		public DataGridUndoItem(ExtendedDarkDataGridView source, int columnIndex, int rowIndex, object value)
		{
			Source = source;
			ColumnIndex = columnIndex;
			RowIndex = rowIndex;
			Value = value;
		}
	}
}
