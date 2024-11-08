namespace TombIDE.ProjectMaster
{
	partial class LevelManager
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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			tableLayout_Main = new System.Windows.Forms.TableLayoutPanel();
			label_OutdatedState = new DarkUI.Controls.DarkLabel();
			label_EngineVersion = new DarkUI.Controls.DarkLabel();
			panel_GameLabel = new System.Windows.Forms.Panel();
			panel_Icon = new System.Windows.Forms.Panel();
			splitContainer = new System.Windows.Forms.SplitContainer();
			tableLayoutPanel_List = new System.Windows.Forms.TableLayoutPanel();
			button_RebuildAll = new DarkUI.Controls.DarkButton();
			section_LevelList = new SectionLevelList();
			section_LevelProperties = new SectionLevelProperties();
			label_Title = new DarkUI.Controls.DarkLabel();
			button_Update = new DarkUI.Controls.DarkButton();
			button_Publish = new DarkUI.Controls.DarkButton();
			toolTip = new System.Windows.Forms.ToolTip(components);
			tableLayout_Main.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			tableLayoutPanel_List.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayout_Main
			// 
			tableLayout_Main.ColumnCount = 7;
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			tableLayout_Main.Controls.Add(label_OutdatedState, 4, 0);
			tableLayout_Main.Controls.Add(label_EngineVersion, 3, 0);
			tableLayout_Main.Controls.Add(panel_GameLabel, 2, 0);
			tableLayout_Main.Controls.Add(panel_Icon, 0, 0);
			tableLayout_Main.Controls.Add(splitContainer, 0, 1);
			tableLayout_Main.Controls.Add(label_Title, 1, 0);
			tableLayout_Main.Controls.Add(button_Update, 5, 0);
			tableLayout_Main.Controls.Add(button_Publish, 6, 0);
			tableLayout_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayout_Main.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			tableLayout_Main.Location = new System.Drawing.Point(0, 0);
			tableLayout_Main.Margin = new System.Windows.Forms.Padding(0);
			tableLayout_Main.Name = "tableLayout_Main";
			tableLayout_Main.RowCount = 2;
			tableLayout_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			tableLayout_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayout_Main.Size = new System.Drawing.Size(1024, 640);
			tableLayout_Main.TabIndex = 0;
			// 
			// label_OutdatedState
			// 
			label_OutdatedState.AutoSize = true;
			label_OutdatedState.Dock = System.Windows.Forms.DockStyle.Left;
			label_OutdatedState.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			label_OutdatedState.ForeColor = System.Drawing.Color.FromArgb(255, 128, 128);
			label_OutdatedState.Location = new System.Drawing.Point(384, 0);
			label_OutdatedState.Margin = new System.Windows.Forms.Padding(0);
			label_OutdatedState.Name = "label_OutdatedState";
			label_OutdatedState.Size = new System.Drawing.Size(63, 80);
			label_OutdatedState.TabIndex = 8;
			label_OutdatedState.Text = "(Outdated)";
			label_OutdatedState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label_EngineVersion
			// 
			label_EngineVersion.AutoSize = true;
			label_EngineVersion.Dock = System.Windows.Forms.DockStyle.Left;
			label_EngineVersion.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label_EngineVersion.Location = new System.Drawing.Point(269, 1);
			label_EngineVersion.Margin = new System.Windows.Forms.Padding(1);
			label_EngineVersion.Name = "label_EngineVersion";
			label_EngineVersion.Size = new System.Drawing.Size(114, 78);
			label_EngineVersion.TabIndex = 7;
			label_EngineVersion.Text = "Engine Version: 1.0.0";
			label_EngineVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel_GameLabel
			// 
			panel_GameLabel.BackgroundImage = Properties.Resources.TRNG_LVL;
			panel_GameLabel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_GameLabel.Dock = System.Windows.Forms.DockStyle.Left;
			panel_GameLabel.Location = new System.Drawing.Point(228, 20);
			panel_GameLabel.Margin = new System.Windows.Forms.Padding(0, 20, 0, 15);
			panel_GameLabel.Name = "panel_GameLabel";
			panel_GameLabel.Size = new System.Drawing.Size(40, 45);
			panel_GameLabel.TabIndex = 6;
			// 
			// panel_Icon
			// 
			panel_Icon.BackgroundImage = Properties.Resources.ide_master_30;
			panel_Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			panel_Icon.Dock = System.Windows.Forms.DockStyle.Fill;
			panel_Icon.Location = new System.Drawing.Point(30, 20);
			panel_Icon.Margin = new System.Windows.Forms.Padding(30, 20, 0, 15);
			panel_Icon.Name = "panel_Icon";
			panel_Icon.Size = new System.Drawing.Size(40, 45);
			panel_Icon.TabIndex = 5;
			// 
			// splitContainer
			// 
			tableLayout_Main.SetColumnSpan(splitContainer, 7);
			splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer.Location = new System.Drawing.Point(0, 80);
			splitContainer.Margin = new System.Windows.Forms.Padding(0);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(tableLayoutPanel_List);
			splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(30, 0, 6, 30);
			splitContainer.Panel1MinSize = 360;
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add(section_LevelProperties);
			splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(6, 0, 30, 30);
			splitContainer.Panel2MinSize = 360;
			splitContainer.Size = new System.Drawing.Size(1024, 560);
			splitContainer.SplitterDistance = 512;
			splitContainer.SplitterWidth = 3;
			splitContainer.TabIndex = 0;
			// 
			// tableLayoutPanel_List
			// 
			tableLayoutPanel_List.ColumnCount = 1;
			tableLayoutPanel_List.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_List.Controls.Add(button_RebuildAll, 0, 1);
			tableLayoutPanel_List.Controls.Add(section_LevelList, 0, 0);
			tableLayoutPanel_List.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel_List.Location = new System.Drawing.Point(30, 0);
			tableLayoutPanel_List.Name = "tableLayoutPanel_List";
			tableLayoutPanel_List.RowCount = 2;
			tableLayoutPanel_List.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel_List.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			tableLayoutPanel_List.Size = new System.Drawing.Size(476, 530);
			tableLayoutPanel_List.TabIndex = 2;
			// 
			// button_RebuildAll
			// 
			button_RebuildAll.Checked = false;
			button_RebuildAll.Dock = System.Windows.Forms.DockStyle.Fill;
			button_RebuildAll.Image = Properties.Resources.actions_compile_16;
			button_RebuildAll.Location = new System.Drawing.Point(0, 500);
			button_RebuildAll.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			button_RebuildAll.Name = "button_RebuildAll";
			button_RebuildAll.Size = new System.Drawing.Size(476, 30);
			button_RebuildAll.TabIndex = 3;
			button_RebuildAll.Text = "Re-build all project levels at once. (Batch)";
			button_RebuildAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			button_RebuildAll.Click += button_RebuildAll_Click;
			// 
			// section_LevelList
			// 
			section_LevelList.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			section_LevelList.Dock = System.Windows.Forms.DockStyle.Fill;
			section_LevelList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			section_LevelList.Location = new System.Drawing.Point(0, 0);
			section_LevelList.Margin = new System.Windows.Forms.Padding(0);
			section_LevelList.Name = "section_LevelList";
			section_LevelList.Size = new System.Drawing.Size(476, 490);
			section_LevelList.TabIndex = 1;
			// 
			// section_LevelProperties
			// 
			section_LevelProperties.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			section_LevelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			section_LevelProperties.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			section_LevelProperties.Location = new System.Drawing.Point(6, 0);
			section_LevelProperties.Margin = new System.Windows.Forms.Padding(0);
			section_LevelProperties.Name = "section_LevelProperties";
			section_LevelProperties.Size = new System.Drawing.Size(473, 530);
			section_LevelProperties.TabIndex = 3;
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
			label_Title.Size = new System.Drawing.Size(156, 78);
			label_Title.TabIndex = 1;
			label_Title.Text = "Level Manager";
			label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// button_Update
			// 
			button_Update.Checked = false;
			button_Update.Dock = System.Windows.Forms.DockStyle.Left;
			button_Update.Image = Properties.Resources.actions_refresh_16;
			button_Update.Location = new System.Drawing.Point(453, 25);
			button_Update.Margin = new System.Windows.Forms.Padding(6, 25, 0, 25);
			button_Update.Name = "button_Update";
			button_Update.Size = new System.Drawing.Size(80, 30);
			button_Update.TabIndex = 9;
			button_Update.Text = "Update...";
			button_Update.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			button_Update.Click += button_Update_Click;
			// 
			// button_Publish
			// 
			button_Publish.Checked = false;
			button_Publish.Dock = System.Windows.Forms.DockStyle.Right;
			button_Publish.Image = Properties.Resources.archive_folder_16;
			button_Publish.Location = new System.Drawing.Point(954, 20);
			button_Publish.Margin = new System.Windows.Forms.Padding(0, 20, 30, 20);
			button_Publish.Name = "button_Publish";
			button_Publish.Size = new System.Drawing.Size(40, 40);
			button_Publish.TabIndex = 10;
			toolTip.SetToolTip(button_Publish, "Create a \"Ready To Publish\" Game Archive...");
			button_Publish.Click += button_Publish_Click;
			// 
			// LevelManager
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			Controls.Add(tableLayout_Main);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			Name = "LevelManager";
			Size = new System.Drawing.Size(1024, 640);
			tableLayout_Main.ResumeLayout(false);
			tableLayout_Main.PerformLayout();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			tableLayoutPanel_List.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private DarkUI.Controls.DarkLabel label_Title;
		private SectionLevelList section_LevelList;
		private SectionLevelProperties section_LevelProperties;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.TableLayoutPanel tableLayout_Main;
		private System.Windows.Forms.Panel panel_Icon;
		private DarkUI.Controls.DarkButton button_RebuildAll;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_List;
		private System.Windows.Forms.Panel panel_GameLabel;
		private DarkUI.Controls.DarkLabel label_OutdatedState;
		private DarkUI.Controls.DarkLabel label_EngineVersion;
		private DarkUI.Controls.DarkButton button_Update;
		private DarkUI.Controls.DarkButton button_Publish;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
