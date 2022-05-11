namespace TombIDE.ProjectMaster
{
	partial class ProjectMaster
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
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.panel_Icon = new System.Windows.Forms.Panel();
			this.label_Title = new DarkUI.Controls.DarkLabel();
			this.section_ProjectInfo = new TombIDE.ProjectMaster.SectionProjectSettings();
			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 2;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.panel_Icon, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.label_Title, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.section_ProjectInfo, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 2;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(1005, 600);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// panel_Icon
			// 
			this.panel_Icon.BackgroundImage = global::TombIDE.ProjectMaster.Properties.Resources.ide_projectmanager;
			this.panel_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.panel_Icon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Icon.Location = new System.Drawing.Point(30, 20);
			this.panel_Icon.Margin = new System.Windows.Forms.Padding(30, 20, 0, 15);
			this.panel_Icon.Name = "panel_Icon";
			this.panel_Icon.Size = new System.Drawing.Size(40, 45);
			this.panel_Icon.TabIndex = 6;
			// 
			// label_Title
			// 
			this.label_Title.AutoSize = true;
			this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Title.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title.Location = new System.Drawing.Point(71, 1);
			this.label_Title.Margin = new System.Windows.Forms.Padding(1);
			this.label_Title.Name = "label_Title";
			this.label_Title.Size = new System.Drawing.Size(933, 78);
			this.label_Title.TabIndex = 3;
			this.label_Title.Text = "Project Manager";
			this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// section_ProjectInfo
			// 
			this.section_ProjectInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tableLayoutPanel.SetColumnSpan(this.section_ProjectInfo, 2);
			this.section_ProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_ProjectInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.section_ProjectInfo.Location = new System.Drawing.Point(30, 80);
			this.section_ProjectInfo.Margin = new System.Windows.Forms.Padding(30, 0, 30, 30);
			this.section_ProjectInfo.Name = "section_ProjectInfo";
			this.section_ProjectInfo.Size = new System.Drawing.Size(945, 490);
			this.section_ProjectInfo.TabIndex = 1;
			// 
			// ProjectMaster
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.tableLayoutPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ProjectMaster";
			this.Size = new System.Drawing.Size(1005, 600);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private SectionProjectSettings section_ProjectInfo;
		private DarkUI.Controls.DarkLabel label_Title;
		private System.Windows.Forms.Panel panel_Icon;
	}
}
