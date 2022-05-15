namespace TombIDE.ScriptingStudio.ToolWindows
{
	partial class SearchResults
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
			this.SuspendLayout();
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ExpandOnDoubleClick = false;
			this.treeView.Location = new System.Drawing.Point(0, 25);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(400, 175);
			this.treeView.TabIndex = 0;
			this.treeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDoubleClick);
			// 
			// SearchResults
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.treeView);
			this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
			this.DockText = "Search Results";
			this.Name = "SearchResults";
			this.SerializationKey = "SearchResults";
			this.Size = new System.Drawing.Size(400, 200);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkTreeView treeView;
	}
}
