namespace TombIDE.ScriptingStudio.ToolWindows
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
			this.contextMenu = new DarkUI.Controls.DarkContextMenu();
			this.menuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.dataGrid = new DarkUI.Controls.DarkDataGridView();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.searchTextBox = new TombIDE.ScriptingStudio.Controls.SearchTextBox();
			this.contextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
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
			this.menuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
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
			this.dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGrid.ColumnHeadersHeight = 4;
			this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGrid.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.dataGrid.Location = new System.Drawing.Point(3, 30);
			this.dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.dataGrid.MultiSelect = false;
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.ReadOnly = true;
			this.dataGrid.RowHeadersWidth = 42;
			this.dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGrid.Size = new System.Drawing.Size(238, 145);
			this.dataGrid.TabIndex = 2;
			this.dataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellDoubleClick);
			this.dataGrid.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGrid_CellMouseClick);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = new System.Drawing.Point(0, 25);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.treeView);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanel);
			this.splitContainer.Size = new System.Drawing.Size(490, 175);
			this.splitContainer.SplitterDistance = 245;
			this.splitContainer.TabIndex = 3;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.searchTextBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.dataGrid, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.tableLayoutPanel.RowCount = 2;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(241, 175);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(245, 175);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			// 
			// searchTextBox
			// 
			this.searchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchTextBox.Location = new System.Drawing.Point(3, 4);
			this.searchTextBox.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.SearchText = "Search References...";
			this.searchTextBox.SearchTextColor = System.Drawing.Color.DarkGray;
			this.searchTextBox.Size = new System.Drawing.Size(238, 22);
			this.searchTextBox.TabIndex = 1;
			this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
			// 
			// ReferenceBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.splitContainer);
			this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
			this.DockText = "Reference Browser";
			this.Name = "ReferenceBrowser";
			this.SerializationKey = "ReferenceBrowser";
			this.Size = new System.Drawing.Size(490, 200);
			this.contextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private DarkUI.Controls.DarkContextMenu contextMenu;
		private DarkUI.Controls.DarkDataGridView dataGrid;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Copy;
		private Controls.SearchTextBox searchTextBox;
		private System.Windows.Forms.SplitContainer splitContainer;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
	}
}
