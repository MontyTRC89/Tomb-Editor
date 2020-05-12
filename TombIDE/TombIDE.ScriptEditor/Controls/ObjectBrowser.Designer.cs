namespace TombIDE.ScriptEditor.Controls
{
	partial class ObjectBrowser
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
			this.components = new System.ComponentModel.Container();
			this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.textBox_Search = new DarkUI.Controls.DarkTextBox();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// backgroundWorker
			// 
			this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
			this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.treeView);
			this.sectionPanel.Controls.Add(this.textBox_Search);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Object Browser";
			this.sectionPanel.Size = new System.Drawing.Size(200, 315);
			this.sectionPanel.TabIndex = 0;
			this.sectionPanel.Text = "Object Browser";
			// 
			// treeView
			// 
			this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView.Location = new System.Drawing.Point(4, 51);
			this.treeView.MaxDragChange = 20;
			this.treeView.Name = "treeView";
			this.treeView.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Size = new System.Drawing.Size(190, 258);
			this.treeView.TabIndex = 1;
			this.treeView.Click += new System.EventHandler(this.treeView_Click);
			// 
			// textBox_Search
			// 
			this.textBox_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Search.Location = new System.Drawing.Point(4, 28);
			this.textBox_Search.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.textBox_Search.Name = "textBox_Search";
			this.textBox_Search.Size = new System.Drawing.Size(190, 20);
			this.textBox_Search.TabIndex = 0;
			this.textBox_Search.Text = "Search objects...";
			this.textBox_Search.TextChanged += new System.EventHandler(this.textBox_Search_TextChanged);
			this.textBox_Search.Enter += new System.EventHandler(this.textBox_Search_Enter);
			this.textBox_Search.Leave += new System.EventHandler(this.textBox_Search_Leave);
			// 
			// timer
			// 
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// ObjectBrowser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.sectionPanel);
			this.Name = "ObjectBrowser";
			this.Size = new System.Drawing.Size(200, 315);
			this.sectionPanel.ResumeLayout(false);
			this.sectionPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTextBox textBox_Search;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.ComponentModel.BackgroundWorker backgroundWorker;
		private System.Windows.Forms.Timer timer;
	}
}
