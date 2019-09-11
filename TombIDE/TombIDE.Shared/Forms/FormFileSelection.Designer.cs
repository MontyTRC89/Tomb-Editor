namespace TombIDE.Shared
{
	partial class FormFileSelection
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

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.button_Confirm = new DarkUI.Controls.DarkButton();
			this.panel = new System.Windows.Forms.Panel();
			this.sectionPanel = new DarkUI.Controls.DarkSectionPanel();
			this.treeView = new DarkUI.Controls.DarkTreeView();
			this.panel.SuspendLayout();
			this.sectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Confirm
			// 
			this.button_Confirm.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Confirm.Enabled = false;
			this.button_Confirm.Location = new System.Drawing.Point(11, 8);
			this.button_Confirm.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.button_Confirm.Name = "button_Confirm";
			this.button_Confirm.Size = new System.Drawing.Size(440, 23);
			this.button_Confirm.TabIndex = 0;
			this.button_Confirm.Text = "Confirm Selection";
			this.button_Confirm.Click += new System.EventHandler(this.button_Confirm_Click);
			// 
			// panel
			// 
			this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel.Controls.Add(this.button_Confirm);
			this.panel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel.Location = new System.Drawing.Point(0, 400);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(464, 41);
			this.panel.TabIndex = 1;
			// 
			// sectionPanel
			// 
			this.sectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel.Controls.Add(this.treeView);
			this.sectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionPanel.Location = new System.Drawing.Point(0, 0);
			this.sectionPanel.Name = "sectionPanel";
			this.sectionPanel.SectionHeader = "Please select one of the following files:";
			this.sectionPanel.Size = new System.Drawing.Size(464, 400);
			this.sectionPanel.TabIndex = 0;
			// 
			// treeView
			// 
			this.treeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.treeView.ItemHeight = 40;
			this.treeView.Location = new System.Drawing.Point(1, 25);
			this.treeView.MaxDragChange = 40;
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(460, 372);
			this.treeView.TabIndex = 0;
			this.treeView.SelectedNodesChanged += new System.EventHandler(this.treeView_SelectedNodesChanged);
			this.treeView.DoubleClick += new System.EventHandler(this.treeView_DoubleClick);
			// 
			// FormFileSelection
			// 
			this.AcceptButton = this.button_Confirm;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 441);
			this.Controls.Add(this.sectionPanel);
			this.Controls.Add(this.panel);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormFileSelection";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.panel.ResumeLayout(false);
			this.sectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Confirm;
		private DarkUI.Controls.DarkSectionPanel sectionPanel;
		private DarkUI.Controls.DarkTreeView treeView;
		private System.Windows.Forms.Panel panel;
	}
}