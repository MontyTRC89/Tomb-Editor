﻿namespace TombLib.FileAssociation
{
	partial class FormMain
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
			this.button_Apply = new DarkUI.Controls.DarkButton();
			this.button_Close = new DarkUI.Controls.DarkButton();
			this.checkBox_prj2 = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_trproj = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_wad2 = new DarkUI.Controls.DarkCheckBox();
			this.label = new DarkUI.Controls.DarkLabel();
			this.panel = new System.Windows.Forms.Panel();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.Checked = false;
			this.button_Apply.Location = new System.Drawing.Point(115, 105);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(75, 23);
			this.button_Apply.TabIndex = 4;
			this.button_Apply.Text = "Apply";
			this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
			// 
			// button_Close
			// 
			this.button_Close.Checked = false;
			this.button_Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Close.Location = new System.Drawing.Point(196, 105);
			this.button_Close.Name = "button_Close";
			this.button_Close.Size = new System.Drawing.Size(75, 23);
			this.button_Close.TabIndex = 5;
			this.button_Close.Text = "Close";
			this.button_Close.Click += new System.EventHandler(this.button_Close_Click);
			// 
			// checkBox_prj2
			// 
			this.checkBox_prj2.AutoSize = true;
			this.checkBox_prj2.Location = new System.Drawing.Point(14, 30);
			this.checkBox_prj2.Name = "checkBox_prj2";
			this.checkBox_prj2.Size = new System.Drawing.Size(146, 17);
			this.checkBox_prj2.TabIndex = 1;
			this.checkBox_prj2.Text = ".prj2 files with TombEditor";
			this.checkBox_prj2.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// checkBox_trproj
			// 
			this.checkBox_trproj.AutoSize = true;
			this.checkBox_trproj.Location = new System.Drawing.Point(14, 76);
			this.checkBox_trproj.Name = "checkBox_trproj";
			this.checkBox_trproj.Size = new System.Drawing.Size(143, 17);
			this.checkBox_trproj.TabIndex = 3;
			this.checkBox_trproj.Text = ".trproj files with TombIDE";
			this.checkBox_trproj.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// checkBox_wad2
			// 
			this.checkBox_wad2.AutoSize = true;
			this.checkBox_wad2.Location = new System.Drawing.Point(14, 53);
			this.checkBox_wad2.Name = "checkBox_wad2";
			this.checkBox_wad2.Size = new System.Drawing.Size(145, 17);
			this.checkBox_wad2.TabIndex = 2;
			this.checkBox_wad2.Text = ".wad2 files with WadTool";
			this.checkBox_wad2.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.ForeColor = System.Drawing.Color.Gainsboro;
			this.label.Location = new System.Drawing.Point(11, 11);
			this.label.Margin = new System.Windows.Forms.Padding(3);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(251, 13);
			this.label.TabIndex = 0;
			this.label.Text = "Which file types would you like to have associated?";
			// 
			// panel
			// 
			this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel.Controls.Add(this.button_Close);
			this.panel.Controls.Add(this.button_Apply);
			this.panel.Controls.Add(this.checkBox_trproj);
			this.panel.Controls.Add(this.checkBox_wad2);
			this.panel.Controls.Add(this.checkBox_prj2);
			this.panel.Controls.Add(this.label);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(284, 141);
			this.panel.TabIndex = 0;
			// 
			// FormMain
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Close;
			this.ClientSize = new System.Drawing.Size(284, 141);
			this.Controls.Add(this.panel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormMain";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TombLib File Association";
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Close;
		private DarkUI.Controls.DarkCheckBox checkBox_prj2;
		private DarkUI.Controls.DarkCheckBox checkBox_trproj;
		private DarkUI.Controls.DarkCheckBox checkBox_wad2;
		private DarkUI.Controls.DarkLabel label;
		private System.Windows.Forms.Panel panel;
	}
}

