using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TombIDE.ScriptEditor.Objects;
using TombIDE.Shared;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombIDE.ScriptEditor.ToolWindows
{
	public partial class ReferenceBrowser : DarkToolWindow
	{
		public ReferenceBrowser()
		{
			InitializeComponent();
			DockText = Strings.Default.ReferenceBrowser;
			searchTextBox.SearchText = Strings.Default.SearchReferences;

			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.MnemonicConstants, ReferenceComboType.MnemonicConstants));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.EnemyDamageValues, ReferenceComboType.EnemyDamageValues));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.KeyboardScancodes, ReferenceComboType.KeyboardScancodes));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.OCBList, ReferenceComboType.OCBList));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.OldCommandsList, ReferenceComboType.OldCommandsList));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.NewCommandsList, ReferenceComboType.NewCommandsList));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.SoundIndices, ReferenceComboType.SoundIndices));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.MoveableSlotIndices, ReferenceComboType.MoveableSlotIndices));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.StaticObjectIndices, ReferenceComboType.StaticObjectIndices));
			comboBox_References.Items.Add(new ReferenceComboItem(Strings.Default.VariablePlaceholders, ReferenceComboType.VariablePlaceholders));

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				comboBox_References.SelectedIndex = 0;
		}

		private void comboBox_References_SelectedIndexChanged(object sender, EventArgs e) => UpdateDataGrid();
		private void searchTextBox_TextChanged(object sender, EventArgs e) => UpdateDataGrid();

		private void UpdateDataGrid()
		{
			dataGrid.Columns.Clear();

			try
			{
				string fileName = (comboBox_References.SelectedItem as ReferenceComboItem).ReferenceType.ToString();
				string xmlPath = Path.Combine(DefaultPaths.ReferencesDirectory, fileName + ".xml");

				using (var reader = XmlReader.Create(xmlPath))
				{
					var dataSet = new DataSet();
					dataSet.ReadXml(reader);

					DataTable dataTable = dataSet.Tables[0];

					if (comboBox_References.SelectedIndex == 0)
						AddPluginMnemonics(dataTable);

					AddFilterRowString(dataTable);
					SetFriendlyColumnHeaders();
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void AddFilterRowString(DataTable dataTable)
		{
			DataColumn dcRowString = dataTable.Columns.Add("_RowString", typeof(string));

			foreach (DataRow dataRow in dataTable.Rows)
			{
				var builder = new StringBuilder();

				for (int i = 0; i < dataTable.Columns.Count - 1; i++)
				{
					builder.Append(dataRow[i].ToString());
					builder.Append("\t");
				}

				dataRow[dcRowString] = builder.ToString();
			}

			string filter = searchTextBox.Text.Trim();
			dataTable.DefaultView.RowFilter = "[_RowString] LIKE '%" + filter + "%'";

			dataGrid.DataSource = dataTable;
			dataGrid.Columns["_RowString"].Visible = false;
		}

		private void AddPluginMnemonics(DataTable dataTable)
		{
			DataTable pluginMnemonicTable = GetPluginMnemonicTable();

			foreach (DataRow row in pluginMnemonicTable.Rows)
				dataTable.Rows.Add(row.ItemArray[0].ToString(), row.ItemArray[1].ToString(), row.ItemArray[2].ToString());
		}

		private DataTable GetPluginMnemonicTable()
		{
			var dataTable = new DataTable();

			dataTable.Columns.Add("decimal", typeof(string));
			dataTable.Columns.Add("hex", typeof(string));
			dataTable.Columns.Add("flag", typeof(string));

			foreach (PluginConstant mnemonic in MnemonicData.PluginConstants)
			{
				DataRow row = dataTable.NewRow();
				row["decimal"] = mnemonic.DecimalValue;
				row["hex"] = mnemonic.HexValue;
				row["flag"] = mnemonic.FlagName;

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
					case "decimal": dataGrid.Columns[column.Index].HeaderText = Strings.Default.DecimalValue; break;
					case "hex": dataGrid.Columns[column.Index].HeaderText = Strings.Default.HexadecimalValue; break;
					case "flag": dataGrid.Columns[column.Index].HeaderText = Strings.Default.Macro; break;
					case "argument1": dataGrid.Columns[column.Index].HeaderText = string.Format(Strings.Default.ArgumentRange, 1); break;
					case "argument2": dataGrid.Columns[column.Index].HeaderText = string.Format(Strings.Default.ArgumentRange, 2); break;
					case "argument3": dataGrid.Columns[column.Index].HeaderText = string.Format(Strings.Default.ArgumentRange, 3); break;
					case "variable": dataGrid.Columns[column.Index].HeaderText = Strings.Default.Variable; break;
					case "description": dataGrid.Columns[column.Index].HeaderText = Strings.Default.Description; break;
					case "sounds": dataGrid.Columns[column.Index].HeaderText = Strings.Default.Sounds; break;
				}
			}
		}

		private void dataGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				if (dataGrid.GetCellDisplayRectangle(dataGrid.SelectedCells[0].ColumnIndex, dataGrid.SelectedCells[0].RowIndex, false).Contains(dataGrid.PointToClient(Cursor.Position)))
					contextMenu.Show(Cursor.Position);
		}

		private void menuItem_Copy_Click(object sender, EventArgs e) =>
			Clipboard.SetText(dataGrid.SelectedCells[0].Value.ToString());

		private void dataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == dataGrid.SelectedCells[0].RowIndex)
			{
				var comboBoxItem = comboBox_References.SelectedItem as ReferenceComboItem;

				switch (comboBoxItem.ReferenceType)
				{
					case ReferenceComboType.MnemonicConstants:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[2, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.MnemonicConstant)); break;
					case ReferenceComboType.OldCommandsList:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.OldCommand)); break;
					case ReferenceComboType.NewCommandsList:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.NewCommand)); break;
					case ReferenceComboType.OCBList:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.OCB)); break;
				}
			}
		}

		public event ReferenceDefinitionRequestedEventHandler ReferenceDefinitionRequested;
		public void OnReferenceDefinitionRequested(ReferenceDefinitionEventArgs e)
			=> ReferenceDefinitionRequested?.Invoke(this, e);
	}
}
