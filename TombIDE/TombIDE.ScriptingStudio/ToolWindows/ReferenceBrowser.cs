using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.Shared;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombIDE.ScriptingStudio.ToolWindows
{
	public partial class ReferenceBrowser : DarkToolWindow
	{
		public ReferenceBrowser()
		{
			InitializeComponent();
			DockText = Strings.Default.ReferenceBrowser;
			searchTextBox.SearchText = Strings.Default.SearchReferences;

			var constantsNode = new DarkTreeNode(Strings.Default.MnemonicConstants) { Tag = ReferenceItemType.MnemonicConstants };

			treeView.Nodes.Add(constantsNode);
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.EnemyDamageValues) { Tag = ReferenceItemType.EnemyDamageValues });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.KeyboardScancodes) { Tag = ReferenceItemType.KeyboardScancodes });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.OCBList) { Tag = ReferenceItemType.OCBList });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.OldCommandsList) { Tag = ReferenceItemType.OldCommandsList });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.NewCommandsList) { Tag = ReferenceItemType.NewCommandsList });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.SoundIndices) { Tag = ReferenceItemType.SoundIndices });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.MoveableSlotIndices) { Tag = ReferenceItemType.MoveableSlotIndices });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.StaticObjectIndices) { Tag = ReferenceItemType.StaticObjectIndices });
			treeView.Nodes.Add(new DarkTreeNode(Strings.Default.VariablePlaceholders) { Tag = ReferenceItemType.VariablePlaceholders });

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				treeView.SelectNode(constantsNode);
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e) => UpdateDataGrid();
		private void searchTextBox_TextChanged(object sender, EventArgs e) => UpdateDataGrid();

		private void UpdateDataGrid()
		{
			dataGrid.Columns.Clear();

			if (treeView.SelectedNodes.Count == 0)
				return;

			try
			{
				string fileName = treeView.SelectedNodes[0].Tag.ToString();
				string xmlPath = Path.Combine(DefaultPaths.ReferencesDirectory, fileName + ".xml");

				using (var reader = XmlReader.Create(xmlPath))
				{
					var dataSet = new DataSet();
					dataSet.ReadXml(reader);

					DataTable dataTable = dataSet.Tables[0];

					if (treeView.SelectedNodes[0] == treeView.Nodes[0])
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

		public void AddPluginMnemonics(DataTable dataTable)
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
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (e.RowIndex == dataGrid.SelectedCells[0].RowIndex)
			{
				switch ((ReferenceItemType)treeView.SelectedNodes[0].Tag)
				{
					case ReferenceItemType.MnemonicConstants:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[2, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.MnemonicConstant)); break;
					case ReferenceItemType.OldCommandsList:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.OldCommand)); break;
					case ReferenceItemType.NewCommandsList:
						OnReferenceDefinitionRequested(new ReferenceDefinitionEventArgs(
							dataGrid[0, dataGrid.SelectedCells[0].RowIndex].Value.ToString(), ReferenceType.NewCommand)); break;
					case ReferenceItemType.OCBList:
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
