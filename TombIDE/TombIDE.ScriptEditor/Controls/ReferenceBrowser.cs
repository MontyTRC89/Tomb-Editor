using DarkUI.Forms;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TombIDE.ScriptEditor.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;

namespace TombIDE.ScriptEditor.Controls
{
	internal partial class ReferenceBrowser : UserControl
	{
		// TODO: Refactor !!!

		private FormMnemonicInfo _mnemonicInfoForm;

		public ReferenceBrowser()
		{
			InitializeComponent();

			comboBox_References.SelectedItem = "Mnemonic Constants";
		}

		public void Initialize(IDE ide)
		{
			_mnemonicInfoForm = new FormMnemonicInfo(ide);
		}

		private void comboBox_References_SelectedIndexChanged(object sender, EventArgs e) => UpdateDataGrid();
		private void textBox_Search_TextChanged(object sender, EventArgs e) => UpdateDataGrid();

		private void UpdateDataGrid()
		{
			dataGrid.Columns.Clear();

			// TEMP !!!

			if (comboBox_References.SelectedItem.ToString() == "OCB List"
				|| comboBox_References.SelectedItem.ToString() == "OLD Commands List"
				|| comboBox_References.SelectedItem.ToString() == "NEW Commands List")
			{
				DataTable listTable = GetListTableFromTextFile(Path.Combine(PathHelper.GetReferencesPath(), comboBox_References.SelectedItem.ToString() + ".txt"), "Items");

				DataColumn dcRowString = listTable.Columns.Add("_RowString", typeof(string));

				foreach (DataRow dataRow in listTable.Rows)
				{
					StringBuilder builder = new StringBuilder();

					for (int i = 0; i < listTable.Columns.Count - 1; i++)
					{
						builder.Append(dataRow[i].ToString());
						builder.Append("\t");
					}

					dataRow[dcRowString] = builder.ToString();
				}

				string filter = string.Empty;

				if (!string.IsNullOrWhiteSpace(textBox_Search.Text) && textBox_Search.Text != "Search references...")
					filter = textBox_Search.Text.Trim();

				listTable.DefaultView.RowFilter = "[_RowString] LIKE '%" + filter + "%'";

				dataGrid.DataSource = listTable;
				dataGrid.Columns["_RowString"].Visible = false;

				return;
			}

			// !!!

			try
			{
				string xmlPath = Path.Combine(PathHelper.GetReferencesPath(), comboBox_References.SelectedItem + ".xml");

				using (XmlReader reader = XmlReader.Create(xmlPath))
				{
					using (DataSet dataSet = new DataSet())
					{
						dataSet.ReadXml(reader);

						DataTable dataTable = dataSet.Tables[0];

						if (comboBox_References.SelectedIndex == 0)
						{
							DataTable pluginMnemonicTable = GetPluginMnemonicTable();

							foreach (DataRow row in pluginMnemonicTable.Rows)
								dataTable.Rows.Add(row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), row.ItemArray[2].ToString());
						}

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

						SetFriendlyColumnHeaders();
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				// Don't do anything
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private DataTable GetPluginMnemonicTable()
		{
			DataTable dataTable = new DataTable();

			dataTable.Columns.Add("decimal", typeof(string));
			dataTable.Columns.Add("hex", typeof(string));
			dataTable.Columns.Add("flag", typeof(string));

			foreach (PluginMnemonic mnemonic in ScriptKeywords.PluginMnemonics)
			{
				DataRow row = dataTable.NewRow();
				row["decimal"] = mnemonic.DecimalValue;
				row["hex"] = mnemonic.HexValue;
				row["flag"] = mnemonic.FlagName;

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		private DataTable GetListTableFromTextFile(string filePath, string listHeaderTitle)
		{
			DataTable dataTable = new DataTable();

			dataTable.Columns.Add(listHeaderTitle, typeof(string));

			string[] entries = File.ReadAllLines(filePath);

			foreach (string entry in entries)
			{
				DataRow row = dataTable.NewRow();
				row[listHeaderTitle] = entry;

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		private void SetFriendlyColumnHeaders()
		{
			foreach (DataGridViewColumn column in dataGrid.Columns)
			{
				switch (column.Name)
				{
					case "decimal":
						dataGrid.Columns[column.Index].HeaderText = "Decimal Value";
						break;

					case "hex":
						dataGrid.Columns[column.Index].HeaderText = "Hexadecimal Value";
						break;

					case "flag":
						dataGrid.Columns[column.Index].HeaderText = "Flag";
						break;

					case "argument1":
						dataGrid.Columns[column.Index].HeaderText = "Argument #1 (Range)";
						break;

					case "argument2":
						dataGrid.Columns[column.Index].HeaderText = "Argument #2 (Range)";
						break;

					case "argument3":
						dataGrid.Columns[column.Index].HeaderText = "Argument #3 (Range)";
						break;

					case "variable":
						dataGrid.Columns[column.Index].HeaderText = "Variable";
						break;

					case "description":
						dataGrid.Columns[column.Index].HeaderText = "Description";
						break;

					case "sounds":
						dataGrid.Columns[column.Index].HeaderText = "Sounds";
						break;
				}
			}
		}

		private void textBox_Search_GotFocus(object sender, EventArgs e)
		{
			if (textBox_Search.Text == "Search references...")
				textBox_Search.Text = string.Empty;
		}

		private void textBox_Search_LostFocus(object sender, EventArgs e)
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

		private void menuItem_Copy_Click(object sender, EventArgs e) =>
			Clipboard.SetText(dataGrid.SelectedCells[0].Value.ToString());

		private void dataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == dataGrid.SelectedCells[0].RowIndex)
			{
				string comboBoxItem = comboBox_References.SelectedItem.ToString();

				if (comboBox_References.SelectedIndex == 0)
					_mnemonicInfoForm.Show(dataGrid[2, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.Other);
				else if (comboBoxItem == "OCB List" || comboBoxItem == "OLD Commands List" || comboBoxItem == "NEW Commands List")
				{
					switch (comboBoxItem)
					{
						case "OCB List":
							_mnemonicInfoForm.Show(dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.OCB);
							break;

						case "OLD Commands List":
							_mnemonicInfoForm.Show(dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.OLDCommands);
							break;

						case "NEW Commands List":
							_mnemonicInfoForm.Show(dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.NEWCommands);
							break;
					}
				}
			}
		}
	}
}
