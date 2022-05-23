﻿namespace TombIDE.ScriptingStudio.ToolWindows
{
	partial class ContentExplorer
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
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.searchTextBox = new TombIDE.ScriptingStudio.Controls.SearchTextBox();
			this.SuspendLayout();
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Location = new System.Drawing.Point(3, 54);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(219, 168);
			this.treeView.TabIndex = 3;
			this.treeView.Click += new System.EventHandler(this.treeView_Click);
			// 
			// searchTextBox
			// 
			this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchTextBox.Location = new System.Drawing.Point(3, 28);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.SearchText = "Search Contents...";
			this.searchTextBox.SearchTextColor = System.Drawing.Color.DarkGray;
			this.searchTextBox.Size = new System.Drawing.Size(219, 22);
			this.searchTextBox.TabIndex = 4;
			this.searchTextBox.TextChanged += new System.EventHandler(this.textBox_Search_TextChanged);
			// 
			// ContentExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.searchTextBox);
			this.Controls.Add(this.treeView);
			this.DockText = "Content Explorer";
			this.Name = "ContentExplorer";
			this.SerializationKey = "ContentExplorer";
			this.Size = new System.Drawing.Size(225, 225);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkTreeView treeView;
		private Controls.SearchTextBox searchTextBox;
	}
}
