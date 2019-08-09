namespace TombIDE.ProjectMaster
{
	partial class FormRenameLevel
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
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.checkBox_RenameDirectory = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_RenameScriptEntry = new DarkUI.Controls.DarkCheckBox();
			this.label = new DarkUI.Controls.DarkLabel();
			this.label_LanguageError = new DarkUI.Controls.DarkLabel();
			this.label_ScriptError = new DarkUI.Controls.DarkLabel();
			this.panel_01 = new System.Windows.Forms.Panel();
			this.textBox_NewName = new DarkUI.Controls.DarkTextBox();
			this.panel_02 = new System.Windows.Forms.Panel();
			this.panel_01.SuspendLayout();
			this.panel_02.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Apply
			// 
			this.button_Apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Apply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Apply.Location = new System.Drawing.Point(301, 8);
			this.button_Apply.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.Size = new System.Drawing.Size(75, 23);
			this.button_Apply.TabIndex = 0;
			this.button_Apply.Text = "Apply";
			this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(379, 8);
			this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 9, 0, 0);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.button_Cancel.TabIndex = 1;
			this.button_Cancel.Text = "Cancel";
			// 
			// checkBox_RenameDirectory
			// 
			this.checkBox_RenameDirectory.Checked = true;
			this.checkBox_RenameDirectory.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox_RenameDirectory.Location = new System.Drawing.Point(9, 54);
			this.checkBox_RenameDirectory.Margin = new System.Windows.Forms.Padding(9, 3, 3, 0);
			this.checkBox_RenameDirectory.Name = "checkBox_RenameDirectory";
			this.checkBox_RenameDirectory.Size = new System.Drawing.Size(260, 23);
			this.checkBox_RenameDirectory.TabIndex = 2;
			this.checkBox_RenameDirectory.Text = "Rename level directory as well (Recommended)";
			// 
			// checkBox_RenameScriptEntry
			// 
			this.checkBox_RenameScriptEntry.Checked = true;
			this.checkBox_RenameScriptEntry.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox_RenameScriptEntry.Location = new System.Drawing.Point(9, 80);
			this.checkBox_RenameScriptEntry.Margin = new System.Windows.Forms.Padding(9, 3, 3, 0);
			this.checkBox_RenameScriptEntry.Name = "checkBox_RenameScriptEntry";
			this.checkBox_RenameScriptEntry.Size = new System.Drawing.Size(345, 23);
			this.checkBox_RenameScriptEntry.TabIndex = 3;
			this.checkBox_RenameScriptEntry.Text = "Rename script entry and language string as well (Recommended)";
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label.Location = new System.Drawing.Point(3, 3);
			this.label.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(38, 13);
			this.label.TabIndex = 0;
			this.label.Text = "Name:";
			// 
			// label_LanguageError
			// 
			this.label_LanguageError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label_LanguageError.AutoSize = true;
			this.label_LanguageError.ForeColor = System.Drawing.Color.Gray;
			this.label_LanguageError.Location = new System.Drawing.Point(20, 122);
			this.label_LanguageError.Margin = new System.Windows.Forms.Padding(12, 3, 3, 9);
			this.label_LanguageError.Name = "label_LanguageError";
			this.label_LanguageError.Size = new System.Drawing.Size(331, 13);
			this.label_LanguageError.TabIndex = 5;
			this.label_LanguageError.Text = "Couldn\'t find a language string that matches the current level\'s name.";
			this.label_LanguageError.Visible = false;
			// 
			// label_ScriptError
			// 
			this.label_ScriptError.AutoSize = true;
			this.label_ScriptError.ForeColor = System.Drawing.Color.Gray;
			this.label_ScriptError.Location = new System.Drawing.Point(20, 106);
			this.label_ScriptError.Margin = new System.Windows.Forms.Padding(12, 3, 3, 0);
			this.label_ScriptError.Name = "label_ScriptError";
			this.label_ScriptError.Size = new System.Drawing.Size(321, 13);
			this.label_ScriptError.TabIndex = 4;
			this.label_ScriptError.Text = "Couldn\'t find a script section that matches the current level\'s name.";
			this.label_ScriptError.Visible = false;
			// 
			// panel_01
			// 
			this.panel_01.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_01.Controls.Add(this.label_LanguageError);
			this.panel_01.Controls.Add(this.label_ScriptError);
			this.panel_01.Controls.Add(this.checkBox_RenameScriptEntry);
			this.panel_01.Controls.Add(this.textBox_NewName);
			this.panel_01.Controls.Add(this.checkBox_RenameDirectory);
			this.panel_01.Controls.Add(this.label);
			this.panel_01.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_01.Location = new System.Drawing.Point(0, 0);
			this.panel_01.Margin = new System.Windows.Forms.Padding(0);
			this.panel_01.Name = "panel_01";
			this.panel_01.Size = new System.Drawing.Size(464, 148);
			this.panel_01.TabIndex = 0;
			// 
			// textBox_NewName
			// 
			this.textBox_NewName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_NewName.Location = new System.Drawing.Point(6, 19);
			this.textBox_NewName.Margin = new System.Windows.Forms.Padding(6, 3, 6, 6);
			this.textBox_NewName.Name = "textBox_NewName";
			this.textBox_NewName.Size = new System.Drawing.Size(450, 26);
			this.textBox_NewName.TabIndex = 1;
			this.textBox_NewName.TextChanged += new System.EventHandler(this.textBox_NewName_TextChanged);
			// 
			// panel_02
			// 
			this.panel_02.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_02.Controls.Add(this.button_Cancel);
			this.panel_02.Controls.Add(this.button_Apply);
			this.panel_02.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel_02.Location = new System.Drawing.Point(0, 148);
			this.panel_02.Name = "panel_02";
			this.panel_02.Size = new System.Drawing.Size(464, 41);
			this.panel_02.TabIndex = 2;
			// 
			// FormRenameLevel
			// 
			this.AcceptButton = this.button_Apply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_Cancel;
			this.ClientSize = new System.Drawing.Size(464, 189);
			this.Controls.Add(this.panel_01);
			this.Controls.Add(this.panel_02);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormRenameLevel";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Rename Level";
			this.panel_01.ResumeLayout(false);
			this.panel_01.PerformLayout();
			this.panel_02.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Apply;
		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkCheckBox checkBox_RenameDirectory;
		private DarkUI.Controls.DarkCheckBox checkBox_RenameScriptEntry;
		private DarkUI.Controls.DarkLabel label;
		private DarkUI.Controls.DarkLabel label_LanguageError;
		private DarkUI.Controls.DarkLabel label_ScriptError;
		private DarkUI.Controls.DarkTextBox textBox_NewName;
		private System.Windows.Forms.Panel panel_01;
		private System.Windows.Forms.Panel panel_02;
	}
}