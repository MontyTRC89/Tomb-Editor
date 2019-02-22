using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class ReferenceBrowser : UserControl
	{
		public ReferenceBrowser()
		{
			InitializeComponent();
		}

		private void ReferenceBrowser_Invalidated(object sender, EventArgs e)
		{
			refSelectionComboBox.SelectedIndex = 0;
		}

		private void refSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				DataTable table = ReadCSV(@"References\" + refSelectionComboBox.Text + ".csv");

				refDataGrid.Rows.Clear();
				refDataGrid.Columns.Clear();

				foreach (DataColumn dc in table.Columns)
				{
					refDataGrid.Columns.Add(new DataGridViewTextBoxColumn());
				}

				foreach (DataRow dr in table.Rows)
				{
					refDataGrid.Rows.Add(dr.ItemArray);
				}
			}
			catch (Exception ex)
			{
				//DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void refSearchTextBox_GotFocus(object sender, EventArgs e)
		{
			if (refSearchTextBox.Text == "Search references...")
			{
				refSearchTextBox.Text = string.Empty;
			}
		}

		private void refSearchTextBox_LostFocus(object sender, EventArgs e)
		{
			if (refSearchTextBox.Text == string.Empty)
			{
				refSearchTextBox.Text = "Search references...";
			}
		}

		private static DataTable ReadCSV(string filePath)
		{
			DataTable table = new DataTable();

			// Creating the columns
			foreach (string headerLine in File.ReadLines(filePath).Take(1))
			{
				foreach (string headerItem in headerLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					table.Columns.Add(headerItem.Trim());
				}
			}

			// Adding the rows
			foreach (string line in File.ReadLines(filePath).Skip(1))
			{
				table.Rows.Add(line.Split(';'));
			}

			return table;
		}
	}
}
