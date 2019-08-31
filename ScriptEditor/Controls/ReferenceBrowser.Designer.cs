namespace ScriptEditor
{
	partial class ReferenceBrowser
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.refDataGrid = new DarkUI.Controls.DarkDataGridView();
			this.refSearchTextBox = new DarkUI.Controls.DarkTextBox();
			this.refSelectionComboBox = new DarkUI.Controls.DarkComboBox();
			((System.ComponentModel.ISupportInitialize)(this.refDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// refDataGrid
			// 
			this.refDataGrid.AllowUserToAddRows = false;
			this.refDataGrid.AllowUserToDeleteRows = false;
			this.refDataGrid.AllowUserToDragDropRows = false;
			this.refDataGrid.AllowUserToOrderColumns = true;
			this.refDataGrid.AllowUserToPasteCells = false;
			this.refDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.refDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.refDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.refDataGrid.ColumnHeadersHeight = 4;
			this.refDataGrid.Location = new System.Drawing.Point(0, 31);
			this.refDataGrid.Margin = new System.Windows.Forms.Padding(2);
			this.refDataGrid.MultiSelect = false;
			this.refDataGrid.Name = "refDataGrid";
			this.refDataGrid.ReadOnly = true;
			this.refDataGrid.RowHeadersWidth = 42;
			this.refDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.refDataGrid.Size = new System.Drawing.Size(800, 129);
			this.refDataGrid.TabIndex = 3;
			// 
			// refSearchTextBox
			// 
			this.refSearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.refSearchTextBox.Location = new System.Drawing.Point(267, 4);
			this.refSearchTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 4, 5);
			this.refSearchTextBox.Name = "refSearchTextBox";
			this.refSearchTextBox.Size = new System.Drawing.Size(529, 20);
			this.refSearchTextBox.TabIndex = 1;
			this.refSearchTextBox.Text = "Search references...";
			this.refSearchTextBox.GotFocus += new System.EventHandler(this.refSearchTextBox_GotFocus);
			this.refSearchTextBox.LostFocus += new System.EventHandler(this.refSearchTextBox_LostFocus);
			// 
			// refSelectionComboBox
			// 
			this.refSelectionComboBox.FormattingEnabled = true;
			this.refSelectionComboBox.Items.AddRange(new object[] {
            "MnemonicConstants",
            "DamageEnemyList",
            "KeyboardScancodeList",
            "OCB LIST",
            "SCRIPT NEW COMMANDS",
            "SCRIPT OLD COMMANDS",
            "SLOT MOVEABLES INDICES LIST",
            "SOUND SFX INDICES LIST",
            "STATICS INDICES LIST",
            "VARIABLE PLACEFOLDERS"});
			this.refSelectionComboBox.Location = new System.Drawing.Point(4, 4);
			this.refSelectionComboBox.Margin = new System.Windows.Forms.Padding(4);
			this.refSelectionComboBox.Name = "refSelectionComboBox";
			this.refSelectionComboBox.Size = new System.Drawing.Size(256, 21);
			this.refSelectionComboBox.TabIndex = 2;
			this.refSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.refSelectionComboBox_SelectedIndexChanged);
			// 
			// ReferenceBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.refDataGrid);
			this.Controls.Add(this.refSelectionComboBox);
			this.Controls.Add(this.refSearchTextBox);
			this.Name = "ReferenceBrowser";
			this.Size = new System.Drawing.Size(800, 160);
			this.Invalidated += new System.Windows.Forms.InvalidateEventHandler(this.ReferenceBrowser_Invalidated);
			((System.ComponentModel.ISupportInitialize)(this.refDataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkComboBox refSelectionComboBox;
		private DarkUI.Controls.DarkDataGridView refDataGrid;
		private DarkUI.Controls.DarkTextBox refSearchTextBox;
	}
}
