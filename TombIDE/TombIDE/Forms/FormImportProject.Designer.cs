namespace TombIDE
{
	partial class FormImportProject
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
			this.components = new System.ComponentModel.Container();
			this.button_BrowseLevels = new DarkUI.Controls.DarkButton();
			this.button_BrowseScript = new DarkUI.Controls.DarkButton();
			this.button_Import = new DarkUI.Controls.DarkButton();
			this.label_01 = new DarkUI.Controls.DarkLabel();
			this.label_02 = new DarkUI.Controls.DarkLabel();
			this.label_03 = new DarkUI.Controls.DarkLabel();
			this.label_04 = new DarkUI.Controls.DarkLabel();
			this.label_05 = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.textBox_LauncherPath = new System.Windows.Forms.TextBox();
			this.textBox_ExePath = new System.Windows.Forms.TextBox();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.textBox_ProjectName = new DarkUI.Controls.DarkTextBox();
			this.textBox_LevelsPath = new DarkUI.Controls.DarkTextBox();
			this.textBox_ScriptPath = new DarkUI.Controls.DarkTextBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_BrowseLevels
			// 
			this.button_BrowseLevels.Checked = false;
			this.button_BrowseLevels.Location = new System.Drawing.Point(363, 123);
			this.button_BrowseLevels.Margin = new System.Windows.Forms.Padding(3, 3, 6, 8);
			this.button_BrowseLevels.Name = "button_BrowseLevels";
			this.button_BrowseLevels.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseLevels.TabIndex = 7;
			this.button_BrowseLevels.Text = "Browse...";
			this.button_BrowseLevels.Click += new System.EventHandler(this.button_BrowseLevels_Click);
			// 
			// button_BrowseScript
			// 
			this.button_BrowseScript.Checked = false;
			this.button_BrowseScript.Location = new System.Drawing.Point(363, 75);
			this.button_BrowseScript.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.button_BrowseScript.Name = "button_BrowseScript";
			this.button_BrowseScript.Size = new System.Drawing.Size(75, 22);
			this.button_BrowseScript.TabIndex = 4;
			this.button_BrowseScript.Text = "Browse...";
			this.button_BrowseScript.Click += new System.EventHandler(this.button_BrowseScript_Click);
			// 
			// button_Import
			// 
			this.button_Import.Checked = false;
			this.button_Import.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Import.Location = new System.Drawing.Point(9, 239);
			this.button_Import.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.button_Import.Name = "button_Import";
			this.button_Import.Size = new System.Drawing.Size(446, 23);
			this.button_Import.TabIndex = 2;
			this.button_Import.Text = "Import Project";
			this.button_Import.Click += new System.EventHandler(this.button_Import_Click);
			// 
			// label_01
			// 
			this.label_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_01.Location = new System.Drawing.Point(6, 6);
			this.label_01.Margin = new System.Windows.Forms.Padding(6, 6, 0, 3);
			this.label_01.Name = "label_01";
			this.label_01.Size = new System.Drawing.Size(111, 20);
			this.label_01.TabIndex = 0;
			this.label_01.Text = "Game\'s .exe path:";
			this.label_01.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_02
			// 
			this.label_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_02.Location = new System.Drawing.Point(6, 32);
			this.label_02.Margin = new System.Windows.Forms.Padding(6, 3, 0, 6);
			this.label_02.Name = "label_02";
			this.label_02.Size = new System.Drawing.Size(111, 20);
			this.label_02.TabIndex = 2;
			this.label_02.Text = "Launcher file location:";
			this.label_02.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_03
			// 
			this.label_03.AutoSize = true;
			this.label_03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_03.Location = new System.Drawing.Point(6, 6);
			this.label_03.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
			this.label_03.Name = "label_03";
			this.label_03.Size = new System.Drawing.Size(72, 13);
			this.label_03.TabIndex = 0;
			this.label_03.Text = "Project name:";
			// 
			// label_04
			// 
			this.label_04.AutoSize = true;
			this.label_04.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_04.Location = new System.Drawing.Point(6, 60);
			this.label_04.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			this.label_04.Name = "label_04";
			this.label_04.Size = new System.Drawing.Size(181, 13);
			this.label_04.TabIndex = 2;
			this.label_04.Text = "Script location: (Existing Script folder)";
			// 
			// label_05
			// 
			this.label_05.AutoSize = true;
			this.label_05.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_05.Location = new System.Drawing.Point(6, 108);
			this.label_05.Margin = new System.Windows.Forms.Padding(6, 9, 3, 0);
			this.label_05.Name = "label_05";
			this.label_05.Size = new System.Drawing.Size(350, 13);
			this.label_05.TabIndex = 5;
			this.label_05.Text = "Levels location: (The folder where newly created levels should be stored)";
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.textBox_LauncherPath);
			this.panel_01.Controls.Add(this.textBox_ExePath);
			this.panel_01.Controls.Add(this.label_01);
			this.panel_01.Controls.Add(this.label_02);
			this.panel_01.Location = new System.Drawing.Point(9, 12);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(446, 60);
			this.panel_01.TabIndex = 0;
			// 
			// textBox_LauncherPath
			// 
			this.textBox_LauncherPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_LauncherPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_LauncherPath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_LauncherPath.Location = new System.Drawing.Point(120, 32);
			this.textBox_LauncherPath.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			this.textBox_LauncherPath.Name = "textBox_LauncherPath";
			this.textBox_LauncherPath.ReadOnly = true;
			this.textBox_LauncherPath.Size = new System.Drawing.Size(318, 20);
			this.textBox_LauncherPath.TabIndex = 3;
			// 
			// textBox_ExePath
			// 
			this.textBox_ExePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.textBox_ExePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_ExePath.ForeColor = System.Drawing.Color.Gainsboro;
			this.textBox_ExePath.Location = new System.Drawing.Point(120, 6);
			this.textBox_ExePath.Margin = new System.Windows.Forms.Padding(3, 6, 6, 3);
			this.textBox_ExePath.Name = "textBox_ExePath";
			this.textBox_ExePath.ReadOnly = true;
			this.textBox_ExePath.Size = new System.Drawing.Size(318, 20);
			this.textBox_ExePath.TabIndex = 1;
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.label_03);
			this.panel_02.Controls.Add(this.textBox_ProjectName);
			this.panel_02.Controls.Add(this.button_BrowseLevels);
			this.panel_02.Controls.Add(this.textBox_LevelsPath);
			this.panel_02.Controls.Add(this.label_05);
			this.panel_02.Controls.Add(this.button_BrowseScript);
			this.panel_02.Controls.Add(this.textBox_ScriptPath);
			this.panel_02.Controls.Add(this.label_04);
			this.panel_02.Location = new System.Drawing.Point(9, 78);
			this.panel_02.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(446, 155);
			this.panel_02.TabIndex = 1;
			// 
			// textBox_ProjectName
			// 
			this.textBox_ProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_ProjectName.Location = new System.Drawing.Point(6, 22);
			this.textBox_ProjectName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.textBox_ProjectName.Name = "textBox_ProjectName";
			this.textBox_ProjectName.Size = new System.Drawing.Size(432, 26);
			this.textBox_ProjectName.TabIndex = 1;
			// 
			// textBox_LevelsPath
			// 
			this.textBox_LevelsPath.Location = new System.Drawing.Point(6, 124);
			this.textBox_LevelsPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 9);
			this.textBox_LevelsPath.Name = "textBox_LevelsPath";
			this.textBox_LevelsPath.Size = new System.Drawing.Size(351, 20);
			this.textBox_LevelsPath.TabIndex = 6;
			this.textBox_LevelsPath.TextChanged += new System.EventHandler(this.textBox_LevelsPath_TextChanged);
			// 
			// textBox_ScriptPath
			// 
			this.textBox_ScriptPath.Location = new System.Drawing.Point(6, 76);
			this.textBox_ScriptPath.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.textBox_ScriptPath.Name = "textBox_ScriptPath";
			this.textBox_ScriptPath.Size = new System.Drawing.Size(351, 20);
			this.textBox_ScriptPath.TabIndex = 3;
			// 
			// toolTip
			// 
			this.toolTip.AutomaticDelay = 1;
			this.toolTip.AutoPopDelay = 5000;
			this.toolTip.InitialDelay = 1;
			this.toolTip.ReshowDelay = 1;
			this.toolTip.UseAnimation = false;
			this.toolTip.UseFading = false;
			// 
			// FormImportProject
			// 
			this.AcceptButton = this.button_Import;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 274);
			this.Controls.Add(this.button_Import);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "FormImportProject";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Import Project";
			this.panel_01.ResumeLayout(false);
			this.panel_01.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.panel_02.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_BrowseLevels;
		private DarkUI.Controls.DarkButton button_BrowseScript;
		private DarkUI.Controls.DarkButton button_Import;
		private DarkUI.Controls.DarkLabel label_01;
		private DarkUI.Controls.DarkLabel label_02;
		private DarkUI.Controls.DarkLabel label_03;
		private DarkUI.Controls.DarkLabel label_04;
		private DarkUI.Controls.DarkLabel label_05;
		private DarkUI.Controls.DarkTextBox textBox_LevelsPath;
		private DarkUI.Controls.DarkTextBox textBox_ProjectName;
		private DarkUI.Controls.DarkTextBox textBox_ScriptPath;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
		private System.Windows.Forms.TextBox textBox_ExePath;
		private System.Windows.Forms.TextBox textBox_LauncherPath;
		private System.Windows.Forms.ToolTip toolTip;
	}
}