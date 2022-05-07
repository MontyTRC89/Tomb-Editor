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
			this.tableLayout_Main = new System.Windows.Forms.TableLayoutPanel();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.section_LevelList = new TombIDE.ProjectMaster.SectionLevelList();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.section_LevelProperties = new TombIDE.ProjectMaster.SectionLevelProperties();
			this.button_Main_Import = new DarkUI.Controls.DarkButton();
			this.button_Main_New = new DarkUI.Controls.DarkButton();
			this.label_Title = new DarkUI.Controls.DarkLabel();
			this.levelFolderWatcher = new System.IO.FileSystemWatcher();
			this.prj2FileWatcher = new System.IO.FileSystemWatcher();
			this.tableLayout_Main.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayout_Main
			// 
			this.tableLayout_Main.ColumnCount = 1;
			this.tableLayout_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout_Main.Controls.Add(this.splitContainer, 0, 1);
			this.tableLayout_Main.Controls.Add(this.label_Title, 0, 0);
			this.tableLayout_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayout_Main.Location = new System.Drawing.Point(0, 0);
			this.tableLayout_Main.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayout_Main.Name = "tableLayout_Main";
			this.tableLayout_Main.RowCount = 2;
			this.tableLayout_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayout_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayout_Main.Size = new System.Drawing.Size(1024, 640);
			this.tableLayout_Main.TabIndex = 0;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(3, 83);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.section_LevelList);
			this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(30, 0, 10, 30);
			this.splitContainer.Panel1MinSize = 360;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(10, 0, 30, 30);
			this.splitContainer.Panel2MinSize = 360;
			this.splitContainer.Size = new System.Drawing.Size(1018, 554);
			this.splitContainer.SplitterDistance = 512;
			this.splitContainer.SplitterWidth = 3;
			this.splitContainer.TabIndex = 0;
			// 
			// section_LevelList
			// 
			this.section_LevelList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_LevelList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_LevelList.Location = new System.Drawing.Point(30, 0);
			this.section_LevelList.Margin = new System.Windows.Forms.Padding(0);
			this.section_LevelList.Name = "section_LevelList";
			this.section_LevelList.Size = new System.Drawing.Size(472, 524);
			this.section_LevelList.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.section_LevelProperties, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.button_Main_Import, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.button_Main_New, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(463, 524);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// section_LevelProperties
			// 
			this.section_LevelProperties.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.section_LevelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.section_LevelProperties.Location = new System.Drawing.Point(0, 180);
			this.section_LevelProperties.Margin = new System.Windows.Forms.Padding(0, 20, 0, 0);
			this.section_LevelProperties.Name = "section_LevelProperties";
			this.section_LevelProperties.Size = new System.Drawing.Size(463, 344);
			this.section_LevelProperties.TabIndex = 3;
			// 
			// button_Main_Import
			// 
			this.button_Main_Import.Checked = false;
			this.button_Main_Import.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Main_Import.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_Main_Import.Image = global::TombIDE.ProjectMaster.Properties.Resources.import_48;
			this.button_Main_Import.Location = new System.Drawing.Point(0, 83);
			this.button_Main_Import.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.button_Main_Import.Name = "button_Main_Import";
			this.button_Main_Import.Size = new System.Drawing.Size(463, 77);
			this.button_Main_Import.TabIndex = 2;
			this.button_Main_Import.Text = "Import an existing level...\r\n";
			this.button_Main_Import.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Main_Import.Click += new System.EventHandler(this.button_Main_Import_Click);
			// 
			// button_Main_New
			// 
			this.button_Main_New.Checked = false;
			this.button_Main_New.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_Main_New.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.button_Main_New.Image = global::TombIDE.ProjectMaster.Properties.Resources.add_48;
			this.button_Main_New.Location = new System.Drawing.Point(0, 0);
			this.button_Main_New.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.button_Main_New.Name = "button_Main_New";
			this.button_Main_New.Size = new System.Drawing.Size(463, 77);
			this.button_Main_New.TabIndex = 1;
			this.button_Main_New.Text = "Create a new level...";
			this.button_Main_New.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.button_Main_New.Click += new System.EventHandler(this.button_Main_New_Click);
			// 
			// label_Title
			// 
			this.label_Title.AutoSize = true;
			this.label_Title.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Title.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Title.Location = new System.Drawing.Point(30, 0);
			this.label_Title.Margin = new System.Windows.Forms.Padding(30, 0, 0, 0);
			this.label_Title.Name = "label_Title";
			this.label_Title.Size = new System.Drawing.Size(994, 80);
			this.label_Title.TabIndex = 1;
			this.label_Title.Text = "Level Manager";
			this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// levelFolderWatcher
			// 
			this.levelFolderWatcher.EnableRaisingEvents = true;
			this.levelFolderWatcher.NotifyFilter = System.IO.NotifyFilters.DirectoryName;
			this.levelFolderWatcher.SynchronizingObject = this;
			this.levelFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.levelFolderWatcher_Deleted);
			// 
			// prj2FileWatcher
			// 
			this.prj2FileWatcher.EnableRaisingEvents = true;
			this.prj2FileWatcher.Filter = "*.prj2";
			this.prj2FileWatcher.IncludeSubdirectories = true;
			this.prj2FileWatcher.SynchronizingObject = this;
			this.prj2FileWatcher.Deleted += new System.IO.FileSystemEventHandler(this.prj2FileWatcher_Deleted);
			// 
			// LevelManager
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.tableLayout_Main);
			this.Name = "LevelManager";
			this.Size = new System.Drawing.Size(1024, 640);
			this.tableLayout_Main.ResumeLayout(false);
			this.tableLayout_Main.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.levelFolderWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.prj2FileWatcher)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Main_Import;
		private DarkUI.Controls.DarkButton button_Main_New;
		private DarkUI.Controls.DarkLabel label_Title;
		private SectionLevelList section_LevelList;
		private SectionLevelProperties section_LevelProperties;
		private System.IO.FileSystemWatcher levelFolderWatcher;
		private System.IO.FileSystemWatcher prj2FileWatcher;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.TableLayoutPanel tableLayout_Main;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
