namespace TombIDE.ProjectMaster
{
	partial class Miscellaneous
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
			tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			panel_GameLabel = new System.Windows.Forms.Panel();
			panel_Icon = new System.Windows.Forms.Panel();
			label_Title = new DarkUI.Controls.DarkLabel();
			section_ProjectInfo = new SectionProjectSettings();
			tableLayoutPanel.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			tableLayoutPanel.ColumnCount = 3;
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel.Controls.Add(panel_GameLabel, 2, 0);
			tableLayoutPanel.Controls.Add(panel_Icon, 0, 0);
			tableLayoutPanel.Controls.Add(label_Title, 1, 0);
			tableLayoutPanel.Controls.Add(section_ProjectInfo, 0, 1);
			tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 2;
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanel.Size = new System.Drawing.Size(1005, 600);
			tableLayoutPanel.TabIndex = 0;
			// 
			// panel_GameLabel
			// 
			panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL;
			panel_GameLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_GameLabel.Dock = System.Windows.Forms.DockStyle.Left;
			panel_GameLabel.Location = new System.Drawing.Point(160, 20);
			panel_GameLabel.Margin = new System.Windows.Forms.Padding(0, 20, 0, 15);
			panel_GameLabel.Name = "panel_GameLabel";
			panel_GameLabel.Size = new System.Drawing.Size(40, 45);
			panel_GameLabel.TabIndex = 7;
			// 
			// panel_Icon
			// 
			panel_Icon.BackgroundImage = Properties.Resources.ide_projectmanager;
			panel_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon.Dock = System.Windows.Forms.DockStyle.Fill;
			panel_Icon.Location = new System.Drawing.Point(30, 20);
			panel_Icon.Margin = new System.Windows.Forms.Padding(30, 20, 0, 15);
			panel_Icon.Name = "panel_Icon";
			panel_Icon.Size = new System.Drawing.Size(40, 45);
			panel_Icon.TabIndex = 6;
			// 
			// label_Title
			// 
			label_Title.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
			label_Title.AutoSize = true;
			label_Title.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_Title.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_Title.Location = new System.Drawing.Point(71, 1);
			label_Title.Margin = new System.Windows.Forms.Padding(1);
			label_Title.Name = "label_Title";
			label_Title.Size = new System.Drawing.Size(88, 78);
			label_Title.TabIndex = 3;
			label_Title.Text = "Utilities";
			label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// section_ProjectInfo
			// 
			section_ProjectInfo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tableLayoutPanel.SetColumnSpan(section_ProjectInfo, 3);
			section_ProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			section_ProjectInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			section_ProjectInfo.Location = new System.Drawing.Point(30, 80);
			section_ProjectInfo.Margin = new System.Windows.Forms.Padding(30, 0, 30, 30);
			section_ProjectInfo.Name = "section_ProjectInfo";
			section_ProjectInfo.Size = new System.Drawing.Size(945, 490);
			section_ProjectInfo.TabIndex = 1;
			// 
			// Miscellaneous
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			Controls.Add(tableLayoutPanel);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			Name = "Miscellaneous";
			Size = new System.Drawing.Size(1005, 600);
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private SectionProjectSettings section_ProjectInfo;
		private DarkUI.Controls.DarkLabel label_Title;
		private System.Windows.Forms.Panel panel_Icon;
		private System.Windows.Forms.Panel panel_GameLabel;
	}
}
