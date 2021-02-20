﻿namespace TombIDE.ScriptEditor.ToolWindows
{
	partial class ReferenceBrowser
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.comboBox_References = new DarkUI.Controls.DarkComboBox();
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.dataGrid = new DarkUI.Controls.DarkDataGridView();
			this.searchTextBox = new TombIDE.ScriptEditor.Controls.SearchTextBox();
			this.contextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// comboBox_References
			// 
			this.comboBox_References.FormattingEnabled = true;
			this.comboBox_References.Location = new System.Drawing.Point(3, 28);
			this.comboBox_References.Margin = new System.Windows.Forms.Padding(3, 3, 0, 4);
			this.comboBox_References.Name = "comboBox_References";
			this.comboBox_References.Size = new System.Drawing.Size(195, 21);
			this.comboBox_References.TabIndex = 0;
			this.comboBox_References.SelectedIndexChanged += new System.EventHandler(this.comboBox_References_SelectedIndexChanged);
			// 
			// contextMenu
			// 
			this.contextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Copy});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(103, 26);
			// 
			// menuItem_Copy
			// 
			this.menuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Copy.ForeColor = System.Drawing.Color.Gainsboro;
			this.menuItem_Copy.Name = "menuItem_Copy";
			this.menuItem_Copy.Size = new System.Drawing.Size(102, 22);
			this.menuItem_Copy.Text = "Copy";
			this.menuItem_Copy.Click += new System.EventHandler(this.menuItem_Copy_Click);
			// 
			// dataGrid
			// 
			this.dataGrid.AllowUserToAddRows = false;
			this.dataGrid.AllowUserToDeleteRows = false;
			this.dataGrid.AllowUserToDragDropRows = false;
			this.dataGrid.AllowUserToOrderColumns = true;
			this.dataGrid.AllowUserToPasteCells = false;
			this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGrid.ColumnHeadersHeight = 4;
			this.dataGrid.Location = new System.Drawing.Point(0, 55);
			this.dataGrid.Margin = new System.Windows.Forms.Padding(2);
			this.dataGrid.MultiSelect = false;
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.ReadOnly = true;
			this.dataGrid.RowHeadersWidth = 42;
			this.dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGrid.Size = new System.Drawing.Size(400, 145);
			this.dataGrid.TabIndex = 2;
			this.dataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellDoubleClick);
			this.dataGrid.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGrid_CellMouseClick);
			// 
			// searchTextBox
			// 
			this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchTextBox.Location = new System.Drawing.Point(202, 29);
			this.searchTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 3, 3);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.SearchText = "Search References...";
			this.searchTextBox.SearchTextColor = System.Drawing.Color.DarkGray;
			this.searchTextBox.Size = new System.Drawing.Size(195, 20);
			this.searchTextBox.TabIndex = 1;
			this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
			// 
			// ReferenceBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.searchTextBox);
			this.Controls.Add(this.dataGrid);
			this.Controls.Add(this.comboBox_References);
			this.DockText = "Reference Browser";
			this.MinimumSize = new System.Drawing.Size(400, 200);
			this.Name = "ReferenceBrowser";
			this.Size = new System.Drawing.Size(400, 200);
			this.contextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkComboBox comboBox_References;
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkDataGridView dataGrid;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Copy;
		private Controls.SearchTextBox searchTextBox;
	}
}
