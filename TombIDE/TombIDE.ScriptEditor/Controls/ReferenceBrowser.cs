using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace TombIDE.ScriptEditor
{
	internal partial class ReferenceBrowser : UserControl
	{
		public ReferenceBrowser()
		{
			InitializeComponent();

			comboBox_References.SelectedItem = "Mnemonic Constants";
		}

		private void comboBox_References_SelectedIndexChanged(object sender, System.EventArgs e) => UpdateDataGrid();
		private void textBox_Search_TextChanged(object sender, System.EventArgs e) => UpdateDataGrid();

		private void UpdateDataGrid()
		{
			try
			{
				using (XmlReader reader = XmlReader.Create(Path.Combine("References", comboBox_References.SelectedItem + ".xml")))
				{
					using (DataSet dataSet = new DataSet())
					{
						dataSet.ReadXml(reader);

						DataTable dataTable = dataSet.Tables[0];
						DataColumn dcRowString = dataTable.Columns.Add("_RowString", typeof(string));

						foreach (DataRow dataRow in dataTable.Rows)
						{
							StringBuilder builder = new StringBuilder();

							for (int i = 0; i < dataTable.Columns.Count - 1; i++)
							{
								builder.Append(dataRow[i].ToString());
								builder.Append("\t");
							}

							dataRow[dcRowString] = builder.ToString();
						}

						string filter = string.Empty;

						if (!string.IsNullOrWhiteSpace(textBox_Search.Text) && textBox_Search.Text != "Search references...")
							filter = textBox_Search.Text.Trim();

						dataTable.DefaultView.RowFilter = "[_RowString] LIKE '%" + filter + "%'";

						dataGrid.DataSource = dataTable;
						dataGrid.Columns["_RowString"].Visible = false;
					}
				}
			}
			catch
			{
				// Whatever.
			}
		}

		private void textBox_Search_GotFocus(object sender, System.EventArgs e)
		{
			if (textBox_Search.Text == "Search references...")
				textBox_Search.Text = string.Empty;
		}

		private void textBox_Search_LostFocus(object sender, System.EventArgs e)
		{
			if (textBox_Search.Text == string.Empty)
				textBox_Search.Text = "Search references...";
		}

		private void dataGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (dataGrid.GetCellDisplayRectangle(dataGrid.SelectedCells[0].ColumnIndex, dataGrid.SelectedCells[0].RowIndex, false).Contains(dataGrid.PointToClient(Cursor.Position)))
					contextMenu.Show(Cursor.Position);
			}
		}

		private void menuItem_Copy_Click(object sender, System.EventArgs e) =>
			Clipboard.SetText(dataGrid.SelectedCells[0].Value.ToString());
	}
}
